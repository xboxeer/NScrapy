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
            var shell = NScrapy.Shell.NScrapy.GetInstance();           
            shell.Crawl("JobSpider");
            return;
        }
    }
    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?industries=&dqs=&salary=&jobKind=&pubTime=30&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=bb314f611fde073c&d_headId=4b294eff4ad202db83d4ed085fcbf94b&d_ckId=01fb643c53d14dd44d7991e27c98c51b&d_sfrom=search_prime&d_curPage=0&d_pageSize=40&siTag=k_cloHQj_hyIn0SLM9IfRg~UoKQA1_uiNxxEb8RglVcHg&key=php")]
    public class JobSpider : Spider.Spider
    {
        List<string> FirmSelector = new List<string>();
        List<string> SalarySelector = new List<string>();
        List<string> TitleSelector = new List<string>();
        List<string> TimeSelector = new List<string>();
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");
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
                //NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, href, Parse);
                
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

        public void Parse(IResponse response)
        {            
            var title = response.CssSelector(TitleSelector).ExtractFirst();    
            
            var firm = response.CssSelector(FirmSelector).ExtractFirst();
            var salary = response.CssSelector(SalarySelector).ExtractFirst();
            var time = response.CssSelector(TimeSelector).ExtractFirst();
            if (title == null ||
                firm == null ||
                salary == null)
            {
                NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {response.URL}");
                return;
            }
            var info = $"{title},{firm.Replace(System.Environment.NewLine, string.Empty).Trim()},{salary.Replace(System.Environment.NewLine, string.Empty).Trim()},{time}";
            Console.WriteLine(info);
            File.AppendAllText($"output-{this.startingTime}.csv",info+System.Environment.NewLine,Encoding.UTF8);
            
        }

        public void ParseItem(IResponse response)
        {
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
