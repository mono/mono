//------------------------------------------------------------------------------
// <copyright file="ComponentChangedEvent.cs" company="Microsoft">
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
    /// <para>Provides data for the <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentChanged'/> event.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ComponentChangedEventArgs : EventArgs {

        private object component;
        private MemberDescriptor member;
        private object oldValue;
        private object newValue;

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the component that is the cause of this event.      
        ///    </para>
        /// </devdoc>
        public object Component {
            get {
                return component;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the member that is about to change.      
        ///    </para>
        /// </devdoc>
        public MemberDescriptor Member {
            get {
                return member;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the new value of the changed member.
        ///    </para>
        /// </devdoc>
        public object NewValue {
            get {
                return newValue;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the old value of the changed member.      
        ///    </para>
        /// </devdoc>
        public object OldValue {
            get {
                return oldValue;
            }
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Design.ComponentChangedEventArgs'/> class.</para>
        /// </devdoc>
        public ComponentChangedEventArgs(object component, MemberDescriptor member, object oldValue, object newValue) {
            this.component = component;
            this.member = member;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }

}
