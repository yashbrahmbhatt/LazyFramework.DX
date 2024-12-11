using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml.Drawing;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Services.Heimdall
{
    public class Heimdall : OdinConsumer
    {
        private readonly FileSystemWatcher _fileWatcher;
        private Settings _settings;

        public Heimdall(IWorkflowDesignApi api, Hermes.Hermes hermes, Odin.Odin odin) : base(api, hermes, odin, "Heimdall")
        {
            Log("Initializing Heimdall service");
            var directoryPath = api.ProjectPropertiesService.GetProjectDirectory();
            _settings = new Settings(api);
            if (!Directory.Exists(directoryPath))
            {
                Log($"Directory not found: {directoryPath}", LogLevel.Error);
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }
            Log(string.Join("\n",_settings.FilesToIgnore), LogLevel.Error);
            _fileWatcher = new FileSystemWatcher(directoryPath)
            {
                NotifyFilter = NotifyFilters.FileName |
                               NotifyFilters.LastWrite |
                               NotifyFilters.CreationTime |
                               NotifyFilters.Size,
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += async (s, e) => await OnEvent(e);
            _fileWatcher.Created += async (s, e) => await OnEvent(e);
            _fileWatcher.Deleted += async (s, e) => await OnEvent(e);
            _fileWatcher.Renamed += async (s, e) => await OnEvent(e);
            NotifyInitialization();
            Log($"Heimdall service initialized");
        }
        public FileType GetFileType(string filePath)
        {
            // Get the file extension (including the dot, e.g., .txt, .xls, .md)
            string extension = Path.GetExtension(filePath)?.ToLower();

            // Determine the FileType based on the extension
            return extension switch
            {
                ".xaml" => FileType.XamlFile,
                ".xlsx" => FileType.ExcelFile,
                ".xls" => FileType.ExcelFile,
                ".docx" => FileType.WordFile,
                ".doc" => FileType.WordFile,
                ".md" => FileType.MarkdownFile,
                ".txt" => FileType.TextFile,
                ".json" => FileType.JsonFile,
                ".cs" => FileType.CSFile,
                _ when Directory.Exists(filePath) => FileType.Directory,
                _ => FileType.OtherFile,
            };
        }
        public async Task NotifyInitialization()
        {
            Log("Notifying initialization");
            var files = Directory.GetFiles(_fileWatcher.Path, "*.*", SearchOption.AllDirectories);
            foreach (var filePath in files)
            {
                var fileType = GetFileType(filePath);
                var eventType = WatcherChangeTypes.Created;
                switch (fileType)
                {
                    case FileType.XamlFile:
                        await _odin.Notify<XamlFileEvent>(new XamlFileEvent(filePath, eventType));
                        break;
                    case FileType.ExcelFile:
                        await _odin.Notify<ExcelFileEvent>(new ExcelFileEvent(filePath, eventType));
                        break;
                    case FileType.WordFile:
                        await _odin.Notify<WordFileEvent>(new WordFileEvent(filePath, eventType));
                        break;
                    case FileType.MarkdownFile:
                        await _odin.Notify<MarkdownFileEvent>(new MarkdownFileEvent(filePath, eventType));
                        break;
                    case FileType.TextFile:
                        await _odin.Notify<TextFileEvent>(new TextFileEvent(filePath, eventType));
                        break;
                    case FileType.OtherFile:
                        await _odin.Notify<OtherFileEvent>(new OtherFileEvent(filePath, eventType));
                        break;
                    case FileType.Directory:
                        await _odin.Notify<DirectoryEvent>(new DirectoryEvent(filePath, eventType));
                        break;
                    case FileType.JsonFile:
                        await _odin.Notify<JsonFileEvent>(new JsonFileEvent(filePath, eventType));
                        break;
                    case FileType.CSFile:
                        await _odin.Notify<CSFileEvent>(new CSFileEvent(filePath, eventType));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(fileType), "Unsupported file type");
                }
            }
        }

        public async Task OnEvent(FileSystemEventArgs e)
        {
            var filePath = e.FullPath;
            if (_settings.FilesToIgnore.Contains(filePath)) { 
                //Log($"Ignoring file {filePath}", LogLevel.Debug);
                return;
            }
            var fileType = GetFileType(e.FullPath);
            var eventType = e.ChangeType;
            switch (fileType)
            {
                case FileType.XamlFile:
                    await _odin.Notify<XamlFileEvent>(new XamlFileEvent(filePath, eventType));
                    break;
                case FileType.ExcelFile:
                    await _odin.Notify<ExcelFileEvent>(new ExcelFileEvent(filePath, eventType));
                    break;
                case FileType.WordFile:
                    await _odin.Notify<WordFileEvent>(new WordFileEvent(filePath, eventType));
                    break;
                case FileType.MarkdownFile:
                    await _odin.Notify<MarkdownFileEvent>(new MarkdownFileEvent(filePath, eventType));
                    break;
                case FileType.TextFile:
                    await _odin.Notify<TextFileEvent>(new TextFileEvent(filePath, eventType));
                    break;
                case FileType.OtherFile:
                    await _odin.Notify<OtherFileEvent>(new OtherFileEvent(filePath, eventType));
                    break;
                case FileType.Directory:
                    await _odin.Notify<DirectoryEvent>(new DirectoryEvent(filePath, eventType));
                    break;
                case FileType.JsonFile:
                    await _odin.Notify<JsonFileEvent>(new JsonFileEvent(filePath, eventType));
                    break;
                case FileType.CSFile:
                    await _odin.Notify<CSFileEvent>(new CSFileEvent(filePath, eventType));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), "Unsupported file type");
            }
        }

        public async Task OnEvent(RenamedEventArgs e)
        {
            var filePath = e.FullPath;
            if (_settings.FilesToIgnore.Contains(filePath))
            {
                Log($"Ignoring file {filePath}", LogLevel.Debug);
                return;
            }
            var fileType = GetFileType(e.FullPath);
            var eventType = e.ChangeType;
            switch (fileType)
            {
                case FileType.XamlFile:
                    await _odin.Notify<XamlFileEvent>(new XamlFileEvent(filePath, eventType));
                    break;
                case FileType.ExcelFile:
                    await _odin.Notify<ExcelFileEvent>(new ExcelFileEvent(filePath, eventType));
                    break;
                case FileType.WordFile:
                    await _odin.Notify<WordFileEvent>(new WordFileEvent(filePath, eventType));
                    break;
                case FileType.MarkdownFile:
                    await _odin.Notify<MarkdownFileEvent>(new MarkdownFileEvent(filePath, eventType));
                    break;
                case FileType.TextFile:
                    await _odin.Notify<TextFileEvent>(new TextFileEvent(filePath, eventType));
                    break;
                case FileType.OtherFile:
                    await _odin.Notify<OtherFileEvent>(new OtherFileEvent(filePath, eventType));
                    break;
                case FileType.Directory:
                    await _odin.Notify<DirectoryEvent>(new DirectoryEvent(filePath, eventType));
                    break;
                case FileType.JsonFile:
                    await _odin.Notify<JsonFileEvent>(new JsonFileEvent(filePath, eventType));
                    break;
                case FileType.CSFile:
                    await _odin.Notify<CSFileEvent>(new CSFileEvent(filePath, eventType));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileType), "Unsupported file type");
            }
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
        }
    }
}
