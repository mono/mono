//------------------------------------------------------------------------------
// <copyright file="ObjectListShowCommandsEventArgs.cs" company="Microsoft">
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
     * Object List show command event arguments
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListShowCommandsEventArgs.uex' path='docs/doc[@for="ObjectListShowCommandsEventArgs"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListShowCommandsEventArgs : EventArgs
    {
        private ObjectListItem _item;
        private ObjectListCommandCollection _commands;

        /// <include file='doc\ObjectListShowCommandsEventArgs.uex' path='docs/doc[@for="ObjectListShowCommandsEventArgs.ObjectListShowCommandsEventArgs"]/*' />
        public ObjectListShowCommandsEventArgs(ObjectListItem item, ObjectListCommandCollection commands)
        {
            _item = item;
            _commands = commands;
        }

        /// <include file='doc\ObjectListShowCommandsEventArgs.uex' path='docs/doc[@for="ObjectListShowCommandsEventArgs.Commands"]/*' />
        public ObjectListCommandCollection Commands
        {
            get
            {
                return _commands;
            }
        }

        /// <include file='doc\ObjectListShowCommandsEventArgs.uex' path='docs/doc[@for="ObjectListShowCommandsEventArgs.ListItem"]/*' />
        public ObjectListItem ListItem
        {
            get
            {
                return _item;
            }
        }
    }
}


