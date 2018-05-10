using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisResponseDistributer
    {
        private static Queue<IResponse> responseQueue = new Queue<IResponse>();
        private static object lockObj = new object();

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
            while(true)
            {
                lock(lockObj)
                {
                    if(responseQueue.Count>0)
                    {
                        var response = responseQueue.Dequeue();
                    }
                }
            }
        }
    }

    
}
