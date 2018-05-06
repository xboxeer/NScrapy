using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;
using NScrapy.Shell;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

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
            var sw = Stopwatch.StartNew();
            sw.Start();
            //init context
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
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
            var result = response5.Result.ReponsePlanText;
            //var html= Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(result.ResponseMessage.Content.ReadAsStringAsync().Result));
            sw.Stop();
            var timeCost = sw.ElapsedMilliseconds;
            return;
        }

        [TestMethod]
        public void SpiderTest()
        {
            Shell.NScrapy.GetInstance().Crawl("JobSpider");
        }

        [TestMethod]
        public void UserAgentMiddlewareTest()
        {
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            NScrapyContext.CurrentContext.RefreshConfigFile("appsettingUserAgent.json");
            var request = new HttpRequest()
            {
                URL = "http://www.sina.com"
            };
            var response = Downloader.Downloader.SendRequestAsync(request);
            Assert.IsTrue(response.Result.ReponsePlanText != null);
        }

        [TestMethod]
        public void ImageSpiderTest()
        {

        }

        [TestMethod]
        public void UrlFilterTest()
        {
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            scrapy.Crawl("UrlFilterTestSpider");
        }
        //this test is for the purpose that NScrapy can safely exit while it found out that there is no more items in Response/Request queue and no more Running Downloader
        //The idea is that if Crawl would not exit if the checking of Response/Request/RunningDownloader failed, as long as it exit properly, log file would logs the url that 
        //the spider has crawled, in this case it is https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1s, so a check of this string in log file would tell if the checking of Response/Request/RunningDownloader works
        [TestMethod]
        public void NScrapySuccessfullyExitTest()
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "log-file.txt");
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();            
            scrapy.Crawl("UrlFilterTestSpider");
            //Make a copy of log file since the original log file is still in using by log4net
            var copiedLogFile = Path.Combine(Directory.GetCurrentDirectory(), "log-file.test.txt");
            File.Copy(logPath, copiedLogFile);
            var logFileContent = string.Empty;
            using (FileStream stream = File.OpenRead(copiedLogFile))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    logFileContent = Encoding.UTF8.GetString(ms.ToArray());
                }                
            }
            File.Delete(copiedLogFile);
            Assert.IsTrue(logFileContent.Contains("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1"));
        }

        //This test case is actually not fully implemented, right now i just directly check the redis by using redis-cli to verify if the message has been published to topic 
        [TestMethod]        
        public void RedisSchedulerTest()
        {
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            NScrapyContext.CurrentContext.RefreshConfigFile("appsettingRedis.json");
            scrapy.ConfigSerivces();
            Shell.NScrapy.GetInstance().Crawl("JobSpider");
        }

        [TestMethod]
        public void DistributedDownloaderTest()
        {
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            NScrapyContext.CurrentContext.RefreshConfigFile("appsettingRedis.json");
            Shell.NScrapy.GetInstance().Crawl("JobSpider");
            //Let's first start the individual Downloader by a thread
            Thread t = new Thread(() => DownloaderShell.Program.Main(null));
            t.Start();
            while(true)
            {

            }
        }

    }

    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1")]
    public class JobSpider : Spider.Spider
    {
        public override void ResponseHandler(IResponse response)
        {
            var httpResponse = response as HttpResponse;
            var returnValue = response.CssSelector(".job-info h3 a::attr(href)") as HttpResponse;
            var hrefs = returnValue.Extract();
            foreach (var href in hrefs)
            {
                NScrapy.Shell.NScrapy.GetInstance().Request(href, Parse);
            }
            //var parser = new HtmlParser();
            //var result=parser.Parse(response.ReponsePlanText);
        }

        public void Parse(IResponse response)
        {
            response.CssSelector(".job-info h3 a");
        }
    }

    [Name(Name = "UrlFilterTestSpider")]
    [URL("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1")]
    public class ImageSpider : Spider.Spider
    {
        public override void ResponseHandler(IResponse response)
        {
            NScrapy.Shell.NScrapy.GetInstance().Request(response.URL,null);
        }
    }
}
