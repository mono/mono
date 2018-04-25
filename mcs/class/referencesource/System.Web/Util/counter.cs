//------------------------------------------------------------------------------
// <copyright file="counter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Web;
    using System.Runtime.InteropServices;


    /// <devdoc>
    ///    <para>Provides access to system timers.</para>
    /// </devdoc>
    internal sealed class Counter {           

        /// <devdoc>
        ///     not creatable
        /// </devdoc>
        private Counter() {
        }


        /// <devdoc>
        ///    Gets the current system counter value.
        /// </devdoc>
        internal static long Value {
            get {
                long count = 0;
                SafeNativeMethods.QueryPerformanceCounter(ref count);
                return count;
            }
        }


        /// <devdoc>
        ///    Gets the frequency of the system counter in counts per second.
        /// </devdoc>
        internal static long Frequency {
            get {
                long freq = 0;
                SafeNativeMethods.QueryPerformanceFrequency(ref freq);
                return freq;
            }
        }
    }
}
