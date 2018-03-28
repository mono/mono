//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization.Configuration
{
    using System.Configuration;

    public sealed class SerializationSectionGroup : ConfigurationSectionGroup
    {
        static public SerializationSectionGroup GetSectionGroup(Configuration config)
        {
            if (config == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }
#pragma warning suppress 56506 // Microsoft, config is checked above
            return (SerializationSectionGroup)config.SectionGroups[ConfigurationStrings.SectionGroupName];
        }

        public DataContractSerializerSection DataContractSerializer
        {
            get { return (DataContractSerializerSection)this.Sections[ConfigurationStrings.DataContractSerializerSectionName]; }
        }

        public NetDataContractSerializerSection NetDataContractSerializer
        {
            get { return (NetDataContractSerializerSection)this.Sections[ConfigurationStrings.NetDataContractSerializerSectionName]; }
        }

    }
}



