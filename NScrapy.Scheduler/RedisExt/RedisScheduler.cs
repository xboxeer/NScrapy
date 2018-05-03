using NScrapy.Infra;
using System;
using System.Reflection;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisScheduler : IScheduler
    {
        private Dictionary<string, IRequest> requests = new Dictionary<string, IRequest>();
        public async void SendRequestToReceiver(IRequest request)
        {
            //add request to redis storage and mapps to its url   
            requests.Add(request.URL,request);
            //then send request to specific queue that the downloader is listening to
            var connection = RedisSchedulerContext.Current.Connection;
            await connection.GetDatabase().ListLeftPushAsync(RedisSchedulerContext.Current.ReceiverQueue, request.URL);
        }

        public void SendResponseToDistributer(IResponse response)
        {
            response.Request = requests[response.URL];
            ResponseDistributer.ResponseQueue.Enqueue(response);
        }
    }
}