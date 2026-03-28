using System;
using System.Collections.Generic;
using NScrapy.Infra;

namespace NScrapy
{
    public interface ISpiderBuilder
    {
        ISpiderBuilder Name(string name);
        ISpiderBuilder StartUrl(string url);
        ISpiderBuilder StartUrls(IEnumerable<string> urls);
        ISpiderBuilder OnResponse(Action<IResponse> handler);
        ISpiderBuilder OnItem<T>(Action<T, NScrapy.ISpider> handler) where T : class;
        ISpiderBuilder OnError(Action<Exception, NScrapy.ISpider> handler);
        ISpiderBuilder AddPipeline<T>() where T : IPipeline;
        ISpiderBuilder AddPipeline(object instance);
        ISpiderBuilder AddDownloaderMiddleware<T>() where T : IDownloaderMiddleware;
        ISpiderBuilder AddSpiderMiddleware<T>() where T : ISpiderMiddleware;
        ISpiderBuilder Configure(Action<SpiderOptions> options);
        ISpiderBuilder Distributed(Action<IDistributedBuilder> configure);
        NScrapy.ISpider Build();
        void Run();
    }
}
