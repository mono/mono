//------------------------------------------------------------------------------
// <copyright file="LiteralLink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Literal Link class. Although public, this is an internal link class used for
     * literal hyperlinks.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\LiteralLink.uex' path='docs/doc[@for="LiteralLink"]/*' />
    [
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class LiteralLink : Link
    {
        internal override bool TrimInnerText
        {
            get
            {
                return false;
            }
        }
    }

}
