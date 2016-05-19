//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicValidatorAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using WebControls = System.Web.UI.WebControls;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicValidatorAdapter.uex' path='docs/doc[@for="XhtmlValidatorAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlValidatorAdapter : XhtmlControlAdapter {

        /// <include file='doc\XhtmlBasicValidatorAdapter.uex' path='docs/doc[@for="XhtmlValidatorAdapter.Control"]/*' />
        protected new BaseValidator Control {
            get {
                return base.Control as BaseValidator;
            }
        }

        /// <include file='doc\XhtmlBasicValidatorAdapter.uex' path='docs/doc[@for="XhtmlValidatorAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            if (!Control.IsValid && Control.Display != WebControls.ValidatorDisplay.None) {
                ConditionalEnterStyle(writer, Style);
                ConditionalRenderOpeningSpanElement(writer);
                writer.WritePendingBreak();
                String controlText = Control.Text;
                String controlErrorMessage = Control.ErrorMessage;
                if (controlText != null & controlText.Length > 0) {
                    // ConditionalClearCachedEndTag() is for a device special case.
                    ConditionalClearCachedEndTag(writer, Control.Text);
                    writer.WriteEncodedText (Control.Text);
                }
                else if (controlErrorMessage != null && controlErrorMessage.Length > 0) {
                    ConditionalClearCachedEndTag(writer, Control.ErrorMessage);
                    writer.WriteEncodedText (Control.ErrorMessage);
                }
                // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
                // ConditionalExitStyle may render a block element and clear the pending break.
                ConditionalSetPendingBreak(writer);            
                ConditionalRenderClosingSpanElement(writer);
                ConditionalExitStyle(writer, Style);
            }
        }
    }
}
