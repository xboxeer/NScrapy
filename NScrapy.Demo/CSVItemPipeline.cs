using System;
using System.IO;
using NScrapy.Infra;
using NScrapy.Demo.Spiders;

namespace NScrapy.Demo
{
    /// <summary>
    /// 链家租房 CSV 输出管道。
    /// 实现 IPipeline&lt;LianjiaRentItem&gt;，数据解析完成后自动写入 CSV。
    /// 由 ItemLoader.LoadItem() 自动触发。
    /// </summary>
    public class LianjiaRentCSVItemPipeline : IPipeline<LianjiaRentItem>
    {
        private static readonly string StartTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        private static bool _headerWritten = false;
        private static readonly object _fileLock = new object();

        public void ProcessItem(LianjiaRentItem item, ISpider spider)
        {
            var path = $"LianjiaRentItem_{StartTime}.csv";

            lock (_fileLock)
            {
                if (!_headerWritten)
                {
                    var headers = "Title,Price,Area,Room,Floor,Orientation,District,Community,Url";
                    File.AppendAllText(path, headers + Environment.NewLine, System.Text.Encoding.UTF8);
                    _headerWritten = true;
                }

                var line = $"\"{item.Title}\",\"{item.Price}\",\"{item.Area}\",\"{item.Room}\"," +
                           $"\"{item.Floor}\",\"{item.Orientation}\",\"{item.District}\"," +
                           $"\"{item.Community}\",\"{item.Url}\"";
                File.AppendAllText(path, line + Environment.NewLine, System.Text.Encoding.UTF8);

                Console.WriteLine($"  💾 {item.Title} | {item.Price}元/月 | {item.District}");
            }
        }
    }
}
