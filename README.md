# NScrapy is a Distributed Spider Framework based on .net core and Redis. the idea of NScrapy comes from Scrapy, so you can write the spider in a very similar way to Scrapy 
# NScrapy 是基于.net core 异步编程框架,Redis内存存储的一款开源分布式爬虫框架, NScrapy的整体思想源于知名的python爬虫框架Scrapy,整体上的写法也接近于Scrapy
# NScrapy Sample code
Below is a sample of NScrapy, the sample will visit Liepin, which is a Recruit web site
Based on the seed URL defined in the [URL] attribute, NScrapy will visit each Postion information in detail page(the ParseItem method) , and visit the next page automatically(the VisitPage method)
如下是一段简单的NScrapy爬虫，该爬虫会抓取猎聘网上所有php的职位信息并做相应的输出
基于定义在[URL] attribute 中的种子URL，NScrapy会访问每一个职位信息的详细信息页面(ParseItem method)， 并且自动爬取下一页信息(VisitPage method)
爬虫作者不需要关心如何管理分布式爬虫之间如何互相通信，下载器如何获取待下载队列，下载器池是如何维护的，仅仅需要告诉NScrapy一个种子链接， 集成Spider.Spider类，并完成默认回调函数就可以爬去信息
NScrapy支持丰富的自定义扩展，包括在配置文件appsetting.json中加入DownloaderMiddware,配置Http请求头，构造User Agent pool等
Usage:

    using NScrapy.Infra;
    using NScrapy.Infra.Attributes.SpiderAttributes;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    namespace NScrapy.Project
    {
        class Program
        {
        static void Main(string[] args)
        {
            //Init shell of NScrapy, which will init the context of NScrapy
            var shell = NScrapy.Shell.NScrapy.GetInstance(); 
            //Specify the Spider that you want to start
            shell.Crawl("JobSpider");
            return;
        }
    }
    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?industries=&dqs=&salary=&jobKind=&pubTime=30&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=bb314f611fde073c&d_headId=4b294eff4ad202db83d4ed085fcbf94b&d_ckId=01fb643c53d14dd44d7991e27c98c51b&d_sfrom=search_prime&d_curPage=0&d_pageSize=40&siTag=k_cloHQj_hyIn0SLM9IfRg~UoKQA1_uiNxxEb8RglVcHg&key=php")]
    public class JobSpider : Spider.Spider
    {
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");
        public JobSpider()
        {
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
                    NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue,page, VisitPage);
                }
            }
            VisitPage(returnValue);
        }

        private void VisitPage(IResponse returnValue)
        {
            var hrefs = returnValue.CssSelector(".job-info h3 a::attr(href)").Extract();
            foreach (var href in hrefs)
            {
                //Use ItemLoader
                NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, href, ParseItem);
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

        public void ParseItem(IResponse response)
        {
            //Add Field Mapping to the HTML Dom element
            var itemLoader = new ItemLoader<JobItem>(response);
            itemLoader.AddFieldMapping("Title", "css:.title-info h1::attr(text)");
            itemLoader.AddFieldMapping("Title","css:.job-title h1::attr(text)");

            itemLoader.AddFieldMapping("Firm","css:.title-info h3 a::attr(text)");
            itemLoader.AddFieldMapping("Firm", "css:.title-info h3::attr(text)");
            itemLoader.AddFieldMapping("Firm","css:.title-info h3");
            itemLoader.AddFieldMapping("Firm","css:.job-title h2::attr(text)");

            itemLoader.AddFieldMapping("Salary", "css:.job-main-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-main-title strong::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title");

            itemLoader.AddFieldMapping("Time","css:.job-title-left time::attr(title)");
            itemLoader.AddFieldMapping("Time","css:.job-title-left time::attr(text)");
            var item = itemLoader.LoadItem();
            //#In the example here, simple write the Position Firm information at the console, you can write the information to anywhere else
            Console.WriteLine(item.Firm);
        }
        
    }

    public class JobItem
    {
        public string Firm { get; set; }
        public string Title { get; set; }
        public string Salary { get; set; }
        public string Time { get; set; }
    }
    }
