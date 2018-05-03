using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using NScrapy.Infra;

namespace NScrapy.Downloader
{
    public class DownloaderContext
    {
        private static DownloaderContext current = null;

        private static object lockObj = new object();

        private IConfiguration config = null;

        private DownloaderRunningMode runningMode;

        private log4net.ILog log = null;

        public IConfiguration CurrentConfig {
            get
            {
                return config;
            }
        }       
        
        public ILog Log
        {
            get
            {
                return this.log;
            }
        }

        public DownloaderRunningMode RunningMode
        {
            get
            {
                return runningMode;
            }
            set
            {
                runningMode = value;
                if (value== DownloaderRunningMode.InMemory)
                {
                    this.config = NScrapyContext.CurrentContext.Configuration;
                    this.log = NScrapyContext.CurrentContext.Log;
                }
                else
                {
                    if(config==NScrapyContext.CurrentContext.Configuration)
                    {
                        var builder = new ConfigurationBuilder();
                        builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");
                        config = builder.Build();
                        log = log4net.LogManager.GetLogger(this.GetType());
                        using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")))
                        {
                            XmlConfigurator.Configure(Log.Logger.Repository, fs);
                        }
                    }
                }
            }
        }

        public static DownloaderContext Context
        {
            get
            {
                lock(lockObj)
                {
                    if(current==null)
                    {
                        current = new DownloaderContext();
                    }
                    return current;
                }
            }
        }

        private DownloaderContext()
        {
            RunningMode = DownloaderRunningMode.InMemory;
        }
    }

    public enum DownloaderRunningMode
    {
        InMemory,
        Distributed
    }
}
