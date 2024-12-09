using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Odin
{
    public interface IOdinSubscriber
    {
        // Check if the subscriber is interested in the file/folder based on the event
        bool IsInterestedIn(string filePath);

        // Notify for file system events
        void OnFileSystemEvent(FileSystemEventArgs e, EventType eventType);

        // Notify for renamed events
        void OnRenamedEvent(RenamedEventArgs e);

        void OnDisposed();
    }
}
