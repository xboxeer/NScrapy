using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NScrapy.Project.DownloaderMiddleware
{
    public class CookiesDownloaderMiddleware:EmptyDownloaderMiddleware
    {
        public override void PreDownload(IRequest request)
        {
        }
    }
}