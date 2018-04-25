//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Security;

    public sealed partial class ProtocolMappingSection : ConfigurationSection
    {
        public ProtocolMappingSection()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ProtocolMappingElementCollection ProtocolMappingCollection
        {
            get { return (ProtocolMappingElementCollection) base[ConfigurationStrings.DefaultCollectionName]; }
        }

        protected override void InitializeDefault()
        {
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("http", "basicHttpBinding", ConfigurationStrings.DefaultName));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.tcp", "netTcpBinding", ConfigurationStrings.DefaultName));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.pipe", "netNamedPipeBinding", ConfigurationStrings.DefaultName));
            this.ProtocolMappingCollection.Add(new ProtocolMappingElement("net.msmq", "netMsmqBinding", ConfigurationStrings.DefaultName));
        }

        internal static ProtocolMappingSection GetSection()
        {
            return (ProtocolMappingSection)ConfigurationHelpers.GetSection(ConfigurationStrings.ProtocolMappingSectionPath);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls Critical method UnsafeGetSection which elevates in order to fetch config."
            + "Caller must guard access to resultant config section.")]
        [SecurityCritical]
        internal static ProtocolMappingSection UnsafeGetSection()
        {
            return (ProtocolMappingSection)ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ProtocolMappingSectionPath);
        }
    }
}


