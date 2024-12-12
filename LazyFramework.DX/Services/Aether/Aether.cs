
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.DX.Models;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Hermes;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;

namespace LazyFramework.DX.Services.Aether
{
    public class Aether : HermesConsumer, IAetherImplementation
    {
        private HttpListener _listener = new HttpListener();
        private string _baseUrl = "http://localhost:7999/";
        private Thread _thread = new Thread(() => { });
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _token;
        private readonly ConcurrentDictionary<string, Func<HttpListenerRequest, object>> _handlers = new ConcurrentDictionary<string, Func<HttpListenerRequest, object>>();
        public Aether(IWorkflowDesignApi api, Hermes.Hermes hermes) : base(api, hermes, "Aether")
        {
            InitializeListener();
            InitializeThread();
            StartThread();
        }

        public void InitializeListener()
        {
            Log("Initializing listener on " + _baseUrl);
            _listener.Prefixes.Add(_baseUrl);
            _listener.Start();
            // Handle incoming requests
            Log("Listener initialized.");
        }

        public void InitializeThread()
        {
            _thread = new Thread(async () =>
            {
                _token = _cts.Token;
                while (!_token.IsCancellationRequested)
                {


                    Log("Waiting for request...");
                    // Wait for a request
                    HttpListenerContext context = _listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    // Add CORS headers
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                    // Log request details
                    Log($"Received request: {request.HttpMethod} {request.Url}");
                    var relativeUrl = new Uri(_baseUrl).MakeRelativeUri(request.Url).ToString();
                    var message = new object();
                    if (_handlers.ContainsKey(relativeUrl))
                    {
                        try
                        {
                            var handler = _handlers[relativeUrl];
                            message = handler.Invoke(request) ?? throw new Exception("Handler returned null.");

                        }
                        catch (Exception ex)
                        {
                            Log($"Error handling request: {ex.Message}", LogLevel.Error);
                            message = new {
                                Message = $"Internal server error: {ex.Message}\n{ex.StackTrace}"
                            };
                            response.StatusCode = 500;
                            
                        }
                    }
                    else
                    {
                        message = new
                        {
                            Message = "No endpoint handler has been registered for " + relativeUrl
                        };
                        response.StatusCode = 404;
                    }
                    var json = JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    Log($"Sending response: {json}");
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentType = "application/json";
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();


                }
            });
        }

        public async Task Register(string endpoint, Func<HttpListenerRequest, object> handler)
        {
            Log($"Registering handler for endpoint {endpoint}.");
            if (!_handlers.TryAdd(endpoint, handler))
                Log($"Handler registered for endpoint {endpoint}.");
            else
                Log(endpoint + " already registered.", LogLevel.Error);

        }

        public void StartThread()
        {
            Log("Starting listener thread...");
            _thread.Start();
            Log("Listener thread started.");
        }

        public void StopListener()
        {
            Log("Stopping aether listener...");
            _listener.Stop();
            Log("Aether listener stopped.");
        }
        public void StopThread()
        {
            Log("Stopping listener thread...");
            _cts.Cancel();
            Log("Listener thread stopped.");
        }

    }
}
