using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Infra
{
    public interface IDownloaderMiddleware
    {
        void PreDownload(IRequest request);
        void PostDownload(IResponse response);
        /// <summary>
        /// Override this method to provide async download handling.
        /// Return null to fall back to normal download, or return IResponse to skip normal download.
        /// </summary>
        Task<IResponse> ProcessAsync(IRequest request);
    }
}
