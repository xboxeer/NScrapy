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
using System.IO;

namespace NScrapy.Shell
{
    public class NScrapy
    {
        private NScrapyContext _context = null;
        private ServiceProvider _provider = null;
        private static NScrapy _instance = null;
        private Regex urlHostReg = new Regex(@"https?://[/s/S]*[^/]*/");
        private object lockObj = new object();
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
            this._context.ConfigRefreshed += _context_ConfigRefreshed;
            SetServices();
            
        }

        private void SetServices()
        {
            this._context.CurrentEngine = this._provider.GetService<IEngine>();
            this._context.CurrentScheduler = this._provider.GetService<IScheduler>();
            //This is a temp solution, later on we will implement IOC in this place so we can chose other approach to filter the url
            this._context.UrlFilter = new InMemoryUrlFilter();
        }

        private void _context_ConfigRefreshed(object arg1, EventArgs arg2)
        {
            ConfigSerivces();
            SetServices();
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
            serviceCollection.AddSingleton(typeof(IScheduler), GetSchedulerType());
            this._provider = serviceCollection.BuildServiceProvider();
        }

        private static Type GetSchedulerType()
        {
            var schedulerName = NScrapyContext.GetInstance().CurrentConfig["AppSettings:Scheduler:SchedulerType"];
            Type schedulerType = null;
            if(string.IsNullOrEmpty(schedulerName))
            {
                return typeof(Scheduler.InMemoryScheduler);
            }
            else
            {

                Assembly schedulerAssembly = null;
                var assemblyName = NScrapyContext.GetInstance().CurrentConfig["AppSettings:Scheduler:SchedulerTypeAssembly"];
                if(string.IsNullOrEmpty(assemblyName))
                {
                    schedulerAssembly = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), "NScrapy.Scheduler.dll"));
                }
                else
                {
                    schedulerAssembly = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), assemblyName));
                }
                schedulerType = schedulerAssembly.GetType(schedulerName);
                if (!schedulerType.GetInterfaces().Contains(typeof(IScheduler)))
                {
                    throw new TypeLoadException($"{schedulerName} must implement NScrapy.Infra.IScheduler interface");
                }
            }
            return schedulerType;
        }

        private static Type GetEngineType()
        {
            var engineName = NScrapyContext.GetInstance().CurrentConfig["AppSettings:SpiderEngine:SpiderEngineName"];
            Type engineType = null;
            if (string.IsNullOrEmpty(engineName))
            {
                return typeof(NScrapyEngine);
            }
            else
            {
                Assembly engineAssembly = null;
                var engineAssemblyName = NScrapyContext.GetInstance().CurrentConfig["AppSettings:SpiderEngine:SpiderEngineAssembly"];
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
                    throw new TypeLoadException($"{engineName} must implement NScrapy.Infra.IEngin interface");
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
            Task.Run(() => AnymoreItemsInQueueAndDownloader());
            lock(lockObj)
            {
                Monitor.Wait(lockObj);
            }
        }

        private void AnymoreItemsInQueueAndDownloader()
        {
            while (true)
            {
                if (NScrapyContext.CurrentContext.CurrentScheduler.GetType() == typeof(Scheduler.InMemoryScheduler))
                {
                    var noMoreItemInQueue = Scheduler.RequestReceiver.RequestQueue.Count == 0 &&
                                        Scheduler.ResponseDistributer.ResponseQueue.Count == 0 &&
                                        Downloader.Downloader.RunningDownloader == 0;
                    Thread.Sleep(10000);
                    if (noMoreItemInQueue)
                    {                        
                        lock (lockObj)
                        {
                            Monitor.Pulse(lockObj);
                        }
                    }
                }
            }
        }

        public void Request(string url,Action<IResponse> responseHandler=null,string cookies=null, Dictionary<string,string> formData=null)
        {
            NScrapyContext.CurrentContext.Log.Info($"Sending Request to {url}");
            var request = new HttpRequest()
            {
                URL = url,
                Callback = responseHandler ?? NScrapyContext.CurrentContext.CurrentSpider.ResponseHandler,
                RequestSpider = NScrapyContext.CurrentContext.CurrentSpider,
                FormData = formData,
                Cookies = cookies
            };
            NScrapyContext.CurrentContext.CurrentScheduler.SendRequestToReceiver(request);
        }

        public void Follow(IResponse sourceResponse, string url,Action<IResponse> responseHandler=null,string cookies=null, Dictionary<string,string> formData=null)
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
            var uri = new Uri(sourceResponse.URL);
            var request = new HttpRequest()
            {
                URL =$"{uri.Scheme}://{uri.Host}{url}",
                Callback = responseHandler ?? NScrapyContext.CurrentContext.CurrentSpider.ResponseHandler,
                RequestSpider = NScrapyContext.CurrentContext.CurrentSpider,
                FormData = formData,
                Cookies=cookies
            };
            NScrapyContext.CurrentContext.CurrentScheduler.SendRequestToReceiver(request);
        }
    }
}
