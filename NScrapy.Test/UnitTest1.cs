using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;
using NScrapy.Infra.Pipeline;
using NScrapy.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
            var result = response5.Result.ResponsePlanText;
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
            Assert.IsTrue(response.Result.ResponsePlanText != null);
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
            string logPath = DeleteLog();
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            scrapy.Crawl("UrlFilterTestSpider");
            //Make a copy of log file since the original log file is still in using by log4net
            string logFileContent = GetLogContent(logPath);
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
            Thread t = new Thread(() => DownloaderShell.Program.Main(null));
            t.Start();
            Shell.NScrapy.GetInstance().Crawl("JobSpider2");
            //Let's first start the individual Downloader by a thread
        }

        [TestMethod]
        public void PipelineTest()
        {
            this.DeleteLog();
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();            
            Shell.NScrapy.GetInstance().Crawl("PipelineTestSpider");
            var logContent = this.GetLogContent(Path.Combine(Directory.GetCurrentDirectory(), "log-file.txt"));
            Assert.IsTrue(logContent.Contains("Mock Pipeline Processed, Mock Value=Hello World"));
            Assert.IsTrue(logContent.Contains("MockValue Mapped!"));
        }

        [TestMethod]
        public void PipelineTestItemLoaderValueSetTest()
        {
            this.DeleteLog();
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            Shell.NScrapy.GetInstance().Crawl("PipelineTestSpider");
            var logContent = this.GetLogContent(Path.Combine(Directory.GetCurrentDirectory(), "log-file.txt"));
            Assert.IsTrue(logContent.Contains("MockValue Mapped!"));
        }

        [TestMethod]
        public void PipelineTestItemLoaderValueSetTest2()
        {
            this.DeleteLog();
            Shell.NScrapy scrapy = NScrapy.Shell.NScrapy.GetInstance();
            Shell.NScrapy.GetInstance().Crawl("PipelineTestSpider");
            var logContent = this.GetLogContent(Path.Combine(Directory.GetCurrentDirectory(), "log-file.txt"));
            Assert.IsTrue(logContent.Contains("MockValue2 Mapped!"));
        }

        [TestMethod]
        public void DownloaderContextConfigProviderTest()
        {
            var context = Downloader.DownloaderContext.CurrentContext;
            context.ConfigProvider = new MockConfigProvider();
            context.RunningMode = Downloader.DownloaderRunningMode.Distributed;
            Assert.AreEqual("192.168.0.103:2181", context.CurrentConfig["AppSettings:ZookeeperEndpoint"]);
        }

        [TestMethod]
        public void NScrapyContextConfigProviderTest()
        {           
            var context = NScrapyContext.GetInstance();
            context.ConfigProvider = new MockConfigProvider();
            Assert.AreEqual("192.168.0.103:2181", context.CurrentConfig["AppSettings:ZookeeperEndpoint"]);
        }

        private string GetLogContent(string logPath)
        {
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
            return logFileContent;
        }

        private string DeleteLog()
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "log-file.txt");
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }

            return logPath;
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


    [Name(Name = "JobSpider2")]
    [URL("https://www.liepin.com/zhaopin/?industries=&dqs=&salary=&jobKind=&pubTime=&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=0af8c9495882e6a7&d_headId=285a1c0df0556fc28874c7d7df42cf55&d_ckId=285a1c0df0556fc28874c7d7df42cf55&d_sfrom=search_fp&d_curPage=0&d_pageSize=40&siTag=9vh8n9z4s8Pwf5Px7ocSyQ~fA9rXquZc5IkJpXC-Ycixw&key=php")]
    public class JobSpider2 : Spider.Spider
    {
        List<string> FirmSelector = new List<string>();
        List<string> SalarySelector = new List<string>();
        List<string> TitleSelector = new List<string>();
        List<string> TimeSelector = new List<string>();
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");
        private Regex salaryReg = new Regex(@"(\d+)-(\d+)Íò");
        public JobSpider2()
        {
            // FirmSelector.Add(".title-info h1::attr(text)");
            TitleSelector.Add(".title-info h1::attr(text)");
            TitleSelector.Add(".job-title h1::attr(text)");

            FirmSelector.Add(".title-info h3 a::attr(text)");
            FirmSelector.Add(".title-info h3::attr(text)");
            FirmSelector.Add(".title-info h3");
            FirmSelector.Add(".job-title h2::attr(text)");

            SalarySelector.Add(".job-main-title p::attr(text)");
            SalarySelector.Add(".job-main-title strong::attr(text)");
            SalarySelector.Add(".job-item-title p::attr(text)");
            SalarySelector.Add(".job-item-title");

            TimeSelector.Add(".job-title-left time::attr(title)");
            TimeSelector.Add(".job-title-left time::attr(text)");
            if (File.Exists("output.csv"))
            {
                File.Delete("output.csv");
            }
        }

        public override void ResponseHandler(IResponse response)
        {
            var httpResponse = response as HttpResponse;
            var returnValue = response.CssSelector(".job-info h3 a::attr(href)");
            var pages = response.CssSelector(".pagerbar a::attr(href)").Extract();
            foreach (var page in pages)
            {
                if (!page.Contains("javascript"))
                {
                    NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, page, VisitPage);
                }
            }
            VisitPage(returnValue);
        }

        private void VisitPage(IResponse returnValue)
        {
            var hrefs = returnValue.CssSelector(".job-info h3 a::attr(href)").Extract();
            foreach (var href in hrefs)
            {
                NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, href, Parse);
            }
            var pages = returnValue.CssSelector(".pagerbar a::attr(href)").Extract();
            foreach (var page in pages)
            {
                if (!page.Contains("javascript"))
                {
                    NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, page, VisitPage);
                }
            }
        }

        public void Parse(IResponse response)
        {
            var title = response.CssSelector(TitleSelector).ExtractFirst();

            var firm = response.CssSelector(FirmSelector).ExtractFirst();
            var salary = response.CssSelector(SalarySelector).ExtractFirst();
            var time = response.CssSelector(TimeSelector).ExtractFirst();
            if (title == null ||
                firm == null ||
                salary == null ||
                time == null)
            {
                NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {response.URL}");
                return;
            }
            var salaryFrom = string.Empty;
            var salaryTo = string.Empty;
            var match = salaryReg.Match(salary);
            if (match.Groups != null && match.Groups.Count > 0)
            {
                salaryFrom = match.Groups[1].Value;
                salaryTo = match.Groups[2].Value;
            }
            var info = $"{title},{firm.Replace(System.Environment.NewLine, string.Empty).Trim()},{salaryFrom.Trim()},{salaryTo.Trim()},{time}";
            Console.WriteLine(info);
            File.AppendAllText($"output-{this.startingTime}.csv", info + System.Environment.NewLine, Encoding.UTF8);

        }

    }

    [Name(Name = "PipelineTestSpider")]
    [URL("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1")]
    public class PipelineTestSpider : Spider.Spider
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
            //response.CssSelector(".job-info h3 a");
            var MockItemLoader = ItemLoaderFactory.GetItemLoader<MockItem>(response);
            MockItemLoader.BeforeValueSetting += MockItemLoader_BeforeValueSetting;
            MockItemLoader.AddFieldMapping("MockValue", "MockValue Mapped!");
            MockItemLoader.AddFieldMapping(u=>u.MockValue2,"MockValue2 Mapped!");
            MockItemLoader.LoadItem();
        }

        private void MockItemLoader_BeforeValueSetting(object arg1, ValueSettingEventArgs<MockItem> arg2)
        {
            NScrapyContext.CurrentContext.Log.Info(arg2.Value);
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
