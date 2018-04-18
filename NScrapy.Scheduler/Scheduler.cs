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
            //We can add some other logic to store the request to some cache like Redis if there is too many request
            RequestReceiver.RequestQueue.Enqueue(request);
        }

        public static void SendResponseToDistributer(IResponse response)
        {
            //We can add some other logic to store the response to some cache like Redis if there is too many response
            ResponseDistributer.ResponseQueue.Enqueue(response);
        }
    }
}
