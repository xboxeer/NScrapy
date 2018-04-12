using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NScrapy.Downloader
{
    public class HttpHeaderMiddleware : IDownloaderMiddleware
    {
        
        public virtual void PostDownload(IResponse response)
        {
            //nothing to do then direct return 
            //throw new NotImplementedException();
            return;
        }

        public virtual void PreDownload(IRequest request)
        {
            var client = request.Client;
            SetHeaderFromConfig(client);
        }

        private void SetHeaderFromConfig(HttpClient client)
        {
            //The default header is constructed by a request to www.baidu.com sent by Chrome
            var accept = NScrapy.Infra.NScrapyContext.CurrentContext.Configuration["AppSettings:HttpHeader:Accept"];
            if(!string.IsNullOrEmpty(accept))
            {
                client.DefaultRequestHeaders.Add("Accept", accept);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            }

            var acceptEncoding= NScrapy.Infra.NScrapyContext.CurrentContext.Configuration["AppSettings:HttpHeader:Accept-Encoding"];
            if(!string.IsNullOrEmpty(acceptEncoding))
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", acceptEncoding);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,br");
            }

            var acceptLanguage = NScrapy.Infra.NScrapyContext.CurrentContext.Configuration["AppSettings:HttpHeader:Accept-Language"];
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", acceptLanguage);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Accept-Encoding", "zh-CN,zh;q=0.9,en;q=0.8");
            }

            var userAgent = NScrapy.Infra.NScrapyContext.CurrentContext.Configuration["AppSettings:HttpHeader:User-Agent"];
            if (!string.IsNullOrEmpty(userAgent))
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }
            else
            {
                client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36");
            }
        }
    }
}
