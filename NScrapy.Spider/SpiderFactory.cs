using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;
using NScrapy.Infra.Attributes.SpiderAttributes;
using NScrapy.Infra;

namespace NScrapy.Spider
{
    public class SpiderFactory
    {
        static AppDomain currentApp = System.AppDomain.CurrentDomain;
        static Assembly spiderProjectAssembly = null;
        static string SpiderProjectName = string.Empty;
        static SpiderFactory()
        {            
            SpiderProjectName = NScrapyContext.CurrentContext.Configuration["AppSettings:SpiderProject"];
            spiderProjectAssembly = currentApp.GetAssemblies().Where(assembly => assembly.GetName().Name == SpiderProjectName).FirstOrDefault();            
        }

        public static Spider GetSpider(string name)
        {
            Type spiderType = null;
            spiderType = spiderProjectAssembly.GetType(name);
            if(spiderType==null)
            {
                var types = spiderProjectAssembly.ExportedTypes;
                foreach(var type in types)
                {
                    var nameAttr = type.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
                    if(name==null)
                    {
                        continue;
                    }
                    if(name==nameAttr.Name && type.GetTypeInfo().IsSubclassOf(typeof(Spider)))
                    {
                        spiderType = type;
                        break;
                    }
                }
            }
            if(spiderType!=null)
            {
                if (Activator.CreateInstance(spiderType) is Spider spider)
                {
                    if (spiderType.GetCustomAttribute(typeof(URLAttribute)) is URLAttribute urlAttr)
                    {
                        spider.URLs= urlAttr.URLs.ToList();
                    }
                    return spider;
                }
                throw new Exception($"Create Spider {spiderType.FullName} failed");
            }
            throw new Exception($"Spider with name {name} can not be found, please check the spider name and if it is inherting NScrapy.Spider.Spider class");
        }
    }
}
