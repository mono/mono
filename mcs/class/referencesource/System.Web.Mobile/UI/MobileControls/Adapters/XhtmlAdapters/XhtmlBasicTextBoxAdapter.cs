//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicTextBoxAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Globalization;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicTextBoxAdapter.uex' path='docs/doc[@for="XhtmlTextBoxAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlTextBoxAdapter : XhtmlControlAdapter {

        /// <include file='doc\XhtmlBasicTextBoxAdapter.uex' path='docs/doc[@for="XhtmlTextBoxAdapter.Control"]/*' />
        public new TextBox Control {
            get {
                return base.Control as TextBox;
            }
        }

        // Used for optimization in RenderAsHiddenField.
        private String _staticValue;
        /// <include file='doc\XhtmlBasicTextBoxAdapter.uex' path='docs/doc[@for="XhtmlTextBoxAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e) {
            _staticValue = Control.Text;
            base.OnInit(e);
        }

        /// <include file='doc\XhtmlBasicTextBoxAdapter.uex' path='docs/doc[@for="XhtmlTextBoxAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            ConditionalClearPendingBreak(writer);
            ConditionalEnterStyle(writer, Style);
            ConditionalRenderOpeningSpanElement(writer);

            if ((String) Device[XhtmlConstants.RequiresOnEnterForward] == "true") {
                writer.AddOnEnterForwardSetVar(Control.UniqueID, Control.Text);
            }
            
            writer.WriteBeginTag("input");

            writer.WriteAttribute("name", Control.UniqueID);

            ConditionalRenderCustomAttribute(writer, XhtmlConstants.AccessKeyCustomAttribute);
            String controlText = Control.Text;
            if (controlText != null && controlText.Length > 0 && !Control.Password) {
                writer.Write(" value=\"");
                writer.WriteEncodedText(controlText);
                writer.Write("\"");
            }

            if (Control.Size > 0) {
                writer.WriteAttribute("size", Control.Size.ToString(CultureInfo.InvariantCulture));
            }

            if (Control.MaxLength > 0) {
                writer.WriteAttribute("maxlength", Control.MaxLength.ToString(CultureInfo.InvariantCulture));
            }

            String requiresType = Device["requiresInputTypeAttribute"];
            if (Control.Password) {
                writer.WriteAttribute("type", "password");
            }
            // InvariantCulture not needed, but included for best practices.
            else if (requiresType != null && String.Equals(requiresType, "true", StringComparison.OrdinalIgnoreCase)) {
                writer.WriteAttribute("type", "text");
            }


            writer.Write("/>");
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreakAfterInline(writer);            
            ConditionalRenderClosingSpanElement(writer);
            ConditionalExitStyle(writer, Style);
        }

        /// <include file='doc\XhtmlBasicTextBoxAdapter.uex' path='docs/doc[@for="XhtmlTextBoxAdapter.RenderAsHiddenInputField"]/*' />
        protected override void RenderAsHiddenInputField(XhtmlMobileTextWriter writer) {
            // Optimization - if viewstate is enabled for this control, and the
            // postback returns to this page, we just let it do the trick.

            if (Control.Form.Action.Length > 0 || (!IsViewStateEnabled() && Control.Text != _staticValue)) {
                writer.WriteHiddenField(Control.UniqueID, Control.Text);
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
