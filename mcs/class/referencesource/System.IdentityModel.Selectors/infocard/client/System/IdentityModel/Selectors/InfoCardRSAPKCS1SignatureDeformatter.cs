//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1SignatureDeformatter : RSAPKCS1SignatureDeformatter
    {
        private RSA m_rsaKey; // RSA Key value to do decrypt operation
        private string m_strOID; // OID value for the HASH algorithm

        //
        // public constructors
        //

        public InfoCardRSAPKCS1SignatureDeformatter() : base() { }
        public InfoCardRSAPKCS1SignatureDeformatter(AsymmetricAlgorithm key)
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

        public override void SetHashAlgorithm(string strName)
        {
            base.SetHashAlgorithm(strName);
            m_strOID = CryptoConfig.MapNameToOID(strName);
        }

        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
        {
            if (!(m_strOID == null || m_rsaKey == null || rgbHash == null || rgbSignature == null)
                && m_rsaKey is InfoCardRSACryptoProvider)
            {
                return ((InfoCardRSACryptoProvider)m_rsaKey).VerifyHash(rgbHash, m_strOID, rgbSignature);
            }
            else
            {
                return base.VerifySignature(rgbHash, rgbSignature);
            }
        }
    }
}
