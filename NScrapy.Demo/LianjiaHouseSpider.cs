using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NScrapy.Downloader;
using NScrapy.Infra;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace NScrapy.Demo;

public class LianjiaHouseSpider
{
    public static async Task CrawlAsync(string city, int maxPages = 3)
    {
        Console.WriteLine($"🦊 链家租房爬虫启动 - 城市: {city}");
        Console.WriteLine(new string('=', 60));

        var spider = new LianjiaHouseSpider();
        await spider.RunAsync(city, maxPages);

        Console.WriteLine($"\n✅ 爬取完成! 共获取 {spider.Results.Count} 条房源");
    }

    public List<HouseInfo> Results { get; } = new List<HouseInfo>();

    private async Task RunAsync(string city, int maxPages)
    {
        // 链家租房列表页 (以北京为例)
        var baseUrl = $"https://{city}.lianjia.com/zufang/";

        for (int page = 1; page <= maxPages; page++)
        {
            var url = page == 1 ? baseUrl : $"{baseUrl}pg{page}/";
            Console.WriteLine($"\n📄 正在抓取第 {page}/{maxPages} 页: {url}");

            try
            {
                var request = new HttpRequest { URL = url };
                var response = await NScrapy.Downloader.Downloader.SendRequestAsync(request);

                var houseInfos = ParseHouseList(response);

                foreach (var house in houseInfos)
                {
                    Console.WriteLine($"  🏠 {house.Title}");
                    Console.WriteLine($"     💰 {house.Price} 元/月 | 📍 {house.District}");
                }

                Results.AddRange(houseInfos);

                // 礼貌延迟，避免被封
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ 抓取失败: {ex.Message}");
            }
        }

        SaveToCsv();
    }

    private List<HouseInfo> ParseHouseList(IResponse response)
    {
        var houses = new List<HouseInfo>();
        var doc = new HtmlDocument();
        doc.LoadHtml(response.ResponsePlanText);

        // 链家当前页面结构: .content__list--item 包含各字段
        var nodes = doc.DocumentNode.QuerySelectorAll(".content__list--item");

        foreach (var node in nodes)
        {
            var titleNode = node.QuerySelector(".content__list--item--title a");
            var priceNode = node.QuerySelector(".content__list--item-price em");
            var districtNode = node.QuerySelector(".content__list--item--des a");
            var areaNode = node.QuerySelector(".content__list--item--des");

            if (titleNode == null) continue;

            // 提取面积（通常在第二个 <a> 标签）
            var areaLinks = areaNode?.QuerySelectorAll("a").ToList();
            var area = areaLinks?.Count > 1 ? areaLinks?[1].InnerText.Trim() : "未知";

            houses.Add(new HouseInfo
            {
                Title = titleNode.InnerText.Trim(),
                Url = "https://bj.lianjia.com" + titleNode.GetAttributeValue("href", ""),
                Price = priceNode?.InnerText.Trim() ?? "未知",
                District = districtNode?.InnerText.Trim() ?? "未知",
                Area = area ?? "未知"
            });
        }

        return houses;
    }

    private void SaveToCsv()
    {
        if (Results.Count == 0) return;

        var path = $"lianjia_houses_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
        writer.WriteLine("标题,价格(元/月),区域,面积,链接");

        foreach (var h in Results)
        {
            writer.WriteLine($"\"{h.Title}\",\"{h.Price}\",\"{h.District}\",\"{h.Area}\",\"{h.Url}\"");
        }

        Console.WriteLine($"\n💾 已保存到 {path}");
    }
}

public class HouseInfo
{
    public string Title { get; set; } = "";
    public string Price { get; set; } = "";
    public string District { get; set; } = "";
    public string Area { get; set; } = "";
    public string Url { get; set; } = "";
}
