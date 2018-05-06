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

namespace NScrapy.DownloaderShell
{
    public class Program
    {
        public static void Main(string[] args)
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

                        var resultBinaryStr = string.Empty;
                        try
                        {                          
                            resultBinaryStr = JsonConvert.SerializeObject(u.Result);
                        }
                        catch (Exception ex)
                        {
                            DownloaderContext.Context.Log.Error($"Serialize response for {request.URL} failed!", ex);
                        }

                        var compressedJson = await NScrapyHelper.CompressString(resultBinaryStr);
                        var publishResult = RedisManager.Connection.GetSubscriber().Publish(responseTopicName, $"{url}|{compressedJson}");
                        DownloaderContext.Context.Log.Info($"Sending request to {request.URL} success!");

                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion);
                    result.ContinueWith(u => DownloaderContext.Context.Log.Info($"Sending request to {request.URL} failed", result.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
                }
            }            
        }
    }
}
