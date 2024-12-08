using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyFramework.Models.Config
{
    public class Asset
    {
        [JsonProperty("Name")]
        public string Name;
        [JsonProperty("Value")]
        public string Value;
        [JsonProperty("Folder")]
        public string Folder;
        [JsonProperty("Description")]
        public string Description;

        public Asset(string name, string value, string folder, string description)
        {
            Name = name; Value = value; Folder = folder; Description = description;
        }

        public string ToMemberString(string indent)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{indent}[Category(\"Assets\")]");
            sb.AppendLine($"{indent}[Description(\"{Description}\")]");
            sb.AppendLine($"{indent}public string {Name} {{ get {{ return Get<string>(\"{Name}\"); }} set {{ Set(\"{Name}\", value); }} }}");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
