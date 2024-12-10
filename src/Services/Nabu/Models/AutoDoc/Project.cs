using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LazyFramework.DX.Services.Nabu.Models.AutoDoc
{
    public class Project
    {
        public string Name;
        public string Description;
        public DataTable EntryPoints = new DataTable();
        public string Language;
        public string ProjectVersion;
        public string StudioVersion;
        public string Type;
        public DataTable Dependencies = new DataTable();
        public DataTable FileInfoCollection = new DataTable();

        public Project(string filePath)
        {
            Console.WriteLine(string.Format("Parsing '{0}' as project.json", filePath));
            var raw = File.ReadAllText(filePath);

            Dependencies.Columns.Add("Name", "".GetType());
            Dependencies.Columns.Add("Version", "".GetType());
            EntryPoints.Columns.Add("Path");

            JObject jObj = JObject.Parse(raw);
            Name = jObj["name"].Value<string>();
            Description = jObj["description"].Value<string>();
            IEnumerable<Object[]> dependencyArrays = jObj["dependencies"].Children<JProperty>()
                .Select(c => new object[2]{
                        c.Name,
                        c.Value.ToString().Trim(new char[2]{'[', ']'})
                });
            Type = jObj["designOptions"].Value<string>("outputType");
            FileInfoCollection = JsonConvert.DeserializeObject<DataTable>(JsonConvert.SerializeObject(jObj["designOptions"]["fileInfoCollection"]));
            var entries = jObj["entryPoints"].Children<JObject>().Select(c => Path.Combine(Directory.GetCurrentDirectory(), c.Value<string>("filePath")));
            foreach (var entry in entries) EntryPoints.Rows.Add(new object[1] { Path.GetRelativePath(Directory.GetCurrentDirectory(), entry) });

            Language = jObj["expressionLanguage"].Value<string>();
            ProjectVersion = jObj["projectVersion"].Value<string>();
            StudioVersion = jObj["studioVersion"].Value<string>();

            foreach (var arr in dependencyArrays)
            {
                Dependencies.Rows.Add(arr);
            }
        }
    }
}
