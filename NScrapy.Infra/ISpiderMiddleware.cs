using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface ISpiderMiddleware
    {
        void PreResponse(IResponse response);
        void PostReponse(IResponse response);
    }
}
