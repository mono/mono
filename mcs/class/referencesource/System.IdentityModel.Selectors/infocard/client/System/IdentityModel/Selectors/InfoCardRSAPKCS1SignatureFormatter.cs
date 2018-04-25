//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Security.Cryptography;

    internal class InfoCardRSAPKCS1SignatureFormatter : RSAPKCS1SignatureFormatter
    {
        private RSA m_rsaKey;
        private string m_strOID;

        //
        // public constructors
        //

        public InfoCardRSAPKCS1SignatureFormatter() : base() { }

        public InfoCardRSAPKCS1SignatureFormatter(AsymmetricAlgorithm key)
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

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            if (!(null == m_strOID || null == m_rsaKey || null == rgbHash) && m_rsaKey is InfoCardRSACryptoProvider)
            {
                return ((InfoCardRSACryptoProvider)m_rsaKey).SignHash(rgbHash, m_strOID);
            }
            else
            {
                return base.CreateSignature(rgbHash);
            }
        }
    }
}
