using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace NScrapy.Infra
{
    public class HttpRequest : IRequest
    {
        public string URL { get; set ; }
        public HttpClient Client { get; set; }
        public ISpider RequestSpider { get; set; }
        public Action<IResponse> Callback { get; set; }
        public HttpMethod Method { get; set; }
        public Dictionary<string, string> FormData { get; set; }
        public string Cookies { get; set; }
    }
}