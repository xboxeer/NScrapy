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
        public Action<IResponse> Callback { get; set; }
        
        public HtmlDocument doc = new HtmlDocument();
        private StringBuilder strBuilder = new StringBuilder();
        private string reponsePlanText = string.Empty;
        private string attr = string.Empty;
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
            
            var attrReg = new Regex(@"(?<=::attr\()[^()]*(?=\))");
            var attrMatch = attrReg.Match(selector);
            if(attrMatch.Success)
            {
                attr = attrMatch.Value;
                //if matches  .job-info h3 a::attr(href), remove ::attr(href)
                selector = selector.Replace($"::attr({attr})","");
            }
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

        public IEnumerable<string> Extract()
        {
            var returnValue = new List<string>();
            var attrValueRegPattern = string.Empty;
            if(attr!=string.Empty)
            {
                attrValueRegPattern = $"(?<={attr}=\\s*['\"]+)[^'\"]*(?=['\"]+)";
            }
            foreach(var item in doc.DocumentNode.ChildNodes)
            {
                if(item.OuterHtml==string.Empty||
                    item.OuterHtml==System.Environment.NewLine)
                {
                    continue;
                }
                if (attr == string.Empty)
                {                     
                    yield return item.OuterHtml;
                }
                else if(attr=="text")
                {
                    yield return item.InnerText;
                }
                else
                {
                    var attrValueMatch = new Regex(attrValueRegPattern);
                    yield return attrValueMatch.Match(item.OuterHtml).Value;
                }
            }
        }

        public string ExtractFirst()
        {
            return this.Extract().FirstOrDefault();
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
            returnValue.attr = this.attr;
            strBuilder.Clear();
            return returnValue;
        }

    }
}