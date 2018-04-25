//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1KeyExchangeDeformatter : RSAPKCS1KeyExchangeDeformatter
    {
        RSA m_rsaKey;

        // Constructors

        public InfoCardRSAPKCS1KeyExchangeDeformatter() { }

        public InfoCardRSAPKCS1KeyExchangeDeformatter(AsymmetricAlgorithm key)
            : base(key)
        {
            m_rsaKey = (RSA)key;
        }

        //
        // public methods
        //

        public override byte[] DecryptKeyExchange(byte[] rgbIn)
        {
            if (null != m_rsaKey && m_rsaKey is InfoCardRSACryptoProvider)
            {
                return ((InfoCardRSACryptoProvider)m_rsaKey).Decrypt(rgbIn, false);
            }
            else
            {
                return base.DecryptKeyExchange(rgbIn);
            }
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            base.SetKey(key);
            m_rsaKey = (RSA)key;
        }
    }
}
