//------------------------------------------------------------------------------
// <copyright file="WmlCheckBoxAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Web.Util;

    public class WmlCheckBoxAdapter : CheckBoxAdapter, IPostBackDataHandler {
        private const String _clientPrefix = "__cb_";
        private string _ivalue = null;        

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter) markupWriter;

            // If control is not enabled, don't render it at
            // all for WML.
            if (!Control.Enabled) {
                RenderDisabled(writer);
                return;
            }

            ((WmlPageAdapter)PageAdapter).RegisterPostField(writer, Control);

            // determine if control is already checked.
            // if so, set initial value.            
            _ivalue = Control.Checked ? "1" : String.Empty;

            ((WmlPageAdapter)PageAdapter).AddFormVariable (writer, Control.ClientID, _ivalue, false /* randomID */);
            // does not render __ivalue if null or form variables written.
            writer.WriteBeginSelect(null, null, Control.ClientID, _ivalue, Control.ToolTip, true /* multiselect*/);            

            if (Control.AutoPostBack) {
                ((WmlPageAdapter)PageAdapter).RenderSelectOptionAsAutoPostBack(writer, Control.Text, null);
            }
            else {
                ((WmlPageAdapter)PageAdapter).RenderSelectOption(writer, Control.Text);
            }

            writer.WriteEndSelect();           
        }

        /// <internalonly/>
        // Parse the WML posted data appropriately.
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        /// <internalonly/>
        // Parse the WML posted data appropriately.
        protected virtual bool LoadPostData(String key, NameValueCollection data) {
            bool dataChanged = false;            
            String[] selectedItems = data.GetValues(key);

            if (String.IsNullOrEmpty(selectedItems)) {
                // This shouldn't happen if we're posting back from the form that
                // contains the checkbox. It could happen when being called 
                // as the result of a postback from another form on the page,
                // so we just return quietly.
                return false;
            }

            // For a checkbox, our selection list
            // has only one item.

            Debug.Assert(selectedItems.Length == 1, "Checkbox selection " + 
                         "list has more than one value");

            string selectedItem = selectedItems[0];
            if (selectedItem != null && selectedItem.Length == 0) {
                dataChanged = Control.Checked == true;
                Control.Checked = false;
            }

            else if (StringUtil.EqualsIgnoreCase(selectedItem, "1")) {
                dataChanged = Control.Checked == false;
                Control.Checked = true;
            }

            return dataChanged;

        }

        /// <internalonly/>
        // Raises the post data changed event.
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }        

        /// <internalonly/>
        // Raises the post data changed event.
        protected virtual void RaisePostDataChangedEvent() {
            ((IPostBackDataHandler)Control).RaisePostDataChangedEvent();
        }
    }
}

#endif
