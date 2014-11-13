//------------------------------------------------------------------------------
// <copyright file="UpWmlPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * UpWmlPageAdapter base class contains wml specific methods.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\UpWmlPageAdapter.uex' path='docs/doc[@for="UpWmlPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class UpWmlPageAdapter : WmlPageAdapter
    {
        /// <include file='doc\UpWmlPageAdapter.uex' path='docs/doc[@for="UpWmlPageAdapter.DeviceQualifies"]/*' />
        public static new bool DeviceQualifies(HttpContext context)
        {
            MobileCapabilities capabilities = ((MobileCapabilities)context.Request.Browser);
            bool qualifies = capabilities.Browser == "Phone.com";
            return qualifies;
        }

        /// <include file='doc\UpWmlPageAdapter.uex' path='docs/doc[@for="UpWmlPageAdapter.CreateTextWriter"]/*' />
        public override HtmlTextWriter CreateTextWriter(TextWriter writer)
        {
            return new UpWmlMobileTextWriter(writer, Device, Page);
        }
    }

}


















