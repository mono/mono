//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;

    public sealed class WorkflowHostingOptionsSection : ConfigurationSection
    {
        public WorkflowHostingOptionsSection() : base()
        {
        }
        
        [ConfigurationProperty(ConfigurationStrings.OverrideSiteName, DefaultValue = "false")]
        public bool OverrideSiteName
        {
            get { return (bool)base[ConfigurationStrings.OverrideSiteName]; }
            set { base[ConfigurationStrings.OverrideSiteName] = value; }
        }

        internal static WorkflowHostingOptionsSection GetSection()
        {
            WorkflowHostingOptionsSection retval = (WorkflowHostingOptionsSection)ConfigurationManager.GetSection(ConfigurationStrings.WorkflowHostingOptionsSectionPath);
            if (retval == null)
            {
                retval = new WorkflowHostingOptionsSection();
            }
            
            return retval;
        }
    }
}
