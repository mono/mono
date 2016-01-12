// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
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
// ***********************************************************************

#if (CLR_2_0 || CLR_4_0) && !NETCF && !SILVERLIGHT && !__TVOS__ && !__WATCHOS__
using System;
using System.Threading;

namespace NUnit.Framework.Internal
{
    /// <summary>
    /// The ThreadUtility class encapsulates several static methods
    /// useful when working with threads.
    /// </summary>
    public class ThreadUtility
    {
        /// <summary>
        /// Do our best to Kill a thread
        /// </summary>
        /// <param name="thread">The thread to kill</param>
        public static void Kill(Thread thread)
        {
            Kill(thread, null);
        }

        /// <summary>
        /// Do our best to kill a thread, passing state info
        /// </summary>
        /// <param name="thread">The thread to kill</param>
        /// <param name="stateInfo">Info for the ThreadAbortException handler</param>
        public static void Kill(Thread thread, object stateInfo)
        {
            try
            {
                if (stateInfo == null)
                    thread.Abort();
                else
                    thread.Abort(stateInfo);
            }
            catch (ThreadStateException)
            {
                // Although obsolete, this use of Resume() takes care of
                // the odd case where a ThreadStateException is received
                // so we continue to use it.
#pragma warning disable 618
                thread.Resume();
#pragma warning restore 618
            }

            if ( (thread.ThreadState & ThreadState.WaitSleepJoin) != 0 )
                thread.Interrupt();
        }

        private ThreadUtility() { }
    }
}
#endif
