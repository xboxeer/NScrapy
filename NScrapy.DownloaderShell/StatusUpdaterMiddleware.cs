using NScrapy.Downloader;
using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NScrapy.DownloaderShell
{
    public class StatusUpdaterMiddleware: EmptyDownloaderMiddleware
    {
        private Process currentProcess = null;
        const int megaBytes = 1024 * 1024;
        public StatusUpdaterMiddleware()
        {            
            currentProcess = Process.GetCurrentProcess();
        }

        public async override void PostDownload(IResponse response)
        {
            await RedisManager.Connection.GetDatabase().StringSetAsync(
                $"NScrapy.DownloaderStatus.{Program.ID.ToString()}", string.Format("{{DownloaderCapbility:{0},RunningDownloaders:{1},RunningTime:{2},StartTime:{3},MemoryUsed:{4}MB, LastUpdate:{5}}}", 
                DownloaderContext.Context.DownloaderCapbility, 
                DownloaderContext.Context.RunningDownloader,
                currentProcess.UserProcessorTime,
                currentProcess.StartTime,
                currentProcess.PrivateMemorySize64/megaBytes,
                DateTime.Now));
        }
    }
}
