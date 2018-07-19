//------------------------------------------------------------------------------
// <copyright file="WmlLabelAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlLabelAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlLabelAdapter.uex' path='docs/doc[@for="WmlLabelAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlLabelAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlLabelAdapter.uex' path='docs/doc[@for="WmlLabelAdapter.Control"]/*' />
        protected new TextControl Control
        {
            get
            {
                return (TextControl)base.Control;
            }
        }

        /// <include file='doc\WmlLabelAdapter.uex' path='docs/doc[@for="WmlLabelAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            writer.RenderText(Control.Text, Control.BreakAfter);
            writer.ExitStyle(Style);
        }
    }

}

