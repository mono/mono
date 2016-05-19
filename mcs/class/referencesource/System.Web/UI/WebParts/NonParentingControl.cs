//------------------------------------------------------------------------------
// <copyright file="NonParentingControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System.Security.Permissions;
    using System.Web.UI;

    internal sealed class NonParentingControl : Control {
        protected internal override void AddedControl(Control control, int index) {
        }

        protected internal override void RemovedControl(Control control) {
        }
    }
}
