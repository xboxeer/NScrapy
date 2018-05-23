using NScrapy.Infra;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NScrapy.Project.Spiders;

namespace NScrapy.Project
{
    public class MongoItemPipeline : IPipeline<House>
    {
        private MongoClient client = new MongoClient("mongodb://localhost:27017");
        public async  void ProcessItem(House item, ISpider spider)
        {
            var db = client.GetDatabase("Lianjia");
            var collection = db.GetCollection<House>("House");
            await collection.InsertOneAsync(item);
        }
    }
}