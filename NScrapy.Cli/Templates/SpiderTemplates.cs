namespace NScrapy.Cli.Templates;

public static class SpiderTemplates
{
    public static string GetSpiderTemplate(string spiderName)
    {
        return $@"using System;
using System.Collections.Generic;
using NScrapy.Infra;
using NScrapy.Spider;

namespace {spiderName}.Spiders
{{
    public class {spiderName}Spider : Spider.Spider
    {{
        public override void StartRequests()
        {{
            this.AddRequest(""https://example.com"", ResponseHandler);
        }}

        public override void ResponseHandler(IResponse response)
        {{
            var title = response.CssSelector(""title::text"").ExtractFirst();
            Console.WriteLine($""Title: {{title}}"");

            var links = response.CssSelector(""a::attr(href)"").Extract();
            foreach (var link in links)
            {{
                this.AddRequest(link, ResponseHandler);
            }}

            var item = new {spiderName}Item
            {{
                Title = title,
                Url = response.URL
            }};
            this.AddItem(item);
        }}
    }}

    public class {spiderName}Item
    {{
        public string Title {{ get; set; }}
        public string Url {{ get; set; }}
    }}
}}
";
    }

    public static string GetItemTemplate(string spiderName)
    {
        return $@"namespace {spiderName}.Items
{{
    public class {spiderName}Item
    {{
        public string Title {{ get; set; }}
        public string Url {{ get; set; }}
    }}
}}
";
    }

    public static string GetPipelineTemplate(string spiderName)
    {
        return $@"using System;
using NScrapy.Infra;

namespace {spiderName}.Pipelines
{{
    public class {spiderName}Pipeline : IPipeline<Items.{spiderName}Item>
    {{
        public void ProcessItem(Items.{spiderName}Item item, ISpider spider)
        {{
            Console.WriteLine($""[{{spider.Name}}] Item: {{item.Title}} - {{item.Url}}"");
        }}
    }}
}}
";
    }

    public static string GetProgramTemplate(string spiderName, bool isDistributed)
    {
        if (isDistributed)
        {
            return $@"using System;

namespace {spiderName}
{{
    class Program
    {{
        static void Main(string[] args)
        {{
            Console.WriteLine(""Use 'nscrapy run' command to run this spider in distributed mode."");
            Console.WriteLine($""Example: nscrapy run {spiderName} --role spider --distributed --redis localhost:6379"");
        }}
    }}
}}
";
        }
        else
        {
            return $@"using System;
using NScrapy;

namespace {spiderName}
{{
    class Program
    {{
        static void Main(string[] args)
        {{
            Console.WriteLine(""Use 'nscrapy run' command to run this spider."");
            Console.WriteLine($""Example: nscrapy run {spiderName}"");
        }}
    }}
}}
";
        }
    }

    public static string GetProjectFileTemplate(string spiderName, bool isDistributed)
    {
        var coreReference = isDistributed
            ? @"    <ProjectReference Include=""..\NScrapy.Cli\NScrapy.Cli.csproj"" />"
            : @"    <ProjectReference Include=""..\NScrapy\NScrapy.csproj"" />";

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
{coreReference}
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""NScrapy.Infra"" Version=""1.0.0"" />
    <PackageReference Include=""NScrapy.Scheduler"" Version=""1.0.0"" />
    <PackageReference Include=""NScrapy.Spider"" Version=""1.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <None Update=""appsettings.json"">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
";
    }

    public static string GetAppSettingsTemplate(bool isDistributed)
    {
        if (isDistributed)
        {
            return @"{
  ""AppSettings"": {
    ""Scheduler"": {
      ""SchedulerType"": ""NScrapy.Scheduler.RedisExt.RedisScheduler"",
      ""SchedulerTypeAssembly"": ""NScrapy.Scheduler.dll""
    },
    ""SpiderEngine"": {
      ""SpiderEngineName"": ""NScrapy.Engine.NScrapyEngine""
    },
    ""Scheduler.RedisExt"": {
      ""RedisServer"": ""localhost"",
      ""RedisPort"": ""6379"",
      ""ReceiverQueue"": ""nscrapy:requests"",
      ""ResponseQueue"": ""nscrapy:responses""
    },
    ""SpiderProject"": ""${SPIDER_PROJECT_NAME}""
  }
}
";
        }
        else
        {
            return @"{
  ""AppSettings"": {
    ""Scheduler"": {
      ""SchedulerType"": ""NScrapy.Scheduler.InMemoryScheduler""
    },
    ""SpiderEngine"": {
      ""SpiderEngineName"": ""NScrapy.Engine.NScrapyEngine""
    },
    ""SpiderProject"": ""${SPIDER_PROJECT_NAME}""
  }
}
";
        }
    }

    public static string GetDistributedProgramTemplate(string spiderName)
    {
        return $@"using System;
using NScrapy;

namespace {spiderName}
{{
    class Program
    {{
        static void Main(string[] args)
        {{
            NScrapy.CreateSpider(""{spiderName}Spider"")
                .StartUrl(""https://example.com"")
                .OnResponse(r => {{
                    var title = r.CssSelector(""title::text"").ExtractFirst();
                    Console.WriteLine($""Title: {{title}}"");
                }})
                .Run();
        }}
    }}
}}
";
    }
}
