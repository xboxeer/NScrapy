using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NScrapy.Test
{
    public class MockConfigProvider : IConfigProvider
    {
        public string GetConfigFilePath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "appsettingMockConfig.json");
        }
    }
}
