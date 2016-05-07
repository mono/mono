//------------------------------------------------------------------------------
// <copyright file="LoadItemsEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Load Items event arguments.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\LoadItemsEventArgs.uex' path='docs/doc[@for="LoadItemsEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class LoadItemsEventArgs : EventArgs
    {

        /// <include file='doc\LoadItemsEventArgs.uex' path='docs/doc[@for="LoadItemsEventArgs.LoadItemsEventArgs"]/*' />
        public LoadItemsEventArgs(int index, int count)
        {
            _itemIndex = index;
            _itemCount = count;
        }

        private int _itemIndex;
        /// <include file='doc\LoadItemsEventArgs.uex' path='docs/doc[@for="LoadItemsEventArgs.ItemIndex"]/*' />
        public int ItemIndex
        {
            get
            {
                return _itemIndex;
            }
        }

        private int _itemCount;
        /// <include file='doc\LoadItemsEventArgs.uex' path='docs/doc[@for="LoadItemsEventArgs.ItemCount"]/*' />
        public int ItemCount
        {
            get
            {
                return _itemCount;
            }
        }

    }
}



