using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace NScrapy.Infra
{
    public class ItemLoaderFactory
    {
        //private static Dictionary<Type, object> registedItemLoader = new Dictionary<Type, object>();
        private static object lockObj = new object();
        private static Dictionary<string, List<object>> itemLoaderPool = new Dictionary<string, List<object>>();
        private static Dictionary<string, object> registedPipelines = new Dictionary<string, object>();
        /// <summary>
        /// For a particular TItemType, there will be only one ItemLoader instacne for it
        /// GetItemLoader will automatically remove the Before/PostValueSetting event everytime you call it in case you are assigning event in a call back(which will result to duplicate event assigned to a particular ItemLoader)
        /// </summary>
        /// <typeparam name="TItemType"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ItemLoader<TItemType> GetItemLoader<TItemType>(IResponse response)
            where TItemType : class, new()
        {
            lock (lockObj)
            {
                ItemLoader<TItemType> returnValue = null;
                //if (!registedItemLoader.ContainsKey(typeof(TItemType)))
                //{
                //    returnValue = new ItemLoader<TItemType>(response);
                //    registedItemLoader.Add(typeof(TItemType), returnValue);
                //    List<IPipeline<TItemType>> itemPipelines = GetPipelines<TItemType>();
                //    returnValue.pipelines = itemPipelines;
                //}
                //else
                //{
                //    returnValue = registedItemLoader[typeof(TItemType)] as ItemLoader<TItemType>;
                //}     
                //Check if we have ItemLoader instance for TItemType
                if (!itemLoaderPool.ContainsKey(typeof(TItemType).FullName))
                {
                    itemLoaderPool.Add(typeof(TItemType).FullName, new List<object>());
                }

                //Check if we have idel ItemLoader
                var idelItemLoader = itemLoaderPool[typeof(TItemType).FullName].Where(loader => (loader as ItemLoader<TItemType>).Status == ItemLoaderStatus.Idel).FirstOrDefault();
                returnValue = idelItemLoader as ItemLoader<TItemType>;
                if (returnValue == null)
                {
                    returnValue = new ItemLoader<TItemType>(response);
                    List<IPipeline<TItemType>> itemPipelines = GetPipelines<TItemType>();
                    returnValue.pipelines = itemPipelines;
                    itemLoaderPool[typeof(TItemType).FullName].Add(returnValue as object);
                }

                returnValue.Status = ItemLoaderStatus.Running;
                returnValue.ClearEvent();
                returnValue._response = response;
                return returnValue;
            }
        }

        private static List<IPipeline<TItemType>> GetPipelines<TItemType>() where TItemType : class, new()
        {
            if(registedPipelines.ContainsKey(typeof(TItemType).FullName))
            {
                return registedPipelines[typeof(TItemType).FullName] as List<IPipeline<TItemType>>;
            }
            var returnValue = new List<IPipeline<TItemType>>();
            var appAssembly = Assembly.GetEntryAssembly();
            var currentAssembly = Assembly.GetExecutingAssembly();
            var pipelineNames = NScrapyContext.CurrentContext.Configuration.GetSection("AppSettings:Pipelines").GetChildren();
            foreach (var pipelineNamePath in pipelineNames)
            {
                var path = $"{pipelineNamePath.Path}:Pipeline";
                var pipelineName = NScrapyContext.CurrentContext.Configuration[path];
                if (string.IsNullOrEmpty(pipelineName))
                {
                    continue;
                }
                var pipelineType = currentAssembly.GetType(pipelineName);
                if (pipelineType == null)
                {
                    pipelineType = appAssembly.GetType(pipelineName);
                }
                if (pipelineType == null)
                {
                    throw new ArgumentNullException($"NScrapy can not find Pipeline {pipelineName}");
                }
                var pipline = Activator.CreateInstance(pipelineType) as IPipeline<TItemType>;
                if (pipline != null)
                {
                    returnValue.Add(pipline);
                }
            }
            registedPipelines.Add(typeof(TItemType).FullName, returnValue as object);
            return returnValue;
        }
    }
}
