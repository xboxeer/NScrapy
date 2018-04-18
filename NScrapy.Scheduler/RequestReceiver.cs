using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace NScrapy.Scheduler
{
    public static class RequestReceiver
    {
        private static Queue<IRequest> queue = new Queue<IRequest>();
        private static object lockObj = new object();
        public const string REQUESTTHREADNAME = "ReceiverThread";
        public static Queue<IRequest> RequestQueue
        {
            get { return queue; }
        }
        public static void StartReceiver()
        {            
            Thread thread = new Thread(ListenToQueue)
            {
                Name = "ReceiverThread"
            };
            thread.Start();           
        }

        private static void ListenToQueue()
        {
            while(true)
            {
                lock (lockObj)
                {
                    if (queue.Count > 0)
                    {
                        var request = queue.Dequeue();
                        var result =  NScrapyContext.CurrentContext.CurrentEngine.ProcessRequestAsync(request);
                        result.ContinueWith(u => Scheduler.SendResponseToDistributer(u.Result));                        
                    }
                }
            }
        }
    }
}
