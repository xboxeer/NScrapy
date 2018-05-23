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
            shell.Crawl("LianjiaSpider");
            while(true)
            {

            }
        }
    }
    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?ckid=65f1263d0787b9af&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&dqs=010&flushckid=1&jobKind=2&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91+%E6%8A%95%E8%B5%84%E6%80%BB%E7%9B%91+%E5%9F%BA%E9%87%91&headckid=c602c61e1f15983a&d_pageSize=40&siTag=lDngfi9MwkyW4dDegF6xCQ~ZmXRTG3Nx-lODupCxpuySA&d_headId=28916eb31f53b2a93f6bda41c49c0cd4&d_ckId=775b4be69312baf620d22e6cc832893e&d_sfrom=search_fp&d_curPage=0",
        "https://www.liepin.com/zhaopin/?ckid=958d9d4fa8eca61a&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=020&jobKind=2&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91+%E6%8A%95%E8%B5%84%E6%80%BB%E7%9B%91+%E5%9F%BA%E9%87%91&headckid=c602c61e1f15983a&d_pageSize=40&siTag=lDngfi9MwkyW4dDegF6xCQ~hgRZ0h1p55DegpQkaoxBXQ&d_headId=28916eb31f53b2a93f6bda41c49c0cd4&d_ckId=e5c013b72bbef07cf1039f26554270b2&d_sfrom=search_fp&d_curPage=0")]
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
            itemLoader.BeforeValueSetting += (sender, e) => e.Item.URL = response.URL;
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
        public string Tech { get; set; }
        public string URL { get; set; }
    }
}
