//------------------------------------------------------------------------------
// <copyright file="ChtmlMobileTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mobile;
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
     * ChtmlMobileTextWriter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ChtmlMobileTextWriter.uex' path='docs/doc[@for="ChtmlMobileTextWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ChtmlMobileTextWriter : HtmlMobileTextWriter
    {
        /// <include file='doc\ChtmlMobileTextWriter.uex' path='docs/doc[@for="ChtmlMobileTextWriter.ChtmlMobileTextWriter"]/*' />
        public ChtmlMobileTextWriter(TextWriter writer, MobileCapabilities device) 
            : base(writer, device)
        {
        }

    }
}


