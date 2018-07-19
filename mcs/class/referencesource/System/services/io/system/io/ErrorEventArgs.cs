//------------------------------------------------------------------------------
// <copyright file="ErrorEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.IO {

    using System.Diagnostics;

    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>Provides
    ///       data for the <see cref='E:System.IO.FileSystemWatcher.Error'/> event.</para>
    /// </devdoc>
    public class ErrorEventArgs : EventArgs {
        private Exception exception;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the class.
        ///    </para>
        /// </devdoc>
        public ErrorEventArgs(Exception exception) {
            this.exception = exception;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Exception'/> that represents the error that occurred.
        ///    </para>
        /// </devdoc>
        public virtual Exception GetException() {
            return this.exception;
        }

    }
}
