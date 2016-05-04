//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Configuration
{
    using System.Globalization;

    static class ConfigurationStrings
    {
        static string GetSectionPath(string sectionName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"{0}/{1}", ConfigurationStrings.SectionGroupName, sectionName);
        }

        static internal string WorkflowHostingOptionsSectionPath
        {
            get { return ConfigurationStrings.GetSectionPath(ConfigurationStrings.WorkflowHostingOptionsSectionName); }
        }

        public const string AllowUnsafeCaching = "allowUnsafeCaching";
        public const string ChannelSettings = "channelSettings";
        public const string FactorySettings = "factorySettings";
        public const string IdleTimeout = "idleTimeout";
        public const string LeaseTimeout = "leaseTimeout";
        public const string MaxItemsInCache = "maxItemsInCache";
        // Default Values
        public const string TimeSpanZero = "00:00:00";

        public const string SectionGroupName = "system.serviceModel.activities";
        public const string WorkflowHostingOptionsSectionName = "workflowHostingOptions";
        public const string OverrideSiteName = "overrideSiteName";
    }
}
