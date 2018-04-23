using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using NScrapy.Infra;

namespace NScrapy.Downloader.Middleware
{
    public class HttpUserAgentPoolMiddleware : HttpHeaderMiddleware
    {
        static List<string> userAgentPool = new List<string>();
        static HttpUserAgentPoolMiddleware()
        {
            var section = NScrapyContext.CurrentContext.Configuration.GetSection("UserAagentPool");
            foreach(var item in section.GetChildren())
            {
                var path = $"{item.Path}:User-Agent";
                var agent = NScrapyContext.CurrentContext.Configuration[path];
                userAgentPool.Add(agent);
            }
        }

		protected override void SetHeaderFromConfig(HttpClient client)
		{
            base.SetHeaderFromConfig(client);
            client.DefaultRequestHeaders.UserAgent.Clear();
            Random random = new Random();
            var choice = random.Next(0, userAgentPool.Count-1);
            var userAgent = userAgentPool[choice];
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
		}
	}
}
