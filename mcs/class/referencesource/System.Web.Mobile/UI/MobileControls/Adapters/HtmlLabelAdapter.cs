//------------------------------------------------------------------------------
// <copyright file="HtmlLabelAdapter.cs" company="Microsoft">
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
     * HtmlLabelAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlLabelAdapter.uex' path='docs/doc[@for="HtmlLabelAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlLabelAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlLabelAdapter.uex' path='docs/doc[@for="HtmlLabelAdapter.Control"]/*' />
        protected new TextControl Control
        {
            get
            {
                return (TextControl)base.Control;
            }
        }

        /// <include file='doc\HtmlLabelAdapter.uex' path='docs/doc[@for="HtmlLabelAdapter.WhiteSpace"]/*' />
        protected internal bool WhiteSpace(String s)
        {
            if (s == null)
            {
                return true;
            }
            int length = s.Length;
            for(int i = 0; i < length; i++)
            {
                char c = s[i];
                if(!Char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <include file='doc\HtmlLabelAdapter.uex' path='docs/doc[@for="HtmlLabelAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            if( (writer.BeforeFirstControlWritten) &&
                (Device.RequiresLeadingPageBreak)  &&
                (String.IsNullOrEmpty(Control.Text) || WhiteSpace(Control.Text) ) )
            {
                writer.WriteBreak();
            }
            else
            {
                writer.WriteText(Control.Text, true);
            }
            writer.ExitStyle(Style, Control.BreakAfter);
        }
    }
}

