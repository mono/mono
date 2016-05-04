//------------------------------------------------------------------------------
// <copyright file="ListDataBindEventArgs.cs" company="Microsoft">
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
     * List item data binding arguments.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ListDataBindEventArgs.uex' path='docs/doc[@for="ListDataBindEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ListDataBindEventArgs : EventArgs {

        private MobileListItem _listItem;
        private Object _dataItem;

        /// <include file='doc\ListDataBindEventArgs.uex' path='docs/doc[@for="ListDataBindEventArgs.ListDataBindEventArgs"]/*' />
        public ListDataBindEventArgs(MobileListItem item, Object dataItem)
        {
            _listItem = item;
            _dataItem = dataItem;
        }

        /// <include file='doc\ListDataBindEventArgs.uex' path='docs/doc[@for="ListDataBindEventArgs.ListItem"]/*' />
        public MobileListItem ListItem 
        {
            get 
            {
                return _listItem;
            }
        }

        /// <include file='doc\ListDataBindEventArgs.uex' path='docs/doc[@for="ListDataBindEventArgs.DataItem"]/*' />
        public Object DataItem
        {
            get 
            {
                return _dataItem;
            }
        }

    }

}


