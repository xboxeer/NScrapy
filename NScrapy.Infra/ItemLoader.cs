using System;
using System.Collections.Generic;
using System.Reflection;
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
        internal List<IPipeline<T>> pipelines = new List<IPipeline<T>>();
        
        internal ItemLoader(IResponse response)
        {
            this._response = response;
            fieldMapping = new Dictionary<string, HashSet<string>>();
        }

        internal void ClearEvent ()
        {
            BeforeValueSetting = null;
            PostValueSetting = null;
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
                if(!fieldMapping.ContainsKey(property.Name))
                {
                    continue;
                }
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
                        value = this._response.XPathSelector(selector).ExtractFirst();
                    }
                    else if (regSelectorReg.IsMatch(map))
                    {
                        throw new NotImplementedException("RegSelector not implemented");
                    }
                    else
                    {
                        value = map;
                    }
                    if (value == null)
                    {
                        NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {_response.URL} by selector {map}");
                        continue;
                    }
                    var eventArg = new ValueSettingEventArgs<T>()
                    {
                        Item = item,
                        Value = value,
                        FieldName = property.Name
                    };
                    if (BeforeValueSetting!=null)
                    {                        
                        BeforeValueSetting(this, eventArg);
                    }
                    property.SetValue(item, value);
                    if (PostValueSetting != null)
                    {
                        PostValueSetting(this, eventArg);
                    }
                }
            }
            foreach(var pipeline in this.pipelines)
            {
                pipeline.ProcessItem(item, NScrapyContext.CurrentContext.CurrentSpider);
            }
            return item;
        }
    }

    public class ValueSettingEventArgs<T>:EventArgs
    {
        public T Item { get; set; }
        public string Value { get; set; }
        public string FieldName { get; set; }
    }

}
