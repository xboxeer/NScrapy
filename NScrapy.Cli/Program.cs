using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using NScrapy.Cli.Commands;

namespace NScrapy.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("nscrapy");
            
            config.AddCommand<NewCommand>("new");
            config.AddCommand<RunCommand>("run");
        });

        return await app.RunAsync(args);
    }
}
