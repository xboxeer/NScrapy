using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Scheduler.RedisExt
{
    public class RedisUrlFilter : IUrlFilter
    {
        public async Task<bool> IsUrlVisited(string url)
        {
            var urlMD5 = NScrapyHelper.GetMD5FromBytes(url);
            var connection = RedisSchedulerContext.Current.Connection;
            var urlSetName = $"{RedisSchedulerContext.Current.ReceiverQueue}.VisitedURLMD5";
            if(await connection.GetDatabase().SetContainsAsync(urlSetName, urlMD5))
            {
                return true;
            }
            await connection.GetDatabase().SetAddAsync(urlSetName, urlMD5);
            return false;
        }


    }
}
