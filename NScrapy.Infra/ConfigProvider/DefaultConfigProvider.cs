using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NScrapy.Infra.ConfigProvider
{
    /// <summary>
    /// Default Config provider is called by defualt by NScrapyContext or DownloaderContext
    /// It will look for other ConfigProvider if they are defined in default config appsetting.json
    /// </summary>
    public class DefaultConfigProvider : IConfigProvider
    {
        public string GetConfigFilePath()
        {
            return ConfigProviderFactory.DEFAULTCONFIG;
        }
    }
}