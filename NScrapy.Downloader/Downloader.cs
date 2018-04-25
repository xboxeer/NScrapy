using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScrapy.Downloader.Middleware;
using System.Reflection;

namespace NScrapy.Downloader
{
    public class Downloader
    {
        private static object lockObj = new object();
        private static List<Downloader> downloaderPool = new List<Downloader>();
        private static Assembly downloaderAssembly = null;
        private static Assembly appAssembly = null;

        public static int DownloaderCount { get; private set; }
        public static int DownloaderPoolCapbility { get; set; }
        public static List<Downloader> DownloaderPool { get => downloaderPool; }

        public DownloaderStatus Status { get; private set; }
        public List<IDownloaderMiddleware> Middlewares { get; private set; }
        private HttpClient httpClient = null;
        
        static Downloader()
        {
            var capbility = NScrapyContext.CurrentContext.Configuration["AppSettings:DownloaderPoolCapbility"];
            //Init a simple Downloader pool, right now does not support dynamicly increase pool size, 
            //default to 4 Downloader if DownloaderPoolCapbility is not setting
            if (string.IsNullOrEmpty(capbility))
            {
                DownloaderPoolCapbility = 4;
            }
            else
            {
                DownloaderPoolCapbility = Convert.ToInt32(capbility);
            }
            for(int i=0;i< DownloaderPoolCapbility;i++)
            {
                var downloader = new Downloader()
                {
                    Status = DownloaderStatus.Idle
                };
                DownloaderPool.Add(downloader);
            }
            downloaderAssembly = Assembly.GetExecutingAssembly();
            appAssembly = Assembly.GetEntryAssembly();
        }

        private Downloader()
        {
            if (downloaderAssembly == null)
            {
                downloaderAssembly = Assembly.GetExecutingAssembly();
            }
            if(appAssembly==null)
            {
                appAssembly = Assembly.GetEntryAssembly();
            }
            httpClient = new HttpClient();
            var middlewareNames = NScrapyContext.CurrentContext.Configuration.GetSection("AppSettings:DownloaderMiddlewares").GetChildren();
            Middlewares = new List<IDownloaderMiddleware>
            {
                new HttpHeaderMiddleware(),
                new HttpDecompressionMiddleware()
            };
            //Add Additional Middleware, Remove additional/default Middleware
            foreach (var middlewareNamePath in middlewareNames)
            {
                var path =$"{middlewareNamePath.Path}:Middleware";
                var middlewareName = NScrapyContext.CurrentContext.Configuration[path];
                if(string.IsNullOrEmpty(middlewareName))
                {
                    continue;
                }
                var middlewareType = downloaderAssembly.GetType(middlewareName);
                if(middlewareType==null)
                {
                    middlewareType = appAssembly.GetType(middlewareName);
                }
                if(middlewareType==null)
                {
                    throw new ArgumentNullException($"NScrapy can not find DownloaderMiddleware {middlewareName}");
                }
                var middleware = Activator.CreateInstance(middlewareType) as IDownloaderMiddleware;
                Middlewares.Add(middleware);
                //Init middlewareName here
                //TODO:Remove Middleware from Middleware list by searching by RemovedMiddleware
            }

        }

        public async Task<IResponse> DownloadPageAsync(IRequest request)
        {
            this.Status = DownloaderStatus.Running;
            request.Client = this.httpClient;
            HttpResponseMessage responseMessage = null;            
            foreach(var middleware in this.Middlewares)
            {
                middleware.PreDownload(request);
            }
            try
            {
                 responseMessage = await this.httpClient.GetAsync(request.URL);
            }
            finally
            {
                this.Status = DownloaderStatus.Idle;
            }
            var response = new HttpResponse()
            {
                Request = request,
                RawResponseMessage = responseMessage,
                URL = request.URL
            };
            foreach (var middleware in this.Middlewares)
            {
                middleware.PostDownload(response);
            }
            return response;
        }

        public static async Task<IResponse> SendRequestAsync(IRequest request)
        {            
            var downloader = GetDownloader();
            return await downloader.DownloadPageAsync(request);
        }
        /// <summary>
        /// This method is thread safe to make sure no 2 requests will reference one downloader
        /// </summary>
        /// <returns></returns>
        private static Downloader GetDownloader()
        {
            Downloader downloader = null;
            lock (lockObj)
            {
                while (downloader==null)
                {
                    downloader = Downloader.DownloaderPool.Where(d => d.Status == DownloaderStatus.Idle).FirstOrDefault();
                    if(downloader==null)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            return downloader;
        }
    }
    public enum DownloaderStatus
    {
        Running,
        Idle
    }
}
