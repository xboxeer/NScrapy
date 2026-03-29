namespace NScrapy
{
    public class DistributedBuilder : IDistributedBuilder
    {
        private readonly DistributedConfig _config = new DistributedConfig();

        public IDistributedBuilder UseRedis(string connectionString)
        {
            _config.RedisConnectionString = connectionString;
            return this;
        }

        public IDistributedBuilder ReceiverQueue(string queueName)
        {
            _config.ReceiverQueue = queueName;
            return this;
        }

        public IDistributedBuilder ResponseQueue(string queueName)
        {
            _config.ResponseQueue = queueName;
            return this;
        }

        internal DistributedConfig GetConfig() => _config;
    }
}
