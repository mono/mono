//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Activities.Tracking;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [Fx.Tag.XamlVisible(false)]
    public class EtwTrackingBehavior : IServiceBehavior
    {
        public EtwTrackingBehavior()
        {
        }

        public string ProfileName
        {
            get;
            set;
        }

        public virtual void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost workflowServiceHost = serviceHostBase as WorkflowServiceHost;
            if (null != workflowServiceHost)
            {
                string workflowDisplayName = workflowServiceHost.Activity.DisplayName;
                string hostReference = string.Empty;

                if (AspNetEnvironment.Enabled)
                {
                    VirtualPathExtension virtualPathExtension = serviceHostBase.Extensions.Find<VirtualPathExtension>();
                    if (virtualPathExtension != null && virtualPathExtension.VirtualPath != null)
                    {
                        //Format Website name\Application Virtual Path|\relative service virtual path|serviceName 
                        string name = serviceDescription != null ? serviceDescription.Name : string.Empty;
                        string application = virtualPathExtension.ApplicationVirtualPath;

                        //If the application is the root, do not include it in servicePath
                        string servicePath = virtualPathExtension.VirtualPath.Replace("~", application + "|");
                        hostReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", virtualPathExtension.SiteName, servicePath, name);
                    }
                }

                TrackingProfile trackingProfile = this.GetProfile(this.ProfileName, workflowDisplayName);
                workflowServiceHost.WorkflowExtensions.Add(
                    () => new EtwTrackingParticipant
                    {
                        ApplicationReference = hostReference,
                        TrackingProfile = trackingProfile
                    });
            }
        }

        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        TrackingProfile GetProfile(string profileName, string displayName)
        {
            DefaultProfileManager profileManager = new DefaultProfileManager();
            return profileManager.GetProfile(profileName, displayName);
        }
    }
}
