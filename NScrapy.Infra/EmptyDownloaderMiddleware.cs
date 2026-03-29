using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public virtual Task<IResponse> ProcessAsync(IRequest request)
        {
            return Task.FromResult<IResponse>(null);
        }
    }
}
