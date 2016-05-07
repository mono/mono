//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.CompilerServices;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    class SecurityTokenProviderContainer
    {
        SecurityTokenProvider tokenProvider;

        public SecurityTokenProviderContainer(SecurityTokenProvider tokenProvider)
        {
            if (tokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenProvider");
            }
            this.tokenProvider = tokenProvider;
        }

        public SecurityTokenProvider TokenProvider
        {
            get { return this.tokenProvider; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Close(TimeSpan timeout)
        {
            SecurityUtils.CloseTokenProviderIfRequired(this.tokenProvider, timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Open(TimeSpan timeout)
        {
            SecurityUtils.OpenTokenProviderIfRequired(this.tokenProvider, timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Abort()
        {
            SecurityUtils.AbortTokenProviderIfRequired(this.tokenProvider);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public X509Certificate2 GetCertificate(TimeSpan timeout)
        {
            X509SecurityToken token = this.tokenProvider.GetToken(timeout) as X509SecurityToken;
            if (token != null)
            {
                return token.Certificate;
            }
            else
            {
                return null;
            }
        }
    }
}
