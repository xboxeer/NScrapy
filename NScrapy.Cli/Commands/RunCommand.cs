using System;
using System.ComponentModel;
using System.Threading;
using Spectre.Console.Cli;
using NScrapy.Cli.Services;

namespace NScrapy.Cli.Commands;

public class RunSettings : CommandSettings
{
    [CommandArgument(0, "[spider-name]")]
    [Description("Name of the spider to run (required for single and spider modes)")]
    public string? SpiderName { get; set; }

    [CommandOption("--role")]
    [Description("Running role: single, spider, or downloader")]
    [DefaultValue("single")]
    public string Role { get; set; } = "single";

    [CommandOption("--distributed")]
    [Description("Enable distributed mode")]
    [DefaultValue(false)]
    public bool Distributed { get; set; }

    [CommandOption("--redis")]
    [Description("Redis endpoint (host:port)")]
    public string? RedisEndpoint { get; set; }

    [CommandOption("--redis-password")]
    [Description("Redis password")]
    public string? RedisPassword { get; set; }

    [CommandOption("--redis-ssl")]
    [Description("Use SSL for Redis connection")]
    [DefaultValue(false)]
    public bool RedisSsl { get; set; }

    [CommandOption("--receiver-queue")]
    [Description("Redis queue name for requests")]
    public string? ReceiverQueue { get; set; }

    [CommandOption("--response-queue")]
    [Description("Redis queue name for responses")]
    public string? ResponseQueue { get; set; }

    [CommandOption("--concurrency")]
    [Description("Number of concurrent requests")]
    public int? Concurrency { get; set; }

    [CommandOption("--delay")]
    [Description("Delay between requests in milliseconds")]
    public int? DelayMs { get; set; }

    [CommandOption("-c|--config")]
    [Description("Path to configuration file")]
    public string? ConfigFile { get; set; }
}

public class RunCommand : Command<RunSettings>
{
    public override int Execute(CommandContext context, RunSettings settings, CancellationToken cancellationToken)
    {
        var role = settings.Role.ToLowerInvariant() switch
        {
            "single" => RunRole.Single,
            "spider" => RunRole.Spider,
            "downloader" => RunRole.Downloader,
            _ => RunRole.Single
        };

        if (role == RunRole.Single || role == RunRole.Spider)
        {
            if (string.IsNullOrEmpty(settings.SpiderName))
            {
                Console.WriteLine("Error: Spider name is required for single and spider modes.");
                return 1;
            }
        }

        Console.WriteLine("NScrapy CLI v1.0.0");
        Console.WriteLine($"Role: {role}");
        
        if (settings.Distributed)
        {
            Console.WriteLine("Mode: Distributed");
        }
        else
        {
            Console.WriteLine("Mode: Local");
        }

        if (!string.IsNullOrEmpty(settings.RedisEndpoint))
        {
            Console.WriteLine($"Redis: {settings.RedisEndpoint}");
        }

        Console.WriteLine();

        var config = new RunConfiguration
        {
            Role = role,
            Distributed = settings.Distributed,
            RedisEndpoint = settings.RedisEndpoint,
            RedisPassword = settings.RedisPassword,
            RedisSsl = settings.RedisSsl,
            ReceiverQueue = settings.ReceiverQueue,
            ResponseQueue = settings.ResponseQueue,
            Concurrency = settings.Concurrency,
            DelayMs = settings.DelayMs,
            ConfigFile = settings.ConfigFile
        };

        try
        {
            var runner = new RunnerService(config);
            runner.Run(settings.SpiderName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
