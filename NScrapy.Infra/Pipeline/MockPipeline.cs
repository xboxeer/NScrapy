using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra.Pipeline
{
    public class MockPipeline : IPipeline<MockItem>
    {
        public void ProcessItem(MockItem item, ISpider spider)
        {
            NScrapyContext.CurrentContext.Log.Info($"Mock Pipeline Processed, Mock Value={item.MockValue}");
        }
    }
    public class MockItem
    {
        private string mockValue;
        public string MockValue { get
            {
                return "Hello World";
            }
            set
            {
                mockValue = value;
            }
        }
    }
}
