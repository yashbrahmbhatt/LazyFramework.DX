#if NET6_0_OR_GREATER
extern alias SAM;
using SAM.System.Activities.Presentation.Metadata;
#endif
#if NETFRAMEWORK
using System.Activities.Presentation.Metadata;
#endif
using System;
using System.Diagnostics;
using UiPath.Studio.Activities.Api;

namespace LazyFramework
{
    public class RegisterMetadata : IRegisterMetadata
    {
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

                if (api.HasFeature(DesignFeatureKeys.Settings))
                {
                    SettingsCreator.CreateSettings(api);
                }

                if (api.HasFeature(DesignFeatureKeys.Wizards))
                {
                    WizardCreator.CreateWizards(api);
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}