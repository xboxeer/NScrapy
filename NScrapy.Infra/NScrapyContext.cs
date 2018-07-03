using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace NScrapy.Infra
{
    public class NScrapyContext
    {
        public IEngine CurrentEngine { get; set; }
        public static NScrapyContext CurrentContext { get; private set; }
        private static NScrapyContext _instance = null;        

        private int visitedUrl = 0;
        private IConfigProvider configProvider = null;

        public IConfiguration CurrentConfig { get; private set; }
        public ISpider CurrentSpider { get; set; }
        public ILog Log { get; set; }
        public IUrlFilter UrlFilter { get; set; }
        public IScheduler CurrentScheduler { get; set; }

        public IConfigProvider ConfigProvider { get
            {
                return this.configProvider;
            }
            set
            {
                this.configProvider = value;
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(value.GetConfigFilePath());
                CurrentConfig = builder.Build();
            }
        }

        public event Action<object, EventArgs> ConfigRefreshed;
        
        public int VisitedUrl { get {return this.visitedUrl; } set
            {
                lock(this.GetType())
                {
                    visitedUrl = value;
                }
            }
        }

        private NScrapyContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");            
            CurrentConfig = builder.Build();
            
            Log = log4net.LogManager.GetLogger(this.GetType());
            var logConfig = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "log4net.config")); //Properties.Resources.log4net;
            using (var configStream = new MemoryStream(logConfig))
            {
                XmlConfigurator.Configure(Log.Logger.Repository, configStream);
            }            
        }        

        public static NScrapyContext GetInstance()
        {
            if(_instance==null)
            {
                _instance = new NScrapyContext();
            }
            CurrentContext = _instance;
            return _instance;
        }
        
        public void RefreshConfigFile(string path="")
        {
            var configFile = path;
            if(string.IsNullOrEmpty(configFile))
            {
                configFile = "appsetting.json";
            }
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path);
            CurrentConfig = builder.Build();
            if(ConfigRefreshed!=null)
            {
                ConfigRefreshed(this, new EventArgs());
            }
        }
        
    }
}
