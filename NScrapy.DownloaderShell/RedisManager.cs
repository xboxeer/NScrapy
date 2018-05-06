using NScrapy.Downloader;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.DownloaderShell
{
    public static class RedisManager
    {
        public static ConnectionMultiplexer Connection { get; private set; }

        public static string ReceiverQueue { get; private set; }

        static RedisManager()
        {
            var redisServer = DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisServer"];
            var redisPort = DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisPort"];
            ReceiverQueue = string.IsNullOrEmpty(DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"]) ? "NScrapy.Downloader" : DownloaderContext.Context.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            var options = new ConfigurationOptions()
            {
                EndPoints = { $"{redisServer}:{redisPort}" },
                SyncTimeout = 60000,
                ConnectTimeout=60000,
            };
            Connection = ConnectionMultiplexer.Connect(options);
        }
    }
}
