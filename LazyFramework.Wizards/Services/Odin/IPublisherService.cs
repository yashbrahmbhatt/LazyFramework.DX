using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.Services.Odin
{
    public interface IPublisherService
    {
        void Subscribe(IOdinSubscriber subscriber);
        void Unsubscribe(IOdinSubscriber subscriber);
        void PublishEvent(FileSystemEventArgs e, EventType eventType);
        void PublishRenamedEvent(RenamedEventArgs e);
    }
}
