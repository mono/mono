//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;

    public sealed partial class ServicesSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        public ServicesSection()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ServiceElementCollection Services
        {
            get { return (ServiceElementCollection)this[ConfigurationStrings.DefaultCollectionName]; }
        }

        internal static ServicesSection GetSection()
        {
            return (ServicesSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ServicesSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method UnsafeGetSection which elevates in order to load config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ServicesSection UnsafeGetSection()
        {
            return (ServicesSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ServicesSectionPath);
        }

        protected override void PostDeserialize()
        {
            this.ValidateSection();
            base.PostDeserialize();
        }

        void ValidateSection()
        {
            ContextInformation context = ConfigurationHelpers.GetEvaluationContext(this);

            if (context != null)
            {
                foreach (ServiceElement service in this.Services)
                {
                    BehaviorsSection.ValidateServiceBehaviorReference(service.BehaviorConfiguration, context, service);

                    foreach (ServiceEndpointElement endpoint in service.Endpoints)
                    {
                        if (string.IsNullOrEmpty(endpoint.Kind))
                        {
                            if (!string.IsNullOrEmpty(endpoint.EndpointConfiguration))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidAttribute, "endpointConfiguration", "endpoint", "kind")));
                            }
                            if (string.IsNullOrEmpty(endpoint.Binding))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.RequiredAttributeMissing, "binding", "endpoint")));
                            }
                        }
                        if (string.IsNullOrEmpty(endpoint.Binding) && !string.IsNullOrEmpty(endpoint.BindingConfiguration))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidAttribute, "bindingConfiguration", "endpoint", "binding")));
                        }
                        BehaviorsSection.ValidateEndpointBehaviorReference(endpoint.BehaviorConfiguration, context, endpoint);
                        BindingsSection.ValidateBindingReference(endpoint.Binding, endpoint.BindingConfiguration, context, endpoint);
                        StandardEndpointsSection.ValidateEndpointReference(endpoint.Kind, endpoint.EndpointConfiguration, context, endpoint);
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.",
            Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }
    }
}

