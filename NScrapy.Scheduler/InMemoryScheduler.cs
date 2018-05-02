using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Scheduler
{
    public class InMemoryScheduler : IScheduler
    {
        public void SendRequestToReceiver(IRequest request)
        {
            RequestReceiver.RequestQueue.Enqueue(request);
        }

        public void SendResponseToDistributer(IResponse response)
        {
            ResponseDistributer.ResponseQueue.Enqueue(response);
        }
    }
}
