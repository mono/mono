// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>[....]</OWNER>
// 

//
// DeriveBytes.cs
//

namespace System.Security.Cryptography {
[System.Runtime.InteropServices.ComVisible(true)]
    public abstract class DeriveBytes
    // On Orcas DeriveBytes is not disposable, so we cannot add the IDisposable implementation to the
    // CoreCLR mscorlib.  However, this type does need to be disposable since subtypes can and do hold onto
    // native resources. Therefore, on desktop mscorlibs we add an IDisposable implementation.
#if !FEATURE_CORECLR
    : IDisposable
#endif // !FEATURE_CORECLR
    {
        //
        // public methods
        //

        public abstract byte[] GetBytes(int cb);
        public abstract void Reset();

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            return;
        }
    }
}
