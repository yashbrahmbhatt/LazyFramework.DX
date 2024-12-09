using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyFramework.DX.Services.Athena.Models
{
    public class Setting
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Value")]
        public string Value { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }

        public Setting(string name, string value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }

        public string ToMemberString(Type type, string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// {Description}");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}[Category(\"Settings\")]");
            sb.AppendLine($"{indent}public {type.Name} {Name} {{ get {{ return Get<{type.Name}>(nameof({Name})); }} set {{ Set(nameof({Name}), value); }} }}");
            sb.AppendLine();
            return sb.ToString();

        }
    }
}
