using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives
{
    public class LibraryOptions
    {
        public bool IncludeOriginalXaml { get; set; } = true;
        public List<string> PrivateWorkflows { get; set; } = new List<string>();


    }
}
