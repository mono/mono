//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Dispatcher;

    // This attribute specifies what the service implementation requires for AspNet Integration mode.
    [AttributeUsage(ServiceModelAttributeTargets.ServiceBehavior)]
    public sealed class AspNetCompatibilityRequirementsAttribute : Attribute, IServiceBehavior
    {
        // AppCompat: The default has been changed in 4.5 to Allowed so that fewer people need to change it.
        // For deployment compat purposes, apps targeting 4.0 should behave the same as if 4.5 was not installed.
        AspNetCompatibilityRequirementsMode requirementsMode = OSEnvironmentHelper.IsApplicationTargeting45 ?
            AspNetCompatibilityRequirementsMode.Allowed : AspNetCompatibilityRequirementsMode.NotAllowed;

        // NotAllowed: Validates that the service is not running in the AspNetCompatibility mode.
        //
        // Required: Validates that service runs in the AspNetCompatibility mode only.
        //
        // Allowed: Allows both AspNetCompatibility mode and the default Indigo mode.
        //
        public AspNetCompatibilityRequirementsMode RequirementsMode
        {
            get
            {
                return this.requirementsMode;
            }
            set
            {
                AspNetCompatibilityRequirementsModeHelper.Validate(value);
                this.requirementsMode = value;
            }
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }

            AspNetEnvironment.Current.ValidateCompatibilityRequirements(RequirementsMode);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }
    }
}

