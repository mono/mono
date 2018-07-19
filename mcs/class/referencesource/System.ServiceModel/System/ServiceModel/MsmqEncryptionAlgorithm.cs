//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    public enum MsmqEncryptionAlgorithm
    {
        RC4Stream,
        Aes
    }

    static class MsmqEncryptionAlgorithmHelper
    {
        public static bool IsDefined(MsmqEncryptionAlgorithm algorithm)
        {
            return algorithm == MsmqEncryptionAlgorithm.RC4Stream || algorithm == MsmqEncryptionAlgorithm.Aes;
        }

        public static int ToInt32(MsmqEncryptionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case MsmqEncryptionAlgorithm.RC4Stream:
                    return UnsafeNativeMethods.CALG_RC4;
                case MsmqEncryptionAlgorithm.Aes:
                    return UnsafeNativeMethods.CALG_AES;
                default:
                    return -1;
            }
        }
    }
}
