using System;
using System.Collections.Generic;
using System.Text;
using NScrapy.Infra;
using NScrapy.Infra.Attributes.SpiderAttributes;

namespace NScrapy.Project.Spiders
{
    [Name(Name ="ZhiHu")]
    [URL("https://www.zhihu.com/api/v3/oauth/sign_in")]
    public class ZhihuSpider : Spider.Spider
    {
        public override void StartRequests()
        {
            //base.StartRequests();
            var shell = NScrapy.Shell.NScrapy.GetInstance();
            var formData = new Dictionary<string, string>();
            //formData.Add("Content-Disposition: form-data; name=\"client_id\"", "c3cef7c66a1843f8b3a9e6a1e3160e20");
            //formData.Add("Content-Disposition: form-data; name=\"client_id\"", "c3cef7c66a1843f8b3a9e6a1e3160e20");
            //formData.Add("Content-Disposition: form-data; name=\"username\"", "18606581100");
            //formData.Add("Content-Disposition: form-data; name=\"password\"", "hasdws4182");
            formData.Add("Content-Disposition: form-data; name=\"client_id\"", "c3cef7c66a1843f8b3a9e6a1e3160e20");
            formData.Add("Content-Disposition: form-data; name=\"grant_type\"", "password");
            formData.Add("Content-Disposition: form-data; name=\"timestamp\"", "1525965484643");
            formData.Add("Content-Disposition: form-data; name=\"source\"", "com.zhihu.web");
            formData.Add("Content-Disposition: form-data; name=\"signature\"", "a411f723ba657e5aa2bcbeef7c1bd98a497f610b");
            formData.Add("Content-Disposition: form-data; name=\"username\"", "+8618606581100");
            formData.Add("Content-Disposition: form-data; name=\"password\"", "hasdws4182");
            formData.Add("Content-Disposition: form-data; name=\"captcha\"", "");
            formData.Add("Content-Disposition: form-data; name=\"lang\"", "cn");
            formData.Add("Content-Disposition: form-data; name=\"ref_source\"", "other_");
            formData.Add("Content-Disposition: form-data; name=\"utm_source\"", "");


            shell.Request("https://www.zhihu.com/signup?next=%2F", null,null,formData);
        }

        public override void ResponseHandler(IResponse response)
        {
            Console.Write(response.ReponsePlanText);
        }
    }
}
