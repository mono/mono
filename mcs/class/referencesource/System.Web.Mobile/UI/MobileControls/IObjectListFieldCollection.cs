//------------------------------------------------------------------------------
// <copyright file="IObjectListFieldCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.MobileControls
{
    /*
     * Object List Field Collection interface. This provides a read-only base
     * interface for the real object list field collection class, and is used when
     * read-only access to a field collection is desired.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\IObjectListFieldCollection.uex' path='docs/doc[@for="IObjectListFieldCollection"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface IObjectListFieldCollection : ICollection
    {
        /// <include file='doc\IObjectListFieldCollection.uex' path='docs/doc[@for="IObjectListFieldCollection.GetAll"]/*' />
        ObjectListField[] GetAll();
        /// <include file='doc\IObjectListFieldCollection.uex' path='docs/doc[@for="IObjectListFieldCollection.this"]/*' />

        ObjectListField this[int index] 
        {
            get;
        }
        /// <include file='doc\IObjectListFieldCollection.uex' path='docs/doc[@for="IObjectListFieldCollection.IndexOf"]/*' />

        int IndexOf(ObjectListField field);
        /// <include file='doc\IObjectListFieldCollection.uex' path='docs/doc[@for="IObjectListFieldCollection.IndexOf1"]/*' />
        int IndexOf(String fieldIDOrTitle);
    }

}

