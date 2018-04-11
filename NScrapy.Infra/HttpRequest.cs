using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NScrapy.Infra
{
    public class HttpRequest : IRequest
    {
        public string URL { get; set ; }
    }
}