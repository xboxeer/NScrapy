using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NScrapy.Infra
{
    public interface IResponse
    {
        string URL { get; set; }
        IRequest Request { get; set; }
        HttpResponseMessage RawResponseMessage { get; set; }
        string ReponsePlanText { get; set; }
    }
}
