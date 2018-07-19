//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicCommandAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicCommandAdapter.uex' path='docs/doc[@for="XhtmlCommandAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlCommandAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicCommandAdapter.uex' path='docs/doc[@for="XhtmlCommandAdapter.Control"]/*' />
        protected new Command Control {
            get {
                return base.Control as Command;
            }
        }

        public override bool LoadPostData(String key, NameValueCollection data, Object controlPrivateData, out bool dataChanged)
        {
            dataChanged = false;

            // HTML input tags of type image postback with the coordinates
            // of the click rather than the name of the control.
            String name = Control.UniqueID;
            String postX = data[name + ".x"];
            String postY = data[name + ".y"];

            if (postX != null && postY != null && postX.Length > 0 && postY.Length > 0)
            {
                // set dataChannged to cause RaisePostDataChangedEvent()
                dataChanged = true;
                return true;
            }

            // For other command control, defer to default logic in control.
            return base.LoadPostData(key, data, controlPrivateData, out dataChanged);
        }


        /// <include file='doc\XhtmlBasicCommandAdapter.uex' path='docs/doc[@for="XhtmlCommandAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            // Note: Since XHTML Basic and MP do not include the script element, we ignore the
            // Format==Link attribute as in CHTML.
            ConditionalClearPendingBreak(writer);
            string imageUrl = Control.ImageUrl;
            if (imageUrl != null && 
                imageUrl.Length > 0 &&
                Device.SupportsImageSubmit) {
                RenderAsInputTypeImage(writer);
            }
            else {
                RenderAsInputTypeSubmit(writer);
            }
        }

        private void RenderAsInputTypeImage(XhtmlMobileTextWriter writer) {
            ConditionalEnterStyle(writer, Style);
            writer.WriteBeginTag("input");
            writer.WriteAttribute("type", "image");
            writer.WriteAttribute("name", Control.UniqueID);
            writer.WriteAttribute("src", Control.ResolveUrl(Control.ImageUrl), true);
            writer.WriteAttribute("alt", Control.Text, true);
            ConditionalRenderClassAttribute(writer);
            ConditionalRenderCustomAttribute(writer, XhtmlConstants.AccessKeyCustomAttribute);
            writer.Write("/>");
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreakAfterInline(writer);
            ConditionalExitStyle(writer, Style);
        }

        private void RenderAsInputTypeSubmit(XhtmlMobileTextWriter writer){
            ConditionalEnterStyle(writer, Style);
            writer.WriteBeginTag("input");
            writer.WriteAttribute("type", "submit");
            writer.WriteAttribute("name", Control.UniqueID);
            writer.WriteAttribute("value", Control.Text, true);
            ConditionalRenderClassAttribute(writer);
            ConditionalRenderCustomAttribute(writer, XhtmlConstants.AccessKeyCustomAttribute);
            writer.Write("/>");
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreakAfterInline(writer);
            ConditionalPopPhysicalCssClass(writer);
            ConditionalExitStyle(writer, Style);
        }
       
    }
}
