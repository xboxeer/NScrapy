using NScrapy.Infra.Attributes.SpiderAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Project.Spiders
{
    [Name(Name ="JobSpider")]
    [URL("www.linkedin","www.lieping.com")]
    public class JobSpider:Spider.Spider
    {
    }
}
