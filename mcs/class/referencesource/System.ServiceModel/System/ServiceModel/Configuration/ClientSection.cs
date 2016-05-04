//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;

    public sealed partial class ClientSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        public ClientSection()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ChannelEndpointElementCollection Endpoints
        {
            get { return (ChannelEndpointElementCollection)this[ConfigurationStrings.DefaultCollectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Metadata)]
        public MetadataElement Metadata
        {
            get { return (MetadataElement)this[ConfigurationStrings.Metadata]; }
        }

        internal static ClientSection GetSection()
        {
            return (ClientSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ClientSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ClientSection UnsafeGetSection()
        {
            return (ClientSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ClientSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ClientSection UnsafeGetSection(ContextInformation contextInformation)
        {
            return (ClientSection)ConfigurationHelpers.UnsafeGetSectionFromContext(contextInformation, ConfigurationStrings.ClientSectionPath);
        }

        protected override void InitializeDefault()
        {
            this.Metadata.SetDefaults();
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
                foreach (ChannelEndpointElement endpoint in this.Endpoints)
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
                        if (string.IsNullOrEmpty(endpoint.Contract))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.RequiredAttributeMissing, "contract", "endpoint")));
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

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - the return value will be used for a security decision -- see comment in interface definition.")]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            Fx.Assert("Not implemented: IConfigurationContextProviderInternal.GetOriginalEvaluationContext");
            return null;
        }
    }
}

