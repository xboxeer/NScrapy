using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Scheduler
{
    public class Scheduler
    {
        public static void SendRequestToReceiver(IRequest request)
        {
            RequestReceiver.RequestQueue.Enqueue(request);
        }
    }
}
