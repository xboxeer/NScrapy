using Microsoft.VisualStudio.TestTools.UnitTesting;
using NScrapy.Infra;
using NScrapy.Shell;
using System.Diagnostics;

namespace NScrapy.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void EngineTest()
        {
            NScrapy.Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            HttpRequest request = new HttpRequest()
            {
                URL = "http://www.baidu.com"
            };
            scrapy.Context.CurrentEngine.ProcessRequest(request);
        }

        [TestMethod]
        public void ConstructDownloader()
        {
            var sw=Stopwatch.StartNew();
            sw.Start();
            //init context
            NScrapy.Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            var request = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var request2 = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var request3 = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var request4 = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var request5 = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var response = Downloader.Downloader.SendRequestAsync(request);
            var response2 = Downloader.Downloader.SendRequestAsync(request2);
            var response3 = Downloader.Downloader.SendRequestAsync(request3);
            var response4 = Downloader.Downloader.SendRequestAsync(request4);
            var response5 = Downloader.Downloader.SendRequestAsync(request5);
            var result = response5.Result;
            sw.Stop();
            var timeCost = sw.ElapsedMilliseconds;
            return;
        }
    }
}
