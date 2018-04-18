using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Infra
{
    public interface IEngine
    {
        Task<IResponse> ProcessRequestAsync(IRequest request);
        IResponse ProcessRequest(IRequest request);
    }
}