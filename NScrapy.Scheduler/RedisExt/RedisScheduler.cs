using NScrapy.Infra;
using System;
using System.Reflection;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisScheduler : IScheduler
    {

        public RedisScheduler()
        {
            ResponseDistributer.StartDistribuiter();
            //RedisSchedulerContext.InitContext();
            ListenToRedisTopic();
        }

        private Dictionary<string, IRequest> requests = new Dictionary<string, IRequest>();
        public async void SendRequestToReceiver(IRequest request)
        {
            //add request to redis storage and mapps to its url  
            if(requests.ContainsKey(request.URL))
            {
                return;
            }
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

        private void ListenToRedisTopic()
        {
            var connection = RedisSchedulerContext.Current.Connection;
            connection.GetSubscriber().Subscribe(RedisSchedulerContext.Current.ResponseTopic, 
                 (channel, value) => {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var responsMessageParts = value.ToString().Split("|");
                        if(responsMessageParts.Count()!=2)
                        {
                            throw new FormatException("Response from Downloader has an invaled foramt, curret format should be URL|ResponseContent(with base64 compressed)");
                        }
                        var url = responsMessageParts[0];
                        if(requests.ContainsKey(url))
                        {
                            var responsePayload = responsMessageParts[1];
                            var response =  NScrapyHelper.DecompressResponse(responsePayload);
                            response.Request = requests[url];
                            this.SendResponseToDistributer(response);
                        }
                    }

                });
        }
    }
}