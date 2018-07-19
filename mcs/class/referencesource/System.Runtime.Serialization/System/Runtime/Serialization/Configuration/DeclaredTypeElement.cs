//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Permissions;
    using System.Security;

    public sealed partial class DeclaredTypeElement : ConfigurationElement
    {
        public DeclaredTypeElement()
        {
        }

        public DeclaredTypeElement(string typeName)
            : this()
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            this.Type = typeName;
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public TypeElementCollection KnownTypes
        {
            get { return (TypeElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Type, DefaultValue = "", Options = ConfigurationPropertyOptions.IsKey)]
        [DeclaredTypeValidator()]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set { base[ConfigurationStrings.Type] = value; }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls the critical methods of PartialTrustHelpers",
            Safe = "PartialTrustHelpers.IsInFullTrust demands for FullTrust")]
        [SecuritySafeCritical]
        protected override void PostDeserialize()
        {
            // Perf optimization. If the configuration is coming from machine.config
            // It is safe and we don't need to check for permissions.
            if (EvaluationContext.IsMachineLevel)
            {
                return;
            }
            if (!PartialTrustHelpers.IsInFullTrust())
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigDataContractSerializerSectionLoadError)));
            }
        }
    }
}


