//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;

    public partial class BehaviorsSection : ConfigurationSection
    {
        [ConfigurationProperty(ConfigurationStrings.EndpointBehaviors, Options = ConfigurationPropertyOptions.None)]
        public EndpointBehaviorElementCollection EndpointBehaviors
        {
            get { return (EndpointBehaviorElementCollection)base[ConfigurationStrings.EndpointBehaviors]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceBehaviors, Options = ConfigurationPropertyOptions.None)]
        public ServiceBehaviorElementCollection ServiceBehaviors
        {
            get { return (ServiceBehaviorElementCollection)base[ConfigurationStrings.ServiceBehaviors]; }
        }

        internal static BehaviorsSection GetSection()
        {
            return (BehaviorsSection)ConfigurationHelpers.GetSection(ConfigurationStrings.BehaviorsSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config.",
            Safe = "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static BehaviorsSection UnsafeGetSection()
        {
            return (BehaviorsSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.BehaviorsSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config.",
            Safe = "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static BehaviorsSection UnsafeGetAssociatedSection(ContextInformation evalContext)
        {
            return (BehaviorsSection)ConfigurationHelpers.UnsafeGetAssociatedSection(evalContext, ConfigurationStrings.BehaviorsSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls UnsafeGetAssociatedSection which elevates.",
            Safe = "Doesn't leak resultant config.")]
        [SecuritySafeCritical]
        internal static void ValidateEndpointBehaviorReference(string behaviorConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            // ValidateBehaviorReference built on assumption that evaluationContext is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == evaluationContext)
            {
                Fx.Assert("ValidateBehaviorReference() should only called with valid ContextInformation");
                DiagnosticUtility.FailFast("ValidateBehaviorReference() should only called with valid ContextInformation");
            }

            if (!String.IsNullOrEmpty(behaviorConfiguration))
            {
                BehaviorsSection behaviors = (BehaviorsSection)ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.BehaviorsSectionPath);
                if (!behaviors.EndpointBehaviors.ContainsKey(behaviorConfiguration))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidEndpointBehavior,
                        behaviorConfiguration),
                        configurationElement.ElementInformation.Source,
                        configurationElement.ElementInformation.LineNumber));
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls UnsafeGetAssociatedSection which elevates.",
            Safe = "Doesn't leak resultant config.")]
        [SecuritySafeCritical]
        internal static void ValidateServiceBehaviorReference(string behaviorConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            // ValidateBehaviorReference built on assumption that evaluationContext is valid.
            // This should be protected at the callers site.  If assumption is invalid, then
            // configuration system is in an indeterminate state.  Need to stop in a manner that
            // user code can not capture.
            if (null == evaluationContext)
            {
                Fx.Assert("ValidateBehaviorReference() should only called with valid ContextInformation");
                DiagnosticUtility.FailFast("ValidateBehaviorReference() should only called with valid ContextInformation");
            }

            if (!String.IsNullOrEmpty(behaviorConfiguration))
            {
                BehaviorsSection behaviors = (BehaviorsSection)ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.BehaviorsSectionPath);
                if (!behaviors.ServiceBehaviors.ContainsKey(behaviorConfiguration))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigInvalidServiceBehavior,
                        behaviorConfiguration),
                        configurationElement.ElementInformation.Source,
                        configurationElement.ElementInformation.LineNumber));
                }
            }
        }
    }
}



