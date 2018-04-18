using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.Text.RegularExpressions;

namespace NScrapy.Infra
{
    public class HttpResponse : IResponse
    {
        public string URL { get ; set ; }
        public IRequest Request { get ; set; }
        public HttpResponseMessage RawResponseMessage { get; set; }        
        public string ReponsePlanText
        {
            get
            {
                return this.reponsePlanText;
            }
            set
            {
                this.reponsePlanText = value;
                doc.LoadHtml(value);
            }
        }       
        public Func<IResponse, IResponse> Callback { get; set; }
        
        public HtmlDocument doc = new HtmlDocument();
        private StringBuilder strBuilder = new StringBuilder();
        private string reponsePlanText = string.Empty;
        public HttpResponse()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector">selecter sample: .job-info h3 a::attr(href)</param>
        /// <returns></returns>
        public IResponse CssSelector(string selector)
        {
            //var cssSelectorReg = new Regex(@"[\s\S]+(?=::attr)");
            //var groups = cssSelectorReg.Match(selector);
            doc.LoadHtml(this.ReponsePlanText);
            var elements=doc.QuerySelectorAll(selector);
            return this.CreateFilteredResponse(elements);
        }

        public IResponse XPathSelector(string xpath)
        {
            //HtmlDocument doc = new HtmlDocument();
            //doc.LoadHtml(this.ReponsePlanText);
            //doc.DocumentNode.DescendantNodes("sojob-item-main").
            throw new NotImplementedException();
        }

        public List<string> Extract()
        {
            var returnValue = new List<string>();
            foreach(var item in doc.DocumentNode.ChildNodes)
            {
                returnValue.Add(item.OuterHtml);
            }
            return returnValue;
        }

        private HttpResponse CreateFilteredResponse(IList<HtmlNode> elements)
        {

            foreach (var node in elements)
            {
                strBuilder.AppendLine(node.OuterHtml);
            }
            var returnValue = new HttpResponse()
            {
                RawResponseMessage = this.RawResponseMessage,
                ReponsePlanText = strBuilder.ToString(),
                Request = this.Request,
                URL = this.URL
            };
            strBuilder.Clear();
            return returnValue;
        }
    }
}