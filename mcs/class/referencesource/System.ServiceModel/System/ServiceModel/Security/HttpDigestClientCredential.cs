//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel;

    public sealed class HttpDigestClientCredential
    {
        TokenImpersonationLevel allowedImpersonationLevel = WindowsClientCredential.DefaultImpersonationLevel;
        NetworkCredential digestCredentials;
        bool isReadOnly;

        internal HttpDigestClientCredential()
        {
            this.digestCredentials = new NetworkCredential();
        }

        internal HttpDigestClientCredential(HttpDigestClientCredential other)
        {
            this.allowedImpersonationLevel = other.allowedImpersonationLevel;
            this.digestCredentials = SecurityUtils.GetNetworkCredentialsCopy(other.digestCredentials);
            this.isReadOnly = other.isReadOnly;
        }

        public TokenImpersonationLevel AllowedImpersonationLevel
        {
            get
            {
                return this.allowedImpersonationLevel;
            }
            set
            {
                ThrowIfImmutable();
                this.allowedImpersonationLevel = value;
            }
        }

        public NetworkCredential ClientCredential
        {
            get
            {
                return this.digestCredentials;
            }
            set
            {
                ThrowIfImmutable();
                this.digestCredentials = value;
            }
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
