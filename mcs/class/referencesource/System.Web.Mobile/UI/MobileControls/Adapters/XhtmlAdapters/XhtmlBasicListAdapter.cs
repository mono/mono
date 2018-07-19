//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicListAdapter.uex' path='docs/doc[@for="XhtmlListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlListAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicListAdapter.uex' path='docs/doc[@for="XhtmlListAdapter.Control"]/*' />
        protected new List Control {
            get {
                return base.Control as List;
            }
        }

        /// <include file='doc\XhtmlBasicListAdapter.uex' path='docs/doc[@for="XhtmlListAdapter.Render"]/*' />
        public override void Render (XhtmlMobileTextWriter writer) {
            if (Control.HasControls()) {
                ConditionalRenderOpeningDivElement(writer);
                RenderChildren (writer);
                ConditionalRenderClosingDivElement(writer);
                return;
            }
            if (Control.Items.Count != 0) {
                ClearPendingBreakIfDeviceBreaksOnBlockLevel(writer); // we are writing a block level element in all cases.
            }
            ConditionalEnterLayout(writer, Style);            
            RenderList (writer);
            ConditionalExitLayout(writer, Style);
        }

        /// <include file='doc\XhtmlBasicListAdapter.uex' path='docs/doc[@for="XhtmlListAdapter.RenderList"]/*' />
        protected virtual void RenderList (XhtmlMobileTextWriter writer) {
            MobileListItemCollection items = Control.Items;
            if (items.Count == 0) {
                return;
            }

            ListDecoration decoration = Control.Decoration;

            // Review: Consider replacing switch.
            switch (decoration) {
                case ListDecoration.Bulleted:
                    RenderBulletedList (writer);
                    break;
                case ListDecoration.Numbered:
                    RenderNumberedList (writer);
                    break;
                default:
                    if (!Device.Tables) {
                        RenderUndecoratedList(writer);
                        return;
                    }
                    RenderTableList (writer);
                    break;
            }
        }

        private void RenderBulletedList (XhtmlMobileTextWriter writer) {
            RenderOpeningListTag(writer, "ul");
            RenderListBody (writer, "<li>", "</li>");
            RenderClosingListTag (writer, "ul");
        }

        private void RenderTableList (XhtmlMobileTextWriter writer) {
            RenderOpeningListTag(writer, "table");
            RenderListBody (writer, "<tr><td>", "</td></tr>");
            RenderClosingListTag(writer, "table");
        }

        private void RenderNumberedList (XhtmlMobileTextWriter writer) {
            RenderOpeningListTag(writer, "ol");
            RenderListBody (writer, "<li>", "</li>");
            RenderClosingListTag(writer, "ol");
        }

        private void RenderListBody (XhtmlMobileTextWriter writer, String itemPrefix, String itemSuffix) {
            int pageStart = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;

            for (int i = 0; i < pageSize; i++) {
                MobileListItem item = Control.Items[pageStart + i];
                writer.Write (itemPrefix);
                RenderListItem (writer, item);
                writer.WriteLine(itemSuffix);
            }        
        }

        private void RenderUndecoratedList (XhtmlMobileTextWriter writer) {
            String br = writer.UseDivsForBreaks ? "</div><div>" : "<br/>";
            if((string)Device["usePOverDiv"] == "true")
                br = "<br/>";
            RenderListBody (writer, "", br);
        }

        private void RenderListItem (XhtmlMobileTextWriter writer, MobileListItem item) {
            String accessKey = GetCustomAttributeValue(item, XhtmlConstants.AccessKeyCustomAttribute);
            String cssClass = GetCustomAttributeValue(item, XhtmlConstants.CssClassCustomAttribute);
            if (Control.ItemsAsLinks) {
                RenderBeginLink (writer, item.Value, accessKey, Style, cssClass);
                writer.WriteEncodedText (item.Text);
                RenderEndLink (writer);
            }
            else if (Control.HasItemCommandHandler) {
                RenderPostBackEventAsAnchor (writer, 
                    item.Index.ToString(CultureInfo.InvariantCulture) /*event argument*/, 
                    item.Text /*link text*/,
                    accessKey,
                    Style,
                    cssClass);
            }
            else {
                writer.WriteEncodedText (item.Text);
            }
        }
    }
}
