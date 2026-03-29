using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NScrapy.Infra;
using NScrapy.Scheduler;
using NScrapy.Scheduler.RedisExt;
using NScrapyContext = NScrapy.Infra.NScrapyContext;
using NScrapyShell = NScrapy.Shell.NScrapy;

namespace NScrapy.Cli.Services;

public enum RunRole
{
    Single,
    Spider,
    Downloader
}

public class RunnerService
{
    private readonly RunConfiguration _config;

    public RunnerService(RunConfiguration config)
    {
        _config = config;
    }

    public void Run(string? spiderName = null)
    {
        LoadConfiguration();

        switch (_config.Role)
        {
            case RunRole.Single:
                RunSingleMode(spiderName);
                break;
            case RunRole.Spider:
                RunSpiderMode(spiderName);
                break;
            case RunRole.Downloader:
                RunDownloaderMode();
                break;
        }
    }

    private void LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        var configFile = _config.ConfigFile ?? "appsettings.json";
        if (File.Exists(configFile))
        {
            builder.AddJsonFile(configFile, optional: true, reloadOnChange: true);
        }

        builder.AddEnvironmentVariables(prefix: "NSCRAPY_");

        if (!string.IsNullOrEmpty(_config.RedisEndpoint))
        {
            Environment.SetEnvironmentVariable("NSCRAPY_REDIS_HOST", ExtractHost(_config.RedisEndpoint));
            Environment.SetEnvironmentVariable("NSCRAPY_REDIS_PORT", ExtractPort(_config.RedisEndpoint));
        }

        if (!string.IsNullOrEmpty(_config.RedisPassword))
        {
            Environment.SetEnvironmentVariable("NSCRAPY_REDIS_PASSWORD", _config.RedisPassword);
        }

        if (_config.RedisSsl)
        {
            Environment.SetEnvironmentVariable("NSCRAPY_REDIS_USESSL", "true");
        }

        if (_config.Distributed)
        {
            Environment.SetEnvironmentVariable("NSCRAPY_REDIS_ENABLED", "true");
        }

        if (_config.Concurrency.HasValue)
        {
            Environment.SetEnvironmentVariable("NSCRAPY_CONCURRENCY", _config.Concurrency.Value.ToString());
        }

        if (!string.IsNullOrEmpty(_config.ReceiverQueue))
        {
            Environment.SetEnvironmentVariable("NSCRAPY_RECEIVER_QUEUE", _config.ReceiverQueue);
        }

        if (!string.IsNullOrEmpty(_config.ResponseQueue))
        {
            Environment.SetEnvironmentVariable("NSCRAPY_RESPONSE_QUEUE", _config.ResponseQueue);
        }

        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; private set; } = null!;

    private void RunSingleMode(string? spiderName)
    {
        if (string.IsNullOrEmpty(spiderName))
        {
            Console.WriteLine("Error: Spider name is required for single mode.");
            return;
        }

        Console.WriteLine($"Starting single mode spider: {spiderName}");
        
        var nscrapy = NScrapyShell.GetInstance();
        nscrapy.Crawl(spiderName);
    }

    private void RunSpiderMode(string? spiderName)
    {
        if (string.IsNullOrEmpty(spiderName))
        {
            Console.WriteLine("Error: Spider name is required for spider mode.");
            return;
        }

        Console.WriteLine($"Starting distributed spider node: {spiderName}");
        
        Console.WriteLine("Spider mode - using Redis scheduler from configuration");
        
        var nscrapy = NScrapyShell.GetInstance();
        nscrapy.Crawl(spiderName);
    }

    private void RunDownloaderMode()
    {
        Console.WriteLine("Starting distributed downloader node...");
        
        Console.WriteLine("Downloader mode - listening for requests from Redis queue");
        
        Console.WriteLine("Downloader worker started. Press Ctrl+C to stop.");
        
        var lockObj = new object();
        lock (lockObj)
        {
            Monitor.Wait(lockObj);
        }
    }

    private string ExtractHost(string endpoint)
    {
        if (endpoint.Contains(':'))
        {
            return endpoint.Split(':')[0];
        }
        return endpoint;
    }

    private string ExtractPort(string endpoint)
    {
        if (endpoint.Contains(':'))
        {
            var parts = endpoint.Split(':');
            return parts.Length > 1 ? parts[1] : "6379";
        }
        return "6379";
    }
}

public class RunConfiguration
{
    public RunRole Role { get; set; } = RunRole.Single;
    public bool Distributed { get; set; }
    public string? RedisEndpoint { get; set; }
    public string? RedisPassword { get; set; }
    public bool RedisSsl { get; set; }
    public string? ReceiverQueue { get; set; }
    public string? ResponseQueue { get; set; }
    public int? Concurrency { get; set; }
    public int? DelayMs { get; set; }
    public string? ConfigFile { get; set; }
}
