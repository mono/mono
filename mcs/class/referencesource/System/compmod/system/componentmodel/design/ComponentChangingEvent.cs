//------------------------------------------------------------------------------
// <copyright file="ComponentChangingEvent.cs" company="Microsoft">
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
    /// <para>Provides data for the <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentChanging'/> event.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ComponentChangingEventArgs : EventArgs {

        private object component;
        private MemberDescriptor member;

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the component that is being changed or that is the parent container of the member being changed.      
        ///    </para>
        /// </devdoc>
        public object Component {
            get {
                return component;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the member of the component that is about to be changed.
        ///    </para>
        /// </devdoc>
        public MemberDescriptor Member {
            get {
                return member;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.Design.ComponentChangingEventArgs'/> class.
        ///    </para>
        /// </devdoc>
        public ComponentChangingEventArgs(object component, MemberDescriptor member) {
            this.component = component;
            this.member = member;
        }
    }
}
