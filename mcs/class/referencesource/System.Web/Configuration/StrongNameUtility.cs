//------------------------------------------------------------------------------
// <copyright file="StrongNameUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using Microsoft.Runtime.Hosting;

    internal class StrongNameUtility {

        // Help class shouldn't be instantiated.
        private StrongNameUtility() {
        }
        
        internal static bool GenerateStrongNameFile(string filename) {
            // variables that hold the unmanaged key
            IntPtr keyBlob = IntPtr.Zero;
            int generatedSize = 0;

            // create the key
            bool createdKey = StrongNameHelpers.StrongNameKeyGen(null,
                0 /*No flags. 1 is to save the key in the key container */, 
                out keyBlob, out generatedSize);

            // if there was a problem, translate it and report it
            if (!createdKey || keyBlob == IntPtr.Zero) {
                throw Marshal.GetExceptionForHR(StrongNameHelpers.StrongNameErrorInfo());
            }

            try {
                Debug.Assert(keyBlob != IntPtr.Zero);

                // make sure the key size makes sense
                Debug.Assert(generatedSize > 0 && generatedSize <= Int32.MaxValue);
                if (generatedSize <= 0 || generatedSize > Int32.MaxValue) {
                    throw new InvalidOperationException(SR.GetString(SR.Browser_InvalidStrongNameKey));
                }

                // get the key into managed memory
                byte[] key = new byte[generatedSize];
                Marshal.Copy(keyBlob, key, 0, (int)generatedSize);

                // write the key to the specified file
                using (FileStream snkStream = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                    using (BinaryWriter snkWriter = new BinaryWriter(snkStream)) {
                        snkWriter.Write(key);
                    }
                }
            }
            finally {
                // release the unmanaged memory the key resides in
                if (keyBlob != IntPtr.Zero) {
                    StrongNameHelpers.StrongNameFreeBuffer(keyBlob);
                }
            }

            return true;
        }
    }
}
