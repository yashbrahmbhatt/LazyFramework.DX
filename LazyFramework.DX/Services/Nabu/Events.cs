using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;

namespace LazyFramework.DX.Services.Nabu
{
    public class WorkflowChangedEvent
    {
        public string Path { get; set; }
        public Editor? Editor { get; set; }
        public WatcherChangeTypes EventType { get; set; }
        public WorkflowChangedEvent(string path, WatcherChangeTypes type, Editor? editor)
        {
            Path = path;
            EventType = type;
            Editor = editor;
        }
    }
}
