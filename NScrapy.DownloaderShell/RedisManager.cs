using NScrapy.Downloader;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NScrapy.DownloaderShell
{
    public static class RedisManager
    {
        private static ConnectionMultiplexer connection;
        private static bool connectionChangeRequested = false;
        private static object lockObj = new object();
        public static ConnectionMultiplexer Connection
        {
            get
            {
                if (connectionChangeRequested)
                {
                    Monitor.Wait(lockObj);
                }
                return connection;
            }
            private set { connection = value; }
        }

        public static string ReceiverQueue { get; private set; }

        static RedisManager()
        {
            Connect();            
        }

        private static void Connect()
        {
            var redisServer = DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisServer"];
            var redisPort = DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisPort"];
            ReceiverQueue = string.IsNullOrEmpty(DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"]) ? "NScrapy.Downloader" : DownloaderContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            var options = new ConfigurationOptions()
            {
                EndPoints = { $"{redisServer}:{redisPort}" },
                SyncTimeout = 60000,
                ConnectTimeout = 60000,
            };
            Connection = ConnectionMultiplexer.Connect(options);
        }

        public static void GetLock(string lockKey, string keyToken)
        {
            while (!Connection.GetDatabase().LockTake(lockKey, keyToken,new TimeSpan(TimeSpan.TicksPerSecond)))
            {
                //Sleep until we got the lock
                Thread.Sleep(10);
            }
        }

        public static void ReleaseLock(string lockKey, string keyToken)
        {
            Connection.GetDatabase().LockRelease(lockKey, keyToken);
        }
    }
}
