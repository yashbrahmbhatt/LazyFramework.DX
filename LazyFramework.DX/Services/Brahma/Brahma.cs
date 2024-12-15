using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.DX.Helpers;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Athena.Models;
using LazyFramework.DX.Services.Brahma.Models;
using LazyFramework.DX.Services.Nabu.WorkflowEditor;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.ProjectProperties;

namespace LazyFramework.DX.Services.Brahma
{
    public class Brahma : AetherConsumer
    {
        private Solution _solution;

        public Brahma(IWorkflowDesignApi api, Hermes.Hermes hermes, Aether.Aether aether) : base(api, hermes, aether, "Brahma")
        {
            Log("Initializing Brahma");
            _solution = new Solution(api, hermes);
            _aether.Register("solution", SolutionEndpoint);
            Log($"Brahma initialized.");
        }

        public object SolutionEndpoint(HttpListenerRequest request)
        {
            Log($"ProjectEndpoint called with method {request.HttpMethod}");
            if (request.HttpMethod == "GET")
            {
                return new { solution = _solution.ToObject() };
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
