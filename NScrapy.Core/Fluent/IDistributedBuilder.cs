namespace NScrapy
{
    public interface IDistributedBuilder
    {
        IDistributedBuilder UseRedis(string connectionString);
        IDistributedBuilder ReceiverQueue(string queueName);
        IDistributedBuilder ResponseQueue(string queueName);
    }
}
