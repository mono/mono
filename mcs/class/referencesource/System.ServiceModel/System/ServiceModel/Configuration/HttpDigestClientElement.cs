//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.Configuration;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Security.Principal;
    using System.Security.Cryptography.X509Certificates;

    public sealed partial class HttpDigestClientElement : ConfigurationElement
    {
        public HttpDigestClientElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.ImpersonationLevel, DefaultValue = WindowsClientCredential.DefaultImpersonationLevel)]
        [ServiceModelEnumValidator(typeof(TokenImpersonationLevelHelper))]
        public TokenImpersonationLevel ImpersonationLevel
        {
            get { return (TokenImpersonationLevel)base[ConfigurationStrings.ImpersonationLevel]; }
            set { base[ConfigurationStrings.ImpersonationLevel] = value; }
        }

        public void Copy(HttpDigestClientElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(SR.GetString(SR.ConfigReadOnly)));
            }
            if (null == from)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }

            this.ImpersonationLevel = from.ImpersonationLevel;
        }

        internal void ApplyConfiguration(HttpDigestClientCredential digest)
        {
            if (digest == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("digest");
            }
            digest.AllowedImpersonationLevel = this.ImpersonationLevel;
        }
    }
}



