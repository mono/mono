//------------------------------------------------------------------------------
// <copyright file="WmlImageAdapter.cs" company="Microsoft">
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
     * WmlImageAdapter class.
     */
    /// <include file='doc\WmlImageAdapter.uex' path='docs/doc[@for="WmlImageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlImageAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlImageAdapter.uex' path='docs/doc[@for="WmlImageAdapter.Control"]/*' />
        protected new Image Control
        {
            get
            {
                return (Image)base.Control;
            }
        }

        /// <include file='doc\WmlImageAdapter.uex' path='docs/doc[@for="WmlImageAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            String source = Control.ImageUrl;
            String target = Control.NavigateUrl;
            String text   = Control.AlternateText;
            bool   breakAfterContents = Control.BreakAfter;
            String softkeyLabel = Control.SoftkeyLabel;
            bool implicitSoftkeyLabel = false;
            if (softkeyLabel.Length == 0)
            {
                implicitSoftkeyLabel = true;
                softkeyLabel = text;
            }

            writer.EnterLayout(Style);
            if (!String.IsNullOrEmpty(target))
            {
                RenderBeginLink(writer, target, softkeyLabel, implicitSoftkeyLabel, true);
                breakAfterContents = false;
            }

            if (String.IsNullOrEmpty(source))
            {
                // Just write the alternate as text
                writer.RenderText(text, breakAfterContents);
            }
            else
            {
                String localSource;

                if (source.StartsWith(Constants.SymbolProtocol, StringComparison.Ordinal))
                {
                    // src is required according to WML
                    localSource = source.Substring(Constants.SymbolProtocol.Length);
                    source = String.Empty;
                }
                else
                {
                    localSource = null;

                    // AUI 3652
                    source = Control.ResolveUrl(source);

                    writer.AddResource(source);
                }

                writer.RenderImage(source, localSource, text, breakAfterContents);
            }

            if (!String.IsNullOrEmpty(target))
            {
                RenderEndLink(writer, target, Control.BreakAfter);
            }
            writer.ExitLayout(Style);
        }
    }
}
