using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IPipeline<T>
    {
        void ProcessItem(T item, ISpider spider);
    }
}
