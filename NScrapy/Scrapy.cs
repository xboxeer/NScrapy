using System;
using System.Collections.Generic;
using System.Text;
using NScrapy.Infra;

namespace NScrapy.Shell
{
    public class Scrapy
    {
        private NScrapyContext _context = null;

        

        public NScrapyContext Context
        {
            get
            {
                return this._context;
            }
        }

        private Scrapy()
        {
            this._context = new NScrapyContext();
        }

        public static Scrapy GetInstance()
        {
            return new Scrapy();
        }

        public IResponse Crawl(string spiderName)
        {
            throw new NotImplementedException();
        }
    }
}
