//------------------------------------------------------------------------------
// <copyright file="ObjectListDataBindEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

using System;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * ObjectList item data binding arguments.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListDataBindEventArgs.uex' path='docs/doc[@for="ObjectListDataBindEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListDataBindEventArgs : EventArgs {

        private ObjectListItem _item;
        private Object _dataItem;

        /// <include file='doc\ObjectListDataBindEventArgs.uex' path='docs/doc[@for="ObjectListDataBindEventArgs.ObjectListDataBindEventArgs"]/*' />
        public ObjectListDataBindEventArgs(ObjectListItem item, Object dataItem)
        {
            _item = item;
            _dataItem = dataItem;
        }

        /// <include file='doc\ObjectListDataBindEventArgs.uex' path='docs/doc[@for="ObjectListDataBindEventArgs.ListItem"]/*' />
        public ObjectListItem ListItem 
        {
            get 
            {
                return _item;
            }
        }

        /// <include file='doc\ObjectListDataBindEventArgs.uex' path='docs/doc[@for="ObjectListDataBindEventArgs.DataItem"]/*' />
        public Object DataItem
        {
            get 
            {
                return _dataItem;
            }
        }

    }

}


