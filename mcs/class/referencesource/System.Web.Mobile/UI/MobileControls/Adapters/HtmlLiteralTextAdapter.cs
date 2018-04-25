//------------------------------------------------------------------------------
// <copyright file="HtmlLiteralTextAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Drawing;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * HtmlLiteralTextAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlLiteralTextAdapter.uex' path='docs/doc[@for="HtmlLiteralTextAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlLiteralTextAdapter : HtmlControlAdapter
    {

        /// <include file='doc\HtmlLiteralTextAdapter.uex' path='docs/doc[@for="HtmlLiteralTextAdapter.Control"]/*' />
        protected new LiteralText Control
        {
            get
            {
                return (LiteralText)base.Control;
            }
        }

        //  calls the more specific render methods
        /// <include file='doc\HtmlLiteralTextAdapter.uex' path='docs/doc[@for="HtmlLiteralTextAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            String text = Control.PagedText;
            
            writer.EnterStyle(Style);
            writer.WriteText(text, true);
            writer.ExitStyle(Style, Control.BreakAfter);
        }
    }
}
