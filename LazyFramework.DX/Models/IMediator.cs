using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Odin;

namespace LazyFramework.DX.Models
{
    public interface IMediator
    {
        Task Register<TEvent>(Action<TEvent> handler);
        Task Notify<TEvent>(TEvent eventArgs);
    }


}
