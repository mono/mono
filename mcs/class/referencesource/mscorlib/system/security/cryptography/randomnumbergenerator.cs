// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// RandomNumberGenerator.cs
//

namespace System.Security.Cryptography {
#if !FEATURE_CORECLR && !SILVERLIGHT
[System.Runtime.InteropServices.ComVisible(true)]
#endif // !FEATURE_CORECLR && !SILVERLIGHT
    public abstract class RandomNumberGenerator
    // On Orcas RandomNumberGenerator is not disposable, so we cannot add the IDisposable implementation to the
    // CoreCLR mscorlib.  However, this type does need to be disposable since subtypes can and do hold onto
    // native resources. Therefore, on desktop mscorlibs we add an IDisposable implementation.
#if !FEATURE_CORECLR
    : IDisposable
#endif // !FEATURE_CORECLR
    {
        protected RandomNumberGenerator() {
        }
    
        //
        // public methods
        //

#if (!FEATURE_CORECLR && !SILVERLIGHT) || FEATURE_LEGACYNETCFCRYPTO
        static public RandomNumberGenerator Create() {
            return Create("System.Security.Cryptography.RandomNumberGenerator");
        }

        static public RandomNumberGenerator Create(String rngName) {
            return (RandomNumberGenerator) CryptoConfig.CreateFromName(rngName);
        }
#endif // (!FEATURE_CORECLR && !SILVERLIGHT) || FEATURE_LEGACYNETCFCRYPTO

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            return;
        }

        public abstract void GetBytes(byte[] data);

#if (!FEATURE_CORECLR && !SILVERLIGHT) || FEATURE_LEGACYNETCFCRYPTO
        public virtual void GetNonZeroBytes(byte[] data)
        {
            // This method does not exist on Silverlight, so for compatibility we cannot have it be abstract
            // on the desktop (otherwise any type deriving from RandomNumberGenerator on Silverlight cannot
            // compile against the desktop CLR).  Since this technically is an abstract method with no
            // implementation, we'll just throw NotImplementedException.
            throw new NotImplementedException();
        }
#endif // (!FEATURE_CORECLR && !SILVERLIGHT) || FEATURE_LEGACYNETCFCRYPTO
    }
}
