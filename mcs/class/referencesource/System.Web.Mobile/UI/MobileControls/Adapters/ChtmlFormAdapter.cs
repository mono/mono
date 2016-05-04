//------------------------------------------------------------------------------
// <copyright file="ChtmlFormAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Globalization;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{
    /*
     * ChtmlFormAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ChtmlFormAdapter.uex' path='docs/doc[@for="ChtmlFormAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ChtmlFormAdapter : HtmlFormAdapter
    {
        private static readonly String _contentTypeMetaTag = "<meta http-equiv=\"Content-Type\" content=\"{0}; charset={1}\">\r\n";
        
        /// <include file='doc\ChtmlFormAdapter.uex' path='docs/doc[@for="ChtmlFormAdapter.ShouldRenderFormTag"]/*' />
        protected override bool ShouldRenderFormTag()
        {
            if (!Device.RequiresOutputOptimization || Control.PageCount > 1)
            {
                return true;
            }

            return IsFormTagNeeded(Control);
        }

        // Recursive method to check if there is any descendant control
        // requires the form tag.  For unknown cases, the method returns true
        // in case the unknown rendering requires the form tag.
        private bool IsFormTagNeeded(Control control)
        {
            // Check itself first
            if (!control.Visible)
            {
                return false;
            }

            MobileControl mobileControl = control as MobileControl;
            if (mobileControl != null)
            {
                // Since we don't have control over what content is included
                // in the template, to be safe we just generate the form tag.
                if (mobileControl.IsTemplated)
                {
                    return true;
                }

                HtmlControlAdapter adapter = mobileControl.Adapter as HtmlControlAdapter;
                if (adapter != null && adapter.RequiresFormTag)
                {
                    return true;
                }
            }
            else if (!(control is UserControl) &&
                     !(control is LiteralControl))
            {
                // UserControl simply acts as a container, so the checking
                // should be delegated to its children below.
                // LiteralControl is a plain text control.  Also, it is
                // generated for the spaces in between mobile control tags so
                // we don't want to consider it as a form required control.
                // For other cases, we should generate form tag as we don't
                // know the content that will be generated.
                return true;
            }

            // No problem with the current control so far, now recursively
            // check its children.
            if (control.HasControls())
            {
                foreach (Control child in control.Controls)
                {
                    if (IsFormTagNeeded(child))
                    {
                        // This is to get out of recursive loop without
                        // further checking on other controls.
                        return true;
                    }
                }
            }

            return false;
        }

        /// <include file='doc\ChtmlFormAdapter.uex' path='docs/doc[@for="ChtmlFormAdapter.RenderExtraHeadElements"]/*' />
        protected override bool RenderExtraHeadElements(HtmlMobileTextWriter writer)
        {
            bool result = false;
            
            String metaTagName = Device.RequiredMetaTagNameValue;
            if (metaTagName != null)
            {
                if (writer != null)
                {
                    writer.Write("<meta NAME=\"" + metaTagName + "\" CONTENT=\"True\">\r\n");
                }
                result = true;
            }

            String charset = Page.Response.Charset;
            if (Device.RequiresContentTypeMetaTag &&
                charset != null && charset.Length > 0)
            {
                if (writer != null)
                {
                    writer.Write(String.Format(CultureInfo.InvariantCulture, _contentTypeMetaTag, Device.PreferredRenderingMime, charset));
                }
                result = true;
            }

            return result;
        }

        /// <include file='doc\ChtmlFormAdapter.uex' path='docs/doc[@for="ChtmlFormAdapter.RenderPagerTag"]/*' />
        protected internal override void RenderPagerTag(
            HtmlMobileTextWriter writer,
            int pageToNavigate,
            String text)
        {
            writer.EnterLayout(Style);
            writer.EnterFormat(Style);
            writer.WriteBeginTag("input");

            // Specially encode the page number with the control id.
            // The corresponding code that handles postback should know how
            // to extract the page number correctly.
            writer.Write(" name=\"");
            writer.Write(Control.UniqueID);
            writer.Write(Constants.PagePrefix);
            writer.Write(pageToNavigate.ToString(CultureInfo.InvariantCulture));
            writer.Write("\"");

            writer.WriteAttribute("type", "submit");
            writer.WriteAttribute("value", text, true);
            writer.Write("/>");
            writer.ExitFormat(Style);
            writer.ExitLayout(Style);
        }
    }
}
