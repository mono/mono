//------------------------------------------------------------------------------
// <copyright file="ObjectListSelectEventArgs.cs" company="Microsoft">
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
     * Object List select event arguments
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListSelectEventArgs.uex' path='docs/doc[@for="ObjectListSelectEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListSelectEventArgs : EventArgs 
    {
        private ObjectListItem _item;
        private bool _selectMore;
        private bool _useDefaultHandling = true;

        /// <include file='doc\ObjectListSelectEventArgs.uex' path='docs/doc[@for="ObjectListSelectEventArgs.ObjectListSelectEventArgs"]/*' />
        public ObjectListSelectEventArgs(ObjectListItem item, bool selectMore)
        {
            _item = item;
            _selectMore = selectMore;
        }

        /// <include file='doc\ObjectListSelectEventArgs.uex' path='docs/doc[@for="ObjectListSelectEventArgs.ListItem"]/*' />
        public ObjectListItem ListItem 
        {
            get 
            {
                return _item;
            }
        }

        /// <include file='doc\ObjectListSelectEventArgs.uex' path='docs/doc[@for="ObjectListSelectEventArgs.SelectMore"]/*' />
        public bool SelectMore
        {
            get 
            {
                return _selectMore;
            }
        }

        /// <include file='doc\ObjectListSelectEventArgs.uex' path='docs/doc[@for="ObjectListSelectEventArgs.UseDefaultHandling"]/*' />
        public bool UseDefaultHandling
        {
            get
            {
                return _useDefaultHandling;
            }

            set
            {
                _useDefaultHandling = value;
            }
        }

    }
}


