#if(NET6_0_OR_GREATER)
extern alias SAM;
using SAM.System.Activities.Presentation.Metadata;
#endif
#if(NET461)
using System.Activities.Presentation.Metadata;
#endif

using System;

using System.Diagnostics;
using UiPath.Studio.Activities.Api;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Windows;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using LazyFramework.DX.Services.Athena;
using LazyFramework.DX.Services.Nabu;
using LazyFramework.DX.Services.Heimdall;
using System.Linq;
using LazyFramework.DX.Services.Aether;
using System.Reflection;
using Newtonsoft.Json;
using LazyFramework.DX.Services.Brahma;

namespace LazyFramework
{
    public class RegisterMetadata : IRegisterMetadata
    {
        private static Hermes _hermes;
        private static Odin _odin;
        private static Athena _athena;
        private static Nabu _nabu;
        private static Heimdall _heimdall;
        private static Aether _aether;
        private static string _context = "LazyFramework";
        private static Brahma _brahma;

        private void InitializeServices(IWorkflowDesignApi api)
        {
            try
            {

                _hermes = new Hermes(api) ?? throw new Exception("Failed to initialize Hermes.");
                _odin = new Odin(api, _hermes) ?? throw new Exception("Failed to initialize Odin.");
                _athena = new Athena(api, _hermes, _odin) ?? throw new Exception("Failed to initialize Athena.");
                _nabu = new Nabu(api, _hermes, _odin) ?? throw new Exception("Failed to initialize Nabu.");
                _heimdall = new Heimdall(api, _hermes, _odin) ?? throw new Exception("Failed to initialize Heimdall.");
                _aether = new Aether(api, _hermes) ?? throw new Exception("Failed to initialize Aether.");
                _brahma = new Brahma(api, _hermes, _aether) ?? throw new Exception("Failed to initialize Brahma.");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ConfigureServices: {ex.Message}, {ex.StackTrace} ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        /// <summary>
        /// This method is discovered in Studio using reflection.
        /// If found, a reference to the studio api is passed
        public void Initialize(object argument)
        {
            try
            {
                var api = argument as IWorkflowDesignApi;
                if (api == null)
                {
                    return;
                }
                if (api.HasFeature(DesignFeatureKeys.Settings) && api.HasFeature(DesignFeatureKeys.Wizards))
                {
                    SettingsCreator.CreateSettings(api);
                    InitializeServices(api);
                    WizardCreator.CreateWizards(api, _hermes);
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}