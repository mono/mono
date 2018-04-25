//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    public enum MsmqSecureHashAlgorithm
    {
        MD5,
        Sha1,
        Sha256,
        Sha512
    }

    static class MsmqSecureHashAlgorithmHelper
    {
        public static bool IsDefined(MsmqSecureHashAlgorithm algorithm)
        {
            return algorithm == MsmqSecureHashAlgorithm.MD5 ||
                algorithm == MsmqSecureHashAlgorithm.Sha1 ||
                algorithm == MsmqSecureHashAlgorithm.Sha256 ||
                algorithm == MsmqSecureHashAlgorithm.Sha512;
        }

        public static int ToInt32(MsmqSecureHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case MsmqSecureHashAlgorithm.MD5:
                    return UnsafeNativeMethods.CALG_MD5;
                case MsmqSecureHashAlgorithm.Sha1:
                    return UnsafeNativeMethods.CALG_SHA1;
                case MsmqSecureHashAlgorithm.Sha256:
                    return UnsafeNativeMethods.CALG_SHA_256;
                case MsmqSecureHashAlgorithm.Sha512:
                    return UnsafeNativeMethods.CALG_SHA_512;
                default:
                    return -1;
            }
        }
    }
}
