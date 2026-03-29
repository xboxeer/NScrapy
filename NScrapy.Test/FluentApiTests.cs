using Microsoft.VisualStudio.TestTools.UnitTesting;
using NScrapy;
using NScrapy.Core.Fluent;
using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Reflection;
using FluentSpider = NScrapy.Core.Fluent.Spider;

namespace NScrapy.Test
{
    [TestClass]
    public class FluentApiTests
    {
        #region Mock Classes for Testing

        private class TestItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private class TestPipeline : IPipeline<TestItem>
        {
            public bool WasCalled { get; private set; }
            public TestItem ReceivedItem { get; private set; }
            public ISpider ReceivedSpider { get; private set; }

            public void ProcessItem(TestItem item, ISpider spider)
            {
                WasCalled = true;
                ReceivedItem = item;
                ReceivedSpider = spider;
            }

            void IPipeline.ProcessItem(object item, ISpider spider)
            {
                if (item is TestItem testItem)
                {
                    ProcessItem(testItem, spider);
                }
            }
        }

        #endregion

        #region TestSpiderBuilder_CanCreateSpiderWithName

        [TestMethod]
        public void TestSpiderBuilder_CanCreateSpiderWithName()
        {
            var spider = NScrapy.CreateSpider("TestSpider");
            Assert.IsNotNull(spider);
            Assert.IsInstanceOfType(spider, typeof(ISpiderBuilder));
        }

        #endregion

        #region TestSpiderBuilder_BuildCreatesSpider

        [TestMethod]
        public void TestSpiderBuilder_BuildCreatesSpider()
        {
            var builder = NScrapy.CreateSpider("TestSpider")
                .StartUrl("https://example.com");

            var spider = builder.Build();
            Assert.IsNotNull(spider);
            Assert.IsInstanceOfType(spider, typeof(ISpider));
            Assert.AreEqual("TestSpider", spider.Name);
        }

        #endregion

        #region TestSpiderBuilder_SetsStartUrls

        [TestMethod]
        public void TestSpiderBuilder_SetsStartUrls()
        {
            var builder = NScrapy.CreateSpider("TestSpider")
                .StartUrl("https://example.com/page1")
                .StartUrls(new[] { "https://example.com/page2", "https://example.com/page3" });

            var spider = builder.Build() as FluentSpider;
            Assert.IsNotNull(spider);

            // Use reflection to verify internal _startUrls field
            var startUrlsField = typeof(FluentSpider).GetField("_startUrls", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(startUrlsField);
            var startUrls = startUrlsField.GetValue(spider) as List<string>;
            Assert.IsNotNull(startUrls);
            Assert.AreEqual(3, startUrls.Count);
            Assert.IsTrue(startUrls.Contains("https://example.com/page1"));
            Assert.IsTrue(startUrls.Contains("https://example.com/page2"));
            Assert.IsTrue(startUrls.Contains("https://example.com/page3"));
        }

        #endregion

        #region TestSpiderBuilder_AddsPipeline

        [TestMethod]
        public void TestSpiderBuilder_AddsPipeline()
        {
            var builder = NScrapy.CreateSpider("TestSpider")
                .AddPipeline<TestPipeline>();

            var spider = builder.Build() as FluentSpider;
            Assert.IsNotNull(spider);

            // Use reflection to verify internal _pipelines field
            var pipelinesField = typeof(FluentSpider).GetField("_pipelines", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(pipelinesField);
            var pipelines = pipelinesField.GetValue(spider) as List<IPipeline>;
            Assert.IsNotNull(pipelines);
            Assert.AreEqual(1, pipelines.Count);
            Assert.IsInstanceOfType(pipelines[0], typeof(TestPipeline));
        }

        #endregion

        #region TestDistributedBuilder_SetsRedisConfig

        [TestMethod]
        public void TestDistributedBuilder_SetsRedisConfig()
        {
            var builder = new DistributedBuilder()
                .UseRedis("localhost:6379")
                .ReceiverQueue("test-requests")
                .ResponseQueue("test-responses");

            // Use reflection to check internal config
            var config = typeof(DistributedBuilder)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(builder) as DistributedConfig;

            Assert.IsNotNull(config);
            Assert.AreEqual("localhost:6379", config.RedisConnectionString);
            Assert.AreEqual("test-requests", config.ReceiverQueue);
            Assert.AreEqual("test-responses", config.ResponseQueue);
        }

        #endregion

        #region TestSpiderOptions_DefaultValues

        [TestMethod]
        public void TestSpiderOptions_DefaultValues()
        {
            var options = new SpiderOptions();
            Assert.AreEqual(10, options.Concurrency);
            Assert.AreEqual(0, options.DelayMs);
            Assert.AreEqual(3, options.MaxRetries);
            Assert.AreEqual(30000, options.TimeoutMs);
        }

        #endregion

        #region TestDistributedBuilder_Chaining

        [TestMethod]
        public void TestDistributedBuilder_Chaining()
        {
            var builder = new DistributedBuilder()
                .UseRedis("redis.example.com:6380")
                .ReceiverQueue("my-requests")
                .ResponseQueue("my-responses");

            Assert.IsNotNull(builder);

            // Verify final config via reflection
            var config = typeof(DistributedBuilder)
                .GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(builder) as DistributedConfig;

            Assert.AreEqual("redis.example.com:6380", config.RedisConnectionString);
            Assert.AreEqual("my-requests", config.ReceiverQueue);
            Assert.AreEqual("my-responses", config.ResponseQueue);
        }

        #endregion

        #region TestSpiderBuilder_ConfiguresOptions

        [TestMethod]
        public void TestSpiderBuilder_ConfiguresOptions()
        {
            var builder = NScrapy.CreateSpider("TestSpider")
                .Configure(options =>
                {
                    options.Concurrency = 20;
                    options.DelayMs = 500;
                    options.MaxRetries = 5;
                    options.TimeoutMs = 60000;
                });

            var spider = builder.Build() as FluentSpider;
            Assert.IsNotNull(spider);

            // Use reflection to verify internal _options field
            var optionsField = typeof(FluentSpider).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(optionsField);
            var options = optionsField.GetValue(spider) as SpiderOptions;
            Assert.IsNotNull(options);
            Assert.AreEqual(20, options.Concurrency);
            Assert.AreEqual(500, options.DelayMs);
            Assert.AreEqual(5, options.MaxRetries);
            Assert.AreEqual(60000, options.TimeoutMs);
        }

        #endregion
    }
}
