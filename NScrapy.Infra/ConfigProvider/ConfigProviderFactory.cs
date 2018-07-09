using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NScrapy.Infra.ConfigProvider
{
    public static class ConfigProviderFactory
    {
        public const string DEFAULTCONFIG = "appsetting.json";
        private const string ZKPROVIDER = "Zookeeper";
        public static IConfigProvider GetProvider()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(DEFAULTCONFIG);
            var config = builder.Build();
            if (string.IsNullOrEmpty(config["AppSettings:ConfigProvider"]))
            {
                return new DefaultConfigProvider();
            }
            var providerName = config["AppSettings:ConfigProvider"];
            if (providerName == ZKPROVIDER)
            {
                return new ZookeeperConfigProvider();
            }
            var entryAssembly = Assembly.GetEntryAssembly();
            var providerType = entryAssembly.GetType(providerName);
            var provider = Activator.CreateInstance(providerType) as IConfigProvider;
            if (provider == null)
            {
                throw new InvalidCastException($"{providerName} does not implement IConfigProvider Interface, a config provider must implement IConfigProvider Interface");
            }
            return provider;
        }
    }
}
