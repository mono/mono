//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel;

    public sealed class WindowsClientCredential
    {
        internal const TokenImpersonationLevel DefaultImpersonationLevel = TokenImpersonationLevel.Identification;
        TokenImpersonationLevel allowedImpersonationLevel = DefaultImpersonationLevel;
        NetworkCredential windowsCredentials;
        bool allowNtlm = SspiSecurityTokenProvider.DefaultAllowNtlm;
        bool isReadOnly;

        internal WindowsClientCredential()
        {
        }

        internal WindowsClientCredential(WindowsClientCredential other)
        {
            if (other.windowsCredentials != null)
                this.windowsCredentials = SecurityUtils.GetNetworkCredentialsCopy(other.windowsCredentials);
            this.allowedImpersonationLevel = other.allowedImpersonationLevel;
            this.allowNtlm = other.allowNtlm;
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

                if (((value == TokenImpersonationLevel.None) || (value == TokenImpersonationLevel.Anonymous)) && System.ServiceModel.Channels.UnsafeNativeMethods.IsTailoredApplication.Value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.UnsupportedTokenImpersonationLevel, "AllowedImpersonationLevel", value.ToString())));
                }

                this.allowedImpersonationLevel = value;
            }
        }

        public NetworkCredential ClientCredential
        {
            get
            {
                if (this.windowsCredentials == null)
                    this.windowsCredentials = new NetworkCredential();
                return this.windowsCredentials;
            }
            set
            {
                ThrowIfImmutable();
                this.windowsCredentials = value;
            }
        }

        [ObsoleteAttribute("This property is deprecated and is maintained for backward compatibility only. The local machine policy will be used to determine if NTLM should be used.")]
        public bool AllowNtlm
        {
            get
            {
                return this.allowNtlm;
            }
            set
            {
                ThrowIfImmutable();
                this.allowNtlm = value;
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
