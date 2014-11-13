//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking
{
    using System.Runtime.Remoting.Messaging;
    using System.Runtime;
    using System.Activities.Tracking;
    using System.Collections.Specialized;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Activities.Tracking.Configuration;

    class DefaultProfileManager : TrackingProfileManager
    {
        ConfigFileProfileStore profileStore;

        public DefaultProfileManager()
        {
        }

        ConfigFileProfileStore ProfileStore
        {
            get
            {
                if (this.profileStore == null)
                {
                    this.profileStore = new ConfigFileProfileStore();
                }
                return this.profileStore;
            }
        }


        public override TrackingProfile Load(string profileName, string activityDefinitionId, TimeSpan timeout)
        {
            if (profileName == null)
            {
                throw FxTrace.Exception.ArgumentNull("profileName");
            }
            return this.GetProfile(profileName, activityDefinitionId);
        }

        internal TrackingProfile GetProfile(string profileName, string activityDefinitionId)
        {
            // Get all profiles from the store
            Collection<TrackingProfile> profiles = this.ProfileStore.ReadProfiles();

            TrackingProfile bestMatch = null;
            if (profiles != null)
            {
                // Go through all the profiles in the data store and find a match to the requested profile
                foreach (TrackingProfile profile in profiles)
                {
                    // Check the profile matches the requested name, and scope type
                    if (string.Compare(profileName, profile.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // If we find a global scope profile, use it as the default profile
                        if (string.Compare("*", profile.ActivityDefinitionId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (bestMatch == null)
                            {
                                bestMatch = profile;
                            }
                        }
                        else if (string.Compare(activityDefinitionId, profile.ActivityDefinitionId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //specific profile for scopetarget found. 
                            bestMatch = profile;
                            break;
                        }
                    }
                }
            }
            if (bestMatch == null)
            {
                //Add a trace message indicating tracking profile with activity definiton id is not found
                if (TD.TrackingProfileNotFoundIsEnabled())
                {
                    TD.TrackingProfileNotFound(profileName, activityDefinitionId);
                }

                //If the profile is not found in config, return an empty profile to suppress
                //events. If .config does not have profiles, return null.
                bestMatch = new TrackingProfile()
                {
                    ActivityDefinitionId = activityDefinitionId
                };
            }
            
            return bestMatch;
        }


        class ConfigFileProfileStore
        {
            Collection<TrackingProfile> trackingProfiles;

            public Collection<TrackingProfile> ReadProfiles()
            {
                if (this.trackingProfiles != null)
                {
                    return this.trackingProfiles;
                }

                TrackingSection trackingSection = null;

                try
                {
                    trackingSection =
                        (TrackingSection)ConfigurationHelpers.GetSection(ConfigurationHelpers.GetSectionPath(TrackingConfigurationStrings.Tracking));
                }
                catch (ConfigurationErrorsException e)
                {
                    if (!Fx.IsFatal(e))
                    {
                        FxTrace.Exception.TraceUnhandledException(e);
                    }

                    throw;
                }

                if (trackingSection == null)
                {
                    return null;
                }

                // Configuration elements are never null, collections are empty
                // and single elements are constructed with the property IsPresent=false
                this.trackingProfiles = new Collection<TrackingProfile>();

                foreach (ProfileElement profileElement in trackingSection.Profiles)
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

                return this.trackingProfiles;
            }

        }
    }
}
