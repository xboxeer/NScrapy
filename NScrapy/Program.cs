using System;

namespace NScrapy
{
    class Program
    {
        static void Main(string[] args)
        {
            var shell = Shell.NScrapy.GetInstance();
            var response=shell.Crawl("Linkedin");
        }
    }
}
