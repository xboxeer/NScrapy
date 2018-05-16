using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NScrapy.Infra
{
    public class ItemLoaderFactory
    {
        private static Dictionary<Type, object> registedItemLoader = new Dictionary<Type, object>();

        public static ItemLoader<TItemType> GetItemLoader<TItemType>(IResponse response)
            where TItemType : class, new()
        {
            ItemLoader<TItemType> returnValue = null;
            if (!registedItemLoader.ContainsKey(typeof(TItemType)))
            {
                returnValue = new ItemLoader<TItemType>(response);
                registedItemLoader.Add(typeof(TItemType), returnValue);
            }
            else
            {
                returnValue = registedItemLoader[typeof(TItemType)] as ItemLoader<TItemType>;
            }
            List<IPipeline<TItemType>> itemPipelines = GetPipelines<TItemType>();
            returnValue.pipelines = itemPipelines;
            returnValue.ClearEvent();
            return returnValue;
        }

        private static List<IPipeline<TItemType>> GetPipelines<TItemType>() where TItemType : class, new()
        {
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
            return returnValue;
        }
    }
}
