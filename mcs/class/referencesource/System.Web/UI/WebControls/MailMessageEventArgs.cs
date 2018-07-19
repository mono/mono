//------------------------------------------------------------------------------
// <copyright file="MailMessageEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Net.Mail;

    /// <devdoc>
    /// An EventArgs that contains a MailMessage as data.
    /// </devdoc>
    public class MailMessageEventArgs : LoginCancelEventArgs {
        private MailMessage _message;


        public MailMessageEventArgs(MailMessage message) {
            _message = message;
        }


        public MailMessage Message {
            get {
                return _message;
            }
        }
    }
}
