using System;
using System.Collections.Generic;
using System.Text;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;

namespace NScrapy.Project.Spiders
{
    [Name(Name = "LianjiaSpider")]
    [URL("https://sh.lianjia.com/ershoufang/")]
    public class Lianjia : Spider.Spider
    {
        public override void ResponseHandler(IResponse response)
        {
            var hrefs = response.CssSelector(".position dl dd div::attr(data-role)",arg=> {
                return arg.ExtractedValue.Contains("ershoufang");
            }).CssSelector("div",arg=> { return arg.ExtractedValue.Contains("title"); }).CssSelector("a::attr(href)").Extract();
            foreach (var href in hrefs)
            {
                NScrapy.Shell.NScrapy.GetInstance().Follow(response, href, VisitSubLocation);
            }
            
        }

        private void VisitSubLocation(IResponse response)
        {
            var hrefs = response.CssSelector(".position dl dd div::attr(data-role)", arg => {
                return arg.ExtractedValue.Contains("ershoufang");
            }).CssSelector("div", arg => { return !arg.ExtractedValue.Contains("title"); }).CssSelector("a::attr(href)").Extract();
            foreach (var href in hrefs)
            {
                Console.WriteLine(href);
                NScrapy.Shell.NScrapy.GetInstance().Follow(response, href, VisitPage);
            }            
        }

        private void VisitPage(IResponse response)
        {
            var nextPage = response.CssSelector(".house-lst-page-box a::attr(href)").ExtractLast();
            for(int i=0;i<=30;i++)
            {
                Console.WriteLine($"{response.URL}pg{i.ToString()}");
                NScrapy.Shell.NScrapy.GetInstance().Follow(response, $"{response.URL}pg{i.ToString()}", VisitDetail);

            }            
            this.VisitDetail(response);
        }

        private void VisitDetail(IResponse response)
        {
            var detailHrefs = response.CssSelector(".title a::attr(href)").Extract();
            foreach(var href in detailHrefs)
            {
                if (!href.Contains("javascript"))
                {
                    NScrapy.Shell.NScrapy.GetInstance().Follow(response, href, ParseItem);
                }
            }
        }

        private void ParseItem(IResponse response)
        {
            var itemLoader = ItemLoaderFactory.GetItemLoader<House>(response);
            itemLoader.AddFieldMapping(u => u.Title, "css:.title h1::attr(title)");
            itemLoader.AddFieldMapping(u => u.SubTitle, "css:.title div::attr(title)");
            itemLoader.AddFieldMapping(u => u.TotalPrice, "css:.price span::attr(text)");
            itemLoader.AddFieldMapping(u => u.UnitPrice, "css:.unitPrice span::attr(text)");
            itemLoader.AddFieldMapping(u => u.Size, "css:.area .mainInfo div::attr(text)");
            itemLoader.AddFieldMapping(u => u.CommunityName, "css:.communityName .info a::attr(text)");
            itemLoader.AddFieldMapping(u => u.Room, "css:.room .mainInfo div::attr(text)");
            var item = itemLoader.LoadItem();
            Console.Write($"{item.Title},{item.SubTitle},{item.TotalPrice},{item.UnitPrice},{item.Size},{item.Room},{item.CommunityName}");
            Console.WriteLine();
        }
    }

    public class House
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string TotalPrice { get; set; }
        public string UnitPrice { get; set; }
        public string Size { get; set; }
        public string Room { get; set; }
        public string CommunityName { get; set; }
    }
}
