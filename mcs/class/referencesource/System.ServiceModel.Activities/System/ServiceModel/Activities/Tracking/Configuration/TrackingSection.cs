//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System;
    using System.Configuration;
    using System.Activities.Tracking;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class TrackingSection : ConfigurationSection
    {
        Collection<TrackingProfile> trackingProfiles;

        ConfigurationPropertyCollection properties;

        public TrackingSection()
        {
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
                    properties.Add(new ConfigurationProperty(TrackingConfigurationStrings.Profiles, typeof(System.ServiceModel.Activities.Tracking.Configuration.ProfileElementCollection), null, null, null, System.Configuration.ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty(TrackingConfigurationStrings.Profiles)]
        public ProfileElementCollection Profiles
        {
            get
            {
                return (ProfileElementCollection)base[TrackingConfigurationStrings.Profiles];
            }
        }
    
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property returns a list of profiles in format suitable for the runtime")]
        public Collection<TrackingProfile> TrackingProfiles
        {
            get
            {
                if (this.trackingProfiles == null)
                {
                    this.trackingProfiles = new Collection<TrackingProfile>();

                    foreach (ProfileElement profileElement in this.Profiles)
                    {
                        if (profileElement.Workflows != null)
                        {
                            foreach (ProfileWorkflowElement workflowElement in profileElement.Workflows)
                            {
                                TrackingProfile profile = new TrackingProfile()
                                {
                                    Name = profileElement.Name,
                                    ImplementationVisibility = profileElement.ImplementationVisibility,
                                    ActivityDefinitionId = workflowElement.ActivityDefinitionId
                                };

                                workflowElement.AddQueries(profile.Queries);

                                this.trackingProfiles.Add(profile);
                            }
                        }
                    }
                }

                return this.trackingProfiles;
            }
        }
    }
}
