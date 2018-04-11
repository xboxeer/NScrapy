using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NScrapy.Downloader
{
    public class Downloader
    {
        private static object lockObj = new object();
        private static List<Downloader> downloaderPool = new List<Downloader>();
        public static int DownloaderCount { get; private set; }
        public static int DownloaderPoolCapbility { get; set; }

        public DownloaderStatus Status { get; private set; }
        public static List<Downloader> DownloaderPool { get => downloaderPool; }

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
        }

        private Downloader()
        {
            httpClient = new HttpClient();
            var middlewares = NScrapyContext.CurrentContext.Configuration.GetSection("AppSettings:DownloaderMiddlewares").GetChildren();
            foreach(var middleware in middlewares)
            {
                var path =$"{middleware.Path}:Middleware";
                var middlewareName = NScrapyContext.CurrentContext.Configuration[path];
                //Init middlewareName here
            }            
        }

        public async Task<IResponse> DownloadPageAsync(IRequest request)
        {
            this.Status = DownloaderStatus.Running;
            HttpResponseMessage responseMessage = null;
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
                    ResponseMessage = responseMessage,
                    URL = request.URL
                };
            return response;
        }

        public static async Task<IResponse> SendRequestAsync(IRequest request)
        {            
            var downloader = GetDownloader();
            return await downloader.DownloadPageAsync(request);
            //Put request into internal queue            
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
                        Thread.Sleep(1000);
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
