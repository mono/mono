//------------------------------------------------------------------------------
// <copyright file="ObjectListCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Object List Command class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ObjectListCommand.uex' path='docs/doc[@for="ObjectListCommand"]/*' />
    [
        PersistName("Command")
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ObjectListCommand
    {
        private String _name;
        private String _text;
        private ObjectListCommandCollection _owner;

        /// <include file='doc\ObjectListCommand.uex' path='docs/doc[@for="ObjectListCommand.ObjectListCommand"]/*' />
        public ObjectListCommand()
        {
        }

        /// <include file='doc\ObjectListCommand.uex' path='docs/doc[@for="ObjectListCommand.ObjectListCommand1"]/*' />
        public ObjectListCommand(String name, String text)
        {
            _name = name;
            _text = text;
        }

        /// <include file='doc\ObjectListCommand.uex' path='docs/doc[@for="ObjectListCommand.Name"]/*' />
        [
            DefaultValue("")
        ]
        public String Name
        {
            get
            {
                return (_name == null) ? String.Empty : _name;
            }

            set
            {
                _name = value;
                if (Owner != null)
                {
                    Owner.SetDirty ();
                }
            }
        }

        /// <include file='doc\ObjectListCommand.uex' path='docs/doc[@for="ObjectListCommand.Text"]/*' />
        [
            DefaultValue("")
        ]
        public String Text
        {
            get
            {
                return (_text == null) ? String.Empty : _text;
            }

            set
            {
                _text = value;
                if (Owner != null)
                {
                    Owner.SetDirty ();
                }
            }
        }

        internal ObjectListCommandCollection Owner
        {
            get
            {
                return _owner;
            }

            set
            {
                _owner = value;
            }
        }
    }
}



