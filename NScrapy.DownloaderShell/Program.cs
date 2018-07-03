using NScrapy.Downloader;
using NScrapy.Infra;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using NScrapy.Scheduler.RedisExt;
using NScrapy.Infra.ConfigProvider;

namespace NScrapy.DownloaderShell
{
    public class Program
    {
        public static Guid ID;
        public static void Main(string[] args)
        {
            ID = Guid.NewGuid();
            var context = Downloader.DownloaderContext.CurrentContext;
            context.ConfigProvider = new ZookeeperConfigProvider();
            context.RunningMode = Downloader.DownloaderRunningMode.Distributed;
            context.Log.Info("Downloader Started");
            var receiveQueueName = DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            var responseQueueName = DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ResponseQueue"];
            while (true)
            {
                var lockToken = Guid.NewGuid().ToString();
                //Get Lock before we count the lengh of queue
                //In case multiple Downloader runs into this part and found there are 1 item in queue
                //Then one of the Downloader gets that item, results to other Downloader get nothing
                RedisManager.GetLock($"{receiveQueueName}.Lock", lockToken);
                if (RedisManager.Connection.GetDatabase().ListLength(receiveQueueName) > 0 &&
                   Downloader.Downloader.RunningDownloader < Downloader.Downloader.DownloaderPoolCapbility)
                {
                    
                    var requestMessage = string.Empty;
                    try
                    {                        
                        requestMessage = RedisManager.Connection.GetDatabase().ListRightPop(receiveQueueName);
                    }
                    catch (Exception ex)
                    {
                        DownloaderContext.CurrentContext.Log.Error($"Aquire lock failed!", ex);
                    }
                    finally
                    {
                        RedisManager.ReleaseLock($"{receiveQueueName}.Lock", lockToken);
                    }
                    ProcessRequestAndSendBack(responseQueueName, requestMessage);
                }
            }            
        }

        private static void ProcessRequestAndSendBack(string responseQueueName, string requestMessage)
        {
            var requestObj = JsonConvert.DeserializeObject<RedisRequestMessage>(requestMessage);
            var request = new HttpRequest()
            {
                URL = requestObj.URL
            };
            var result = Downloader.Downloader.SendRequestAsync(request);

            result.ContinueWith(async u =>
            {

                var resultPayload = string.Empty;
                try
                {
                    resultPayload = JsonConvert.SerializeObject(u.Result);
                }
                catch (Exception ex)
                {
                    DownloaderContext.CurrentContext.Log.Error($"Serialize response for {request.URL} failed!", ex);
                }

                resultPayload = await NScrapyHelper.CompressString(resultPayload);
                var responseMessage = new RedisResponseMessage()
                {
                    URL = requestObj.URL,
                    CallbacksFingerprint = requestObj.CallbacksFingerprint,
                    Payload = resultPayload
                };

                //var publishResult = RedisManager.Connection.GetSubscriber().Publish(responseQueueName, $"{JsonConvert.SerializeObject(responseMessage)}");
                var publishResult = RedisManager.Connection.GetDatabase().ListLeftPush(responseQueueName, $"{JsonConvert.SerializeObject(responseMessage)}");
                DownloaderContext.CurrentContext.Log.Info($"Sending request to {request.URL} success!");

            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
            result.ContinueWith(u =>
            DownloaderContext.CurrentContext.Log.Info($"Sending request to {request.URL} failed", result.Exception.InnerException),
            TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
