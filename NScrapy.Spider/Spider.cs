using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NScrapy.Spider
{
    public abstract class Spider
    {
        public List<string> URLs { get; set; }

        public virtual void StartRequests()
        {
            throw new NotImplementedException();
        }

        public virtual IResponse  ResponseHandler(IResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
