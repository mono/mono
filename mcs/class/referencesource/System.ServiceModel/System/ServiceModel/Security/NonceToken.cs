//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections;
    using System.ServiceModel;
    using System.IO;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    sealed class NonceToken : BinarySecretSecurityToken
    {
        public NonceToken(byte[] key)
            : this(SecurityUniqueId.Create().Value, key)
        {
        }

        public NonceToken(string id, byte[] key)
            : base(id, key, false)
        {
        }

        public NonceToken(int keySizeInBits)
            : base(SecurityUniqueId.Create().Value, keySizeInBits, false)
        {
        }
    }
}
