//------------------------------------------------------------------------------
// <copyright file="ListCommandEventArgs.cs" company="Microsoft">
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
     * List command event arguments
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ListCommandEventArgs : CommandEventArgs 
    {
        /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs.DefaultCommand"]/*' />
        protected static readonly String DefaultCommand = "Default";
        private MobileListItem _item;
        private Object _commandSource;

        /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs.ListCommandEventArgs"]/*' />
        public ListCommandEventArgs(MobileListItem item, Object commandSource, CommandEventArgs originalArgs) : base(originalArgs) 
        {
            _item = item;
            _commandSource = commandSource;
        }

        /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs.ListCommandEventArgs1"]/*' />
        public ListCommandEventArgs(MobileListItem item, Object commandSource) : base(DefaultCommand, item) 
        {
            _item = item;
            _commandSource = commandSource;
        }

        /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs.ListItem"]/*' />
        public MobileListItem ListItem 
        {
            get 
            {
                return _item;
            }
        }

        /// <include file='doc\ListCommandEventArgs.uex' path='docs/doc[@for="ListCommandEventArgs.CommandSource"]/*' />
        public Object CommandSource 
        {
            get 
            {
                return _commandSource;
            }
        }

    }
}


