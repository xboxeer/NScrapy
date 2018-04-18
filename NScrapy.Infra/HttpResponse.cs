using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace NScrapy.Infra
{
    public class HttpResponse : IResponse
    {
        public string URL { get ; set ; }
        public IRequest Request { get ; set; }
        public HttpResponseMessage RawResponseMessage { get; set; }
        public string ReponsePlanText { get; set; }

        public IResponse CssSelector(string selector)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(this.ReponsePlanText);
            doc.QuerySelectorAll(selector);
            throw new NotImplementedException();
        }

        public IResponse XPathSelector(string xpath)
        {
            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(this.ReponsePlanText);
            //doc.DocumentNode.DescendantNodes("sojob-item-main").
            throw new NotImplementedException();
        }
    }
}