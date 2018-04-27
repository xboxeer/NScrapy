using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NScrapy.Infra
{
    public class InMemoryUrlFilter : IUrlFilter
    {
        public HashSet<string> VisitedUrl { get; private set; }
        
        public InMemoryUrlFilter()
        {
            VisitedUrl = new HashSet<string>();
        }
        public bool IsUrlVisited(string url)
        {
            var urlMd5 = this.GetMD5Hash(url);
            if(VisitedUrl.Contains(urlMd5))
            {
                return true;
            }
            VisitedUrl.Add(urlMd5);
            return false;
        }

        private string GetMD5Hash(string url)
        {
            var sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(url);
                var hashBytes = md5.ComputeHash(bytes);
                foreach(var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }
            }
            return sb.ToString();
        }
    }
}
