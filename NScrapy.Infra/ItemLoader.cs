using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NScrapy.Infra
{
    public class ItemLoader<T>
        where T : class, new()
    {
        public event Action<object, ValueSettingEventArgs<T>> BeforeValueSetting;
        public event Action<object, ValueSettingEventArgs<T>> PostValueSetting;
        

        internal IResponse _response = null;

        //TODO: later on we might replace the fieldMapping with a tree structure
        private Dictionary<string, HashSet<string>> fieldMapping;
        private Dictionary<PropertyInfo, HashSet<string>> fieldMappingCache;
        private Regex cssSelectorReg = new Regex(@"(?<=css:)[\s\S]*");
        private Regex xPathSelectorReg = new Regex(@"(?<=xpath:)[\s\S]*");
        private Regex regSelectorReg = new Regex(@"(?<=reg:)[\s\S]*");
        internal List<IPipeline<T>> pipelines = new List<IPipeline<T>>();
        private bool fieldMapped = false;
        private object lockObj = new object();

        public ItemLoaderStatus Status { get; set; }
        internal ItemLoader(IResponse response)
        {
            this._response = response;
            fieldMapping = new Dictionary<string, HashSet<string>>();
            fieldMappingCache = new Dictionary<PropertyInfo, HashSet<string>>();
        }

        internal void ClearEvent ()
        {
            if (BeforeValueSetting != null)
            {
                BeforeValueSetting = null;
            }
            if (PostValueSetting != null)
            {
                PostValueSetting = null;
            }
        }
        /// <summary>
        /// Only support FirstLevel Property Visit at this moment like 
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="mapping"></param>
        public void AddFieldMapping<TReturn>(Expression<Func<T,TReturn>> fieldExpression,string mapping)
        {
            if (!fieldMapped)
            {
                var vistor = new MyExpVistor();
                vistor.Visit(fieldExpression);
                var fieldName = vistor.Field;
                AddFieldMapping(fieldName, mapping);
            }
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
            lock (lockObj)
            {
                if (!fieldMapped)
                {
                    if (!fieldMapping.ContainsKey(fieldName))
                    {
                        fieldMapping.Add(fieldName, new HashSet<string>());
                    }
                    if (!fieldMapping[fieldName].Contains(mapping))
                    {
                        fieldMapping[fieldName].Add(mapping);
                    }
                }
            }
        }
        
        public T LoadItem()
        {
            T item = new T();
            try
            {
                Status = ItemLoaderStatus.Running;
                if (!fieldMapped)
                {
                    var properties = typeof(T).GetProperties();
                    foreach (var property in properties)
                    {
                        if (!fieldMapping.ContainsKey(property.Name))
                        {
                            continue;
                        }
                        var maps = fieldMapping[property.Name];
                        lock (fieldMappingCache)
                        {
                            if (!fieldMappingCache.ContainsKey(property))
                            {
                                fieldMappingCache.Add(property, maps);
                            }
                        }
                        SetValueForProperty(item, property, maps);
                    }
                }
                else
                {
                    foreach (var property in fieldMappingCache.Keys)
                    {
                        var maps = fieldMappingCache[property];
                        SetValueForProperty(item, property, maps);
                    }
                }
                foreach (var pipeline in this.pipelines)
                {
                    pipeline.ProcessItem(item, NScrapyContext.CurrentContext.CurrentSpider);
                }
                fieldMapped = true;
            }
            catch(Exception ex)
            {
                NScrapyContext.CurrentContext.Log.Error($"Load Item {typeof(T).ToString()} failed", ex);
            }
            finally
            {
                Status = ItemLoaderStatus.Idel;
            }
            return item;
        }

        private void SetValueForProperty(T item, PropertyInfo property, HashSet<string> maps)
        {
            Regex selectorReg = null;
            string value = null;
            foreach (var map in maps)
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
                if (BeforeValueSetting != null)
                {
                    BeforeValueSetting(this, eventArg);
                }
                property.SetValue(item, eventArg.Value);
                if (PostValueSetting != null)
                {
                    PostValueSetting(this, eventArg);
                }
            }
        }
    }

    class MyExpVistor:ExpressionVisitor
    {
        public string Field { get; set; }
        protected override Expression VisitMember(MemberExpression node)
        {
            Field = node.Member.Name;
            return base.VisitMember(node);
        }
    }

    public class ValueSettingEventArgs<T>:EventArgs
    {
        public T Item { get; set; }
        public string Value { get; set; }
        public string FieldName { get; set; }
    }

    public enum ItemLoaderStatus
    {
        Running,
        Idel
    }

}
