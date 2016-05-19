//------------------------------------------------------------------------------
// <copyright file="SendMailErrorEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Web.Security;

    public class SendMailErrorEventArgs : EventArgs {
        private Exception _exception;
        private bool _handled = false;


        public SendMailErrorEventArgs(Exception e) {
            _exception = e;
        }


        /// <devdoc>
        /// Gets or sets the exception which caused the failure
        /// </devdoc>
        public Exception Exception {
            get {
                return _exception;
            }
            set {
                _exception = value;
            }
        }


        /// <devdoc>
        /// Gets or sets whether the error has been handled
        /// </devdoc>
        public bool Handled {
            get {
                return _handled;
            }
            set {
                _handled = value;
            }
        }
    }
}
