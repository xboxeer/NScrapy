using NScrapy.Infra;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Engine
{
    public class NScrapyEngine:IEngine
    {
        public IResponse ProcessRequest(IRequest request)
        {
            //Put request to Downloader pool

            //Waiting for Downloader Complete
            //Once Completed, send response to ReponseDistributer

            //return response;
            throw new NotImplementedException();
        }


        public async Task<IResponse> ProcessRequestAsync(IRequest request)
        {
            var response = await Downloader.Downloader.SendRequestAsync(request);
            return response;
        }

    }
}
