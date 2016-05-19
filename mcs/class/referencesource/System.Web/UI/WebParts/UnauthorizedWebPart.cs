//------------------------------------------------------------------------------
// <copyright file="UnauthorizedWebPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    ToolboxItem(false)
    ]
    public sealed class UnauthorizedWebPart : ProxyWebPart {

        public UnauthorizedWebPart(WebPart webPart) : base(webPart) {
        }

        public UnauthorizedWebPart(string originalID, string originalTypeName, string originalPath, string genericWebPartID) :
            base(originalID, originalTypeName, originalPath, genericWebPartID) {
        }
    }
}
