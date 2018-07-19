//------------------------------------------------------------------------------
// <copyright file="WebPartEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public class WebPartEventArgs : EventArgs {
        private WebPart _webPart;

        public WebPartEventArgs(WebPart webPart) {
            _webPart = webPart;
        }

        public WebPart WebPart {
            get {
                return _webPart;
            }
        }
    }
}
