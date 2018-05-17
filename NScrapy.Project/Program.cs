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
    [URL("https://www.liepin.com/zhaopin/?sfrom=click-pc_homepage-centre_searchbox-search_new&d_sfrom=search_fp&key=.net")]
    public class JobSpider : Spider.Spider
    {
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");
        private Regex salaryReg = new Regex(@"(\d+)-(\d+)万");
        public JobSpider()
        {
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
            var hrefs = returnValue.XPathSelector("/html/body/div/div/div/div/ul/li[i/b=\"企\"]/div/div/h3/a@href").Extract();
            foreach (var href in hrefs)
            {
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
            var itemLoader = ItemLoaderFactory.GetItemLoader<JobItem>(response);
            itemLoader.BeforeValueSetting += ItemLoader_BeforeValueSetting;
            itemLoader.AddFieldMapping("Title", "css:.title-info h1::attr(text)");
            itemLoader.AddFieldMapping("Title", "css:.job-title h1::attr(text)");

            itemLoader.AddFieldMapping("Firm", "css:.title-info h3 a::attr(text)");
            itemLoader.AddFieldMapping("Firm", "css:.title-info h3::attr(text)");
            itemLoader.AddFieldMapping("Firm", "css:.title-info h3");
            itemLoader.AddFieldMapping("Firm", "css:.job-title h2::attr(text)");

            itemLoader.AddFieldMapping("Salary", "css:.job-main-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-main-title strong::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title");

            itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(title)");
            itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(text)");

            itemLoader.AddFieldMapping(u => u.Location, "css:.basic-infor span  a::attr(text)");
            itemLoader.AddFieldMapping(u => u.JobType, "IT");
            var item = itemLoader.LoadItem();
            Console.WriteLine(item.Firm);
        }

        private void ItemLoader_BeforeValueSetting(object arg1, ValueSettingEventArgs<JobItem> arg2)
        {
            arg2.Value = arg2.Value.Replace(System.Environment.NewLine, "").Trim();
            if(arg2.FieldName=="Salary")
            {
                var salaryFrom = string.Empty;
                var salaryTo = string.Empty;
                var match = salaryReg.Match(arg2.Value);
                if (match.Groups != null && match.Groups.Count > 0)
                {
                    salaryFrom = match.Groups[1].Value;
                    salaryTo = match.Groups[2].Value;
                }
                arg2.Item.SalaryFrom = salaryFrom;
                arg2.Item.SalaryTo = salaryTo;
            }
            
        }
    }

    public class JobItem
    {
        public string Firm { get; set; }
        public string Title { get; set; }
        public string Salary { get; set; }
        public string SalaryFrom { get; set; }
        public string SalaryTo { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
    }
}
