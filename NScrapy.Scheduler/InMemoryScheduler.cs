using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Scheduler
{
    public class InMemoryScheduler : IScheduler
    {
        private IUrlFilter urlFilter;

        public InMemoryScheduler()
        {
            RequestReceiver.StartReceiver();
            ResponseDistributer.StartDistribuiter();
            urlFilter = new InMemoryUrlFilter();
        }

        public IUrlFilter UrlFilter { get => urlFilter; set => urlFilter = value; }

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
