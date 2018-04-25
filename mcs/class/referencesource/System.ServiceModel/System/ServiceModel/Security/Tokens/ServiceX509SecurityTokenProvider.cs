//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;

    class ServiceX509SecurityTokenProvider : X509SecurityTokenProvider
    {
        public ServiceX509SecurityTokenProvider(X509Certificate2 certificate)
            : base(certificate)
        {
        }

        public ServiceX509SecurityTokenProvider(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
            : base(storeLocation, storeName, findType, findValue)
        {
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return new X509SecurityToken(this.Certificate, false, false);
        }
    }
}
