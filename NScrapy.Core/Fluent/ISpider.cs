using System;
using System.Threading;
using System.Threading.Tasks;

namespace NScrapy
{
    public interface ISpider
    {
        string Name { get; }
        bool IsDistributed { get; }
        void Start();
        void Stop();
        Task RunAsync(CancellationToken cancellationToken = default);
        event EventHandler<Exception> OnError;
    }
}
