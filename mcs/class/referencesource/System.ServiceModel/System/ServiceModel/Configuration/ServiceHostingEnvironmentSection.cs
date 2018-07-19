//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;

    public sealed partial class ServiceHostingEnvironmentSection : ConfigurationSection
    {
        public ServiceHostingEnvironmentSection()
        {
        }

        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
            {
                return;
            }

            if (PropertyValueOrigin.SetHere ==
                ElementInformation.Properties[ConfigurationStrings.MinFreeMemoryPercentageToActivateService].ValueOrigin)
            {
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                }
                catch (SecurityException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.Hosting_MemoryGatesCheckFailedUnderPartialTrust)));

                }
            }
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public TransportConfigurationTypeElementCollection TransportConfigurationTypes
        {
            get { return (TransportConfigurationTypeElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.BaseAddressPrefixFilters, Options = ConfigurationPropertyOptions.None)]
        public BaseAddressPrefixFilterElementCollection BaseAddressPrefixFilters
        {
            get { return (BaseAddressPrefixFilterElementCollection)base[ConfigurationStrings.BaseAddressPrefixFilters]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceActivations, Options = ConfigurationPropertyOptions.None)]
        public ServiceActivationElementCollection ServiceActivations
        {
            get { return (ServiceActivationElementCollection)base[ConfigurationStrings.ServiceActivations]; }
        }

        [ConfigurationProperty(ConfigurationStrings.AspNetCompatibilityEnabled, DefaultValue = false)]
        public bool AspNetCompatibilityEnabled
        {
            get { return (bool)base[ConfigurationStrings.AspNetCompatibilityEnabled]; }
            set { base[ConfigurationStrings.AspNetCompatibilityEnabled] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CloseIdleServicesAtLowMemory, DefaultValue = false)]
        public bool CloseIdleServicesAtLowMemory
        {
            get { return (bool)base[ConfigurationStrings.CloseIdleServicesAtLowMemory]; }
            set { base[ConfigurationStrings.CloseIdleServicesAtLowMemory] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MinFreeMemoryPercentageToActivateService, DefaultValue = 5)]
        [IntegerValidator(MinValue = 0, MaxValue = 99)]
        public int MinFreeMemoryPercentageToActivateService
        {
            get { return (int)base[ConfigurationStrings.MinFreeMemoryPercentageToActivateService]; }
            set { base[ConfigurationStrings.MinFreeMemoryPercentageToActivateService] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MultipleSiteBindingsEnabled, DefaultValue = false)]
        public bool MultipleSiteBindingsEnabled
        {
            get { return (bool)base[ConfigurationStrings.MultipleSiteBindingsEnabled]; }
            set { base[ConfigurationStrings.MultipleSiteBindingsEnabled] = value; }
        }

        internal static ServiceHostingEnvironmentSection GetSection()
        {
            return (ServiceHostingEnvironmentSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ServiceHostingEnvironmentSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ServiceHostingEnvironmentSection UnsafeGetSection()
        {
            return (ServiceHostingEnvironmentSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ServiceHostingEnvironmentSectionPath);
        }
    }
}



