using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NScrapy.Spider
{
    public abstract class Spider
    {
        public abstract IResponse StartRequests();
    }
}
