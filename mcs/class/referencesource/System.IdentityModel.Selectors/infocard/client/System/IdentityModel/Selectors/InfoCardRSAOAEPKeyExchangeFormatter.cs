//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;

    internal class InfoCardRSAOAEPKeyExchangeFormatter : RSAOAEPKeyExchangeFormatter
    {
        private RSA m_rsaKey;

        //
        // public constructors
        //

        public InfoCardRSAOAEPKeyExchangeFormatter() : base() { }
        public InfoCardRSAOAEPKeyExchangeFormatter(AsymmetricAlgorithm key)
            : base(key)
        {
            m_rsaKey = (RSA)key;
        }

        //
        // public methods
        //

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            m_rsaKey = (RSA)key;
        }

        public override byte[] CreateKeyExchange(byte[] rgbData)
        {
            if (null != m_rsaKey && m_rsaKey is InfoCardRSACryptoProvider)
            {
                return ((InfoCardRSACryptoProvider)m_rsaKey).Encrypt(rgbData, true);
            }
            else
            {
                return base.CreateKeyExchange(rgbData);
            }
        }
    }
}
