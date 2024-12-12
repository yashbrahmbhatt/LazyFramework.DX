using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives;

namespace LazyFramework.DX.Services.Brahma.Models
{
    public class UiPathProject
    {
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string Description { get; set; } = "";
        public string Main { get; set; } = "";
        public Dictionary<string, string> Dependencies { get; set; }
        public List<object> WebServices { get; set; } = new List<object>();
        public List<object> EntitiesStores { get; set; } = new List<object>();
        public Version SchemaVersion { get; set; }
        public Version StudioVersion { get; set; }
        public Version ProjectVersion { get; set; } = new Version(1, 1, 1);
        public RuntimeOptions RuntimeOptions { get; set; } = new RuntimeOptions();
        public DesignOptions DesignOptions { get; set; } = new DesignOptions();
        public string ExpressionLanguage { get; set; } = "VisualBasic";
        public List<EntryPoint> EntryPoints { get; set; } = new List<EntryPoint>();
        public bool IsTemplate { get; set; } = false;
        public Dictionary<string, object> TemplateProjectData { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> PublishData { get; set; } = new Dictionary<string, object>();
        public string TargetFramework { get; set; } = "Windows";

        public UiPathProject(string name, Dictionary<string, string> dependencies, Version schemaVersion, Version studioVersion)
        {
            Name = name;
            Dependencies = dependencies;
            SchemaVersion = schemaVersion;
            StudioVersion = studioVersion;
        }
    }
}
