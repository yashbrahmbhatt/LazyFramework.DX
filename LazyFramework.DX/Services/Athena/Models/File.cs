using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.Models;
using Newtonsoft.Json;
namespace LazyFramework.DX.Services.Athena
{
    public class File
    {
        [JsonProperty("Name")]
        public string Name;
        [JsonProperty("Path")]
        public string Path;
        [JsonProperty("Folder")]
        public string Folder;
        [JsonProperty("Bucket")]
        public string Bucket;
        [JsonProperty("Description")]
        public string Description;
        [JsonProperty("Type")]
        public FileType Type;

        public File(string name, string path, string folder, string bucket, string description, FileType type) {
            Name = name;
            Path = path;
            Folder = folder;
            Bucket = bucket;
            Description = description;
            Type = type;
        }

        public async Task<T> Read<T>()
        {
            if (Type == FileType.Text && typeof(T) == typeof(string))
            {
                return (T)(object) System.IO.File.ReadAllText(Path);
            }
            else if (Type == FileType.Excel && typeof(T) == typeof(DataSet))
            {
                // Simulating reading an Excel file
                return (T)(object) await ExcelHandler.ReadExcelConfigFile(Path, null);
            }
            else
            {
                throw new InvalidOperationException($"Invalid return type {typeof(T)} for file type {Type}.");
            }
        }

        public string ToMemberString(string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// {Description}");
            sb.AppendLine($"{indent}/// </summary>");
            if (Type == FileType.Excel)
            {
                sb.AppendLine($"{indent}[Category(\"TextFiles\")]");
                sb.AppendLine($"{indent}public DataSet {Name} {{ get {{ return Get<DataSet>(nameof({Name})); }} set {{ Set(nameof({Name}), value); }} }}");
            }
            else
            {
                sb.AppendLine($"{indent}[Category(\"ExcelFiles\")]");
                sb.AppendLine($"{indent}public string {Name} {{ get {{ return Get<string>(nameof({Name})); }} set {{ Set(nameof({Name}), value); }} }}");
            }
            sb.AppendLine();
            return sb.ToString();
        }


    }


    public class FileType
    {
        private FileType(string value) { Value = value; }
        public string Value { get; private set; }
        public static FileType Text { get { return new FileType("Text"); } }
        public static FileType Excel { get { return new FileType("Excel"); } }
    }
}
