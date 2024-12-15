using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using LazyFramework.DX.Helpers;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Athena.Models;
using LazyFramework.DX.Services.Brahma.Models;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using LazyFramework.DX.Services.Nabu.WorkflowEditor.Primitives;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Expressions;
using UiPath.Studio.Activities.Api.ProjectProperties;

namespace LazyFramework.DX.Services.Brahma
{
    public class Brahma : AetherConsumer
    {
        private Solution _solution;
        public Dictionary<string, string> Files = new Dictionary<string, string>();
        public string Name;
        public string Id;
        public string Root;
        public Version StudioVersion;
        public ExpressionLanguage ExpressionLanguage;
        public string TargetFramework;

        public Brahma(IWorkflowDesignApi api, Hermes.Hermes hermes, Aether.Aether aether) : base(api, hermes, aether, "Brahma")
        {
            Log("Initializing Brahma");
            
            _aether.Register("solution", SolutionEndpoint);
            Log($"Brahma initialized.");
        }
        public class TokenClass
        {
            public IEnumerable<Token> Tokens;
            public string Value;
            public TokenClass(IEnumerable<Token> tokens, string value)
            {
                Tokens = tokens;
                Value = value;
            }
        }

        public object SolutionEndpoint(HttpListenerRequest request)
        {
            Log($"ProjectEndpoint called with method {request.HttpMethod}");
            if (request.HttpMethod == "GET")
            {
                Files = new Dictionary<string, string>();
                var Expressions = new Dictionary<string, Dictionary<string, List<TokenClass>>>();
                var files = Directory.GetFiles(Path.Combine(_api.ProjectPropertiesService.GetProjectDirectory()), "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var relativePath = PathExtensions.GetRelativePath(_api.ProjectPropertiesService.GetProjectDirectory(), file);
                    if (relativePath.StartsWith(".")) continue;
                    var raw = File.ReadAllText(file);
                    if(file.EndsWith(".xaml"))
                    {
                        var editor = new Editor(file, _hermes, _api);
                        Log(editor.Expressions.Count + " expressions found in " + file);
                        var fileExpressions = new Dictionary<string, List<TokenClass>>();
                        foreach (var exp in editor.Expressions)
                        {
                            if(!fileExpressions.ContainsKey(exp.Path))
                            {
                                fileExpressions.Add(exp.Path, new List<TokenClass>());
                            }
                            fileExpressions[exp.Path].Add(new TokenClass(_api.ExpressionService.GetIdentifierAndLiteralList(exp.Value, _api.ProjectPropertiesService.GetExpressionLanguage() == 0 ? "VisualBasic" : "CSharp"), exp.Value));
                            Log($"Expression {exp.Value} found in editor {editor.Path}");
                        }
                        Expressions.Add(file, fileExpressions);
                    }
                    Files.Add(file, raw);
                }
                Name = _api.ProjectPropertiesService.GetProjectName();
                Id = _api.ProjectPropertiesService.GetProjectId();
                Root = _api.ProjectPropertiesService.GetProjectDirectory();
                ExpressionLanguage = (ExpressionLanguage)_api.ProjectPropertiesService.GetExpressionLanguage();
                TargetFramework = _api.ProjectPropertiesService.GetTargetFramework();
                StudioVersion = _api.StudioDesignSettings.Version;
                return new {
                    Files,
                    Name,
                    Id,
                    Root,
                    StudioVersion,
                    ExpressionLanguage,
                    TargetFramework,
                    Expressions
                };
            }
            else if (request.HttpMethod == "POST")
            {
                object message;
                try
                {
                    _solution = JsonConvert.DeserializeObject<Solution>(new StreamReader(request.InputStream).ReadToEnd());
                    message = new { message = "Solution updated.", status = 200 };
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to update solution.", ex);
                }

            }
            throw new Exception("Invalid request method.");
        }
    }
}
