//------------------------------------------------------------------------------
// <copyright file="CommandID.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Represents a
    ///       numeric Command ID and globally unique
    ///       ID (GUID) menu identifier that together uniquely identify a command.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class CommandID {
        private readonly Guid menuGroup;
        private readonly int  commandID;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.CommandID'/>
        ///       class. Creates a new command
        ///       ID.
        ///    </para>
        /// </devdoc>
        public CommandID(Guid menuGroup, int commandID) {
            this.menuGroup = menuGroup;
            this.commandID = commandID;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the numeric command ID.
        ///    </para>
        /// </devdoc>
        public virtual int ID {
            get {
                return commandID;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Overrides Object's Equals method.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (!(obj is CommandID)) {
                return false;
            }
            CommandID cid = (CommandID)obj;
            return cid.menuGroup.Equals(menuGroup) && cid.commandID == commandID;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return menuGroup.GetHashCode() << 2 | commandID;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the globally
        ///       unique ID
        ///       (GUID) of the menu group that the menu command this CommandID
        ///       represents belongs to.
        ///    </para>
        /// </devdoc>
        public virtual Guid Guid {
            get {
                return menuGroup;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Overrides Object's ToString method.
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            return menuGroup.ToString() + " : " + commandID.ToString(CultureInfo.CurrentCulture);
        }
    }

}
