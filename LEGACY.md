# NScrapy Legacy API / NScrapy 旧版 API

> ⚠️ 以下为旧版类继承 API，已不再推荐使用。新项目请使用 [Fluent API](../README.md#编程指南--programming-guide)。
> The following is the legacy class-inheritance API, no longer recommended for new projects. Please use the [Fluent API](../README.md#编程指南--programming-guide) for new projects.

---

## NScrapy Sample Code / NScrapy 示例代码

Below is a sample of NScrapy, the sample will visit Liepin, which is a Recruit web site.
Based on the seed URL defined in the `[URL]` attribute, NScrapy will visit each Position information in detail page (the `ParseItem` method), and visit the next page automatically (the `VisitPage` method).
It is not necessary for the Spider writer to know how the Spiders distributed in different machine/process communicate with each other, and how the Downloader process get the URL that need to be downloaded. Just tell NScrapy the seed URL, inherit `Spider.Spider` class and write some callback, NScrapy will take the rest of the work.
NScrapy supports different kind of extension, including add your own DownloaderMiddleware, config HTTP header, user agent pool.

如下是一段简单的 NScrapy 爬虫，该爬虫会抓取猎聘网上所有 PHP 的职位信息并做相应的输出。
基于定义在 `[URL]` attribute 中的种子 URL，NScrapy 会访问每一个职位信息的详细信息页面（`ParseItem` method），并且自动爬取下一页信息（`VisitPage` method）。
爬虫作者不需要关心如何管理分布式爬虫之间如何互相通信，下载器如何获取待下载队列，下载器池是如何维护的，仅仅需要告诉 NScrapy 一个种子链接，继承 `Spider.Spider` 类，并完成默认回调函数就可以爬取信息。
NScrapy 支持丰富的自定义扩展，包括在配置文件 `appsetting.json` 中加入 `DownloaderMiddware`、配置 Http 请求头、构造 User Agent pool 等。

### Usage / 使用方法

```csharp
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NScrapy.Project
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init shell of NScrapy, which will init the context of NScrapy
            var shell = NScrapy.Shell.NScrapy.GetInstance();
            // Specify the Spider that you want to start
            shell.Crawl("JobSpider");
            return;
        }
    }

    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?industries=&dqs=&salary=&jobKind=&pubTime=30&compkind=&compscale=&industryType=&searchType=1&clean_condition=&isAnalysis=&init=1&sortFlag=15&flushckid=0&fromSearchBtn=1&headckid=bb314f611fde073c&d_headId=4b294eff4ad202db83d4ed085fcbf94b&d_ckId=01fb643c53d14dd44d7991e27c98c51b&d_sfrom=search_prime&d_curPage=0&d_pageSize=40&siTag=k_cloHQj_hyIn0SLM9IfRg~UoKQA1_uiNxxEb8RglVcHg&key=php")]
    public class JobSpider : Spider.Spider
    {
        private string startingTime = DateTime.Now.ToString("yyyyMMddhhmm");

        public JobSpider()
        {
        }

        // 爬取种子链接 / Crawl seed URL
        public override void ResponseHandler(IResponse response)
        {
            var httpResponse = response as HttpResponse;
            var returnValue = response.CssSelector(".job-info h3 a::attr(href)");
            var pages = response.CssSelector(".pagerbar a::attr(href)").Extract();

            foreach (var page in pages)
            {
                if (!page.Contains("javascript"))
                {
                    NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, page, VisitPage);
                }
            }
            VisitPage(returnValue);
        }

        // 翻页 / Visit next page
        private void VisitPage(IResponse returnValue)
        {
            var hrefs = returnValue.CssSelector(".job-info h3 a::attr(href)").Extract();

            foreach (var href in hrefs)
            {
                // Use ItemLoader / 使用 ItemLoader
                NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, href, ParseItem);
            }

            var pages = returnValue.CssSelector(".pagerbar a::attr(href)").Extract();
            foreach (var page in pages)
            {
                if (!page.Contains("javascript"))
                {
                    NScrapy.Shell.NScrapy.GetInstance().Follow(returnValue, page, VisitPage);
                }
            }
        }

        // 在具体岗位的招聘页面上获取信息 / Get info from job detail page
        public void ParseItem(IResponse response)
        {
            // Add Field Mapping to the HTML Dom element / 添加字段映射到 HTML DOM 元素
            var itemLoader = new ItemLoader<JobItem>(response);
            itemLoader.AddFieldMapping("Title", "css:.title-info h1::attr(text)");
            itemLoader.AddFieldMapping("Title", "css:.job-title h1::attr(text)");

            itemLoader.AddFieldMapping("Firm", "css:.title-info h3 a::attr(text)");
            itemLoader.AddFieldMapping("Firm", "css:.title-info h3::attr(text)");
            itemLoader.AddFieldMapping("Firm", "css:.title-info h3");
            itemLoader.AddFieldMapping("Firm", "css:.job-title h2::attr(text)");

            itemLoader.AddFieldMapping("Salary", "css:.job-main-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-main-title strong::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title p::attr(text)");
            itemLoader.AddFieldMapping("Salary", "css:.job-item-title");

            itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(title)");
            itemLoader.AddFieldMapping("Time", "css:.job-title-left time::attr(text)");

            var item = itemLoader.LoadItem();

            // 在示例中简单输出职位公司信息到控制台，你可以将信息输出到任何其他地方
            // Simple write the Position Firm information at the console,
            // you can write the information to anywhere else
            Console.WriteLine(item.Firm);
        }
    }

    public class JobItem
    {
        public string Firm { get; set; }
        public string Title { get; set; }
        public string Salary { get; set; }
        public string Time { get; set; }
    }
}
```

---

## 分布式运行 / Distributed NScrapy

### 修改 Spider 项目中的 appsettings.json / Modify appsettings.json in Spider Project

```json
{
  "Scheduler": {
    "SchedulerType": "NScrapy.Scheduler.RedisExt.RedisScheduler"
  },
  "Scheduler.RedisExt": {
    "RedisServer": "192.168.0.106",
    "RedisPort": "6379",
    "ReceiverQueue": "NScrapy.Downloader",
    "ResponseQueue": "NScrapy.ResponseQueue"
  }
}
```

### 修改 Downloader 项目中的 appsettings.json / Modify appsettings.json in Downloader Project

```json
{
  "Scheduler": {
    "SchedulerType": "NScrapy.Scheduler.RedisExt.RedisScheduler"
  },
  "Scheduler.RedisExt": {
    "RedisServer": "192.168.0.106",
    "RedisPort": "6379",
    "ReceiverQueue": "NScrapy.Downloader",
    "ResponseQueue": "NScrapy.ResponseQueue"
  }
}
```

### 单独运行 DownloaderShell / Run DownloaderShell Individually

```bash
dotnet /path/to/NScrapy.DownloaderShell.dll
```

### 状态更新中间件 / Status Updater Middleware

如果需要将 Downloader 状态更新到 Redis，可以添加下面的中间件到 appsettings.json：
If you want to update Downloader status to Redis, add the below middleware to appsettings.json:

```json
"DownloaderMiddlewares": [
  { "Middleware": "NScrapy.DownloaderShell.StatusUpdaterMiddleware" }
]
```

> 💡 [NScrapyWebConsole](https://github.com/xboxeer/NScrapyWebConsole) 会从 Redis 中读取 Downloader 状态数据。
> [NScrapyWebConsole](https://github.com/xboxeer/NScrapyWebConsole) will read Downloader status from Redis.

---

### MongoDB Pipeline（旧版 / Legacy）

如果需要将抓取到的内容添加到 MongoDB 中，可以创建如下 PipelineItem：
If you want to add the data that you captured to a MongoDB, you can add below PipelineItem:

```csharp
public class MongoItemPipeline : IPipeline<JobItem>
{
    private MongoClient client = new MongoClient("mongodb://localhost:27017");

    public async void ProcessItem(JobItem item, ISpider spider)
    {
        var db = client.GetDatabase("NScrapy");
        var collection = db.GetCollection<JobItem>("JobItem");
        await collection.InsertOneAsync(item);
    }
}
```

添加到 `appsettings.json`：
Add the Pipeline to your project's `appsettings.json`:

```json
"Pipelines": [
  { "Pipeline": "NScrapy.Project.MongoItemPipeline" }
]
```

---

### CSV Pipeline（旧版 / Legacy）

如果想要存储到 CSV 文件中，也可以添加 CSV pipeline：
You can also save your data in CSV by adding CSV pipeline:

```csharp
public class CSVItemPipeline : IPipeline<JobItem>
{
    private string startTime = DateTime.Now.ToString("yyyyMMddhhmm");

    public void ProcessItem(JobItem item, ISpider spider)
    {
        var info = $"{item.Title},{item.Firm},{item.SalaryFrom},{item.SalaryTo},{item.Location},{item.Time},{item.URL},{System.Environment.NewLine}";
        Console.WriteLine(info);
        File.AppendAllText($"output-{startTime}.csv", info, Encoding.UTF8);
    }
}
```

添加到 `appsettings.json`：
Add the pipeline item in `appsettings.json`:

```json
"Pipelines": [
  { "Pipeline": "NScrapy.Project.MongoItemPipeline" },
  { "Pipeline": "NScrapy.Project.CSVItemPipeline" }
]
```
