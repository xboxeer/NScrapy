using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IScheduler
    {
        void SendRequestToReceiver(IRequest request);
        void SendResponseToDistributer(IResponse response);
    }
}
