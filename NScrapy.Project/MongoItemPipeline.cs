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

namespace NScrapy.Project
{
    public class MongoItemPipeline : IPipeline<JobItem>
    {
        private MongoClient client = new MongoClient("mongodb://localhost:27017");
        public async  void ProcessItem(JobItem item, ISpider spider)
        {
            var db = client.GetDatabase("Job");
            var collection = db.GetCollection<JobItem>("JobItems");
            await collection.InsertOneAsync(item);
        }
    }
}