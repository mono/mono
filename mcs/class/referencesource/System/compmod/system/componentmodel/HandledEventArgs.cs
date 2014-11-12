//------------------------------------------------------------------------------
// <copyright file="HandledEventArgs.cs" company="Microsoft">
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
    ///       Provides data for the <see cref='System.ComponentModel.HandledEventArgs.Handled'/>
    ///       event.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class HandledEventArgs : EventArgs {

        /// <devdoc>
        ///     Indicates, on return, whether or not the event was handled in the application's event handler.  
        ///     'true' means the application handled the event, 'false' means it didn't.
        /// </devdoc>
        private bool handled;
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.HandledEventArgs'/> class with
        ///       handled set to <see langword='false'/>.
        ///    </para>
        /// </devdoc>
        public HandledEventArgs() : this(false) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.HandledEventArgs'/> class with
        ///       handled set to the given value.
        ///    </para>
        /// </devdoc>
        public HandledEventArgs(bool defaultHandledValue)
        : base() {
            this.handled = defaultHandledValue;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value
        ///       indicating whether the event is handled.
        ///    </para>
        /// </devdoc>
        public bool Handled {
            get {
                return this.handled;
            }
            set {
                this.handled = value;
            }
        }
    }
}
