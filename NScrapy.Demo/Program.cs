using NScrapy.Shell;

namespace NScrapy.Demo
{
    /// <summary>
    /// NScrapy 框架入口 — 完全遵循框架约定：
    /// 1. 配置 appsetting.json（Scheduler、Pipeline、SpiderProject）
    /// 2. 调用 NScrapy.GetInstance().Crawl("SpiderName")
    /// 3. 框架自动发现 Spider，自动加载 Pipeline
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("🦊 NScrapy Demo - 链家租房爬虫");
            System.Console.WriteLine("================================\n");

            // 初始化框架并启动爬虫
            // NScrapy.GetInstance() 读取 appsetting.json，
            // SpiderFactory 从 NScrapy.Demo 程序集发现 LianjiaRentSpider，
            // Pipeline 自动加载，所有解析完成后写入 CSV
            NScrapy.Shell.NScrapy.GetInstance().Crawl("LianjiaRentSpider");
        }
    }
}
