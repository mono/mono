//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ServiceModelActivationSectionGroup : ConfigurationSectionGroup
    {
        public DiagnosticSection Diagnostics
        {
            get { return (DiagnosticSection)this.Sections[ConfigurationStrings.DiagnosticSectionName]; }
        }

        static public ServiceModelActivationSectionGroup GetSectionGroup(Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }
#pragma warning suppress 56506 // [....], Configuration.SectionGroups cannot be null
            return (ServiceModelActivationSectionGroup)config.SectionGroups[ConfigurationStrings.SectionGroupName];
        }

        public NetPipeSection NetPipe
        {
            get { return (NetPipeSection)this.Sections[ConfigurationStrings.NetPipeSectionName]; }
        }

        public NetTcpSection NetTcp
        {
            get { return (NetTcpSection)this.Sections[ConfigurationStrings.NetTcpSectionName]; }
        }
    }
}



