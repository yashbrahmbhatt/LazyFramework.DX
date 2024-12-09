using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyFramework.DX.Services.Athena.Models
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// {Description}");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}[Category(\"Assets\")]");
            sb.AppendLine($"{indent}public string {Name} {{ get {{ return Get<string>(nameof({Name})); }} set {{ Set(nameof({Name}), value); }} }}");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
