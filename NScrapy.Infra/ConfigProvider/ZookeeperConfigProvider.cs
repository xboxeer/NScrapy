using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using org.apache.zookeeper;
using org.apache.zookeeper.common;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;

namespace NScrapy.Infra.ConfigProvider
{
    public class ZookeeperConfigProvider : IConfigProvider
    {
        internal static ZooKeeper ZK;
        private Watcher configWatcher;
        public Watcher ConfigWatcher
        {
            get
            {
                return this.configWatcher;
            }
        }
        static ZookeeperConfigProvider()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");
            var localConfig = builder.Build();
            var zkEndpoint = localConfig["AppSettings:ZookeeperEndpoint"];
            var watcher = new ZkConfigWatcher();
            ZK = new ZooKeeper(zkEndpoint, 100000, watcher);
        }

        public string GetConfigFilePath()
        {
            throw new NotImplementedException();
        }
    }

    public class ZkConfigWatcher : Watcher
    {

        public override async Task process(WatchedEvent @event)
        {
            if (@event.getPath() == "/nscrapy/conf")
            {
                var newValue = await ZookeeperConfigProvider.ZK.getDataAsync(@event.getPath(), true);
                Console.WriteLine(UTF8Encoding.UTF8.GetString(newValue.Data));
            }
        }
    }

    public static class ZkHelper
    {
        static string zkEndpoint;
        static ZkHelper()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsetting.json");
            var localConfig = builder.Build();
            zkEndpoint = localConfig["AppSettings:ZookeeperEndpoint"];
        }

        public static void Create(string path,byte[] data)
        {
            ZooKeeper.Using(zkEndpoint, 100000, new ZkConfigWatcher(), async zk =>
            {
                await zk.createAsync(path,data,Ids.OPEN_ACL_UNSAFE,CreateMode.PERSISTENT);
            });
        }

        public static void Get(string path)
        {
            ZooKeeper.Using(zkEndpoint, 100000, new ZkConfigWatcher(), async zk =>
            {
                await zk.getDataAsync(path,true);
            });
        }

        public static void Set(string path, string value)
        {
            
            ZooKeeper.Using(zkEndpoint, 100000, new ZkConfigWatcher(), async zk =>
            {
                await zk.setDataAsync(path, Encoding.UTF8.GetBytes(value));
            });
        }
    }
}
