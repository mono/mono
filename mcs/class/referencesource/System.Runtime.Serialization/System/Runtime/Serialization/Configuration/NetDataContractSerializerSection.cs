//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Security;

    public sealed partial class NetDataContractSerializerSection : ConfigurationSection
    {
        public NetDataContractSerializerSection()
            : base()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates in order to get the NetDataContractSerializerSection config section."
            + " Caller should not leak config section instance to untrusted code.")]
        [SecurityCritical]
        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static bool TryUnsafeGetSection(out NetDataContractSerializerSection section)
        {
            section = (NetDataContractSerializerSection)ConfigurationManager.GetSection(ConfigurationStrings.NetDataContractSerializerSectionPath);
            return section != null;
        }

        [ConfigurationProperty(ConfigurationStrings.EnableUnsafeTypeForwarding, DefaultValue = false)]
        public bool EnableUnsafeTypeForwarding
        {
            get { return (bool)base[ConfigurationStrings.EnableUnsafeTypeForwarding]; }
        }
    }

}
