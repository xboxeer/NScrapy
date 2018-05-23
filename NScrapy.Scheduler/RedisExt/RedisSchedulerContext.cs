using NScrapy.Infra;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NScrapy.Scheduler.RedisExt
{
    internal class RedisSchedulerContext
    {
        public string RedisServer { get; private set; }
        public string RedisPort { get; private set; }
        public string ReceiverQueue { get; private set; }
        public string ResponseQueue { get; private set; }
        public ConnectionMultiplexer Connection { get; private set; }
        private static RedisSchedulerContext instance = null;
        private static object lockObj = new object();

        public static RedisSchedulerContext Current
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new RedisSchedulerContext();
                    }
                }
                return instance;
            }
        }

        private RedisSchedulerContext()
        {
            RedisServer = NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:RedisServer"];
            RedisPort = NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:RedisPort"];
            ReceiverQueue = string.IsNullOrEmpty(NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:ReceiverQueue"]) ? "NScrapy.Downloader" : NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            ResponseQueue = string.IsNullOrEmpty(NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:ResponseQueue"]) ? "NScrapy.ResponseQueue" : NScrapyContext.CurrentContext.Configuration["AppSettings:Scheduler.RedisExt:ResponseQueue"];
            ConfigurationOptions options = new ConfigurationOptions()
            {
                EndPoints = { $"{RedisServer}:{RedisPort}" }
            };
            Connection = ConnectionMultiplexer.Connect(options);
        }

        public void GetLock(string lockKey, string keyToken)
        {
            while (!Connection.GetDatabase().LockTake(lockKey, keyToken, new TimeSpan(TimeSpan.TicksPerSecond)))
            {
                //Sleep until we got the lock
                Thread.Sleep(10);
            }
        }

        public void ReleaseLock(string lockKey, string keyToken)
        {
            try
            {
                Connection.GetDatabase().LockRelease(lockKey, keyToken);
            }
            catch(Exception ex)
            {

            }
        }
    }
}