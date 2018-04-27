using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Spider.Middleware
{
    public class DeepthMiddleware :EmptySpiderMiddleware
    {
        public override void PreResponse(IResponse response)
        {
            base.PreResponse(response);
        }
    }
}
