using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NScrapy.Infra
{
    public class ItemLoader<T>
        where T:class,new()
    {
        private IResponse _response = null;

        //TODO: later on we might replace the fieldMapping with a tree structure
        private Dictionary<string, List<string>> fieldMapping;
        private Regex cssSelectorReg = new Regex(@"(?<=css:)[\s\S]*");
        private Regex xPathSelectorReg = new Regex(@"(?<=xpath:)[\s\S]*");
        public ItemLoader(IResponse response)
        {
            this._response = response;
            fieldMapping = new Dictionary<string, List<string>>();
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
                fieldMapping.Add(fieldName, new List<string>());                
            }
            fieldMapping[fieldName].Add(mapping);
        }
        
        public T LoadItem()
        {
            T item = new T();
            var properties = typeof(T).GetProperties();
            foreach(var property in properties)
            {
                var maps = fieldMapping[property.Name];
                foreach(var map in maps)
                {
                    if(cssSelectorReg.IsMatch(map))
                    {
                        var cssSelector=cssSelectorReg.Match(map).Value;
                        var value=this._response.CssSelector(cssSelector).ExtractFirst();
                        if(value==null)
                        {
                            NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {_response.URL} by selector {map}");
                            continue;
                        }
                        property.SetValue(item, value);
                    }
                }
            }
            return item;
        }
    }
}
