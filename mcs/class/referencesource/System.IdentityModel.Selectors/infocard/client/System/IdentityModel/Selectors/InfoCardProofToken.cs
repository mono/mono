//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Claims;   
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    //
    // Summary:
    //  This class implements a SecurityToken to wrap a remoted crypto key.  It provides indirect
    //  access to the private proof key associated with a released token.
    //
    internal class InfoCardProofToken : SecurityToken, IDisposable
    {
        string m_id;
        DateTime m_expiration;
        ReadOnlyCollection<SecurityKey> m_securityKeys;
        SecurityKey m_securityKey;

        public InfoCardProofToken( AsymmetricCryptoHandle cryptoHandle, DateTime expiration ) : this( expiration )
        {
            InitCrypto( new InfoCardAsymmetricCrypto( cryptoHandle ) );
        }

        public InfoCardProofToken( SymmetricCryptoHandle cryptoHandle, DateTime expiration ) : this( expiration )
        {
            InitCrypto( new InfoCardSymmetricCrypto( cryptoHandle ) );
        }

        private InfoCardProofToken( DateTime expiration ) : base()
        {
            m_id = Guid.NewGuid().ToString();
            m_expiration = expiration.ToUniversalTime();
        }

        public override string Id
        {
            get { return m_id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return m_securityKeys;
            }
        }

        public override DateTime ValidTo
        {
            get { return m_expiration; }
        }

        public override DateTime ValidFrom
        {
            get { return DateTime.UtcNow; }
        }

        private void InitCrypto(SecurityKey securityKey)
        {
            m_securityKey = securityKey;
            List<SecurityKey> securityKeys = new List<SecurityKey>(1);
            securityKeys.Add(securityKey);
            m_securityKeys = securityKeys.AsReadOnly();
        }

        public void Dispose()
        {
            m_securityKeys = null;
            ((IDisposable)m_securityKey).Dispose();
        }
    }
}
