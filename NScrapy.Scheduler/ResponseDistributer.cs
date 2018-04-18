using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NScrapy.Scheduler
{
    public class ResponseDistributer
    {
        private static Queue<IResponse> responseQueue = new Queue<IResponse>();
        private static object lockObj = new object();
        public const string RESPONSETHREADNAME = "DistributorThread";
        public static Queue<IResponse> ResponseQueue
        {
            get { return responseQueue; }
        }
        public static void StartDistribuiter()
        {
            Thread thread = new Thread(ListenToQueue)
            {
                Name = "DistributorThread"
            };
            thread.Start();
        }

        private static void ListenToQueue()
        {
            while (true)
            {
                lock (lockObj)
                {
                    if (responseQueue.Count > 0)
                    {
                        var response = responseQueue.Dequeue();
                        var spider = response.Request.RequestSpider;
                        spider.ResponseHandler(response);
                        //Put Response to ResponseProcessor
                    }
                }
            }
        }
    }
}
