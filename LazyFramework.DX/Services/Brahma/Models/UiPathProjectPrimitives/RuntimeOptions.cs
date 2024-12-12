using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives
{
    public class RuntimeOptions
    {
        public bool AutoDispose { get; set; } = false;
        public bool NetFrameworkLazyLoading { get; set; } = false;
        public bool IsPausable { get; set; } = true;
        public bool IsAttended { get; set; } = false;
        public bool RequiresUserInteraction { get; set; } = true;
        public bool SupportsPersistence { get; set; } = true;
        public string WorkflowSerialization { get; set; } = "DataContract";
        public List<string> ExcludedLoggedData { get; set; } = new List<string>() { "Private:*", "password", "Password" };
        public string ExecutionType { get; set; } = "Workflow";
        public bool ReadyForPiP { get; set; } = false;
        public bool StartsInPiP { get; set; } = false;
        public bool MustRestoreAllDependencies { get; set; } = true;
        public string PipType { get; set; } = "ChildSession";
    }
}
