using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisResponseMessage
    {
        public string URL { get; set; }
        public string CallbacksFingerprint { get; set; }
        public string Payload { get; set; }
    }
}
