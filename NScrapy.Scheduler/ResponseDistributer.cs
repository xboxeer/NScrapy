using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                        if (response == null)
                        {
                            continue;
                        }
                        var callBack = response.Request.Callback;
                        var callBackTask = new Task(() =>
                          {
                              if (callBack == null)
                              {
                                  response.Request.RequestSpider.ResponseHandler(response);
                              }
                              else
                              {
                                  response.Request.Callback(response);
                              }
                          });
                        callBackTask.Start();
                        
                    }
                }
            }
        }
    }
}
