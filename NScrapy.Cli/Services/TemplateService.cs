using System;
using System.IO;
using NScrapy.Cli.Templates;

namespace NScrapy.Cli.Services;

public class TemplateService
{
    public void CreateSpiderProject(string spiderName, string outputPath, bool isDistributed, bool force)
    {
        var projectPath = Path.Combine(outputPath, spiderName);
        
        if (Directory.Exists(projectPath))
        {
            if (force)
            {
                Console.WriteLine($"Warning: Overwriting existing directory: {projectPath}");
            }
            else
            {
                Console.WriteLine($"Error: Directory already exists: {projectPath}");
                Console.WriteLine("Use --force to overwrite.");
                return;
            }
        }
        else
        {
            Directory.CreateDirectory(projectPath);
        }

        var spiderDir = Path.Combine(projectPath, "Spiders");
        var pipelinesDir = Path.Combine(projectPath, "Pipelines");
        var itemsDir = Path.Combine(projectPath, "Items");

        Directory.CreateDirectory(spiderDir);
        Directory.CreateDirectory(pipelinesDir);
        Directory.CreateDirectory(itemsDir);

        WriteFile(Path.Combine(spiderDir, $"{spiderName}Spider.cs"), 
            SpiderTemplates.GetSpiderTemplate(spiderName));
        
        WriteFile(Path.Combine(itemsDir, $"{spiderName}Item.cs"), 
            SpiderTemplates.GetItemTemplate(spiderName));
        
        WriteFile(Path.Combine(pipelinesDir, $"{spiderName}Pipeline.cs"), 
            SpiderTemplates.GetPipelineTemplate(spiderName));
        
        WriteFile(Path.Combine(projectPath, "Program.cs"), 
            isDistributed 
                ? SpiderTemplates.GetDistributedProgramTemplate(spiderName)
                : SpiderTemplates.GetProgramTemplate(spiderName, isDistributed));
        
        var projectFileContent = SpiderTemplates.GetProjectFileTemplate(spiderName, isDistributed)
            .Replace("${SPIDER_PROJECT_NAME}", $"{spiderName}");
        WriteFile(Path.Combine(projectPath, $"{spiderName}.csproj"), projectFileContent);
        
        var appSettingsContent = SpiderTemplates.GetAppSettingsTemplate(isDistributed)
            .Replace("${SPIDER_PROJECT_NAME}", spiderName);
        WriteFile(Path.Combine(projectPath, "appsettings.json"), appSettingsContent);

        Console.WriteLine($"Created spider project: {projectPath}");
        Console.WriteLine();
        Console.WriteLine("To run the spider:");
        Console.WriteLine($"  cd {projectPath}");
        Console.WriteLine($"  nscrapy run {spiderName}");
        
        if (isDistributed)
        {
            Console.WriteLine();
            Console.WriteLine("For distributed mode:");
            Console.WriteLine($"  nscrapy run {spiderName} --role spider --distributed --redis localhost:6379");
        }
    }

    private void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}
