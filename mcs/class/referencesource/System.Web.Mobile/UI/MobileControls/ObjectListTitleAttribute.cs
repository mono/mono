//------------------------------------------------------------------------------
// <copyright file="ObjectListTitleAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Object List Title attribute. Can be attached to a property to provide its
     * title in an object list field
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListTitleAttribute.uex' path='docs/doc[@for="ObjectListTitleAttribute"]/*' />
    [
        AttributeUsage(AttributeTargets.Property)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListTitleAttribute : Attribute 
    {
        private readonly String _title;

        /// <include file='doc\ObjectListTitleAttribute.uex' path='docs/doc[@for="ObjectListTitleAttribute.ObjectListTitleAttribute"]/*' />
        public ObjectListTitleAttribute(String title)
        {
            _title = title;
        }

        /// <include file='doc\ObjectListTitleAttribute.uex' path='docs/doc[@for="ObjectListTitleAttribute.Title"]/*' />
        public virtual String Title
        {
            get 
            {
                return _title;
            }
        }
    }
}


