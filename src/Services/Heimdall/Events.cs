using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Heimdall
{
    public enum FileType
    {
        XamlFile,
        ExcelFile,
        CSFile,
        JsonFile,
        WordFile,
        MarkdownFile,
        TextFile,
        OtherFile,
        Directory
    }
    public class FileEvent
    {
        public string Path { get; set; }
        public WatcherChangeTypes EventType { get; set; }
        public FileType FileType { get; set; }
        public string? OldPath { get; set; }
        public FileEvent(string path, WatcherChangeTypes eventType, FileType fileType, string? oldPath = null)
        {
            Path = path;
            EventType = eventType;
            FileType = fileType;
        }
    }

    public class CSFileEvent : FileEvent
    {
        public CSFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
     : base(path, eventType, FileType.CSFile, oldPath)
        {
        }
    }
        public class JsonFileEvent : FileEvent
    {
        public JsonFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.JsonFile, oldPath)
        {
        }
    }
        public class XamlFileEvent : FileEvent
    {
        public XamlFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.XamlFile, oldPath)
        {
        }
    }

    public class ExcelFileEvent : FileEvent
    {
        public ExcelFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.ExcelFile, oldPath)
        {
        }
    }

    public class WordFileEvent : FileEvent
    {
        public WordFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.WordFile, oldPath)
        {
        }
    }

    public class MarkdownFileEvent : FileEvent
    {
        public MarkdownFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.MarkdownFile, oldPath)
        {
        }
    }

    public class TextFileEvent : FileEvent
    {
        public TextFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.TextFile, oldPath)
        {
        }
    }

    public class OtherFileEvent : FileEvent
    {
        public OtherFileEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.OtherFile, oldPath)
        {
        }
    }

    public class DirectoryEvent : FileEvent
    {
        public DirectoryEvent(string path, WatcherChangeTypes eventType, string? oldPath = null)
            : base(path, eventType, FileType.Directory, oldPath)
        {
        }
    }


}
