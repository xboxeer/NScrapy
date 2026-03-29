using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IPipeline
    {
        void ProcessItem(object item, ISpider spider);
    }

    public interface IPipeline<T> : IPipeline
    {
        new void ProcessItem(T item, ISpider spider);
    }
}
