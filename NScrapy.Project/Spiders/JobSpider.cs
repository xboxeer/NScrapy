using HtmlAgilityPack;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Project.Spiders
{
    [Name(Name = "JobSpider")]
    [URL("https://c.liepin.com/?time=1523890592601")]
    public class JobSpider : Spider.Spider
    {
        public override IResponse ResponseHandler(IResponse response)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(response.ReponsePlanText);
            return null;
            //var parser = new HtmlParser();
            //var result=parser.Parse(response.ReponsePlanText);
        }
    }
}
