using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyFramework.DX.Services.Brahma.Models.UiPathProjectPrimitives
{
    public class FileInfo
    {
        public string EditingStatus { get; set; } = "InProgress";
        public string TestCaseId { get; set; }
        public string TestCaseType { get; set; } = "TestCase";
        public string ExecutionTemplateType { get; set; } = "Local";
        public string FileName { get; set; }

        public FileInfo(string testCaseId, string fileName)
        {
            TestCaseId = testCaseId;
            FileName = fileName;
        }
    }
}
