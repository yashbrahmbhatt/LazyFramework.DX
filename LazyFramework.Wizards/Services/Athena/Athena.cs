using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.Models;
using LazyFramework.Models.Config;
using LazyFramework.Services.Hermes;
using LazyFramework.Services.Odin;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;
using File = System.IO.File;

namespace LazyFramework.Services.Athena
{
    public class Athena : Services.Hermes.LoggerConsumer, IOdinSubscriber
    {
        private Odin.Odin odin;

        private Settings settings;
        private string projectPath;
        private string outputRoot;
        private string configFilePath;
        private List<ConfigObject> configObjects = new ();

        private SettingsObserver settingsObserver = new SettingsObserver();



        public Athena(IServiceProvider provider) { 
            Logger = provider.GetService<Hermes.Hermes>() ?? throw new Exception("Hermes service doesn't exist.");
            LoggerContext = "Athena";
            Log("Initializing Athena.");
            odin = provider.GetService<Odin.Odin>() ?? throw new Exception("Odin service doesn't exist.");
            odin.Subscribe(this);
            Log("Odin service subscribed.");
            var api = provider.GetService<IWorkflowDesignApi>() ?? throw new Exception("IWorkflowDesign service doesn't exist.");
            
            settings = new Settings(api);
            Log("Settings initialized.");
            projectPath = api.ProjectPropertiesService.GetProjectDirectory();
            outputRoot = Path.Combine(projectPath, settings.OutputPath);
            configFilePath = Path.Combine(projectPath, settings.ConfigFilePath);
            var configExists = CheckConfigExists();
            if (configExists)
            {
                Log($"Config file path initialized to '{configFilePath}'.");
                UpdateConfigClasses();
            }
            CreateBaseClassFile(settings.OutputNamespace);
            api.Settings.RegisterValueChangedObserver(settingsObserver);
            settingsObserver.ValueChanged += OnSettingChanged;
            Log("Athena initialized.");
        }

        public async void UpdateConfigClasses()
        {
            configObjects = new List<ConfigObject>();
            await ReadConfigFile();
            foreach (var configObject in configObjects)
            {
                WriteConfig(Path.Combine(outputRoot, configObject.ClassName + ".cs"), configObject);
            }
        }

        public bool CheckConfigExists()
        {
            var exists = System.IO.File.Exists(configFilePath);
            if(!exists)
            {
                Log($"Could not find config file with path '{configFilePath}'. Please update the Config File Path setting under Athena in the project settings.", LogLevel.Error);
               
            }
            return exists;
        }

        public void OnSettingChanged(SettingsValueChangedArgs e)
        {
            Log($"Setting changed: {e.ChangedSettings}");
            if(e.ChangedSettings.Contains(SettingKeys.ConfigFilePathSettingKey))
            {
                configFilePath = Path.Combine(projectPath, settings.ConfigFilePath);
                Log($"Config file path updated to '{configFilePath}'.");
                var exists = CheckConfigExists();
                if (exists)
                {
                    UpdateConfigClasses();
                }
            }
        }

        public bool IsInterestedIn(string filePath)
        {
            return filePath.ToLower().Trim() == configFilePath.ToLower().Trim();
        }

        public void OnFileSystemEvent(FileSystemEventArgs e, Odin.EventType eventType)
        {
            Log($"{eventType.ToString()} : {e.FullPath}", LogLevel.Info);
            var exists = CheckConfigExists();
            if (!exists) return;
            if (e.FullPath == configFilePath)
            {
                Log($"Config file {eventType.ToString()} event detected.");
                if (eventType == Odin.EventType.Deleted)
                {
                    Log($"Config file deleted. Please update the Config File Path setting under Athena in the project settings.", LogLevel.Error);
                }
                else
                {
                    UpdateConfigClasses();
                }
            }
        }

