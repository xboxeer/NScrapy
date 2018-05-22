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
    [URL("https://www.liepin.com/zhaopin/?ckid=6cd70b2d2e808b71&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=010&jobKind=2&key=%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+&headckid=4ea599aaf53e425e&d_pageSize=40&siTag=4owhilU_PEm8egvyPtTnBQ~5_NiSw97HP2fC0L42YSK3Q&d_headId=45093ff3ccf788c48b43224217774f45&d_ckId=0709ad93c03074a0ba9f160f6864aee2&d_sfrom=search_fp&d_curPage=0",
        "https://www.liepin.com/zhaopin/?ckid=59fda27124be46b9&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=020&jobKind=2&key=%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+&headckid=4ea599aaf53e425e&d_pageSize=40&siTag=4owhilU_PEm8egvyPtTnBQ~hgRZ0h1p55DegpQkaoxBXQ&d_headId=45093ff3ccf788c48b43224217774f45&d_ckId=82edfc479679106c1afd26833c70bb20&d_sfrom=search_fp&d_curPage=0",
        "https://www.liepin.com/zhaopin/?ckid=d3c6ba1cc98e41dc&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=010&jobKind=2&key=%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+&headckid=4ea599aaf53e425e&d_pageSize=40&siTag=4owhilU_PEm8egvyPtTnBQ~5_NiSw97HP2fC0L42YSK3Q&d_headId=45093ff3ccf788c48b43224217774f45&d_ckId=a8af9e3325e1678cc87d5cf1313b3f73&d_sfrom=search_fp&d_curPage=0",
        "https://www.liepin.com/zhaopin/?ckid=59b546a02ba485ef&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=020&jobKind=2&key=%E6%89%A7%E8%A1%8C%E8%91%A3%E4%BA%8B+&headckid=4ea599aaf53e425e&d_pageSize=40&siTag=4owhilU_PEm8egvyPtTnBQ~hgRZ0h1p55DegpQkaoxBXQ&d_headId=45093ff3ccf788c48b43224217774f45&d_ckId=9a2b80b04878801920d3711e6bb08b9d&d_sfrom=search_fp&d_curPage=0",
        "https://www.liepin.com/zhaopin/?industries=&dqs=010&salary=&jobKind=2&pubTime=&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=4ea599aaf53e425e&d_headId=45093ff3ccf788c48b43224217774f45&d_ckId=70c7217acc908b9e6c730f33475268dd&d_sfrom=search_fp&d_curPage=0&d_pageSize=40&siTag=4owhilU_PEm8egvyPtTnBQ~hgRZ0h1p55DegpQkaoxBXQ&key=%E6%89%A7%E8%A1%8C%E6%80%BB%E7%BB%8F%E7%90%86",
        "https://www.liepin.com/zhaopin/?pubTime=&ckid=2ab1f32889c3c80b&fromSearchBtn=2&compkind=&isAnalysis=&init=-1&searchType=1&flushckid=1&dqs=020&industryType=&jobKind=2&sortFlag=15&industries=&salary=&compscale=&key=%E6%89%A7%E8%A1%8C%E6%80%BB%E7%BB%8F%E7%90%86&clean_condition=&headckid=2ab1f32889c3c80b&d_pageSize=40&siTag=kI7RLE8_eu4PVAtXNkjXDg~hgRZ0h1p55DegpQkaoxBXQ&d_headId=8d879c894ff37809dd3c902b62323697&d_ckId=8d879c894ff37809dd3c902b62323697&d_sfrom=search_prime&d_curPage=0",
        "https://www.liepin.com/zhaopin/?pubTime=&ckid=c184f4aee39a32fb&fromSearchBtn=2&compkind=&isAnalysis=&init=-1&searchType=1&flushckid=1&dqs=010&industryType=&jobKind=2&sortFlag=15&industries=&salary=&compscale=&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91&clean_condition=&headckid=c184f4aee39a32fb&d_pageSize=40&siTag=CwwZj9Wy1DUZoLS1krewgQ~5_NiSw97HP2fC0L42YSK3Q&d_headId=4d3d7658a7255b8264a0f40bf4ebe452&d_ckId=4d3d7658a7255b8264a0f40bf4ebe452&d_sfrom=search_prime&d_curPage=0",
        "https://www.liepin.com/zhaopin/?pubTime=&ckid=ff67e57f7568d271&fromSearchBtn=2&compkind=&isAnalysis=&init=-1&searchType=1&flushckid=1&dqs=020&industryType=&jobKind=2&sortFlag=15&industries=&salary=&compscale=&clean_condition=&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91&headckid=c184f4aee39a32fb&d_pageSize=40&siTag=CwwZj9Wy1DUZoLS1krewgQ~hgRZ0h1p55DegpQkaoxBXQ&d_headId=4d3d7658a7255b8264a0f40bf4ebe452&d_ckId=032eb538b86f86004757255500c01c85&d_sfrom=search_prime&d_curPage=0",
        "https://www.liepin.com/zhaopin/?pubTime=&ckid=561059611e131577&fromSearchBtn=2&compkind=&isAnalysis=&init=-1&searchType=1&flushckid=1&dqs=010&industryType=&jobKind=2&sortFlag=15&industries=&salary=&compscale=&key=%E6%8A%95%E8%B5%84%E6%80%BB%E7%9B%91&clean_condition=&headckid=561059611e131577&d_pageSize=40&siTag=JYmfzpXomIbS_39b_zCZrg~5_NiSw97HP2fC0L42YSK3Q&d_headId=1a59e5736af76b3a3ea0fa35f4dde4db&d_ckId=1a59e5736af76b3a3ea0fa35f4dde4db&d_sfrom=search_prime&d_curPage=0",
        "https://www.liepin.com/zhaopin/?pubTime=&ckid=1e97b7c4520b16e2&fromSearchBtn=2&compkind=&isAnalysis=&init=-1&searchType=1&flushckid=1&dqs=020&industryType=&jobKind=2&sortFlag=15&industries=&salary=&compscale=&clean_condition=&key=%E6%8A%95%E8%B5%84%E6%80%BB%E7%9B%91&headckid=561059611e131577&d_pageSize=40&siTag=JYmfzpXomIbS_39b_zCZrg~hgRZ0h1p55DegpQkaoxBXQ&d_headId=1a59e5736af76b3a3ea0fa35f4dde4db&d_ckId=556edf44772da9c7a1900ba455528a7b&d_sfrom=search_prime&d_curPage=0")]
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
            itemLoader.AddFieldMapping(u => u.URL, response.URL);
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
