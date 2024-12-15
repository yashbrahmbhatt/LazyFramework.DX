using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Models;

namespace LazyFramework.DX.Services.Aether
{
    public interface IAetherImplementation
    {
        new Task Register(string endpoint, Func<HttpListenerRequest, object> handler);
    }
}
