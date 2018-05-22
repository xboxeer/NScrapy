using NScrapy.Infra;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Project
{
    public class CSVItemPipeline : IPipeline<JobItem>
    {
        private string startTime = DateTime.Now.ToString("yyyyMMddhhmm");
        
        public void ProcessItem(JobItem item, ISpider spider)
        {
            var info = $"{item.Title},{item.Firm},{item.SalaryFrom},{item.SalaryTo},{item.Location},{item.Time},{item.URL},{System.Environment.NewLine}";
            Console.WriteLine(info);
            File.AppendAllText($"output-{startTime}.csv", info,Encoding.UTF8);    
        }
    }
}
