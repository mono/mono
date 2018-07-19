//------------------------------------------------------------------------------
// <copyright file="CancelEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Diagnostics;
    using System;

    public class LoginCancelEventArgs : EventArgs {

        private bool _cancel;

        public LoginCancelEventArgs() : this(false) {
        }

        public LoginCancelEventArgs(bool cancel) {
            _cancel = cancel;
        }

        public bool Cancel {
            get {
                return _cancel;
            }
            set {
                _cancel = value;
            }
        }
    }
}
