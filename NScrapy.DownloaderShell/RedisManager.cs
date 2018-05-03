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
            Connection = ConnectionMultiplexer.Connect($"{redisServer}:{redisPort}");
        }
    }
}
