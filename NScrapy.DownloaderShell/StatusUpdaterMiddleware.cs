using NScrapy.Downloader;
using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.DownloaderShell
{
    public class StatusUpdaterMiddleware: EmptyDownloaderMiddleware
    {
        public override void PostDownload(IResponse response)
        {
            RedisManager.Connection.GetDatabase().StringSetAsync($"NScrapy.DownloaderStatus.{Program.ID.ToString()}", string.Format("{{DownloaderCapbility:{0},RunningDownloaders:{1}}}", DownloaderContext.Context.DownloaderCapbility, DownloaderContext.Context.RunningDownloader));
        }

        private class DownloaderStatus
        {
            public int RunningDownloaders { get; set; }
            public int DownloaderCapbility { get; set; }
        }
    }
}
