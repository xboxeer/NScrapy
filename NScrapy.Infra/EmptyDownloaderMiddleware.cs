using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public class EmptyDownloaderMiddleware : IDownloaderMiddleware
    {
        public virtual void PostDownload(IResponse response)
        {
            return;
        }

        public virtual void PreDownload(IRequest request)
        {
            return;
        }
    }
}
