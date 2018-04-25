//------------------------------------------------------------------------------
// <copyright file="ObjectListCommandEventArgs.cs" company="Microsoft">
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
     * Object List command event arguments
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListCommandEventArgs : CommandEventArgs 
    {
        /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs.DefaultCommand"]/*' />
        protected static readonly String DefaultCommand = "Default";
        private ObjectListItem _item;
        private Object _commandSource;

        /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs.ObjectListCommandEventArgs"]/*' />
        public ObjectListCommandEventArgs(ObjectListItem item, Object commandSource, CommandEventArgs originalArgs) : base(originalArgs) 
        {
            _item = item;
            _commandSource = commandSource;
        }

        /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs.ObjectListCommandEventArgs1"]/*' />
        public ObjectListCommandEventArgs(ObjectListItem item, String commandName) : base(commandName, item)
        {
            _item = item;
            _commandSource = null;
        }

        /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs.ListItem"]/*' />
        public ObjectListItem ListItem 
        {
            get 
            {
                return _item;
            }
        }

        /// <include file='doc\ObjectListCommandEventArgs.uex' path='docs/doc[@for="ObjectListCommandEventArgs.CommandSource"]/*' />
        public Object CommandSource 
        {
            get 
            {
                return _commandSource;
            }
        }

    }
}


