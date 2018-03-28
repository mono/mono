//------------------------------------------------------------------------------
// <copyright file="ActiveDocumentEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    using Microsoft.Win32;

    /// <devdoc>
    /// <para>Provides data for the <see cref='System.ComponentModel.Design.IDesignerEventService.ActiveDesigner'/>
    /// event.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class ActiveDesignerEventArgs : EventArgs {
        /// <devdoc>
        ///     The document that is losing activation.
        /// </devdoc>
        private readonly IDesignerHost oldDesigner;

        /// <devdoc>
        ///     The document that is gaining activation.
        /// </devdoc>
        private readonly IDesignerHost newDesigner;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.Design.ActiveDesignerEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public ActiveDesignerEventArgs(IDesignerHost oldDesigner, IDesignerHost newDesigner) {
            this.oldDesigner = oldDesigner;
            this.newDesigner = newDesigner;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the document that is losing activation.
        ///    </para>
        /// </devdoc>
        public IDesignerHost OldDesigner {
            get {
                return oldDesigner;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the document that is gaining activation.
        ///    </para>
        /// </devdoc>
        public IDesignerHost NewDesigner {
            get {
                return newDesigner;
            }
        }

    }
}
