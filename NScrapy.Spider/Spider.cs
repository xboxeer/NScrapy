using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NScrapy.Spider
{
    public abstract class Spider:ISpider
    {
        public List<string> URLs { get; set; }
        public List<ISpiderMiddleware> Middlewares { get ; set; }

        public virtual void StartRequests()
        {
            foreach (var url in this.URLs)
            {
                HttpRequest request = new HttpRequest()
                {
                    URL = url,
                    RequestSpider = this
                };
                Scheduler.Scheduler.SendRequestToReceiver(request);
            }
        }

        public abstract void ResponseHandler(IResponse response);
    }
}
