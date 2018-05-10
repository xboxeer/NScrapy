using NScrapy.Infra;
using System;
using System.Reflection;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisScheduler : IScheduler
    {
        private Dictionary<string, IRequest> requests = new Dictionary<string, IRequest>();
        private Dictionary<string, Action<IResponse>> registedCallback = new Dictionary<string, Action<IResponse>>();
        //当有好几个爬虫在运行的时候 由于UrlFilter机制 后加入的爬虫会没有机会爬最开始的网页 导致回调函数无法注册到Scheduler里面
        //进而导致消息队列发消息回来的时候 后加入的爬虫根本没有机会执行任何操作（因为消息队列中指定的回调函数指纹根本没有注册到Scheduler中）
        //因此加入一个字典 callBackExcutedList 来记录回调函数是否有被执行过，如果没有被执行过 那么忽略UrlFilter的结果 强制再爬一遍这个网页
        //这样可以确保对于每个单独运行的爬虫，所有回调函数都被注册到了
        //这个机制的坏处是假如有100个爬虫 每个爬虫有10个方法回调，那么总共会有100*10条重复数据（如果每个回调方法都产生数据记录的话）
        //然而实际中这样的情况应该不多 
        private Dictionary<string, bool> callBackExcutedList = new Dictionary<string, bool>();
        private string defaultCallbackName = string.Empty;
        public IUrlFilter UrlFilter { get; set; }

        public RedisScheduler()
        {
            ResponseDistributer.StartDistribuiter();
            //RedisSchedulerContext.InitContext();
            Thread t = new Thread(ListenToRedisTopic);
            t.Name = "ResponseDistributeThread";
            t.Start();
            UrlFilter = new RedisUrlFilter();
        }        

        //Add ResponseHandler method of the Spider to registedCallback Dic as Default Callback
        private void AddDefaultHandlerToCallbackList()
        {
            if(!string.IsNullOrEmpty(defaultCallbackName))
            {
                return;
            }
            var currentSpider = NScrapyContext.CurrentContext.CurrentSpider;
            var defaultCallback = new Action<IResponse>(currentSpider.ResponseHandler);
            defaultCallbackName = defaultCallback.GetMethodInfo().Name;
            //defaultCallbackName = NScrapyHelper.GetMD5FromString(methodName);
            registedCallback.Add(defaultCallbackName, defaultCallback);
            callBackExcutedList.Add(defaultCallbackName, false);
        }

        public void SendRequestToReceiver(IRequest request)
        {
            AddDefaultHandlerToCallbackList();
            var task = this.UrlFilter.IsUrlVisited(request.URL);
             task.ContinueWith( u =>
                 {                     
                     var connection = RedisSchedulerContext.Current.Connection;
                     var callbackName = this.defaultCallbackName;
                     if (request.Callback != null)
                     {
                         callbackName = request.Callback.GetMethodInfo().Name;
                         if (!registedCallback.ContainsKey(callbackName))
                         {
                             registedCallback.Add(callbackName, request.Callback);
                             callBackExcutedList.Add(callbackName, false);
                         }
                     }
                     //Url Visted and coresponding call back already Executed before
                     if (u.Result &&
                        callBackExcutedList.ContainsKey(callbackName) &&
                        callBackExcutedList[callbackName] == true)
                     {
                         return;
                     }
                     var requestMessage = new RedisRequestMessage()
                     {
                         URL = request.URL,
                         CallbacksFingerprint = callbackName
                     };
                     connection.GetDatabase().ListLeftPushAsync(RedisSchedulerContext.Current.ReceiverQueue, JsonConvert.SerializeObject(requestMessage));
                 }, TaskContinuationOptions.OnlyOnRanToCompletion
            );
            task.ContinueWith(u => {
                NScrapyContext.CurrentContext.Log.Info($"Sending request to {request.URL} failed", u.Exception.InnerException);
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }

        public void SendResponseToDistributer(IResponse response)
        {
            //response.Request = requests[response.URL];
            //ResponseDistributer.ResponseQueue.Enqueue(response);
        }

        private void ListenToRedisTopic()
        {
            var connection = RedisSchedulerContext.Current.Connection;
            while (true)
            {
                var lockToken = new Guid();
                RedisResponseMessage responseMessage = null;
                RedisValue value = new RedisValue();
                try
                {
                    RedisSchedulerContext.Current.GetLock($"{RedisSchedulerContext.Current.ResponseQueue}.Lock", lockToken.ToString());
                    if (RedisSchedulerContext.Current.Connection.GetDatabase().ListLength(RedisSchedulerContext.Current.ResponseQueue) > 0)
                    {
                        value = RedisSchedulerContext.Current.Connection.GetDatabase().ListRightPop(RedisSchedulerContext.Current.ResponseQueue);
                    }
                }
                catch (Exception ex)
                {
                    NScrapyContext.CurrentContext.Log.Error($"Error processing {responseMessage.URL} with Handler {responseMessage.CallbacksFingerprint}", ex);
                }
                finally
                {
                    RedisSchedulerContext.Current.ReleaseLock($"{RedisSchedulerContext.Current.ResponseQueue}.Lock", lockToken.ToString());
                }
                responseMessage = JsonConvert.DeserializeObject<RedisResponseMessage>(value);
                if (registedCallback.ContainsKey(responseMessage.CallbacksFingerprint))
                {
                    var callback = registedCallback[responseMessage.CallbacksFingerprint];
                    var response = NScrapyHelper.DecompressResponse(responseMessage.Payload);
                    callback(response);
                    callBackExcutedList[responseMessage.CallbacksFingerprint] = true;
                }                
            }
        }
    }
}