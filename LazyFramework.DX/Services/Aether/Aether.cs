
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.DX.Models;
using LazyFramework.DX.Models.Consumers;
using LazyFramework.DX.Services.Brahma.Models;
using LazyFramework.DX.Services.Hermes;
using Newtonsoft.Json;
using NuGet.Configuration;
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
        private string _staticSiteRoot;
        public Aether(IWorkflowDesignApi api, Hermes.Hermes hermes) : base(api, hermes, "Aether")
        {
            var nugetSettings = NuGet.Configuration.Settings.LoadDefaultSettings(root: null);
            var packageCachePath = SettingsUtility.GetGlobalPackagesFolder(nugetSettings);
            var project = JsonConvert.DeserializeObject<UiPathProject>(File.ReadAllText(Path.Combine(_api.ProjectPropertiesService.GetProjectDirectory(), "project.json")));
            var version = project.Dependencies.First(d => d.Key == "LazyFramework.DX").Value.Replace("[", "").Replace("]", "");
            _staticSiteRoot = Path.Combine(packageCachePath, "LazyFramework.DX", version, "content", "StaticSiteFiles");
            Log($"Initializing Aether with static site root: {_staticSiteRoot}");
            InitializeListener();
            InitializeThread();
            StartThread();
        }

        public void InitializeListener()
        {
            Log("Initializing listener on " + _baseUrl);
            _listener.Prefixes.Add(_baseUrl);
            _listener.Start();
            Log("Listener initialized.");
        }

        public void InitializeThread()
        {
            _thread = new Thread(async () =>
            {
                _token = _cts.Token;
                var first = true;
                while (!_token.IsCancellationRequested)
                {
                    if(first) Log("Listening for requests...");
                    HttpListenerContext context = _listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    response.Headers.Add("Pragma", "no-cache");
                    response.Headers.Add("Expires", "0");
                    // Add CORS headers
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    Log("Request received to path " + request.Url.AbsolutePath);
                    if (request.Url.AbsolutePath.StartsWith("/dev") || request.Url.AbsolutePath.Contains("/_app"))
                    {   
                        Log("Serving static files...");
                        // Serve static files
                        ServeStaticFiles(request, response);
                    }
                    else if (request.IsWebSocketRequest)
                    {
                        // Handle WebSocket requests
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                        var socket = webSocketContext.WebSocket;
                        _hermes.AddClient(socket);
                        HandleWebSocketCommunication(socket);
                    }
                    else
                    {
                        // Handle API requests
                        HandleApiRequest(request, response);
                    }
                }
            });
        }

        private void ServeStaticFiles(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                string basePath = _staticSiteRoot; // Replace with your actual static files path
                string relativePath = request.Url.AbsolutePath.Replace("/dev", "").TrimStart('/').Split('?')[0];
                if(relativePath.Contains("_app"))
                {
                    relativePath = relativePath.Substring(relativePath.IndexOf('_'));
                }
                string filePath = Path.Combine(basePath, relativePath.Replace("/", "\\"));
                // Default to index.html for directories or non-existent files
                Log("Static file path to serve: " + filePath);
                if (string.IsNullOrEmpty(Path.GetExtension(filePath)) || !File.Exists(filePath))
                {
                    filePath = Path.Combine(basePath, "index.html");
                }

                if (File.Exists(filePath))
                {
                    Log("Sending file: " + filePath + " with mime type " + GetMimeType(filePath ));
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        response.ContentType = GetMimeType(filePath);
                        //response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                        //response.Headers.Add("Pragma", "no-cache");
                        //response.Headers.Add("Expires", "0");

                        response.StatusCode = 200;
                        fileStream.CopyTo(response.OutputStream);
                    }
                }
                else
                {
                    MessageBox.Show($"Static file not found: {filePath}");
                    response.StatusCode = 404;
                    byte[] errorBytes = Encoding.UTF8.GetBytes("File not found.");
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Log($"Error serving file: {ex.Message}");
                response.StatusCode = 500;
                byte[] errorBytes = Encoding.UTF8.GetBytes("Internal server error.");
                response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            }
            finally
            {
                response.Close();
            }
        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".html" => "text/html",
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }

        private void HandleApiRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
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
                    message = new
                    {
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
        private async Task HandleWebSocketCommunication(WebSocket webSocket)
        {
            try
            {
                var buffer = new byte[1024];
                WebSocketReceiveResult result;

                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            Log("WebSocket client disconnected.");
                            _hermes.RemoveClient(webSocket);
                            break; // Exit the loop since the client disconnected
                        }

                        // Handle other message types, such as text or binary
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            Log($"Received message: {message}");
                            // Process the received message
                        }
                    }
                    catch (WebSocketException wsEx)
                    {
                        Log($"WebSocketException: {wsEx.Message}");
                        break; // Exit the loop on WebSocket exceptions
                    }
                    catch (Exception ex)
                    {
                        Log($"Exception in WebSocket receive loop: {ex.Message}");
                        break; // Exit the loop on other exceptions
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Outer exception: {JsonConvert.SerializeObject(ex, Formatting.Indented)}");
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleaning up", CancellationToken.None);
                    Log("WebSocket closed in finally block.");
                }
                _hermes.RemoveClient(webSocket); // Ensure cleanup
            }
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
