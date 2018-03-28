//------------------------------------------------------------------------------
// <copyright file="WebPartCancelEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public class WebPartCancelEventArgs : CancelEventArgs {
        private WebPart _webPart;

        public WebPartCancelEventArgs(WebPart webPart) {
            _webPart = webPart;
        }

        public WebPart WebPart {
            get {
                return _webPart;
            }
            set {
                _webPart = value;
            }
        }
    }
}
