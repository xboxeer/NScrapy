using System;

namespace NScrapy.Project
{
    class Program
    {
        static void Main(string[] args)
        {
            var shell = NScrapy.Shell.NScrapy.GetInstance();
            shell.Crawl("JobSpider");
            return;
        }
    }
}
