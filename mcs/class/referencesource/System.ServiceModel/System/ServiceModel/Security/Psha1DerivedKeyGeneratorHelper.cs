//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using Psha1DerivedKeyGenerator = System.IdentityModel.Psha1DerivedKeyGenerator;

    static class Psha1DerivedKeyGeneratorHelper
    {
        internal static byte[] GenerateDerivedKey(byte[] key, byte[] label, byte[] nonce, int derivedKeySize, int position)
        { 
            Psha1DerivedKeyGenerator psha1 = new Psha1DerivedKeyGenerator(key);
            return psha1.GenerateDerivedKey(label, nonce, derivedKeySize, position);
        }
    }
}
