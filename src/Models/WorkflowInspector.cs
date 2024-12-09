using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Activities;
using System.Xaml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Windows;
using LazyFramework.DX.Services.Hermes;
using Newtonsoft.Json;
using XamlReader = System.Windows.Markup.XamlReader;
namespace LazyFramework.DX.Models
{
    public class WorkflowInspector
    {
        public string Path;
        public string Name;
        public string Description;

        public IDynamicActivity Root;
        public Dictionary<string, IDynamicActivity> Activities;

        private Action<string, LogLevel?> Log;

        
        public WorkflowInspector(string path, Action<string, LogLevel?> log)
        {
            Log = log;
            LoadWorkflow(path);
        }

        public void LoadWorkflow(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException($"Workflow file not found: {filePath}");

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Log.Invoke($"Loading workflow '{filePath}'", LogLevel.Debug);

                var w = XamlReader.Load(fileStream);
                Log.Invoke($"{JsonConvert.SerializeObject(w, Formatting.Indented)}", LogLevel.Debug);
                Root = (IDynamicActivity)w;
                Activities = WorkflowHelpers.GetActivityMap(Root);
                Log.Invoke($"Workflow '{filePath}' loaded. Found {Activities.Count} activities.", LogLevel.Debug);
            }
        }

        public void SaveWorkflow(string filePath, Activity workflow)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow), "Workflow cannot be null.");

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                XamlServices.Save(fileStream, workflow);
            }
        }

        public void ModifyWorkflow(Activity workflow, Action<Activity> modification)
        {
            if (workflow == null)
                throw new ArgumentNullException(nameof(workflow), "Workflow cannot be null.");

            modification?.Invoke(workflow);
        }


    }

    public static class WorkflowHelpers
    {
        public static bool HasChildActivities(IDynamicActivity activity)
        {
            if (activity == null) return false;

            // Use reflection to inspect properties
            var properties = activity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value = property.GetValue(activity);
                if (value == null) continue;

                // Check if the property is a single Activity
                if (value is IDynamicActivity) return true;

                // Check if the property is a collection of Activities
                if (value is IEnumerable enumerable)
                {
                    if (enumerable.OfType<IDynamicActivity>().Any()) return true;
                }
            }

            return false;
        }

        public static IEnumerable<IDynamicActivity> GetChildActivities(IDynamicActivity activity)
        {
            if (activity == null) yield break;

            // Use reflection to inspect properties
            var properties = activity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value = property.GetValue(activity);
                if (value == null) continue;

                // If the property is a single Activity
                if (value is IDynamicActivity childActivity)
                {
                    yield return childActivity;
                }

                // If the property is a collection of Activities
                if (value is IEnumerable<IDynamicActivity> enumerable)
                {
                    foreach (var child in enumerable)
                    {
                        yield return child;
                    }
                }
            }
        }


        /// <summary>
        /// Retrieves a dictionary of Activity IDs and their corresponding Activity instances from a root activity.
        /// </summary>
        /// <param name="rootActivity">The root activity to traverse.</param>
        /// <returns>A dictionary where the key is the Activity ID and the value is the Activity instance.</returns>
        public static Dictionary<string, IDynamicActivity> GetActivityMap(IDynamicActivity rootActivity)
        {
            var activityMap = new Dictionary<string, IDynamicActivity>();
            PopulateActivityMap(rootActivity, activityMap);
            return activityMap;
        }

        private static void PopulateActivityMap(IDynamicActivity activity, Dictionary<string, IDynamicActivity> activityMap)
        {
            if (activity == null) return;

            // Add the activity to the dictionary
            activityMap[activity.Name] = activity;

            // Get child activities
            var childActivities = GetChildActivities(activity);

            // Recursively process child activities
            foreach (var child in childActivities)
            {
                PopulateActivityMap(child, activityMap);
            }
        }
    }
}
