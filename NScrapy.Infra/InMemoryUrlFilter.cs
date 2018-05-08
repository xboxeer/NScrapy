using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Infra
{
    public class InMemoryUrlFilter : IUrlFilter
    {
        public HashSet<string> VisitedUrl { get; private set; }
        
        public InMemoryUrlFilter()
        {
            VisitedUrl = new HashSet<string>();
        }
        public async Task<bool> IsUrlVisited(string url)
        {
            var urlMd5 = NScrapyHelper.GetMD5FromBytes(url);
            if(VisitedUrl.Contains(urlMd5))
            {
                return true;
            }
            VisitedUrl.Add(urlMd5);
            return false;
        }

        
    }
}
