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
        private string defaultCallbackName = string.Empty;
        private bool callBacksRegistered = false;
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
            if(callBacksRegistered)
            {
                return;
            }
            var currentSpider = NScrapyContext.CurrentContext.CurrentSpider;
            var defaultCallback = new Action<IResponse>(currentSpider.ResponseHandler);
            defaultCallbackName = defaultCallback.GetMethodInfo().Name;
            //defaultCallbackName = NScrapyHelper.GetMD5FromString(methodName);
            registedCallback.Add(defaultCallbackName, defaultCallback);
            var methods = currentSpider.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
            var availableMethods = methods.Where(method => method.Name!=defaultCallbackName&& method.GetParameters().Count() == 1&& method.GetParameters().FirstOrDefault().ParameterType == typeof(IResponse));
            foreach(var availableMethod in availableMethods)
            {
                var callBack = new Action<IResponse>(response => availableMethod.Invoke(currentSpider, new object[] { response as object }));            
                registedCallback.Add(availableMethod.Name, callBack);
            }
            callBacksRegistered = true;
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
                         //request.Callback.GetMethodInfo().
                         callbackName = request.Callback.GetMethodInfo().Name;
                         if (!registedCallback.ContainsKey(callbackName))
                         {
                             registedCallback.Add(callbackName, request.Callback);
                         }
                     }
                    //Url Visted and coresponding call back already Executed before
                     if (u.Result)
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
                var lockToken = Guid.NewGuid();
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
                    NScrapyContext.CurrentContext.Log.Error($"Error getting value from Redis", ex);
                }
                finally
                {
                    RedisSchedulerContext.Current.ReleaseLock($"{RedisSchedulerContext.Current.ResponseQueue}.Lock", lockToken.ToString());
                }
                if(!value.HasValue)
                {
                    continue;
                }
                responseMessage = JsonConvert.DeserializeObject<RedisResponseMessage>(value);
                if (registedCallback.ContainsKey(responseMessage.CallbacksFingerprint))
                {
                    var callback = registedCallback[responseMessage.CallbacksFingerprint];
                    var response = NScrapyHelper.DecompressResponse(responseMessage.Payload);
                    var task = new Task(()=>callback(response));
                    task.ContinueWith(t => NScrapyContext.CurrentContext.Log.Error($"Error Process Callback {responseMessage.CallbacksFingerprint}",task.Exception ),TaskContinuationOptions.OnlyOnFaulted);
                    task.Start();
                }                
            }
        }
    }
}