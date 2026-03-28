using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NScrapy.Infra;
using NScrapy.Scheduler;
using NScrapy.Engine;

namespace NScrapy
{
    public class SpiderBuilder : ISpiderBuilder
    {
        private string _name;
        private List<string> _startUrls = new List<string>();
        private Action<IResponse> _responseHandler;
        private Dictionary<Type, Action<object, ISpider>> _itemHandlers = new Dictionary<Type, Action<object, ISpider>>();
        private Action<Exception, ISpider> _errorHandler;
        private List<IPipeline> _pipelines = new List<IPipeline>();
        private List<IDownloaderMiddleware> _downloaderMiddlewares = new List<IDownloaderMiddleware>();
        private List<ISpiderMiddleware> _spiderMiddlewares = new List<ISpiderMiddleware>();
        private SpiderOptions _options = new SpiderOptions();
        private bool _isDistributed;
        private DistributedBuilder _distributedBuilder;

        public ISpiderBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public ISpiderBuilder StartUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _startUrls.Add(url);
            }
            return this;
        }

        public ISpiderBuilder StartUrls(IEnumerable<string> urls)
        {
            if (urls != null)
            {
                foreach (var url in urls)
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        _startUrls.Add(url);
                    }
                }
            }
            return this;
        }

        public ISpiderBuilder OnResponse(Action<IResponse> handler)
        {
            _responseHandler = handler;
            return this;
        }

        public ISpiderBuilder OnItem<T>(Action<T, ISpider> handler) where T : class
        {
            if (handler != null)
            {
                _itemHandlers[typeof(T)] = (item, spider) => handler((T)item, spider);
            }
            return this;
        }

        public ISpiderBuilder OnError(Action<Exception, ISpider> handler)
        {
            _errorHandler = handler;
            return this;
        }

        public ISpiderBuilder AddPipeline<T>() where T : IPipeline
        {
            var pipeline = Activator.CreateInstance<T>();
            _pipelines.Add(pipeline);
            return this;
        }

        public ISpiderBuilder AddPipeline(object instance)
        {
            if (instance is IPipeline pipeline)
            {
                _pipelines.Add(pipeline);
            }
            return this;
        }

        public ISpiderBuilder AddDownloaderMiddleware<T>() where T : IDownloaderMiddleware
        {
            var middleware = Activator.CreateInstance<T>();
            _downloaderMiddlewares.Add(middleware);
            return this;
        }

        public ISpiderBuilder AddSpiderMiddleware<T>() where T : ISpiderMiddleware
        {
            var middleware = Activator.CreateInstance<T>();
            _spiderMiddlewares.Add(middleware);
            return this;
        }

        public ISpiderBuilder Configure(Action<SpiderOptions> options)
        {
            options?.Invoke(_options);
            return this;
        }

        public ISpiderBuilder Distributed(Action<IDistributedBuilder> configure)
        {
            _distributedBuilder = new DistributedBuilder();
            configure?.Invoke(_distributedBuilder);
            _isDistributed = true;
            _options.DistributedConfig = _distributedBuilder.GetConfig();
            return this;
        }

        public ISpider Build()
        {
            return new Spider(
                _name,
                _startUrls,
                _responseHandler,
                _itemHandlers,
                _errorHandler,
                _pipelines,
                _downloaderMiddlewares,
                _spiderMiddlewares,
                _options,
                _isDistributed);
        }

        public void Run()
        {
            var spider = Build();
            spider.RunAsync().GetAwaiter().GetResult();
        }
    }
}
