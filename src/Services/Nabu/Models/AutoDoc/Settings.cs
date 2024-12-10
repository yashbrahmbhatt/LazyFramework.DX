using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Nabu.Models.AutoDoc
{
    public class Settings
    {
        public string ProjectRoot = Directory.GetCurrentDirectory();
        public string[] IgnoredDirectories = new string[2] { "Design", ".templates" };
        public string OutputRoot = "Documentation";
        public string TemplatesRoot = Path.Combine(Directory.GetCurrentDirectory(), ".autodoc");
    }
}
