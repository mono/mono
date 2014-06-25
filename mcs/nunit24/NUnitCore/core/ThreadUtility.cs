// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org.
// ****************************************************************

using System;
using System.Threading;

namespace NUnit.Core
{
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
                // This is deprecated but still needed in this case
                // in order to kill the thread. The warning can't
                // be disabled because the #pragma directive is not
                // recognized by the .NET 1.1 compiler.
                thread.Resume();
            }

            if ( (thread.ThreadState & ThreadState.WaitSleepJoin) != 0 )
                thread.Interrupt();
        }

        private ThreadUtility() { }
    }
}
