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

    public sealed partial class DataContractSerializerSection : ConfigurationSection
    {
        public DataContractSerializerSection()
            : base()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates in order to get the DataContractSerializerSection config section."
            + " Caller should not leak config section instance to untrusted code.")]
        [SecurityCritical]
        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static DataContractSerializerSection UnsafeGetSection()
        {
            DataContractSerializerSection section =
                (DataContractSerializerSection)ConfigurationManager.GetSection(ConfigurationStrings.DataContractSerializerSectionPath);
            if (section == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigDataContractSerializerSectionLoadError)));
            }
            return section;
        }

        [ConfigurationProperty(ConfigurationStrings.DeclaredTypes, DefaultValue = null)]
        public DeclaredTypeElementCollection DeclaredTypes
        {
            get { return (DeclaredTypeElementCollection)base[ConfigurationStrings.DeclaredTypes]; }
        }
    }

}



