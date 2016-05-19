//------------------------------------------------------------------------------
// <copyright file="ListControlBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Control builder for lists and selection lists, that allows list items to be placed inline.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ListControlBuilder.uex' path='docs/doc[@for="ListControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ListControlBuilder : MobileControlBuilder
    {
        /// <include file='doc\ListControlBuilder.uex' path='docs/doc[@for="ListControlBuilder.GetChildControlType"]/*' />
        public override Type GetChildControlType(String tagName, IDictionary attributes) 
        {
            if (String.Compare(tagName, "item", StringComparison.OrdinalIgnoreCase) == 0) 
            {
                return typeof(MobileListItem);
            }
            else
            {
                return base.GetChildControlType(tagName, attributes);
            }
        }
    }

}

