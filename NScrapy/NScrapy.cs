using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NScrapy.Engine;
using NScrapy.Infra;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NScrapy.Shell
{
    public class NScrapy
    {
        private NScrapyContext _context = null;
        private ServiceProvider _provider = null;
        private static NScrapy _instance = null;
        public NScrapyContext Context
        {
            get
            {
                return this._context;
            }
        }

        private NScrapy()
        {
            ConfigSerivces();
            this._context = NScrapyContext.GetInstance();
            this._context.CurrentEngine = this._provider.GetService<IEngine>();
            Scheduler.RequestReceiver.StartReceiver();
        }

        public static NScrapy GetInstance()
        {
            if(_instance==null)
            {
                _instance = new NScrapy();
            }
            return _instance;
        }

        public void ConfigSerivces()
        {
            var serviceCollection = new ServiceCollection();
            Type engineType = GetEngineType();
            serviceCollection.AddSingleton(typeof(IEngine), engineType);
            this._provider = serviceCollection.BuildServiceProvider();
        }

        private static Type GetEngineType()
        {
            var engineName = NScrapyContext.GetInstance().Configuration["AppSettings:SpiderEngine:SpiderEngineName"];
            Type engineType = null;
            if (string.IsNullOrEmpty(engineName))
            {
                return typeof(NScrapyEngine);
            }
            else
            {
                Assembly engineAssembly = null;
                var engineAssemblyName = NScrapyContext.GetInstance().Configuration["AppSettings:SpiderEngine:SpiderEngineAssembly"];
                if (engineAssemblyName != null)
                {
                    engineAssembly = Assembly.LoadFrom(engineAssemblyName);
                }
                else
                {
                    engineAssembly = Assembly.GetEntryAssembly();
                }
                engineType = engineAssembly.GetType(engineName);
                if (!engineType.GetInterfaces().Contains(typeof(IEngine)))
                {
                    throw new InvalidCastException($"{engineName} must implement NScrapy.Infra.IEngin interface");
                }
            }

            return engineType;
        }

        public IResponse Crawl(string spiderName)
        {
            var spider = Spider.SpiderFactory.GetSpider(spiderName);
            foreach(var url in spider.URLs)
            {
                HttpRequest request = new HttpRequest()
                {
                    URL = url
                };
                Scheduler.Scheduler.SendRequestToReceiver(request);
            }
            return null;
        }
    }
}
