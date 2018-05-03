using NScrapy.Downloader;
using NScrapy.Infra;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NScrapy.DownloaderShell
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = Downloader.DownloaderContext.Context;
            context.RunningMode = Downloader.DownloaderRunningMode.Distributed;
            context.Log.Info("Downloader Started");
            var receiveQueueName = DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            var responseTopicName = DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:ResponseTopic"];
            while (true)
            {
                if (RedisManager.Connection.GetDatabase().ListLength(receiveQueueName) > 0 &&
                   Downloader.Downloader.RunningDownloader < Downloader.Downloader.DownloaderPoolCapbility)
                {
                    var url = RedisManager.Connection.GetDatabase().ListRightPop(receiveQueueName);
                    var request = new HttpRequest()
                    {
                        URL = url
                    };
                    var result = Downloader.Downloader.SendRequestAsync(request);
                    result.ContinueWith(async u =>
                    {
                        var resultJson = JsonConvert.SerializeObject(u.Result);
                        var compressedJson =await NScrapyHelper.CompressString(resultJson);                        
                        var publishResult = RedisManager.Connection.GetSubscriber().Publish(responseTopicName,$"{url}:{compressedJson}");
                        DownloaderContext.Context.Log.Info($"Sending request to {request.URL} success!");
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);
                    result.ContinueWith(u => DownloaderContext.Context.Log.Info($"Sending request to {request.URL} failed", result.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
                }
            }            
        }
    }
}