        // Notify for renamed events
        public void OnRenamedEvent(RenamedEventArgs e)
        {
            if(e.OldFullPath == configFilePath)
            {
                Log($"Config file renamed to {e.FullPath}.");
                var ask = MessageBox.Show($"Config file renamed to {e.FullPath}. Would you like to update the setting?", "Athena", MessageBoxButton.YesNo);
                if (ask == MessageBoxResult.Yes)
                {
                    settings.ConfigFilePath = Path.GetRelativePath(projectPath, e.FullPath);
                    UpdateConfigClasses();
                }
                else CheckConfigExists();
                
            }
        }

        public async void CreateBaseClassFile(string ns)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns} {{"); // open
            sb.AppendLine($"\tpublic class BaseConfig {{"); // open
            sb.AppendLine("\t\t// Houses the actual values of the config");
            sb.AppendLine($"\t\tpublic Dictionary<string, object> _config = new Dictionary<string, object>();");
            sb.AppendLine();
            sb.AppendLine("\t\t// Generic getter method, now safely casts to the requested type");
            sb.AppendLine($"\t\tpublic T Get<T>(string name) {{"); // open
            sb.AppendLine($"\t\t\tif (_config.ContainsKey(name)) return (T)_config[name];");
            sb.AppendLine($"\t\t\tthrow new KeyNotFoundException($\"Key '\" + name + \"' not found in the config.\");");
            sb.AppendLine($"\t\t}}"); // close
            sb.AppendLine();
            sb.AppendLine($"\t\t// Setter with type safety");
            sb.AppendLine($"\t\tpublic void Set(string name, object value) {{"); // open
            sb.AppendLine($"\t\t\tif (value == null) throw new ArgumentNullException(nameof(value), \"Value cannot be null.\");");
            sb.AppendLine($"\t\t\t_config[name] = value;");
            sb.AppendLine($"\t\t}}"); // close
            sb.AppendLine($"\t}}"); // close
            sb.AppendLine($"}}"); // close
            if(!Directory.Exists(outputRoot))
            {
                Directory.CreateDirectory(outputRoot);
            }
            await System.IO.File.WriteAllTextAsync(Path.Combine(outputRoot, "BaseConfig.cs"), sb.ToString());
        }

        public async Task ReadConfigFile()
        {
            var exists = CheckConfigExists();
            if(!exists) return;

            Log($"Reading config file: {configFilePath}");

            try
            {
                if (settings.ConfigFileType == ConfigFileType.Json.Value)
                {
                    // Read the JSON file
                    string json = await System.IO.File.ReadAllTextAsync(configFilePath);
                    var configObject = JsonConvert.DeserializeObject<ConfigObject>(json);

                    if (configObject == null)
                    {
                        Log("Failed to deserialize config file as single config.", LogLevel.Error);
                        return;
                    }
                    var cObjects = JsonConvert.DeserializeObject<Dictionary<string,ConfigObject>>(json);
                    if(cObjects == null)
                    {
                        Log("Failed to deserialize config file as multiple configs.", LogLevel.Error);
                        return;
                    }
                }
                else if (settings.ConfigFileType == ConfigFileType.Excel.Value)
                {
                    // Read and process the Excel file
                    var dataSet = await ExcelHandler.ReadExcelConfigFile(configFilePath, this);
                    var configObject = new ConfigObject("", dataSet, this);
                    configObject.ClassName = "Config";
                    configObjects.Add(configObject);
                    Log($"Excel file processed. Found {configObject.Settings.Count} settings, {configObject.Assets.Count} assets, {configObject.TextFiles.Count} text files, and {configObject.ExcelFiles.Count} excel files.", LogLevel.Debug);
                }
                else
                {
                    Log($"Unsupported config file type: {settings.ConfigFileType}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Log($"Error reading config file: {ex.Message}", LogLevel.Error);
            }
        }

        public async void WriteConfig(string path, ConfigObject configObject)
        {
            
            var classString = configObject.GetClassString(settings.OutputNamespace, this);
            if(!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            await System.IO.File.WriteAllTextAsync(path, classString);
        }



        public void OnDisposed()
        {
            Log($"Odin Disposed", LogLevel.Info);
        }
    }
}
