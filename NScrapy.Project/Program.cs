using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NScrapy.Project
{
    class Program
    {
        static void Main(string[] args)
        {
            var shell = NScrapy.Shell.NScrapy.GetInstance();
            shell.Crawl("JobSpider");
            while(true)
            {

            }
        }
    }
    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?industries=&dqs=010&salary=&jobKind=2&pubTime=&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=08e25e12d56ae93f&d_headId=e4344cdfda34e93a3c98f13b3949c261&d_ckId=e367780a4342aa7b9e9b6de574e6182b&d_sfrom=search_fp&d_curPage=0&d_pageSize=40&siTag=nFvzsMNPVoNpz19ogQr-uA~hgRZ0h1p55DegpQkaoxBXQ&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91+%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+%E6%89%A7%E8%A1%8C%E6%80%BB%E8%A3%81", "https://www.liepin.com/zhaopin/?industries=&dqs=020&salary=&jobKind=2&pubTime=&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=63b8295d2f5cc5fa&d_headId=e4344cdfda34e93a3c98f13b3949c261&d_ckId=8adaa7b7cbf46a906a60b66ce4bcf050&d_sfrom=search_fp&d_curPage=0&d_pageSize=40&siTag=nFvzsMNPVoNpz19ogQr-uA~5_NiSw97HP2fC0L42YSK3Q&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91+%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+%E6%89%A7%E8%A1%8C%E6%80%BB%E8%A3%81")]
    public class JobSpider : Spider.Spider
    {
        List<string> FirmSelector = new List<string>();
        List<string> SalarySelector = new List<string>();
        List<string> TitleSelector = new List<string>();
        List<string> TimeSelector = new List<string>();
        List<string> LocationSelector = new List<string>();
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");
        private Regex salaryReg = new Regex(@"(\d+)-(\d+)万");
        public JobSpider()
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

            LocationSelector.Add(".basic-infor span  a::attr(text)");
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
            //var blocks = returnValue.CssSelector(".sojob-list li");
            //foreach(var block in blocks)
            //{
            //    //var type=
            //}
            var hrefs = returnValue.XPathSelector("/html/body/div/div/div/div/ul/li[i/b=\"企\"]/div/div/h3/a@href").Extract();
            //var hrefs = returnValue.CssSelector(".job-info h3 a::attr(href)").Extract();
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
            var location = response.CssSelector(LocationSelector).ExtractFirst();
            if (title == null ||
                firm == null ||
                salary == null||
                time==null)
            {
                NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {response.URL}");
                return;
            }
            var salaryFrom = string.Empty;
            var salaryTo = string.Empty;
            var match = salaryReg.Match(salary);
            if(match.Groups!=null &&match.Groups.Count>0)
            {
                salaryFrom = match.Groups[1].Value;
                salaryTo = match.Groups[2].Value;
            }
            if(firm.Contains("某")||firm.Contains("知名"))
            {
                return;
            }
            var info = $"{title},{firm.Replace(System.Environment.NewLine, string.Empty).Trim()},{salaryFrom.Trim()},{salaryTo.Trim()},{time},{location},{response.URL}";
            Console.WriteLine(info);
            File.AppendAllText($"output-{this.startingTime}.csv",info+System.Environment.NewLine,Encoding.UTF8);
            
        }

        //public void ParseItem(IResponse response)
        //{
        //    var itemLoader = new ItemLoader<JobItem>(response);
        //    itemLoader.AddFieldMapping("Title", "css:.title-info h1::attr(text)");
        //    itemLoader.AddFieldMapping("Title", "css:.job-title h1::attr(text)");

        //    itemLoader.AddFieldMapping("Firm", "css:.title-info h3 a::attr(text)");
        //    itemLoader.AddFieldMapping("Firm", "css:.title-info h3::attr(text)");
        //    itemLoader.AddFieldMapping("Firm", "css:.title-info h3");
        //    itemLoader.AddFieldMapping("Firm", "css:.job-title h2::attr(text)");

        //    itemLoader.AddFieldMapping("Salary", "css:.job-main-title p::attr(text)");
        //    itemLoader.AddFieldMapping("Salary", "css:.job-main-title strong::attr(text)");
        //    itemLoader.AddFieldMapping("Salary", "css:.job-item-title p::attr(text)");
        //    itemLoader.AddFieldMapping("Salary", "css:.job-item-title");

        //    itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(title)");
        //    itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(text)");
        //    var item = itemLoader.LoadItem();
        //    Console.WriteLine(item.Firm);
        //}

    }

    public class JobItem
    {
        public string Firm { get; set; }
        public string Title { get; set; }
        public string Salary { get; set; }
        public string Time { get; set; }
    }
}
