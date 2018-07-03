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

        public int DownloaderCapbility
        {
            get;private set;
        }
        
        public int RunningDownloader
        {
            //There is no need to lock the set of RunningDownloader even there will be multiple Downloader increase/decrease this number
            //beacuse it is not a critical value to the framework, only for the purpose of letting other knows how many Downloader is runing
            get; internal set;
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
                if (value == DownloaderRunningMode.InMemory && NScrapyContext.CurrentContext != null)
                {
                    this.config = NScrapyContext.CurrentContext.CurrentConfig;
                    this.log = NScrapyContext.CurrentContext.Log;
                }
                else
                {
                    var builder = new ConfigurationBuilder();
                    if (this.ConfigProvider==null)
                    {                        
                        builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");                        
                    }
                    else
                    {
                        builder.AddJsonFile(this.ConfigProvider.GetConfigFilePath());
                    }
                    this.config = builder.Build();
                    log = log4net.LogManager.GetLogger(this.GetType());
                    using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")))
                    {
                        XmlConfigurator.Configure(Log.Logger.Repository, fs);
                    }
                }
            }
        }

        public IConfigProvider ConfigProvider { get; set; }

        public static DownloaderContext CurrentContext
        {
            get
            {
                lock(lockObj)
                {
                    if(current==null)
                    {
                        current = new DownloaderContext();
                        current.DownloaderCapbility= int.Parse(CurrentContext.CurrentConfig["AppSettings:DownloaderPoolCapbility"] ?? "4");
                        current.RunningDownloader = 0;
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
