﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LazyFramework.Models;
using LazyFramework.Services.Hermes;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api;



namespace LazyFramework.Services.Odin
{
    public class Odin : LoggerConsumer, IPublisherService, IDisposable
    {
        private IWorkflowDesignApi Api { get; set; }
        private Settings _settings;
        private readonly FileSystemWatcher _fileWatcher;
        private readonly List<IOdinSubscriber> _subscribers;
        public Odin(IServiceProvider provider)
        {
            try
            {

                Api = (IWorkflowDesignApi)(provider.GetService(typeof(IWorkflowDesignApi)) ?? throw new Exception("Workflow design api is not initialized."));
                _settings = new Settings(Api);
                Logger = (Hermes.Hermes)(provider.GetService(typeof(Hermes.Hermes)) ?? throw new Exception("Hermes service is not initialized."));
                LoggerContext = "Odin";
                Log("Initializing Odin."); // Error here 'No key exists for 'Hermes' ???????
                _subscribers = new List<IOdinSubscriber>();
                var folderPath = Api.ProjectPropertiesService.GetProjectDirectory();
                if (!Directory.Exists(folderPath))
                {
                    Log($"Directory does not exist: {folderPath}", LogLevel.Error);
                    throw new DirectoryNotFoundException($"Directory does not exist: {folderPath}");
                }


                _fileWatcher = new FileSystemWatcher
                {
                    Path = folderPath,
                    NotifyFilter = NotifyFilters.FileName |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.Size,
                    Filter = "*.*",
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true
                };

                _fileWatcher.Changed += (s, e) => PublishEvent(e, EventType.Changed);
                _fileWatcher.Created += (s, e) => PublishEvent(e, EventType.Created);
                _fileWatcher.Deleted += (s, e) => PublishEvent(e, EventType.Deleted);
                _fileWatcher.Renamed += (s, e) => PublishRenamedEvent(e);
                Log("Odin initialized.");
                Log($"Watching directory: {folderPath}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize Odin: {ex.Message}");
                throw;

            }
        }

        public void Subscribe(IOdinSubscriber subscriber)
        {
            if (subscriber != null && !_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                Log($"Subscriber added: {subscriber.GetType().Name}", LogLevel.Info);
            }
        }

        public void Unsubscribe(IOdinSubscriber subscriber)
        {
            if (subscriber != null && _subscribers.Contains(subscriber))
            {
                _subscribers.Remove(subscriber);
                Log($"Subscriber removed: {subscriber.GetType().Name}", LogLevel.Info);
            }
        }

        public void PublishEvent(FileSystemEventArgs e, EventType eventType)
        {
            if (!IsIgnored(e.FullPath))
            {
                foreach (var subscriber in _subscribers)
                {
                    if (subscriber.IsInterestedIn(e.FullPath))
                    {
                        subscriber.OnFileSystemEvent(e, eventType);
                    }
                }
            }
        }

        public void PublishRenamedEvent(RenamedEventArgs e)
        {
            if (!IsIgnored(e.FullPath) && !IsIgnored(e.OldFullPath))
            {
                foreach (var subscriber in _subscribers)
                {
                    if (subscriber.IsInterestedIn(e.FullPath) || subscriber.IsInterestedIn(e.OldFullPath))
                    {
                        subscriber.OnRenamedEvent(e);
                    }
                }
            }
        }

        private bool IsIgnored(string filePath)
        {
            return _settings.FilesToIgnore.Any(ignorePath => filePath.StartsWith(ignorePath, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            _fileWatcher?.Dispose();
            foreach (var subscriber in _subscribers) subscriber.OnDisposed();
            Log("File watcher service disposed.", LogLevel.Info);
        }


    }
    public enum EventType
    {
        Created,
        Deleted,
        Changed
    }

}
