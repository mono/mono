//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.ServiceModel;

    public sealed partial class ComContractsSection : ConfigurationSection
    {
        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ComContractElementCollection ComContracts
        {
            get { return (ComContractElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        internal static ComContractsSection GetSection()
        {
            return (ComContractsSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ComContractsSectionPath);
        }
    }
}



