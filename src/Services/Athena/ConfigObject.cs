using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.Services.Athena;
using LazyFramework.Services.Hermes;
using LazyFramework.Wizards;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LazyFramework.Models.Config
{
    public class ConfigObject
    {
        [JsonProperty("Settings")]
        public List<Setting> Settings { get; set; } = new();

        [JsonProperty("Assets")]
        public List<Asset> Assets { get; set; } = new();

        [JsonProperty("TextFiles")]
        public List<File> TextFiles { get; set; } = new();

        [JsonProperty("ExcelFiles")]
        public List<File> ExcelFiles { get; set; } = new();

        public string ClassName;
        [JsonConstructor]
        public ConfigObject()
        {

        }
        public ConfigObject(string className)
        {
            ClassName = className;
        }
        public ConfigObject(string className, DataSet dataSet, LoggerConsumer logger = null)
        {
            ClassName = className;
            ParseConfigObject(dataSet, logger);
        }

        public ConfigObject(string className, string rawJson, LoggerConsumer logger = null)
        {
            ClassName = className;
            JObject json = JObject.Parse(rawJson);
            Settings = json["Settings"].ToObject<List<Setting>>();
            Assets = json["Assets"].ToObject<List<Asset>>();
            TextFiles = json["TextFiles"].ToObject<List<File>>();
            ExcelFiles = json["ExcelFiles"].ToObject<List<File>>();
        }

        public ConfigObject ParseConfigObject(DataSet dataSet, LoggerConsumer logger = null)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                if(logger != null) logger.Log("Parsing table " + table.TableName, LogLevel.Debug);
                string[] columnNames;
                bool valid;
                foreach (DataRow row in table.Rows)
                {
                    if (row["Name"] == null)
                    {
                        if (logger != null) logger.Log($"Missing 'Name' column in {table.TableName} table.", LogLevel.Error);
                        valid = false;
                        break;
                    }
                    if (row[0] == DBNull.Value)
                    {
                        if(logger != null) logger.Log($"Empty row in {table.TableName} table.", LogLevel.Warning);
                        continue;
                    }
                    switch (table.TableName)
                    {
                        case "Assets":
                            columnNames = new string[] { "Name", "Value", "Folder", "Description" };
                            valid = ValidateConfigTable(table, columnNames, logger);
                            if (!valid) break;
                            Assets.Add(new Asset(row["Name"].ToString(), row["Value"].ToString(), row["Folder"].ToString(), row["Description"].ToString()));
                            break;
                        case "TextFiles":
                            columnNames = new string[] { "Name", "Path", "Folder", "Bucket", "Description", "Folder" };
                            valid = ValidateConfigTable(table, columnNames, logger);
                            if (!valid) break;
                            TextFiles.Add(new File(row["Name"].ToString(), row["Path"].ToString(), row["Folder"].ToString(), row["Bucket"].ToString(), row["Description"].ToString(), FileType.Text));
                            break;
                        case "ExcelFiles":
                            columnNames = new string[] { "Name", "Path", "Folder", "Bucket", "Description", "Folder" };
                            valid = ValidateConfigTable(table, columnNames, logger);
                            if (!valid) break;
                            TextFiles.Add(new File(row["Name"].ToString(), row["Path"].ToString(), row["Folder"].ToString(), row["Bucket"].ToString(), row["Description"].ToString(), FileType.Excel));
                            break;
                        default:
                            columnNames = new string[] { "Name", "Value", "Description" };
                            valid = ValidateConfigTable(table, columnNames, logger);
                            if (!valid) break;

                            Settings.Add(new Setting(row["Name"].ToString(), row["Value"].ToString(), row["Description"].ToString()));
                            break;
                    }

                }
            }
            return this;
        }

        public bool ValidateConfigTable(DataTable table, string[] columnNames, LoggerConsumer logger = null)
        {
            bool valid = true;
            foreach (var name in columnNames)
            {
                if (!table.Columns.Contains(name))
                {
                    if(logger != null) logger.Log($"Missing column '{name}' in {table.TableName} table.", LogLevel.Error);
                    valid = false;
                }
            }
            return valid;
        }

        public string GetClassString(string ns, LoggerConsumer logger = null)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendLine($"using System;");
                sb.AppendLine($"using System.Collections.Generic;");
                sb.AppendLine($"using System.ComponentModel;");
                sb.AppendLine();
                sb.AppendLine($"namespace {ns} {{");
                sb.AppendLine($"\tpublic class {ClassName} : BaseConfig {{");
                //sb.AppendLine("\t// Houses the actual values of the config");
                //sb.AppendLine($"\tprivate Dictionary<string, object> _config = new Dictionary<string, object>();");
                //sb.AppendLine();
                //sb.AppendLine("\t// Generic getter method, now safely casts to the requested type");
                //sb.AppendLine($"\tpublic T Get<T>(string name) {{");
                //sb.AppendLine($"\t\tif (_config.ContainsKey(name)) return (T)_config[name];");
                //sb.AppendLine($"\t\tthrow new KeyNotFoundException($\"Key '{{name}}' not found in the config.\");");
                //sb.AppendLine($"\t}}");
                //sb.AppendLine($"\t// Setter with type safety");
                //sb.AppendLine($"\tpublic void Set(string name, object value) {{");
                //sb.AppendLine($"\t\tif (value == null) throw new ArgumentNullException(nameof(value), \"Value cannot be null.\");");
                //sb.AppendLine($"\t\t_config[name] = value;");
                //sb.AppendLine($"\t}}");
                sb.AppendLine();
                sb.AppendLine("\t\t#region Settings");
                foreach (var setting in Settings)
                {
                    (Type type, object value) = TryParse(setting.Value);
                    sb.AppendLine($"{setting.ToMemberString(type, "\t\t")}");
                }
                sb.AppendLine("\t\t#endregion");
                sb.AppendLine();
                sb.AppendLine("\t\t#region Assets");
                foreach (var asset in Assets)
                {
                    sb.AppendLine($"{asset.ToMemberString("\t\t")}");
                }
                sb.AppendLine("\t\t#endregion");
                sb.AppendLine();
                sb.AppendLine("\t\t#region Text Files");
                foreach (var textFile in TextFiles)
                {
                    sb.AppendLine($"{textFile.ToMemberString("\t\t")}");
                }
                sb.AppendLine("\t\t#endregion");
                sb.AppendLine();
                sb.AppendLine("\t\t#region Excel Files");
                foreach (var excelFile in ExcelFiles)
                {
                    sb.AppendLine($"{excelFile.ToMemberString("\t\t")}");
                }
                sb.AppendLine("\t\t#endregion");
                sb.AppendLine();
                sb.AppendLine($"\t\tpublic {ClassName}(Dictionary<string, object> config) {{");
                sb.AppendLine($"\t\t\t_config = config;");
                sb.AppendLine($"\t\t}}");

                sb.AppendLine();
                sb.AppendLine($"\t}}");
                sb.AppendLine($"}}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                if (logger != null) logger.Log($"Error getting class string file: {ex.Message}", LogLevel.Error);
                return "";
            }
        }

        public (Type type, object value) TryParse(string value)
        {
            if(value == null)
            {
                return (typeof(string), "");
            }
            else if (DateTime.TryParse(value, out DateTime dateTimeValue))
            {
                return (typeof(DateTime), dateTimeValue);
            }
            else if (int.TryParse(value, out int intValue))
            {
                return (typeof(int), intValue);
            }
            else if (double.TryParse(value, out double doubleValue))
            {
                return (typeof(double), doubleValue);
            }
            else if (bool.TryParse(value, out bool boolValue))
            {
                return (typeof(bool), boolValue);
            }
            else
            {
                return (typeof(string), value);
            }
        }
    }

}
