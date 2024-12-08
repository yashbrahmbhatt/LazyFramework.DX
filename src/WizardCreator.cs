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
using LazyFramework.Wizards;

namespace LazyFramework
{
    public static class WizardCreator
    {
        public static void CreateWizards(IWorkflowDesignApi workflowDesignApi)
        {
            Wizards.Main.CreateWizard(workflowDesignApi);
        }

    }
}
