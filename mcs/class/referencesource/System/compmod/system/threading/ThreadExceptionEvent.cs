//------------------------------------------------------------------------------
// <copyright file="ThreadExceptionEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Threading {
    using System.Threading;
    using System.Diagnostics;
    using System;

    
    /// <devdoc>
    ///    <para>
    ///       Provides data for the System.Windows.Forms.Application.ThreadException event.
    ///    </para>
    /// </devdoc>
    public class ThreadExceptionEventArgs : EventArgs {
    
        private Exception exception;
    
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Threading.ThreadExceptionEventArgs'/> class.
        ///    </para>
        /// </devdoc>
        public ThreadExceptionEventArgs(Exception t) {
            exception = t;
        }

        /// <devdoc>
        /// <para>Specifies the <see cref='System.Exception'/> that occurred.</para>
        /// </devdoc>
        public Exception Exception {
            get {
                return exception;
            }
        }
    }
}
