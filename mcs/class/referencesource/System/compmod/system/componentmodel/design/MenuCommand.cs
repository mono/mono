//------------------------------------------------------------------------------
// <copyright file="MenuCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       Represents a Windows
    ///       menu or toolbar item.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class MenuCommand {

        // Events that we suface or call on
        //
        private EventHandler execHandler;
        private EventHandler statusHandler;

        private CommandID    commandID;
        private int          status;
        private IDictionary  properties;

        /// <devdoc>
        ///     Indicates that the given command is enabled.  An enabled command may
        ///     be selected by the user (it's not greyed out).
        /// </devdoc>
        private const int ENABLED   =  0x02;  //tagOLECMDF.OLECMDF_ENABLED;

        /// <devdoc>
        ///     Indicates that the given command is not visible on the command bar.
        /// </devdoc>
        private const int INVISIBLE = 0x10;

        /// <devdoc>
        ///     Indicates that the given command is checked in the "on" state.
        /// </devdoc>
        private const int CHECKED   = 0x04; // tagOLECMDF.OLECMDF_LATCHED;

        /// <devdoc>
        ///     Indicates that the given command is supported.  Marking a command
        ///     as supported indicates that the shell will not look any further up
        ///     the command target chain.
        /// </devdoc>
        private const int SUPPORTED = 0x01; // tagOLECMDF.OLECMDF_SUPPORTED


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.ComponentModel.Design.MenuCommand'/>.
        ///    </para>
        /// </devdoc>
        public MenuCommand(EventHandler handler, CommandID command) {
            this.execHandler = handler;
            this.commandID = command;
            this.status = SUPPORTED | ENABLED;
        }

        /// <devdoc>
        ///    <para> Gets or sets a value indicating whether this menu item is checked.</para>
        /// </devdoc>
        public virtual bool Checked {
            get {
                return (status & CHECKED) != 0;
            }

            set {
                SetStatus(CHECKED, value);
            }
        }

        /// <devdoc>
        ///    <para> Gets or
        ///       sets a value indicating whether this menu item is available.</para>
        /// </devdoc>
        public virtual bool Enabled {
            get{
                return (status & ENABLED) != 0;
            }

            set {
                SetStatus(ENABLED, value);
            }
        }

        private void SetStatus(int mask, bool value){
            int newStatus = status;

            if (value) {
                newStatus |= mask;
            }
            else {
                newStatus &= ~mask;
            }

            if (newStatus != status) {
                status = newStatus;
                OnCommandChanged(EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// </devdoc>
        public virtual IDictionary Properties
        {
            get {
                if (properties == null) {
                    properties = new HybridDictionary();
                }

                return properties;
            }
        }




        /// <devdoc>
        ///    <para> Gets or sets a value
        ///       indicating whether this menu item is supported.</para>
        /// </devdoc>
        public virtual bool Supported {
            get{
                return (status & SUPPORTED) != 0;
            }
            set{
                SetStatus(SUPPORTED, value);
            }
        }

        /// <devdoc>
        ///    <para> Gets or sets a value
        ///       indicating if this menu item is visible.</para>
        /// </devdoc>
        public virtual bool Visible {
            get {
                return (status & INVISIBLE) == 0;
            }
            set {
                SetStatus(INVISIBLE, !value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Occurs when the menu command changes.
        ///    </para>
        /// </devdoc>
        public event EventHandler CommandChanged {
            add {
                statusHandler += value;
            }
            remove {
                statusHandler -= value;
            }
        }

        /// <devdoc>
        /// <para>Gets the <see cref='System.ComponentModel.Design.CommandID'/> associated with this menu command.</para>
        /// </devdoc>
        public virtual CommandID CommandID {
            get {
                return commandID;
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Invokes a menu item.
        ///    </para>
        /// </devdoc>
        public virtual void Invoke() {
            if (execHandler != null){
                try {
                    execHandler(this, EventArgs.Empty);
                }
                catch (CheckoutException cxe) {
                    if (cxe == CheckoutException.Canceled)
                        return;
                    
                    throw;
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Invokes a menu item.  The default implementation of this method ignores 
        ///       the argument, but deriving classes may override this method.
        ///    </para>
        /// </devdoc>
        public virtual void Invoke(object arg) {
            Invoke();
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the OLE command status code for this menu item.
        ///    </para>
        /// </devdoc>
        public virtual int OleStatus {
            get {
                return status;
            }
        }

        /// <devdoc>
        ///    <para>Provides notification and is called in response to 
        ///       a <see cref='System.ComponentModel.Design.MenuCommand.CommandChanged'/> event.</para>
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] // Safe: FullTrust LinkDemand to instantiate an object of this class.
        protected virtual void OnCommandChanged(EventArgs e) {
            if (statusHandler != null) {
                statusHandler(this, e);
            }
        }

        /// <devdoc>
        ///    Overrides object's ToString().
        /// </devdoc>
        public override string ToString() {
            string str = CommandID.ToString() + " : ";
            if ((status & SUPPORTED) != 0) {
                str += "Supported";
            }
            if ((status & ENABLED) != 0) {
                str += "|Enabled";
            }
            if ((status & INVISIBLE) == 0) {
                str += "|Visible";
            }
            if ((status & CHECKED) != 0) {
                str += "|Checked";
            }
            return str;
        }
    }

}
