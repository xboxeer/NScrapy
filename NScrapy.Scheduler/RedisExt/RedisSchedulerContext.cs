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
        private ConnectionMultiplexer connection;
        private bool connectionChangeRequested = false;
        private object instanceLockObj = new object();

        public string RedisServer { get; private set; }
        public string RedisPort { get; private set; }
        public string ReceiverQueue { get; private set; }
        public string ResponseQueue { get; private set; }
        public ConnectionMultiplexer Connection
        {
            get
            {
                //While connection is in change, block the thread of getting a connection
                if(connectionChangeRequested)
                {
                    Monitor.Wait(instanceLockObj);
                }
                return this.connection;
            }
            private set
            {
                connection = value;
            }
        }
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
            Connect();
            NScrapyContext.CurrentContext.ConfigRefreshed += CurrentContext_ConfigRefreshed;
        }

        private void CurrentContext_ConfigRefreshed(object arg1, EventArgs arg2)
        {
            //While connect after a config refresh, mark the connectionChangeRequested as true so that block all the request of gettting Connection
            this.connectionChangeRequested = true;
            Monitor.Enter(instanceLockObj);
            this.Connect();
            Monitor.Pulse(instanceLockObj);
            this.connectionChangeRequested = false;
        }

        private void Connect()
        {
            RedisServer = NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisServer"];
            RedisPort = NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:RedisPort"];
            ReceiverQueue = string.IsNullOrEmpty(NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"]) ? "NScrapy.Downloader" : NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ReceiverQueue"];
            ResponseQueue = string.IsNullOrEmpty(NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ResponseQueue"]) ? "NScrapy.ResponseQueue" : NScrapyContext.CurrentContext.CurrentConfig["AppSettings:Scheduler.RedisExt:ResponseQueue"];
            ConfigurationOptions options = new ConfigurationOptions()
            {
                EndPoints = { $"{RedisServer}:{RedisPort}" },
                SyncTimeout = 10000 * 10//10 seconds till timeout
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