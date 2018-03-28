//------------------------------------------------------------------------------
// <copyright file="WmlRadioButtonListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;

    /// <devdoc>
    /// Provides adaptive rendering for the RadioButtonList control.
    /// </devdoc>
    public class WmlRadioButtonListAdapter : WmlListControlAdapter {

        internal override void RenderSelectOption(WmlTextWriter writer, ListItem item) {

            // We don't do autopostback if the radio button has been selected.
            // This is to make it consistent that it only posts back if its
            // state has been changed.  Also, it avoids the problem of missing
            // validation since the data changed event would not be fired if the
            // selected radio button was posting back.
            if (Control.AutoPostBack && item.Selected) {
                ((WmlPageAdapter)PageAdapter).RenderSelectOption(writer, item.Text);
            }
            else {
                base.RenderSelectOption(writer, item);
            }
        }
    }
}

#endif

