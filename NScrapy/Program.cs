using System;

namespace NScrapy
{
    class Program
    {
        static void Main(string[] args)
        {
            var shell = Shell.Shell.GetInstance();
            var response=shell.Crawl("Linkedin");
        }
    }
}
