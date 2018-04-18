using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NScrapy.Infra
{
    public interface IRequest
    {
        string URL { get; set; }
        HttpClient Client { get; set; }
        ISpider RequestSpider { get; set; }
    }
}
