using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Infra
{
    public interface IUrlFilter
    {
         Task<bool> IsUrlVisited(string url);
    }
}
