using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IConfigProvider
    {
        /// <summary>
        /// Get new appsetting.json file path
        /// </summary>
        /// <returns></returns>
        string GetConfigFilePath();
    }
}
