using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public class EmptySpiderMiddleware : ISpiderMiddleware
    {
        public virtual void PostReponse(IResponse response)
        {
            //do nothing since it is an empty middleware
        }

        public virtual void PreResponse(IResponse response)
        {
            //do nothing sing it is an empty middleware
        }
    }
}
