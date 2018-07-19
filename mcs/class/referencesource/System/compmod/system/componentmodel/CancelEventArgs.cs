//------------------------------------------------------------------------------
// <copyright file="CancelEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       Provides data for the <see cref='System.ComponentModel.CancelEventArgs.Cancel'/>
    ///       event.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class CancelEventArgs : EventArgs {

        /// <devdoc>
        ///     Indicates, on return, whether or not the operation should be cancelled
        ///     or not.  'true' means cancel it, 'false' means don't.
        /// </devdoc>
        private bool cancel;
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.CancelEventArgs'/> class with
        ///       cancel set to <see langword='false'/>.
        ///    </para>
        /// </devdoc>
        public CancelEventArgs() : this(false) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.CancelEventArgs'/> class with
        ///       cancel set to the given value.
        ///    </para>
        /// </devdoc>
        public CancelEventArgs(bool cancel)
        : base() {
            this.cancel = cancel;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the operation should be cancelled.
        ///    </para>
        /// </devdoc>
        public bool Cancel {
            get {
                return cancel;
            }
            set {
                this.cancel = value;
            }
        }
    }
}
