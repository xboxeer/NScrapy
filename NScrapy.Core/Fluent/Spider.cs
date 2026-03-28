using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScrapy.Infra;
using NScrapy.Scheduler;
using NScrapy.Scheduler.RedisExt;
using NScrapy.Engine;

namespace NScrapy
{
    public class Spider : NScrapy.Spider.Spider, ISpider
    {
        private readonly string _name;
        private readonly List<string> _startUrls;
        private readonly Action<IResponse> _responseHandler;
        private readonly Dictionary<Type, Action<object, ISpider>> _itemHandlers;
        private readonly Action<Exception, ISpider> _errorHandler;
        private readonly List<IPipeline> _pipelines;
        private readonly List<IDownloaderMiddleware> _downloaderMiddlewares;
        private readonly List<ISpiderMiddleware> _spiderMiddlewares;
        private readonly SpiderOptions _options;
        private readonly bool _isDistributed;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public string Name => _name;
        public bool IsDistributed => _isDistributed;

        public event EventHandler<Exception> OnError;

        internal Spider(
            string name,
            List<string> startUrls,
            Action<IResponse> responseHandler,
            Dictionary<Type, Action<object, ISpider>> itemHandlers,
            Action<Exception, ISpider> errorHandler,
            List<IPipeline> pipelines,
            List<IDownloaderMiddleware> downloaderMiddlewares,
            List<ISpiderMiddleware> spiderMiddlewares,
            SpiderOptions options,
            bool isDistributed)
        {
            _name = name;
            _startUrls = startUrls ?? new List<string>();
            _responseHandler = responseHandler;
            _itemHandlers = itemHandlers ?? new Dictionary<Type, Action<object, ISpider>>();
            _errorHandler = errorHandler;
            _pipelines = pipelines ?? new List<IPipeline>();
            _downloaderMiddlewares = downloaderMiddlewares ?? new List<IDownloaderMiddleware>();
            _spiderMiddlewares = spiderMiddlewares ?? new List<ISpiderMiddleware>();
            _options = options ?? new SpiderOptions();
            _isDistributed = isDistributed;
        }

        public void Start()
        {
            // Initialize context with spider
            var context = NScrapyContext.GetInstance();
            context.CurrentSpider = this;

            // Set up scheduler based on configuration
            if (_isDistributed && _options.DistributedConfig != null)
            {
                SetupDistributedScheduler();
            }
            else
            {
                SetupInMemoryScheduler();
            }

            // Set up engine
            context.CurrentEngine = new NScrapyEngine();

            // Start the spider
            _cts = new CancellationTokenSource();
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            Start();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts?.Token ?? CancellationToken.None);

            try
            {
                await Task.Run(() =>
                {
                    // Send start URLs to scheduler
                    foreach (var url in _startUrls)
                    {
                        var request = new HttpRequest
                        {
                            URL = url,
                            RequestSpider = this,
                            Callback = HandleResponse
                        };
                        NScrapyContext.CurrentContext.CurrentScheduler.SendRequestToReceiver(request);
                    }

                    // Wait for completion or cancellation
                    WaitForCompletion(linkedCts.Token);
                }, linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                _errorHandler?.Invoke(ex, this);
            }
        }

        private void WaitForCompletion(CancellationToken ct)
        {
            var scheduler = NScrapyContext.CurrentContext.CurrentScheduler;

            while (!ct.IsCancellationRequested && _isRunning)
            {
                if (scheduler is InMemoryScheduler)
                {
                    var noMoreItems = RequestReceiver.RequestQueue.Count == 0 &&
                                      ResponseDistributer.ResponseQueue.Count == 0 &&
                                      Downloader.Downloader.RunningDownloader == 0;

                    if (noMoreItems)
                    {
                        Thread.Sleep(1000); // Wait a bit more to ensure everything is processed
                        if (RequestReceiver.RequestQueue.Count == 0 &&
                            ResponseDistributer.ResponseQueue.Count == 0 &&
                            Downloader.Downloader.RunningDownloader == 0)
                        {
                            break; // All done
                        }
                    }
                }

                Thread.Sleep(500);
            }
        }

        private void HandleResponse(IResponse response)
        {
            try
            {
                // Run spider middlewares pre-response
                foreach (var middleware in _spiderMiddlewares)
                {
                    middleware.PreResponse(response);
                }

                // Call the user's response handler
                _responseHandler?.Invoke(response);

                // Run spider middlewares post-response
                foreach (var middleware in _spiderMiddlewares)
                {
                    middleware.PostReponse(response);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                _errorHandler?.Invoke(ex, this);
            }
        }

        private void SetupInMemoryScheduler()
        {
            var scheduler = new InMemoryScheduler();
            var context = NScrapyContext.GetInstance();
            context.CurrentScheduler = scheduler;
            context.UrlFilter = scheduler.UrlFilter;
        }

        private void SetupDistributedScheduler()
        {
            // Configure Redis scheduler context before creating RedisScheduler
            var config = _options.DistributedConfig;
            ConfigureRedisSchedulerContext(config);

            var scheduler = new RedisScheduler();
            var context = NScrapyContext.GetInstance();
            context.CurrentScheduler = scheduler;
            context.UrlFilter = scheduler.UrlFilter;
        }

        private void ConfigureRedisSchedulerContext(DistributedConfig config)
        {
            // Parse connection string to get server and port
            var parts = config.RedisConnectionString.Split(':');
            var server = parts.Length > 0 ? parts[0] : "localhost";
            var port = parts.Length > 1 ? parts[1] : "6379";

            // Use reflection to set the private properties on RedisSchedulerContext
            var redisContextType = typeof(RedisSchedulerContext);
            var instance = RedisSchedulerContext.Current;

            // Set RedisServer
            var serverProp = redisContextType.GetProperty("RedisServer");
            if (serverProp != null && serverProp.CanWrite)
            {
                serverProp.SetValue(instance, server);
            }

            // Set RedisPort
            var portProp = redisContextType.GetProperty("RedisPort");
            if (portProp != null && portProp.CanWrite)
            {
                portProp.SetValue(instance, port);
            }

            // Set ReceiverQueue
            var receiverQueueProp = redisContextType.GetProperty("ReceiverQueue");
            if (receiverQueueProp != null && receiverQueueProp.CanWrite)
            {
                receiverQueueProp.SetValue(instance, config.ReceiverQueue);
            }

            // Set ResponseQueue
            var responseQueueProp = redisContextType.GetProperty("ResponseQueue");
            if (responseQueueProp != null && responseQueueProp.CanWrite)
            {
                responseQueueProp.SetValue(instance, config.ResponseQueue);
            }

            // Reconnect with new settings
            var connectMethod = redisContextType.GetMethod("Connect", BindingFlags.NonPublic | BindingFlags.Instance);
            connectMethod?.Invoke(instance, null);
        }

        internal void ProcessItem<T>(T item) where T : class
        {
            if (_itemHandlers.TryGetValue(typeof(T), out var handler))
            {
                handler(item, this);
            }

            // Run through pipelines
            foreach (var pipeline in _pipelines)
            {
                if (pipeline is IPipeline<T> typedPipeline)
                {
                    typedPipeline.ProcessItem(item, this);
                }
            }
        }
    }
}
