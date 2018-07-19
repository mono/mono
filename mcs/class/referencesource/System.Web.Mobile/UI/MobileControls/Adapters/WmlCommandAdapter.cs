//------------------------------------------------------------------------------
// <copyright file="WmlCommandAdapter.cs" company="Microsoft">
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
     * WmlCommandAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlCommandAdapter.uex' path='docs/doc[@for="WmlCommandAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlCommandAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlCommandAdapter.uex' path='docs/doc[@for="WmlCommandAdapter.Control"]/*' />
        protected new Command Control
        {
            get
            {
                return (Command)base.Control;
            }
        }

        /// <include file='doc\WmlCommandAdapter.uex' path='docs/doc[@for="WmlCommandAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            RenderSubmitEvent(writer, Control.SoftkeyLabel, Control.Text, Control.BreakAfter);
            writer.ExitStyle(Style);
        }
    }

}
