using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;

namespace NScrapy.Infra
{

    public class HttpResponse : IResponse
    {
        private StringBuilder strBuilder = new StringBuilder();
        private string reponsePlanText = string.Empty;
        private string attr = string.Empty;
        private HttpResponseMessage rawResponse;
        private HtmlDocument doc = new HtmlDocument();
        #region Manually these field beacuse HttpResponseHeader can not be Serialized by JsonConvert
        private List<string> acceptRanges;
        private List<ViaHeaderValue> via;
        private List<string> vary;
        private List<ProductHeaderValue> upgrade;
        private bool? transferEncodingChunked;
        private List<TransferCodingHeaderValue> transferEncoding;
        private List<string> trailer;
        private List<ProductInfoHeaderValue> server;
        private RetryConditionHeaderValue retryAfter;
        private List<AuthenticationHeaderValue> proxyAuthenticate;
        private List<NameValueHeaderValue> pragma;
        private Uri location;
        private EntityTagHeaderValue eTag;
        private DateTimeOffset? date;
        private bool? connectionClose;
        private List<string> connection;
        private CacheControlHeaderValue cacheControl;
        private TimeSpan? age;
        private List<WarningHeaderValue> warning;
        private List<AuthenticationHeaderValue> wwwAuthenticate;
        #endregion

        #region Ignored Property While JsonFormat.SerializeObject
        [JsonIgnore]
        public Action<IResponse> Callback { get; set; }

        [JsonIgnore]
        public IRequest Request { get; set; }

        [JsonIgnore]
        public HttpResponseMessage RawResponseMessage
        {
            get
            {
                return rawResponse;
            }
            set
            {
                rawResponse = value;
                AcceptRanges = RawResponseMessage.Headers.AcceptRanges.ToList();
                Via = RawResponseMessage.Headers.Via.ToList();
                Vary = RawResponseMessage.Headers.Vary.ToList();
                Upgrade = RawResponseMessage.Headers.Upgrade.ToList();
                TransferEncodingChunked = RawResponseMessage.Headers.TransferEncodingChunked;
                TransferEncoding = RawResponseMessage.Headers.TransferEncoding.ToList();
                Trailer = RawResponseMessage.Headers.Trailer.ToList();
                Server = RawResponseMessage.Headers.Server.ToList();
                RetryAfter = RawResponseMessage.Headers.RetryAfter;
                ProxyAuthenticate = RawResponseMessage.Headers.ProxyAuthenticate.ToList();
                Pragma = RawResponseMessage.Headers.Pragma.ToList();
                Location = RawResponseMessage.Headers.Location;
                ETag = RawResponseMessage.Headers.ETag;
                Date = RawResponseMessage.Headers.Date;
                ConnectionClose = RawResponseMessage.Headers.ConnectionClose;
                Connection = RawResponseMessage.Headers.Connection.ToList();
                CacheControl = RawResponseMessage.Headers.CacheControl;
                Age = RawResponseMessage.Headers.Age;
                Warning = RawResponseMessage.Headers.Warning.ToList();
                WwwAuthenticate = RawResponseMessage.Headers.WwwAuthenticate.ToList();
            }
        }
        #endregion

        public string URL { get; set; }

        public string ResponsePlanText
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
        #region HttpResponseHeader Properties
        //Manually create these field beacuse HttpResponseHeader can not be Serialized by JsonConvert
        public List<string> AcceptRanges { get => acceptRanges; set => acceptRanges = value; }
        public List<ViaHeaderValue> Via { get => via; set => via = value; }
        public List<string> Vary { get => vary; set => vary = value; }
        public List<ProductHeaderValue> Upgrade { get => upgrade; set => upgrade = value; }
        public bool? TransferEncodingChunked { get => transferEncodingChunked; set => transferEncodingChunked = value; }
        public List<TransferCodingHeaderValue> TransferEncoding { get => transferEncoding; set => transferEncoding = value; }
        public List<string> Trailer { get => trailer; set => trailer = value; }
        public List<ProductInfoHeaderValue> Server { get => server; set => server = value; }
        public RetryConditionHeaderValue RetryAfter { get => retryAfter; set => retryAfter = value; }
        public List<AuthenticationHeaderValue> ProxyAuthenticate { get => proxyAuthenticate; set => proxyAuthenticate = value; }
        public List<NameValueHeaderValue> Pragma { get => pragma; set => pragma = value; }
        public Uri Location { get => location; set => location = value; }
        public EntityTagHeaderValue ETag { get => eTag; set => eTag = value; }
        public DateTimeOffset? Date { get => date; set => date = value; }
        public bool? ConnectionClose { get => connectionClose; set => connectionClose = value; }
        public List<string> Connection { get => connection; set => connection = value; }
        public CacheControlHeaderValue CacheControl { get => cacheControl; set => cacheControl = value; }
        public TimeSpan? Age { get => age; set => age = value; }
        public List<WarningHeaderValue> Warning { get => warning; set => warning = value; }
        public List<AuthenticationHeaderValue> WwwAuthenticate { get => wwwAuthenticate; set => wwwAuthenticate = value; }
        #endregion
        public HttpResponse()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector">selecter sample: .class div a::attr(href)</param>
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
            doc.LoadHtml(this.ResponsePlanText);
            var elements=doc.QuerySelectorAll(selector);
            return this.CreateFilteredResponse(elements);
        }

        public IResponse CssSelector(IEnumerable<string> possableSelector)
        {
            IResponse returnValue = null;
            foreach (var selector in possableSelector)
            {
                returnValue = this.CssSelector(selector);
                if(returnValue.ExtractFirst()!=null)
                {
                    break;
                }
            }
            return returnValue;
        }

        public IResponse XPathSelector(string xpath)
        {
            doc.LoadHtml(this.ResponsePlanText);
            HtmlNodeCollection elements = null;
            var attrReg = new Regex(@"(?<=[\w]@)[\w]*");
            var attrMatch = attrReg.Match(xpath);
            if (attrMatch.Success)
            {
                attr = attrMatch.Value;
                xpath = xpath.Replace($"@{attr}", "");
            }
            elements = doc.DocumentNode.SelectNodes(xpath);
            return this.CreateFilteredResponse(elements);
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

        public string ExtractLast()
        {
            return this.Extract().LastOrDefault();
        }

        private HttpResponse CreateFilteredResponse(IList<HtmlNode> elements)
        {
            if (elements != null)
            {
                foreach (var node in elements)
                {
                    strBuilder.AppendLine(node.OuterHtml);
                }
            }
            var returnValue = new HttpResponse()
            {
                ResponsePlanText = strBuilder.ToString(),
                Request = this.Request,
                URL = this.URL
            };
            returnValue.attr = this.attr;
            strBuilder.Clear();
            return returnValue;
        }

        public void InitHeaderProperties()
        {

        }
    }
}