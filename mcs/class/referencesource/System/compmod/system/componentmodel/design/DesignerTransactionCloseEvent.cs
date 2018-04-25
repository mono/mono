//------------------------------------------------------------------------------
// <copyright file="DesignerTransactionCloseEvent.cs" company="Microsoft">
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
    using System.Reflection;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class DesignerTransactionCloseEventArgs : EventArgs {
        private bool commit;
        private bool lastTransaction;
        
        /// <devdoc>
        ///     Creates a new event args.  Commit is true if the transaction is committed.  This
        ///     defaults the LastTransaction property to true.
        /// </devdoc>
        [Obsolete("This constructor is obsolete. Use DesignerTransactionCloseEventArgs(bool, bool) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public DesignerTransactionCloseEventArgs(bool commit) : this(commit, true) {
        }
        
        /// <devdoc>
        ///     Creates a new event args.  Commit is true if the transaction is committed, and
        ///     lastTransaction is true if this is the last transaction to close.
        /// </devdoc>
        public DesignerTransactionCloseEventArgs(bool commit, bool lastTransaction) {
            this.commit = commit;
            this.lastTransaction = lastTransaction;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool TransactionCommitted {
            get {
                return commit;
            }
        }
        
        /// <devdoc>
        ///    Returns true if this is the last transaction to close.
        /// </devdoc>
        public bool LastTransaction {
            get {
                return lastTransaction;
            }
        }
    }
}
