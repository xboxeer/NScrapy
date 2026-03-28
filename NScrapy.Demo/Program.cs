using System;
using System.IO;
using NScrapy.Shell;
using NScrapy.Infra;

namespace NScrapy.Demo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🦊 NScrapy Demo - 链家租房爬虫\n");

        // 初始化 NScrapy 框架（InMemory 模式，无需 ZooKeeper/Redis）
        NScrapyContext.GetInstance();
        Console.WriteLine("✅ NScrapy 框架初始化完成 (InMemory 模式)\n");

        // 启动爬虫
        LianjiaHouseSpider.CrawlAsync("bj", 2).Wait();

        Console.WriteLine("\n👋 爬虫运行完毕!");
    }
}
