using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace NScrapy.Infra
{
    public interface IResponse
    {
        string URL { get; set; }
        IRequest Request { get; set; }
        string ReponsePlanText { get; set; }
        HttpResponseMessage RawResponseMessage { get; set; }
        List<string> AcceptRanges { get; set; }
        List<ViaHeaderValue> Via { get; set; }
        List<string> Vary { get; set; }
        List<ProductHeaderValue> Upgrade { get; set; }
        bool? TransferEncodingChunked { get; set; }
        List<TransferCodingHeaderValue> TransferEncoding { get; set; }
        List<string> Trailer { get; set; }
        List<ProductInfoHeaderValue> Server { get; set; }
        RetryConditionHeaderValue RetryAfter { get; set; }
        List<AuthenticationHeaderValue> ProxyAuthenticate { get; set; }
        List<NameValueHeaderValue> Pragma { get; set; }
        Uri Location { get; set; }
        EntityTagHeaderValue ETag { get; set; }
        DateTimeOffset? Date { get; set; }
        bool? ConnectionClose { get; set; }
        List<string> Connection { get; set; }
        CacheControlHeaderValue CacheControl { get; set; }
        TimeSpan? Age { get; set; }
        List<WarningHeaderValue> Warning { get; set; }
        List<AuthenticationHeaderValue> WwwAuthenticate { get; set; }

        IResponse CssSelector(string selector);
        IResponse CssSelector(IEnumerable<string> possableSelector);
        IResponse XPathSelector(string xpath);
        IEnumerable<string> Extract();        
        string ExtractFirst();
        string ExtractLast();
    }
}
