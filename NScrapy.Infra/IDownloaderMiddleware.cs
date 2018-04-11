using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IDownloaderMiddleware
    {
        void PreDownload(IRequest request);
        void PostDownload(IResponse response);
    }
}
