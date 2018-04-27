using System;
using System.Collections.Generic;
using System.Text;

namespace NScrapy.Infra
{
    public interface IUrlFilter
    {
        bool IsUrlVisited(string url);
    }
}
