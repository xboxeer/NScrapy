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
        HttpResponseMessage ResponseMessage { get; set; }
    }
}
