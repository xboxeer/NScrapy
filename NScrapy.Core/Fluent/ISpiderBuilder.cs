using System;
using System.Collections.Generic;
using global::NScrapy.Infra;

namespace NScrapy.Core.Fluent
{
    public interface ISpiderBuilder
    {
        ISpiderBuilder Name(string name);
        ISpiderBuilder StartUrl(string url);
        ISpiderBuilder StartUrls(IEnumerable<string> urls);
        ISpiderBuilder OnResponse(Action<IResponse> handler);
        ISpiderBuilder OnItem<T>(Action<T, ISpider> handler) where T : class;
        ISpiderBuilder OnError(Action<Exception, ISpider> handler);
        ISpiderBuilder AddPipeline<T>() where T : IPipeline;
        ISpiderBuilder AddPipeline(object instance);
        ISpiderBuilder AddDownloaderMiddleware<T>() where T : IDownloaderMiddleware;
        ISpiderBuilder AddSpiderMiddleware<T>() where T : ISpiderMiddleware;
        ISpiderBuilder Configure(Action<SpiderOptions> options);
        ISpiderBuilder Distributed(Action<IDistributedBuilder> configure);
        Spider Build();
        void Run();
    }
}
