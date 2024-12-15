using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;
using UiPath.Studio.Activities.Api;
using LazyFramework.DX.Models.Consumers;
using Timer = System.Timers.Timer;
using System.Linq;
using System.Windows;

namespace LazyFramework.DX.Services.Hermes
{
    public class Hermes : BaseConsumer
    {
        private ConcurrentQueue<Log> _logs = new ConcurrentQueue<Log>();
        private Timer _debounceTimer = new Timer()
        {
            Interval = 500,
            AutoReset = false,
        };
        private bool _refreshPending = false;
        private ConcurrentBag<WebSocket> _clients = new ConcurrentBag<WebSocket>();  // To hold WebSocket clients

        private HttpListener _listener = new HttpListener();
        private string _baseUrl = "ws://localhost:7999/";
        private string _context = "Hermes"; 

        public Hermes(IWorkflowDesignApi api) : base(api)
        {
            _debounceTimer.Elapsed += OnDebounceElapsed;

            Log("Hermes initialized.", "Hermes", LogLevel.Info);
        }

        public void AddClient(WebSocket socket)
        {
            _clients.Add(socket);
            Log("WebSocket client connected.", _context);
        }

        private async Task BroadcastToClients(string message)
        {
            try
            {

                List<WebSocket> clientsToRemove = new List<WebSocket>();
                List<WebSocket> clientsCopy;
                lock (_clients)
                {
                    clientsCopy = new List<WebSocket>(_clients);
                }

                foreach (var client in _clients)
                {
                    if (client.State == WebSocketState.Open)
                    {
                        var buffer = Encoding.UTF8.GetBytes(message);
                        await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        clientsToRemove.Add(client);
                    }
                }

                lock (_clients)
                {

                    // Clean up closed clients
                    foreach (var client in clientsToRemove)
                    {
                        RemoveClient(client);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(JsonConvert.SerializeObject(ex, Formatting.Indented));
            }
            
        }
        public void RemoveClient(WebSocket socket)
        {
            _clients = new ConcurrentBag<WebSocket>(_clients.Where(s => s != socket));
            Log("WebSocket client disconnected.", _context);
        }

        public void Log(string message, string context, LogLevel level = LogLevel.Info)
        {
            var log = new Log(DateTime.Now, level, message, context);
            if (_logs.Count == 2000) _logs.TryDequeue(out var result);
            _logs.Enqueue(log);
            TriggerDebouncedRefresh();
        }

        private void TriggerDebouncedRefresh()
        {
            if (_refreshPending) return;
            _refreshPending = true;
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private async void OnDebounceElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_clients)
            {
                _refreshPending = false;
            }

            // Prepare message to send to clients
            var message = JsonConvert.SerializeObject(_logs);

            // Send logs to all connected clients
            await BroadcastToClients(message);
        }

    }
}
