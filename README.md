# NScrapy
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;

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
    [Name(Name = "JobSpider")]
    [URL("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=1")]
    public class JobSpider : Spider.Spider
    {
        public override IResponse ResponseHandler(IResponse response)
        {
            var httpResponse = response as HttpResponse;
            var returnValue = response.CssSelector(".job-info h3 a");
            NScrapy.Shell.NScrapy.GetInstance().Request("https://www.liepin.com/zhaopin/?d_sfrom=search_fp_nvbar&init=2", Parse);
            return returnValue;
        }

        public IResponse Parse(IResponse response)
        {
            return response.CssSelector(".job-info h3 a");
        }
    }
}
