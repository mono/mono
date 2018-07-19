//------------------------------------------------------------------------------
// <copyright file="Semaphore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#pragma warning disable 618

namespace System.Net 
{  


	using System;
	using System.Threading;
    using System.Security.Permissions;


    // used for Connection Pooling
    internal sealed class Semaphore : WaitHandle
    {
        internal Semaphore(int initialCount, int maxCount) : base() {
            lock (this) {
#if MONO
                int errorCode;
                Handle = System.Threading.Semaphore.CreateSemaphore_internal(initialCount, maxCount, null, out errorCode);
#else
                // 
                Handle = UnsafeNclNativeMethods.CreateSemaphore(IntPtr.Zero, initialCount, maxCount, IntPtr.Zero);
#endif
            }
        }

        /*
        // Consider removing.
        public Semaphore(int initialCount, int maxCount, string name) : base() {
            lock (this) {
                // 



*/

        internal bool ReleaseSemaphore() {
#if MONO
            int previousCount;
            return System.Threading.Semaphore.ReleaseSemaphore_internal (Handle, 1, out previousCount);
#else
#if DEBUG
            int previousCount;
            bool success = UnsafeNclNativeMethods.ReleaseSemaphore(Handle, 1, out previousCount);        
            GlobalLog.Print("ReleaseSemaphore#"+ValidationHelper.HashString(this)+" success:"+success+" previousCount:"+previousCount.ToString());
            return success;
#else            
            return UnsafeNclNativeMethods.ReleaseSemaphore(Handle, 1, IntPtr.Zero);
#endif
#endif
        }

        /*
        // Consider removing.
        internal bool ReleaseSemaphore(int releaseCount, out int previousCount) {
            return UnsafeNclNativeMethods.ReleaseSemaphore(Handle, releaseCount, out previousCount);        
        }
        */
    }
}
