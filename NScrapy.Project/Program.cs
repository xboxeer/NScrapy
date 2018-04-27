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
    [URL("https://www.liepin.com/zhaopin/?ckid=14f634c4f64673b4&fromSearchBtn=2&init=-1&sfrom=click-pc_homepage-centre_searchbox-search_new&flushckid=1&dqs=&key=.net&headckid=46374f76c9769ff7&d_pageSize=40&siTag=LVCXL87NN2EpVFUH8QYgiQ%7Er3i1HcfrfE3VRWBaGW6LoA&d_headId=c84844d2749548903e20ddde1d713d12&d_ckId=7768bacafa68ea7a6181aa2a59daa5cf&d_sfrom=search_fp&d_curPage=0")]
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
                salary == null)
            {
                NScrapyContext.CurrentContext.Log.Info($"Unable to get items from page {response.URL}");
            }
            var info = $"{title},{firm.Replace(System.Environment.NewLine, string.Empty).Trim()},{salary.Replace(System.Environment.NewLine, string.Empty).Trim()},{time}";
            Console.WriteLine(info);
            File.AppendAllText($"output-{this.startingTime}.csv",info+System.Environment.NewLine,Encoding.UTF8);
            
        }
        
    }
}
