//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicSelectionListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Web.UI;
using System.Collections.Specialized;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Text.RegularExpressions;
using System.Text;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    /// <include file='doc\XhtmlBasicSelectionListAdapter.uex' path='docs/doc[@for="XhtmlSelectionListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlSelectionListAdapter : XhtmlControlAdapter {

        /// <include file='doc\XhtmlBasicSelectionListAdapter.uex' path='docs/doc[@for="XhtmlSelectionListAdapter.Control"]/*' />
        protected new SelectionList Control {
            get {
                return base.Control as SelectionList;
            }
        }

        /// <include file='doc\XhtmlBasicSelectionListAdapter.uex' path='docs/doc[@for="XhtmlSelectionListAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            // Assumption: XhtmlBasic devices all support tables (conforming to spec).

            if (Control.Items.Count == 0) {
                return;
            }
            

            if ((String) Device[XhtmlConstants.RequiresOnEnterForward] == "true") {
                AddOnEnterForward(writer);
            }

            int selectedIndex = Control.SelectedIndex;
            
            switch(Control.SelectType) {
                case ListSelectType.DropDown:
                case ListSelectType.ListBox:
                case ListSelectType.MultiSelectListBox:
                    RenderSelectElement (writer);
                    break;

                case ListSelectType.Radio:
                case ListSelectType.CheckBox:
                    RenderInputElementSet (writer);
                    break;
            }
        }

        private void AddOnEnterForward(XhtmlMobileTextWriter writer) {
            if (Control.SelectType == ListSelectType.CheckBox) {
                // ASURT 142732
                writer.AddOnEnterForwardSetVar(Control.UniqueID);
                return;
            }
            bool firstIndex = true;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Control.Items.Count; i++) {
                if (Control.Items[i].Selected) {
                    if (!firstIndex) {
                        builder.Append(";");
                    }
                    builder.Append(i.ToString(CultureInfo.InvariantCulture));
                    firstIndex = false;
                }
            }
            writer.AddOnEnterForwardSetVar(Control.UniqueID, builder.ToString());
        }

        /// <include file='doc\XhtmlBasicSelectionListAdapter.uex' path='docs/doc[@for="XhtmlSelectionListAdapter.LoadPostData"]/*' />
        public override bool LoadPostData (String key,
            NameValueCollection data,
            Object controlPrivateData,
            out bool dataChanged) {
            String[] selectedItems = data.GetValues(key);

            // If no post data is included, and the control is either not visible, or
            // not on active form, this call should be ignored (the lack of post data 
            // is not due to there being no selection, but due to there being no 
            // markup rendered that could generate the post data).
            if (selectedItems == null && 
                (!Control.Visible || Control.Form != Control.MobilePage.ActiveForm)) {
                dataChanged = false;
                return true;
            }

            // Case where nothing is selected.
            if(selectedItems == null ||
                (selectedItems.Length == 1 && (selectedItems[0] == null || selectedItems[0].Length == 0))) {
                selectedItems = new String[] {};
            }

            // multiselect case with more than one selection.
            if(selectedItems.Length == 1 && selectedItems[0].IndexOf(';') > -1) {
                String selected = selectedItems[0];
                // Eliminate trailing semicolon, if there is one.
                selected = Regex.Replace(selected, ";$", "");
                selectedItems = Regex.Split(selected, ";");
            }

            int[] selectedItemIndices = new int[selectedItems.Length];

            // Note: controlPrivateData is selected indices from the viewstate.
            int[] originalSelectedIndices = (int[])controlPrivateData;
            dataChanged = false;

            // If SelectType is DropDown && nothing was selected, select
            // first elt.  (Non-mobile DropDown does same by getting SelectedIndex).
            if(Control.SelectType == ListSelectType.DropDown &&
                originalSelectedIndices.Length == 0 &&
                Control.Items.Count > 0) {
                Control.Items[0].Selected = true;
                originalSelectedIndices = new int[]{0};
            }

            for(int i = 0; i < selectedItems.Length; i++) {
                selectedItemIndices[i] = Int32.Parse(selectedItems[i], CultureInfo.InvariantCulture);
            }

            // Do not assume posted selected indices are ascending.
            // We do know originalSelectedIndices are ascending.
            Array.Sort(selectedItemIndices);

            // Check whether selections have changed.
            if(selectedItemIndices.Length != originalSelectedIndices.Length) {
                dataChanged = true;
            }
            else {
                for(int i = 0; i < selectedItems.Length; i++) {
                    if(selectedItemIndices[i] != originalSelectedIndices[i]) {
                        dataChanged = true;
                    }
                }
            }

            for (int i = 0; i < Control.Items.Count; i++) {
                Control.Items[i].Selected = false;
            }

            for(int i = 0; i < selectedItemIndices.Length; i++) {
                Control.Items[selectedItemIndices[i]].Selected = true;
            }
            return true;
        }


        void RenderInputElementSet (XhtmlMobileTextWriter writer) {
            string wrappingTag = Device.Tables ? "table" : 
                (((string)Device["usePOverDiv"]  == "true") ? "p" : "div");
            ListSelectType selectType = Control.SelectType;
            MobileListItemCollection items = Control.Items;
            // Review: We always render a table.  Should we optimize away the table tags when the alignment is left?
            String selectTypeString =
                (selectType == ListSelectType.Radio) ?
                "radio" :
                "checkbox";

            ClearPendingBreakIfDeviceBreaksOnBlockLevel(writer); // Since we are writing a block-level element in all cases.
            ConditionalEnterLayout(writer, Style);
            RenderOpeningListTag(writer, wrappingTag);
            for(int itemIndex = 0; itemIndex < items.Count; itemIndex++) {
                MobileListItem item = items[itemIndex];
                if (Device.Tables) {
                    writer.WriteFullBeginTag("tr");
                    writer.WriteFullBeginTag("td");
                }

                writer.WriteBeginTag("input");
                writer.WriteAttribute("type", selectTypeString);
                writer.WriteAttribute("name", Control.UniqueID);
                WriteItemValueAttribute(writer, itemIndex, item.Value);
                String accessKey = GetCustomAttributeValue(item, XhtmlConstants.AccessKeyCustomAttribute);
                if (accessKey != null && accessKey.Length > 0) {
                    writer.WriteAttribute("accesskey", accessKey, true);
                }
                // Assumption: Device.SupportsUncheck is always true for Xhtml devices.
                if (item.Selected && 
                    (Control.IsMultiSelect || itemIndex == Control.SelectedIndex)) {
                    writer.Write(" checked=\"checked\"/>");
                }
                else {
                    writer.Write("/>");
                }

                writer.WriteEncodedText(item.Text);
                if (Device.Tables) {
                    writer.WriteEndTag("td");
                }
                if((string)Device["usePOverDiv"]  == "true" || !Device.Tables) {
                    writer.Write("<br/>");
                }
                if (Device.Tables) {
                    writer.WriteEndTag("tr");
                }
            }
            RenderClosingListTag(writer, wrappingTag);
            ConditionalExitLayout(writer, Style);
        }

        void RenderSelectElement (XhtmlMobileTextWriter writer) {
            
            if ((String)Device["supportsSelectFollowingTable"] == "false" && writer.CachedEndTag == "table") {
                writer.Write("&nbsp;");
            }

            ConditionalEnterStyle(writer, Style);
            writer.WriteBeginTag ("select");

            if (Control.SelectType == ListSelectType.MultiSelectListBox) {
                writer.WriteAttribute ("multiple", "multiple");
            }

            if (Control.SelectType == ListSelectType.ListBox || Control.SelectType == ListSelectType.MultiSelectListBox) {
                writer.WriteAttribute ("size", Control.Rows.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteAttribute ("name", Control.UniqueID);
            ConditionalRenderClassAttribute(writer);
            writer.Write (">");

            for (int itemIndex = 0; itemIndex < Control.Items.Count; itemIndex++) {
                MobileListItem item = Control.Items[itemIndex];
                writer.WriteBeginTag ("option");
                WriteItemValueAttribute (writer, itemIndex, item.Value);
                if (item.Selected && (Control.IsMultiSelect || itemIndex == Control.SelectedIndex)) {
                    writer.Write (" selected=\"selected\">");
                }
                else {
                    writer.Write (">");
                }
                writer.WriteEncodedText (item.Text);
                writer.WriteLine ("</option>");
            }
            ConditionalSetPendingBreak(writer);
            writer.WriteEndTag ("select");            
            writer.WriteLine ();      
            ConditionalPopPhysicalCssClass(writer);
            ConditionalExitStyle(writer, Style);
        }

        private void WriteItemValueAttribute (XhtmlMobileTextWriter writer, int index, String value) {
            String controlFormAction = Control.Form.Action;
            if (controlFormAction == null || controlFormAction.Length == 0) {
                writer.WriteAttribute ("value", index.ToString(CultureInfo.InvariantCulture));
            }
            else {
                writer.WriteAttribute ("value", value, true /*encode*/);
            }
        }

        /// <include file='doc\XhtmlBasicSelectionListAdapter.uex' path='docs/doc[@for="XhtmlSelectionListAdapter.RenderAsHiddenInputField"]/*' />
        protected override void RenderAsHiddenInputField(XhtmlMobileTextWriter writer) {
            // Optimization - if viewstate is enabled for this control, and the
            // postback returns to this page, we just let it do the trick.
            // One catch though - if the control is multiselect, it always 
            // interprets return values, so we do need to write out.

            if (Control.IsMultiSelect || Control.Form.Action.Length > 0 || !IsViewStateEnabled()) {
                String uniqueID = Control.UniqueID;
                MobileListItemCollection items = Control.Items;
                for (int i = 0; i < items.Count; i++) {
                    if (items[i].Selected) {
                        writer.WriteHiddenField(uniqueID, i.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private bool IsViewStateEnabled() {
            Control ctl = Control;
            while (ctl != null) {
                if (!ctl.EnableViewState) {
                    return false;
                }
                ctl = ctl.Parent;
            }
            return true;
        }
    }
}
