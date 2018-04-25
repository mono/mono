//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;

    internal class InfoCardRSAOAEPKeyExchangeDeformatter : RSAOAEPKeyExchangeDeformatter
    {
        private RSA m_rsaKey; // RSA Key value to do decrypt operation

        //
        // public constructors
        //

        public InfoCardRSAOAEPKeyExchangeDeformatter() : base() { }
        public InfoCardRSAOAEPKeyExchangeDeformatter(AsymmetricAlgorithm key)
            : base(key)
        {
            m_rsaKey = (RSA)key;
        }

        //
        // public methods
        //

        public override byte[] DecryptKeyExchange(byte[] rgbData)
        {
            if (null != m_rsaKey && m_rsaKey is InfoCardRSACryptoProvider)
            {
                return ((InfoCardRSACryptoProvider)m_rsaKey).Decrypt(rgbData, true);
            }
            else
            {
                return base.DecryptKeyExchange(rgbData);
            }
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            m_rsaKey = (RSA)key;
        }
    }
}
