using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.Models;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;
using File = System.IO.File;


using LazyFramework.DX.Services.Athena.Models;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Heimdall;
using LazyFramework.DX.Services.Nabu;
using UiPath.Studio.Activities.Api.ProjectProperties;
using System.Text.RegularExpressions;

namespace LazyFramework.DX.Services.Athena
{
    public class Athena : OdinConsumer
    {
        private AthenaSettings _settings;
        private string _projectPath;
        private string _outputRoot;
        private string _configFilePath;
        private List<ConfigObject> _configObjects = new List<ConfigObject>();
        private Nabu.Nabu _nabu;



        public Athena(IWorkflowDesignApi api, Hermes.Hermes hermes, Odin.Odin odin) : base(api, hermes, odin, "Athena")
        {
            Log("Initializing Athena.");
            _settings = new AthenaSettings(_api, _hermes);
            _projectPath = api.ProjectPropertiesService.GetProjectDirectory();
            _outputRoot = Path.Combine(_projectPath, _settings.OutputPath);
            _configFilePath = Path.Combine(_projectPath, _settings.ConfigFilePath);
            var configExists = CheckConfigExists().Result;
            if (configExists)
            {
                Log($"Config file path initialized to '{_configFilePath}'.");
                UpdateConfigClasses();
            }
            _odin.Register<SettingChangedEvent>(async (e) => await OnSettingChanged(e));
            //_odin.Register<WorkflowChangedEvent>(async (e) => await OnWorkflowEvent(e));
            switch(_settings.ConfigFileType)
            {
                case "Json":
                    Log("Config file type set to JSON.");
                    _odin.Register<JsonFileEvent>(async (e) => await OnFileEvent(e));
                    break;
                case "Excel":
                    Log("Config file type set to Excel.");
                    _odin.Register<ExcelFileEvent>(async (e) => await OnFileEvent(e));
                    break;
                default:
                    Log("Unsupported config file type.");
                    break;
            }
            WriteBaseClass(_settings.OutputNamespace);
            Log("Athena initialized.");
        }

        public async Task OnWorkflowEvent(WorkflowChangedEvent e)
        {
            var editor = e.Editor;
            var path = e.Path;
        }
        public async Task UpdateConfigClasses()
        {
            _configObjects = new List<ConfigObject>();
            await ReadConfigFile();
            foreach (var configObject in _configObjects)
            {
                await WriteConfig(Path.Combine(_outputRoot, configObject.ClassName + ".cs"), configObject);
            }
        }

        public async Task<bool> CheckConfigExists()
        {
            var exists = System.IO.File.Exists(_configFilePath);
            if (!exists)
            {
                Log($"Could not find config file with path '{_configFilePath}'. Please update the Config File Path setting under Athena in the project settings.", LogLevel.Error);

            }
            return exists;
        }

        public async Task OnSettingChanged(SettingChangedEvent e)
        {
            if (SettingsCreator.GetAllSettingKeysForClass(typeof(AthenaSettingKeys)).Contains(e.Key))
            {
                Log($"Athena settings changed: {e.Key}");
                _configFilePath = Path.Combine(_projectPath, _settings.ConfigFilePath);
                Log($"Config file path updated to '{_configFilePath}'.");
                var exists = await CheckConfigExists();
                if (exists)
                {
                    await WriteBaseClass(_settings.OutputNamespace);
                    await UpdateConfigClasses();
                }
            }
        }

        public async Task OnFileEvent(FileEvent e)
        {
            var path = e.Path;
            var type = e.EventType;
            if (path != _configFilePath) return;

            Log($"Config file {type} event detected.");
            if (type == WatcherChangeTypes.Deleted)
            {
                Log("Config file deleted. Please update the Config File Path setting under Athena in the project settings.", LogLevel.Error);
                _configObjects = new List<ConfigObject>();
            }
            else if (type == WatcherChangeTypes.Renamed)
            {
                if(e.OldPath == null)
                {
                    Log("Config file renamed, but no old path supplied in event args.", LogLevel.Error);
                    return;
                }
                if (e.OldPath == _configFilePath)
                {
                    Log($"Config file renamed to {e.Path}. Updating setting...");
                    _settings.ConfigFilePath = Helpers.PathExtensions.GetRelativePath(_projectPath, e.Path);
                }
            }
            else
            {
                await UpdateConfigClasses();
            }

        }

