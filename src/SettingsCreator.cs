﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Settings;

namespace LazyFramework
{
    public static class SettingsCreator
    {


        public static void CreateSettings(IWorkflowDesignApi workflowDesignApi)
        {
            new DX.Services.Odin.SettingsCategory().Initialize(workflowDesignApi.Settings);
            new DX.Services.Athena.SettingsCategory().Initialize(workflowDesignApi);
        }
    }
}
