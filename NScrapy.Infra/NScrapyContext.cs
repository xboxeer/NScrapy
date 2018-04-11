using Microsoft.Extensions.Configuration;
using NScrapy.Infra;
using System;
using System.IO;

namespace NScrapy.Infra
{
    public class NScrapyContext
    {
        public IEngine CurrentEngine { get; set; }
        public static NScrapyContext CurrentContext { get; private set; }
        private static NScrapyContext _instance = null;
        public IConfiguration Configuration { get; private set; }
        private NScrapyContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");
            Configuration = builder.Build();
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
      
    }
}
