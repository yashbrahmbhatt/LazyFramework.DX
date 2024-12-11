using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api.Wizards;
using UiPath.Studio.Activities.Api;
using System.Windows.Input;
using System.Activities;
using Activity = System.Activities.Activity;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LazyFramework.DX.Services.Hermes;
using LazyFramework.DX.Services.Odin;
using LazyFramework.DX.Services.Athena;
using Newtonsoft.Json;
using System.Reflection;
using LazyFramework.DX.Services.Nabu;
using System.Threading;

namespace LazyFramework
{
    public static class WizardCreator
    {

        public static void CreateWizards(IWorkflowDesignApi workflowDesignApi, Hermes hermes)
        {
                DX.Services.Hermes.Wizard.CreateWizard(workflowDesignApi, hermes);
        }

    }
}
