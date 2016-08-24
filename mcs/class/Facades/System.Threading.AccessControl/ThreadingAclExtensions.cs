//
// ThreadingAclExtensions.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.AccessControl;
using System.Diagnostics.Contracts;

namespace System.Threading
{
    public static class ThreadingAclExtensions
    {
        public static EventWaitHandleSecurity GetAccessControl (this EventWaitHandle handle)
        {
            if (handle == null)
                throw new ArgumentNullException (nameof (handle));

            return handle.GetAccessControl ();
        }

        public static void SetAccessControl (this EventWaitHandle handle, EventWaitHandleSecurity eventSecurity)
        {
            if (handle == null)
                throw new ArgumentNullException (nameof (handle));

            handle.SetAccessControl (eventSecurity);
        }

        public static MutexSecurity GetAccessControl (this Mutex mutex)
        {
            if (mutex == null)
                throw new ArgumentNullException (nameof (mutex));

            return mutex.GetAccessControl ();
        }

        public static void SetAccessControl (this Mutex mutex, MutexSecurity mutexSecurity)
        {
            if (mutex == null)
                throw new ArgumentNullException (nameof (mutex));

            mutex.SetAccessControl (mutexSecurity);
        }

        public static SemaphoreSecurity GetAccessControl (this Semaphore semaphore)
        {
            if (semaphore == null)
                throw new ArgumentNullException (nameof (semaphore));

            return semaphore.GetAccessControl ();
        }

        public static void SetAccessControl (this Semaphore semaphore, SemaphoreSecurity semaphoreSecurity)
        {
            if (semaphore == null)
                throw new ArgumentNullException (nameof (semaphore));

            semaphore.SetAccessControl (semaphoreSecurity);
        }
    }
}