//------------------------------------------------------------------------------
// <copyright file="DocumentEventArgs.cs" company="Microsoft">
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
    /// <para>Provides data for the System.ComponentModel.Design.IDesignerEventService.DesignerEvent
    /// event that is generated when a document is created or disposed.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class DesignerEventArgs : EventArgs {
        private readonly IDesignerHost host;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the System.ComponentModel.Design.DesignerEventArgs
        ///       class.
        ///    </para>
        /// </devdoc>
        public DesignerEventArgs(IDesignerHost host) {
            this.host = host;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the host of the document.
        ///    </para>
        /// </devdoc>
        public IDesignerHost Designer {
            get {
                return host;
            }
        }
    }
}

