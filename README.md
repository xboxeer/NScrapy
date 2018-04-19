# NScrapy Sample code
Usage:

    using NScrapy.Infra;
    using NScrapy.Infra.Attributes.SpiderAttributes;

    namespace NScrapy.Project
    {
        class Program
        {
            static void Main(string[] args)
            {
                var shell = NScrapy.Shell.NScrapy.GetInstance();
                shell.Crawl("PersonSpider");
                return;
            }
        }
        [Name(Name = "JobSpider")]
        [URL("https://www.liepin.com/zhaopin/?industries=&dqs=&salary=&jobKind=&pubTime=&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=1773084a3c558acd&d_headId=4999cfe588ed1ed20f0a479b74934008&d_ckId=4999cfe588ed1ed20f0a479b74934008&d_sfrom=search_fp_nvbar&d_curPage=0&d_pageSize=40&siTag=1B2M2Y8AsgTpgAmY7PhCfg~fA9rXquZc5IkJpXC-Ycixw&key=%E8%9E%8D%E8%B5%84%E6%80%BB%E7%9B%91")]
        public class JobSpider : Spider.Spider
        {
            public override IResponse ResponseHandler(IResponse response)
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
                return returnValue;
            }

            private void VisitPage(IResponse returnValue)
            {
                var hrefs = returnValue.CssSelector(".job-info h3 a::attr(href)").Extract();
                foreach (var href in hrefs)
                {
                    NScrapy.Shell.NScrapy.GetInstance().Request( href, Parse);
                }
            }

            public void Parse(IResponse response)
            {
                var title = response.CssSelector(".title-info h1::attr(title)").ExtractFirst();
                var firm = response.CssSelector(".title-info h3 a::attr(title)").ExtractFirst();
                var salary = response.CssSelector(".job-item-title p::attr(text)").ExtractFirst();
                Console.WriteLine($"{title} {firm} {salary}");
            }
        }
