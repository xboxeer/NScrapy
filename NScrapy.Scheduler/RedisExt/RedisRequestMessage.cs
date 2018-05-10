using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisRequestMessage
    {
        public string URL { get; set; }
        public string CallbacksFingerprint { get; set; }
    }
}
