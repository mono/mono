//------------------------------------------------------------------------------
// <copyright file="HtmlImageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif

{
    /*
     * HtmlImageAdapter class.
     */
    /// <include file='doc\HtmlImageAdapter.uex' path='docs/doc[@for="HtmlImageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlImageAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlImageAdapter.uex' path='docs/doc[@for="HtmlImageAdapter.Control"]/*' />
        protected new Image Control
        {
            get
            {
                return (Image)base.Control;
            }
        }

        /// <include file='doc\HtmlImageAdapter.uex' path='docs/doc[@for="HtmlImageAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            String target = Control.NavigateUrl;

            writer.EnterLayout(Style);
            if (!String.IsNullOrEmpty(target))
            {
                RenderBeginLink(writer, target);
            }

            if (String.IsNullOrEmpty(Control.ImageUrl))
            {
                // Just write the alternate as text
                writer.EnsureStyle();
                writer.MarkStyleContext();
                writer.EnterFormat(Style);
                writer.WriteText(Control.AlternateText, true);
                writer.ExitFormat(Style);
                writer.UnMarkStyleContext();
            }
            else
            {
                RenderImage(writer);
            }

            if (!String.IsNullOrEmpty(target))
            {
                RenderEndLink(writer);
            }
            writer.ExitLayout(Style, Control.BreakAfter);
        }

        /// <include file='doc\HtmlImageAdapter.uex' path='docs/doc[@for="HtmlImageAdapter.RenderImage"]/*' />
        protected internal virtual void RenderImage(HtmlMobileTextWriter writer)
        {
            String source = Control.ImageUrl;

            writer.WriteBeginTag("img");

            if (!String.IsNullOrEmpty(source))
            {
                // AUI 3652
                source = Control.ResolveUrl(source);

                writer.WriteAttribute("src", source, true /*encode*/);
                writer.AddResource(source);
            }

            if (!String.IsNullOrEmpty(Control.AlternateText))
            {
                writer.WriteAttribute("alt", Control.AlternateText, true);
            }

            writer.WriteAttribute("border", "0");
            writer.Write(" />");
        }
    }
}
