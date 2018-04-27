using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NScrapy.Project.DownloaderMiddleware
{
    public class DownloaderLatencyMiddleware:EmptyDownloaderMiddleware
    {
        public override void PreDownload(IRequest request)
        {
            base.PreDownload(request);
            Random r = new Random();
            var waitFor = r.Next(1000, 3000);
            Thread.Sleep(waitFor);
        }
    }
}