using System.Collections.Generic;

namespace NScrapy
{
    public class SpiderOptions
    {
        public int Concurrency { get; set; } = 10;
        public int DelayMs { get; set; } = 0;
        public ICollection<string> UserAgents { get; set; } = new List<string>();
        public int MaxRetries { get; set; } = 3;
        public int TimeoutMs { get; set; } = 30000;
        public string SchedulerType { get; set; } = "NScrapy.Scheduler.InMemoryScheduler";

        // Distributed configuration
        internal DistributedConfig DistributedConfig { get; set; }
    }

    public class DistributedConfig
    {
        public string RedisConnectionString { get; set; } = "localhost:6379";
        public string ReceiverQueue { get; set; } = "nscrapy:requests";
        public string ResponseQueue { get; set; } = "nscrapy:responses";
    }
}
