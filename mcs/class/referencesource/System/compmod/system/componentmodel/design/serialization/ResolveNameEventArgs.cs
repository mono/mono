//------------------------------------------------------------------------------
// <copyright file="ResolveNameEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {

    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///     EventArgs for the ResolveNameEventHandler.  This event is used
    ///     by the serialization process to match a name to an object
    ///     instance.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class ResolveNameEventArgs : EventArgs {
        private string name;
        private object value;
        
        /// <devdoc>
        ///     Creates a new resolve name event args object.
        /// </devdoc>
        public ResolveNameEventArgs(string name) {
            this.name = name;
            this.value = null;
        }
    
        /// <devdoc>
        ///     The name of the object that needs to be resolved.
        /// </devdoc>
        public string Name {
            get {
                return name;
            }
        }
        
        /// <devdoc>
        ///     The object that matches the name.
        /// </devdoc>
        public object Value {
            get {
                return value;
            }
            set {
                this.value = value;
            }
        }
    }
}

