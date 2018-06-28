using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using org.apache.zookeeper;
using org.apache.zookeeper.common;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;
using System.Linq;

namespace NScrapy.Infra.ConfigProvider
{
    public class ZookeeperConfigProvider : IConfigProvider
    {
        static ZookeeperConfigProvider()
        {
        }

        public string GetConfigFilePath()
        {
            return ZkHelper.GetAsync("/nscrapy/conf").Result;
        }
    }

    public class NScrapyConfigWatcher : Watcher
    {

        public override async Task process(WatchedEvent @event)
        {
            if (@event.getPath() == "/nscrapy/conf")
            {
                var newValue = await ZkHelper.GetAsync(@event.getPath());                
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

        public static void Create(string path,string data)
        {
            var bytes = data!=null? Encoding.UTF8.GetBytes(data):null;
            var subPathes = path.Split("/",StringSplitOptions.RemoveEmptyEntries).ToList();
            if (subPathes.Count > 1)
            {
                Create(string.Join("/", subPathes.Take(subPathes.Count - 1)), string.Empty);
            }
            //Since each path node need to be created by their sequence in the path, we should not run the task in async way, 
            //have to wait until the create is done for current path node 
            ZooKeeper.Using(zkEndpoint, 100000, new NScrapyConfigWatcher(),async zk =>
            {
                if(path[0]!='/')
                {
                    path = "/" + path;
                }
                if (await zk.existsAsync(path)==null)
                {
                    await zk.createAsync(path, bytes, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
            }).Wait();
        }

        public async static Task<string> GetAsync(string path)
        {
            DataResult result = null;
            await ZooKeeper.Using(zkEndpoint, 1000, new NScrapyConfigWatcher(), async zk =>
            {
                result = await zk.getDataAsync(path,true);
            });
            return Encoding.UTF8.GetString(result.Data);
        }

        public async static void SetAsync(string path, string value)
        {
            
            await ZooKeeper.Using(zkEndpoint, 1000, new NScrapyConfigWatcher(), async zk =>
            {
                await zk.setDataAsync(path, Encoding.UTF8.GetBytes(value));
            });
        }

        public static void Delete(string path)
        {
            ZooKeeper.Using(zkEndpoint, 1000, new NScrapyConfigWatcher(), async zk =>
            {
                await zk.deleteAsync(path);
            });            
        }

        //public static void DeleteRecursive(string path)
        //{
        //    var children = ZooKeeper.Using(zkEndpoint, 100000, new NScrapyConfigWatcher(), async zk =>
        //         {
        //             zk.
        //         });
        //    var subPathes = path.Split("/", StringSplitOptions.RemoveEmptyEntries).ToList();
        //    var temp=ZooKeeper.Using(zkEndpoint, 100000, new NScrapyConfigWatcher(),  zk =>
        //       {
        //           return zk.deleteAsync(path);
        //       });
        //    if(subPathes.Count>1)
        //    {
        //        var parentPath = "/" + string.Join("/", subPathes.Take(subPathes.Count - 1));
        //        DeleteRecursive(parentPath);
        //    }
        //}

        public static bool Exists(string path)
        {
            return ZooKeeper.Using(zkEndpoint, 1000, new NScrapyConfigWatcher(), async zk =>
               {
                   var stat = await zk.existsAsync(path, true);
                   return stat != null;
               }).Result;
        }
        

    }
}
