using System;
using System.Collections.Generic;
using System.Text;
using NScrapy.Infra;

namespace NScrapy.Shell
{
    public class Shell
    {
        private NScrapyContext _context = null;

        

        public NScrapyContext Context
        {
            get
            {
                return this._context;
            }
        }

        private Shell()
        {
            this._context = new NScrapyContext();
        }

        public static Shell GetInstance()
        {
            return new Shell();
        }

        public IResponse Crawl(string spiderName)
        {
            throw new NotImplementedException();
        }
    }
}
