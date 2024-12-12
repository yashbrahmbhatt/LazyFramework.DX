using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives
{
    public class EntryPoint
    {
        public string FilePath { get; set; }
        public string UniqueId { get; set; }
        public List<object> Input { get; set; } = new List<object>();
        public List<object> Output { get; set; } = new List<object>();

        public EntryPoint(string filePath, string uniqueId)
        {
            FilePath = filePath;
            UniqueId = uniqueId;
        }
    }
}
