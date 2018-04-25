//------------------------------------------------------------------------------
// <copyright file="ITemplateable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{

    /*
     * Marker interface to indicate that control supports templates.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ITemplateable.uex' path='docs/doc[@for="ITemplateable"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface ITemplateable
    {
    }
}

