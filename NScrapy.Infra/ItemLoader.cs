using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NScrapy.Infra
{
    public class ItemLoader<T>
        where T : class, new()
    {
        public event Action<object, EventArgs> BeforeValueSetting;
        public event Action<object, EventArgs> PostValueSetting;

        private IResponse _response = null;

        //TODO: later on we might replace the fieldMapping with a tree structure
        private Dictionary<string, HashSet<string>> fieldMapping;
        private Regex cssSelectorReg = new Regex(@"(?<=css:)[\s\S]*");
        private Regex xPathSelectorReg = new Regex(@"(?<=xpath:)[\s\S]*");
        private Regex regSelectorReg = new Regex(@"(?<=reg:)[\s\S]*");

        private static Dictionary<Type, object> registedItemLoader;
        private ItemLoader(IResponse response)
        {
            this._response = response;
            fieldMapping = new Dictionary<string, HashSet<string>>();
        }

        public static ItemLoader<TItemType> GetItemLoader<TItemType>(IResponse response)
            where TItemType:class,new()
        {
            ItemLoader<TItemType> returnValue = null;
            if (!registedItemLoader.ContainsKey(typeof(TItemType)))
            {
                returnValue= new ItemLoader<TItemType>(response);
                registedItemLoader.Add(typeof(TItemType), returnValue);
            }
            else
            {
                returnValue = registedItemLoader[typeof(TItemType)] as ItemLoader<TItemType>;
            }            
            return returnValue;
        }                
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldSelector">for css selector=>"css:{selector}" such as css:.className div a::attr(text)
        /// for xpath selector=>"xpath:{selector}"
        /// for direct valueMapping, directly use the value</param>
        public void AddFieldMapping(string fieldName,string mapping)
        {
            if(!fieldMapping.ContainsKey(fieldName))
            {
                fieldMapping.Add(fieldName, new HashSet<string>());                
            }
            if(!fieldMapping[fieldName].Contains(mapping))
            {
                fieldMapping[fieldName].Add(mapping);
            }            
        }
        
        public T LoadItem()
        {
            T item = new T();
            var properties = typeof(T).GetProperties();
            foreach(var property in properties)
            {
                var maps = fieldMapping[property.Name];
                Regex selectorReg = null;
                string value = null;
                foreach(var map in maps)
                {
                    if (cssSelectorReg.IsMatch(map))
                    {
                        selectorReg = cssSelectorReg;
                        var selector = selectorReg.Match(map).Value;
                        value = this._response.CssSelector(selector).ExtractFirst();
                    }
                    else if (xPathSelectorReg.IsMatch(map))
                    {
                        selectorReg = xPathSelectorReg;
                        var selector = selectorReg.Match(map).Value;
                        value = this._response.CssSelector(selector).ExtractFirst();
                    }
                    else if (regSelectorReg.IsMatch(map))
                    {
                        throw new NotImplementedException("RegSelector not implemented");
                    }
                    if (value == null)
                    {
                        NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {_response.URL} by selector {map}");
                        continue;
                    }
                                      
                    property.SetValue(item, value);
                }
            }
            return item;
        }
    }
}
