using System;
using System.Collections.Generic;
using System.Text;
using NScrapy.Engine;
using NScrapy.Infra;
using Microsoft.Extensions.DependencyInjection;

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
            serviceCollection.AddSingleton<IEngine, MockEngine>();
            this._provider = serviceCollection.BuildServiceProvider();
        }

        public IResponse Crawl(string spiderName)
        {
            var spider = Spider.SpiderFactory.GetSpider(spiderName);
            Scheduler.Scheduler.SendRequestToReceiver(null);
            return null;
        }
    }
}