        public async Task WriteBaseClass(string ns)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns} {{"); // open
            sb.AppendLine($"\tpublic class DictionaryClass {{"); // open
            sb.AppendLine($"\t\t// <summary>");
            sb.AppendLine($"\t\t// Base class for all config classes.");
            sb.AppendLine($"\t\t// </summary>");
            sb.AppendLine($"\t\tpublic Dictionary<string, object> _config = new Dictionary<string, object>();");
            sb.AppendLine();
            sb.AppendLine($"\t\t// <summary>");
            sb.AppendLine($"\t\t// <remarks>A getter with type safety.</remarks>");
            sb.AppendLine($"\t\t// <param name=\"name\">The key of the config value to get.</param>");
            sb.AppendLine($"\t\t// <returns>The value of the config key.</returns>");
            sb.AppendLine($"\t\t// <exception cref=\"KeyNotFoundException\">Thrown when the key is not found in the config.</exception>");
            sb.AppendLine($"\t\t//</summary>");
            sb.AppendLine($"\t\tpublic T Get<T>(string name) {{"); // open
            sb.AppendLine($"\t\t\tif (_config.ContainsKey(name)) return (T)_config[name];");
            sb.AppendLine($"\t\t\tthrow new KeyNotFoundException($\"Key '\" + name + \"' not found in the config.\");");
            sb.AppendLine($"\t\t}}"); // close
            sb.AppendLine();
            sb.AppendLine($"\t\t// <summary>");
            sb.AppendLine($"\t\t// <remarks>A setter with type safety.</remarks>");
            sb.AppendLine($"\t\t// <param name=\"name\">The key of the config value to set.</param>");
            sb.AppendLine($"\t\t// <param name=\"value\">The value to set.</param>");
            sb.AppendLine($"\t\t// <exception cref=\"ArgumentNullException\">Thrown when the value is null.</exception>");
            sb.AppendLine($"\t\t// </summary>");
            sb.AppendLine($"\t\tpublic void Set(string name, object value) {{"); // open
            sb.AppendLine($"\t\t\tif (value == null) throw new ArgumentNullException(nameof(value), \"Value cannot be null.\");");
            sb.AppendLine($"\t\t\t_config[name] = value;");
            sb.AppendLine($"\t\t}}"); // close
            sb.AppendLine($"\t}}"); // close
            sb.AppendLine($"}}"); // close
            if (!Directory.Exists(_outputRoot))
            {
                Directory.CreateDirectory(_outputRoot);
            }
            #if (NET6_0_OR_GREATER)
            await System.IO.File.WriteAllTextAsync(Path.Combine(_outputRoot, "DictionaryClass.cs"), sb.ToString());
            #else
            System.IO.File.WriteAllText(Path.Combine(_outputRoot, "BaseConfig.cs"), sb.ToString());
            #endif
        }

        public async Task ReadConfigFile()
        {
            var exists = await CheckConfigExists();
            if (!exists) return;

            Log($"Reading config file: {_configFilePath}");

            try
            {
                if (_settings.ConfigFileType == ConfigFileType.Json.Value)
                {
                    // Read the JSON file
                    string json = System.IO.File.ReadAllText(_configFilePath);
                    var configObject = JsonConvert.DeserializeObject<ConfigObject>(json);

                    if (configObject == null)
                    {
                        Log("Failed to deserialize config file as single config.", LogLevel.Error);
                        return;
                    }
                    var cObjects = JsonConvert.DeserializeObject<Dictionary<string, ConfigObject>>(json);
                    if (cObjects == null)
                    {
                        Log("Failed to deserialize config file as multiple configs.", LogLevel.Error);
                        return;
                    }
                }
                else if (_settings.ConfigFileType == ConfigFileType.Excel.Value)
                {
                    // Read and process the Excel file
                    var dataSet = await ExcelHandler.ReadExcelConfigFile(_configFilePath, async (s, l) => Log(s, l));
                    if(dataSet == null)
                    {
                        Log("Failed to read excel file.", LogLevel.Error);
                        return;
                    }
                    var configObject = new ConfigObject("", dataSet, async (s, l) => Log(s, l));
                    configObject.ClassName = "Config";
                    _configObjects.Add(configObject);
                    Log($"Excel file processed. Found {configObject.Settings.Count} settings, {configObject.Assets.Count} assets, {configObject.TextFiles.Count} text files, and {configObject.ExcelFiles.Count} excel files.", LogLevel.Debug);
                }
                else
                {
                    Log($"Unsupported config file type: {_settings.ConfigFileType}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"Error reading config file: {ex.Message}", LogLevel.Error);
            }
        }

        public async Task WriteConfig(string path, ConfigObject configObject)
        {
            var classString = configObject.GetClassString(_settings.OutputNamespace, "DictionaryClass");
            var folder = Path.GetDirectoryName(path);
            if (folder == null)
            {
                Log($"Failed to get folder for path: {path}", LogLevel.Error);
                return;
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
#if (NET6_0_OR_GREATER)
            await System.IO.File.WriteAllTextAsync(path, classString);
#else
            System.IO.File.WriteAllText(path, classString);
#endif
        }
    }
}
