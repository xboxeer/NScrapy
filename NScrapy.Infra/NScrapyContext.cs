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
        public IConfiguration Configuration { get; private set; }
        public ISpider CurrentSpider { get; set; }
        public ILog Log { get; set; }
        private NScrapyContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");
            Configuration = builder.Build();
            Log = log4net.LogManager.GetLogger(this.GetType());
            var logConfig = Properties.Resources.log4net;
            using (var configStream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
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
        
        public  void RefreshConfigFile(string path="")
        {
            var configFile = path;
            if(string.IsNullOrEmpty(configFile))
            {
                configFile = "appsetting.json";
            }
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path);
            Configuration = builder.Build();
        }
        
    }
}
