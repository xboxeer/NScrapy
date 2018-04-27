using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NScrapy.Engine;
using NScrapy.Infra;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace NScrapy.Shell
{
    public class NScrapy
    {
        private NScrapyContext _context = null;
        private ServiceProvider _provider = null;
        private static NScrapy _instance = null;
        private Regex urlHostReg = new Regex(@"https?://[/s/S]*[^/]*/");
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
            //This is a temp solution, later on we will implement IOC in this place so we can chose other approach to filter the url
            this._context.UrlFilter = new InMemoryUrlFilter();
            Scheduler.RequestReceiver.StartReceiver();
            Scheduler.ResponseDistributer.StartDistribuiter();
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

        public void Crawl(string spiderName)
        {
            NScrapyContext.CurrentContext.Log.Info($"Start Crawling with spider {spiderName}");
            var spider = Spider.SpiderFactory.GetSpider(spiderName);
            NScrapyContext.CurrentContext.CurrentSpider = spider;
            spider.StartRequests();
            while(true)
            {
            }
        }

        public void Request(string url,Action<IResponse> responseHandler)
        {
            NScrapyContext.CurrentContext.Log.Info($"Sending Request to {url}");
            var request = new HttpRequest()
            {
                URL = url,
                Callback = responseHandler,
                RequestSpider= NScrapyContext.CurrentContext.CurrentSpider
            };
            Scheduler.Scheduler.SendRequestToReceiver(request);
        }

        public void Follow(IResponse sourceResponse, string url,Action<IResponse> responseHandler)
        {
            //Replace uri.Schema and uri.host incase the url already have those inforamtion
            url = urlHostReg.Replace(url, "");
            //if the url comes in like /a/1234, then nothing changes
            //if the url comes in like http://www.baidu.com/a/1234, then it becomes a/1234
            //so basiclly the url should becomes like /a/1234
            if (url.Length > 0 && !url.StartsWith("/"))
            {
                url = "/" + url;
            }
            var uri = new Uri(sourceResponse.Request.URL);
            var request = new HttpRequest()
            {
                URL =$"{uri.Scheme}://{uri.Host}{url}",
                Callback = responseHandler,
                RequestSpider = NScrapyContext.CurrentContext.CurrentSpider
            };
            Scheduler.Scheduler.SendRequestToReceiver(request);
        }
    }
}
