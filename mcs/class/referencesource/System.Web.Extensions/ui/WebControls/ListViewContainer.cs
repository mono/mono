//------------------------------------------------------------------------------
// <copyright file="ListViewContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    internal class ListViewContainer : Control, INamingContainer {
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is ListViewCommandEventArgs) {
                RaiseBubbleEvent(source, e);
                return true;
            }
            if (e is CommandEventArgs) {
                // todo: should we bubble events from non-item containers?
                ListViewCommandEventArgs args = new ListViewCommandEventArgs(null, source, (CommandEventArgs)e);
                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }
    }
}
