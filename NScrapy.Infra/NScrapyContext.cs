using NScrapy.Infra;
using System;

namespace NScrapy.Infra
{
    public class NScrapyContext
    {
        public IEngine CurrentEngine { get; set; }
        public static NScrapyContext CurrentContext { get; private set; }
        private static NScrapyContext _instance = null;
        private NScrapyContext()
        {
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
