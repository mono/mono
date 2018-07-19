//------------------------------------------------------------------------------
// <copyright file="ComponentEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Provides data for the System.ComponentModel.Design.IComponentChangeService.ComponentEvent
    /// event raised for component-level events.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class ComponentEventArgs : EventArgs {

        private IComponent component;

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the component associated with the event.
        ///    </para>
        /// </devdoc>
        public virtual IComponent Component {
            get {
                return component;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the System.ComponentModel.Design.ComponentEventArgs class.
        ///    </para>
        /// </devdoc>
        public ComponentEventArgs(IComponent component) {
            this.component = component;
        }
    }
}
