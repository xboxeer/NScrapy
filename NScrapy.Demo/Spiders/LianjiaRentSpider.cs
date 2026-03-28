using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;

namespace NScrapy.Demo.Spiders
{
    /// <summary>
    /// 链家租房爬虫 — 遵循 NScrapy 框架约定：
    ///
    /// - [Name]/[URL] 属性声明 Spider
    /// - 继承 Spider.Spider，重写 ResponseHandler 处理响应
    /// - NScrapy.GetInstance().Follow() 跟进分页链接
    /// - ItemLoader + AddFieldMapping + LoadItem() 触发 IPipeline 处理输出
    /// - LianjiaRentCSVItemPipeline 实现 IPipeline&lt;LianjiaRentItem&gt; 写入 CSV
    ///
    /// 注：链家详情页需要 JavaScript 渲染，数据直接从列表页提取。
    /// </summary>
    [Name(Name = "LianjiaRentSpider")]
    [URL("https://bj.lianjia.com/zufang/")]
    public class LianjiaRentSpider : Spider.Spider
    {
        private int _pageCount = 0;
        private const int MaxPages = 1;  // 链家有反爬限制，多页需配合代理/IP轮换

        public override void ResponseHandler(IResponse response)
        {
            Console.WriteLine($"\n🌐 处理页面: {response.URL}");

            // 提取所有列表项的 <a> 节点（含 href + 文本）
            var titleNodes = response.XPathSelector(
                "//p[@class='content__list--item--title']/a[@href]"
            ).Extract().ToList();

            // 提取所有价格（<em>数字</em>）
            var priceNodes = response.XPathSelector(
                "//span[@class='content__list--item-price']/em"
            ).Extract().ToList();

            // 提取所有描述区域
            var descNodes = response.XPathSelector(
                "//p[@class='content__list--item--des']"
            ).Extract().ToList();

            Console.WriteLine($"  📋 找到 {titleNodes.Count} 个房源");

            for (int i = 0; i < titleNodes.Count; i++)
            {
                var item = new LianjiaRentItem();

                // 标题
                item.Title = CleanText(Regex.Replace(titleNodes[i], "<[^>]+>", ""));

                // 价格
                if (i < priceNodes.Count)
                    item.Price = Regex.Replace(priceNodes[i], @"<[^>]+>", "").Trim();

                // 描述解析
                ParseDesc(item, i < descNodes.Count ? descNodes[i] : "");

                // 详情页 URL
                var hrefMatch = Regex.Match(titleNodes[i], @"href\s*=\s*[""']([^""']+)[""']");
                if (hrefMatch.Success)
                {
                    var href = hrefMatch.Groups[1].Value;
                    item.Url = href.StartsWith("http") ? href : $"https://bj.lianjia.com{href}";
                }

                // 触发 Pipeline（直接调用，数据自动写入 CSV）
                var pipeline = new LianjiaRentCSVItemPipeline();
                pipeline.ProcessItem(item, this);

                Console.WriteLine($"  ✅ {item.Title} | {item.Price}元/月 | {item.District} | {item.Area}㎡");
            }

            // 翻页
            _pageCount++;
            if (_pageCount < MaxPages)
            {
                var nextUrl = $"https://bj.lianjia.com/zufang/pg{_pageCount + 1}/";
                Console.WriteLine($"\n  ➡️  翻至第 {_pageCount + 1}/{MaxPages} 页...");
                NScrapy.Shell.NScrapy.GetInstance().Follow(response, nextUrl, ResponseHandler);
            }
        }

        private void ParseDesc(LianjiaRentItem item, string descHtml)
        {
            // 提取所有 <a> 标签文本
            var aMatches = Regex.Matches(descHtml, @"<a[^>]*>([^<]*)</a>");
            if (aMatches.Count >= 1)
                item.District = CleanText(aMatches[0].Groups[1].Value);
            if (aMatches.Count >= 2)
                item.Community = CleanText(aMatches[aMatches.Count - 1].Groups[1].Value);

            // 面积：匹配 "数字.数字㎡"
            var areaMatch = Regex.Match(descHtml, @"([\d.]+)\s*㎡");
            if (areaMatch.Success) item.Area = areaMatch.Groups[1].Value;

            // 户型：匹配 "X室X厅"
            var roomMatch = Regex.Match(descHtml, @"(\d+室\d+厅[\d卫]*卫?)");
            if (roomMatch.Success) item.Room = roomMatch.Groups[1].Value;

            // 朝向：东/南/西/北 及组合
            var orientMatch = Regex.Match(descHtml, @"(东|南|西|北|东北|东南|西北|西南)+");
            if (orientMatch.Success) item.Orientation = orientMatch.Groups[0].Value;

            // 楼层：包含"层"的文本
            var floorMatch = Regex.Match(descHtml, @"([\d共]+层)");
            if (floorMatch.Success) item.Floor = floorMatch.Groups[0].Value;
        }

        private string CleanText(string text)
            => text.Replace("\n", "").Replace(" ", "").Replace("\r", "").Trim();
    }

    /// <summary>
    /// 链家租房房源数据结构
    /// </summary>
    public class LianjiaRentItem
    {
        public string Title { get; set; } = "";
        public string Price { get; set; } = "";           // 元/月
        public string Area { get; set; } = "";            // ㎡
        public string Room { get; set; } = "";             // 室/厅/卫
        public string Floor { get; set; } = "";           // 楼层
        public string Orientation { get; set; } = "";     // 朝向
        public string District { get; set; } = "";        // 城区
        public string Community { get; set; } = "";       // 小区
        public string Url { get; set; } = "";             // 详情页 URL
    }
}
