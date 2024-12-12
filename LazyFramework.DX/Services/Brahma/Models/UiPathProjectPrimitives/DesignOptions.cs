using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives
{
    public class DesignOptions
    {
        public string ProjectProfile { get; set; } = "Development";
        public string OutputType { get; set; } = "Process";
        public LibraryOptions LibraryOptions { get; set; } = new LibraryOptions();
        public ProcessOptions ProcessOptions { get; set; } = new ProcessOptions();
        public List<FileInfo> FileInfoCollection { get; set; } = new List<FileInfo>();
        public bool SaveToCloud { get; set; } = false;
    }
}
