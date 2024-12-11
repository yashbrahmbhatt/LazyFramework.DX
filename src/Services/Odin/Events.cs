using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Odin
{
    public class SettingChangedEvent
    {
        public string Key { get; set; }
        public SettingChangedEvent(string key)
        {
            Key = key;
        }
    }
}
