using System;
using System.ComponentModel;
using System.IO;
using Spectre.Console.Cli;
using NScrapy.Cli.Services;

namespace NScrapy.Cli.Commands;

public class NewSettings : CommandSettings
{
    [CommandArgument(0, "<SpiderName>")]
    [Description("Name of the spider to create")]
    public string SpiderName { get; set; } = string.Empty;

    [CommandOption("-t|--type")]
    [Description("Type of spider template (basic or distributed)")]
    [DefaultValue("basic")]
    public string Type { get; set; } = "basic";

    [CommandOption("-o|--output")]
    [Description("Output directory for the spider project")]
    [DefaultValue(".")]
    public string Output { get; set; } = ".";

    [CommandOption("--force")]
    [Description("Overwrite existing files")]
    [DefaultValue(false)]
    public bool Force { get; set; }
}

public class NewCommand : Command<NewSettings>
{
    public override int Execute(CommandContext context, NewSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.SpiderName))
        {
            Console.WriteLine("Error: Spider name is required.");
            return 1;
        }

        if (!IsValidSpiderName(settings.SpiderName))
        {
            Console.WriteLine("Error: Invalid spider name. Use PascalCase (e.g., MySpider).");
            return 1;
        }

        var templateType = settings.Type.ToLowerInvariant() switch
        {
            "basic" => false,
            "distributed" => true,
            _ => false
        };

        var templateName = templateType ? "distributed" : "basic";

        Console.WriteLine($"Creating {templateName} spider: {settings.SpiderName}");
        
        var outputPath = Path.GetFullPath(settings.Output);
        
        var templateService = new TemplateService();
        templateService.CreateSpiderProject(
            settings.SpiderName,
            outputPath,
            templateType,
            settings.Force
        );

        return 0;
    }

    private bool IsValidSpiderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (char.IsDigit(name[0])) return false;
        
        foreach (var c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }
        return true;
    }
}
