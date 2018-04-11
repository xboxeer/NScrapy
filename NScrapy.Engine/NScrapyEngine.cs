using NScrapy.Infra;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Engine
{
    public class NScrapyEngine:IEngine
    {
        public IResponse ProcessRequest(IRequest request)
        {
            //Put request to Downloader pool
            var response=Downloader.Downloader.SendRequestAsync(request);
            //Waiting for Downloader Complete
            //Once Completed, send response to ReponseDistributer

            throw new NotImplementedException();
        }
    }
}
