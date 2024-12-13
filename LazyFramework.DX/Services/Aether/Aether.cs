
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
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
                    if (request.IsWebSocketRequest)
                    {
                        //MessageBox.Show("Got a websocket request.");
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                        var socket = webSocketContext.WebSocket;
                        _hermes.AddClient(socket);
                        //MessageBox.Show("Added client.");
                        await HandleWebSocketCommunication(socket);
                    }
                    else
                    {

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

                }
            });
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
