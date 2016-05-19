//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Description;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    class ComPlusServiceLoader
    {
        ServiceInfo info;
        ConfigLoader configLoader;
        ComPlusTypeLoader typeLoader;

        public ComPlusServiceLoader (ServiceInfo info)
        {
            this.info = info;
            this.typeLoader = new ComPlusTypeLoader(info);
            this.configLoader = new ConfigLoader(typeLoader);
        }
        
        public ServiceDescription Load(ServiceHostBase host)
        {
            ServiceDescription service = new ServiceDescription(this.info.ServiceName);

            // ServiceBehaviorAttribute needs to go first in the behaviors collection (before config stuff)
            AddBehaviors(service);

            this.configLoader.LoadServiceDescription(host, service, this.info.ServiceElement, host.LoadConfigurationSectionHelper);

            ValidateConfigInstanceSettings(service);

            ComPlusServiceHostTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationServiceHostCreatedServiceEndpoint,
                                SR.TraceCodeComIntegrationServiceHostCreatedServiceEndpoint,  this.info, service.Endpoints);            

            return service;
        }


        void AddBehaviors(ServiceDescription service)
        {
            // The philosophy here is to respect settings from configuration
            // At the moment, none of the settings we care about can be modified
            // through configuration. That may change in the future.
            // However, we never want to silently overwrite a user's configuration.
            // So we should either accept overrides or reject them, but never 
            // silently update them.
            //

            ServiceBehaviorAttribute serviceBehavior = EnsureBehaviorAttribute(service);

            serviceBehavior.InstanceProvider = new ComPlusInstanceProvider(this.info);

            serviceBehavior.InstanceContextMode = InstanceContextMode.Single;

            // SHOULD: There is no reason to not allow concurrency at this level
            serviceBehavior.ConcurrencyMode = ConcurrencyMode.Multiple;
            serviceBehavior.UseSynchronizationContext = false;

            service.Behaviors.Add(new SecurityCookieModeValidator());

            if (AspNetEnvironment.Enabled)
            {
                AspNetCompatibilityRequirementsAttribute aspNetCompatibilityRequirements = service.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>();
                if (aspNetCompatibilityRequirements == null)
                {
                    aspNetCompatibilityRequirements = new AspNetCompatibilityRequirementsAttribute();
                    service.Behaviors.Add(aspNetCompatibilityRequirements);
                }
            }
        }

        ServiceBehaviorAttribute EnsureBehaviorAttribute(ServiceDescription service)
        {
            ServiceBehaviorAttribute serviceBehavior;
            if (service.Behaviors.Contains(typeof(ServiceBehaviorAttribute)))
            {
                serviceBehavior = (ServiceBehaviorAttribute)service.Behaviors[typeof(ServiceBehaviorAttribute)];
            }
            else
            {
                serviceBehavior = new ServiceBehaviorAttribute();
                service.Behaviors.Insert(0, serviceBehavior);
            }
            return serviceBehavior;
        }

        void ValidateConfigInstanceSettings(ServiceDescription service)
        {
            ServiceBehaviorAttribute serviceBehavior = EnsureBehaviorAttribute(service);

            foreach (ServiceEndpoint endpoint in service.Endpoints)
            {
                if (endpoint != null && !endpoint.InternalIsSystemEndpoint(service))
                {
                    if (endpoint.Contract.SessionMode == SessionMode.Required)
                    {
                        if (serviceBehavior.InstanceContextMode == InstanceContextMode.PerCall)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.InconsistentSessionRequirements());

                        serviceBehavior.InstanceContextMode = InstanceContextMode.PerSession;
                    }
                    else
                    {
                        if (serviceBehavior.InstanceContextMode == InstanceContextMode.PerSession)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.InconsistentSessionRequirements());

                        serviceBehavior.InstanceContextMode = InstanceContextMode.PerCall;
                    }
                }
            }
            if (serviceBehavior.InstanceContextMode == InstanceContextMode.Single)
                serviceBehavior.InstanceContextMode = InstanceContextMode.PerSession;
        }
    }
}
