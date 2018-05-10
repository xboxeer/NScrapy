using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IScheduler
    {
        IUrlFilter UrlFilter
        {
            get;
            set;
        }
        void SendRequestToReceiver(IRequest request);
        void SendResponseToDistributer(IResponse response);
    }
}
