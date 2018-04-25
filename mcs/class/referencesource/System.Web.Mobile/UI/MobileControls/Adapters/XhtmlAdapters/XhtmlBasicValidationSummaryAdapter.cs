//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicValidationSummary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicValidationSummaryAdapter.uex' path='docs/doc[@for="XhtmlValidationSummaryAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlValidationSummaryAdapter : XhtmlControlAdapter {

        private List _list;  // to paginate error messages
        private Link _link;  // to go back to the form validated by this control

        /// <include file='doc\XhtmlBasicValidationSummaryAdapter.uex' path='docs/doc[@for="XhtmlValidationSummaryAdapter.Control"]/*' />
        protected new ValidationSummary Control {
            get {
                return base.Control as ValidationSummary;
            }
        }

        /// <include file='doc\XhtmlBasicValidationSummaryAdapter.uex' path='docs/doc[@for="XhtmlValidationSummaryAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e) {
            // Create child controls to help on rendering
            _list = new List();
            Control.Controls.Add(_list);
            _link = new Link();
            Control.Controls.Add(_link);
        }

        /// <include file='doc\XhtmlBasicValidationSummaryAdapter.uex' path='docs/doc[@for="XhtmlValidationSummaryAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            String[] errorMessages = null;

            if (Control.Visible) {
                errorMessages = Control.GetErrorMessages();
            }

            if (errorMessages != null) {
                ConditionalEnterStyle(writer, Style, "div");
                ConditionalRenderOpeningDivElement(writer);
                if (Control.HeaderText.Length > 0) {
                    // ConditionalClearCachedEndTag() is for a device special case.
                    ConditionalClearCachedEndTag(writer, Control.HeaderText);
                    writer.WriteEncodedText (Control.HeaderText);
                }

                ArrayList arr = new ArrayList();
                foreach (String errorMessage in errorMessages) {
                    Debug.Assert(errorMessage != null && errorMessage.Length > 0, "Bad Error Messages");
                    arr.Add(errorMessage);
                }

                _list.Decoration = ListDecoration.Bulleted;
                _list.DataSource = arr;
                _list.DataBind();

                if (String.Compare(Control.FormToValidate, Control.Form.UniqueID, true, CultureInfo.CurrentCulture) != 0) {
                    _link.NavigateUrl = Constants.FormIDPrefix + Control.FormToValidate;
                    String controlBackLabel = Control.BackLabel;
                    _link.Text = controlBackLabel == null || controlBackLabel.Length == 0 ? GetDefaultLabel(BackLabel) : controlBackLabel;
                    // Summary writes its own break so last control should write one.
                    _link.BreakAfter = false;
                    ((IAttributeAccessor)_link).SetAttribute(XhtmlConstants.AccessKeyCustomAttribute, GetCustomAttributeValue(XhtmlConstants.AccessKeyCustomAttribute));
                }
                else {
                    _link.Visible = false;
                    // Summary writes its own break so last control should write one.
                    _list.BreakAfter = false;
                }

                // Render the child controls to display error message list and a
                // link for going back to the Form that is having error
                RenderChildren(writer);
                // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
                // ConditionalExitStyle may render a block element and clear the pending break.
                ConditionalSetPendingBreak(writer);            
                ConditionalRenderClosingDivElement(writer);
                ConditionalExitStyle(writer, Style);
            }
        }
    }
}
