using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NScrapy.Infra
{
    public interface IEngine
    {
        IResponse ProcessRequest(IRequest request);
    }
}