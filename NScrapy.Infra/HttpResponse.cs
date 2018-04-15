using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace NScrapy.Infra
{
    public class HttpResponse : IResponse
    {
        public string URL { get ; set ; }
        public IRequest Request { get ; set; }
        public HttpResponseMessage RawResponseMessage { get; set; }
        public string ReponsePlanText { get; set; }
    }
}