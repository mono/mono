//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ServiceModelActivitiesSectionGroup : ConfigurationSectionGroup
    {
        public WorkflowHostingOptionsSection WorkflowHostingOptionsSection
        {
            get { return (WorkflowHostingOptionsSection)this.Sections[ConfigurationStrings.WorkflowHostingOptionsSectionName]; }
        }

        public static ServiceModelActivitiesSectionGroup GetSectionGroup(Configuration config)
        {
            if (config == null)
            {
                throw FxTrace.Exception.ArgumentNull("config");
            }

            return (ServiceModelActivitiesSectionGroup)config.SectionGroups[ConfigurationStrings.SectionGroupName];
        }
    }
}
