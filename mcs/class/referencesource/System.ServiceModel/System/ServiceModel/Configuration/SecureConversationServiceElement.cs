//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Security;
    using System.Xml;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;

    public sealed partial class SecureConversationServiceElement : ConfigurationElement
    {
        public SecureConversationServiceElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.SecurityStateEncoderType, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string SecurityStateEncoderType
        {
            get { return (string)base[ConfigurationStrings.SecurityStateEncoderType]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.SecurityStateEncoderType] = value;
            }
        }

        public void Copy(SecureConversationServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.SecurityStateEncoderType = from.SecurityStateEncoderType;
        }

        internal void ApplyConfiguration(SecureConversationServiceCredential secureConversation)
        {
            if (secureConversation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("secureConversation");
            }
            if (!string.IsNullOrEmpty(this.SecurityStateEncoderType))
            {
                Type type = System.Type.GetType(this.SecurityStateEncoderType, true);
                if (!typeof(SecurityStateEncoder).IsAssignableFrom(type))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidSecurityStateEncoderType, this.SecurityStateEncoderType, typeof(SecurityStateEncoder).ToString())));
                }
                secureConversation.SecurityStateEncoder = (SecurityStateEncoder)Activator.CreateInstance(type);
            }
        }
    }
}



