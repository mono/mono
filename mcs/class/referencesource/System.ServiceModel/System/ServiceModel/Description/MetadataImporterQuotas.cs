//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    public sealed class MetadataImporterQuotas
    {
        const int DefaultMaxPolicyConversionContexts = 32;
        const int DefaultMaxPolicyNodes = 4096;
        const int DefaultMaxPolicyAssertions = 1024;
        const int DefaultMaxYields = 1024;
        
        int maxPolicyConversionContexts;
        int maxPolicyNodes;
        int maxPolicyAssertions;
        int maxYields;

        public MetadataImporterQuotas()
        {
            this.maxYields = DefaultMaxYields;
        }

        public static MetadataImporterQuotas Defaults
        {
            get
            {
                return CreateDefaultSettings();
            }
        }
        public static MetadataImporterQuotas Max
        {
            get
            {
                return CreateMaxSettings();
            }
        }

        internal int MaxPolicyConversionContexts
        {
            get { return this.maxPolicyConversionContexts; }
            set { this.maxPolicyConversionContexts = value; }
        }
        internal int MaxPolicyNodes
        {
            get { return this.maxPolicyNodes; }
            set { this.maxPolicyNodes = value; }
        }
        internal int MaxPolicyAssertions
        {
            get { return this.maxPolicyAssertions; }
            set { this.maxPolicyAssertions = value; }
        }

        internal int MaxYields
        {
            get { return this.maxYields; }
            set { this.maxYields = value; }
        }

        static MetadataImporterQuotas CreateDefaultSettings()
        {
            MetadataImporterQuotas settings = new MetadataImporterQuotas();
            settings.maxPolicyConversionContexts = DefaultMaxPolicyConversionContexts;
            settings.maxPolicyNodes = DefaultMaxPolicyNodes;
            settings.maxPolicyAssertions = DefaultMaxPolicyAssertions;

            return settings;
        }
        static MetadataImporterQuotas CreateMaxSettings()
        {
            MetadataImporterQuotas settings = new MetadataImporterQuotas();
            settings.maxPolicyConversionContexts = DefaultMaxPolicyConversionContexts;
            settings.maxPolicyNodes = int.MaxValue;
            settings.maxPolicyAssertions = int.MaxValue;

            return settings;
        }
    }
}
