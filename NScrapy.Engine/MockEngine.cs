using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Engine
{
    public class MockEngine : IEngine
    {
        public IResponse ProcessRequest(IRequest request)
        {
            System.Threading.Thread.Sleep(3000);
            Console.Write("Mock Engine Called");
            Console.Read();
            return null;
        }

        public Task<IResponse> ProcessRequestAsync(IRequest request)
        {
            throw new NotImplementedException();
        }

    }
}
