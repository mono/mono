//------------------------------------------------------------------------------
// <copyright file="DesignerTransaction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///     Identifies a transaction within a designer.  Transactions are
    ///     used to wrap serveral changes into one unit of work, which 
    ///     helps performance.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class DesignerTransaction : IDisposable {
        private bool committed = false;
        private bool canceled = false;
        private bool suppressedFinalization = false;
        private string desc;
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected DesignerTransaction() : this("") {
        }
        
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected DesignerTransaction(string description) {
            this.desc = description;
        }
        
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Canceled {
            get {
                return canceled;
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Committed {
            get {
                return committed;
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Description {
            get {
                return desc;
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Cancel() {
            if (!canceled && !committed) {
                canceled = true;
                GC.SuppressFinalize(this);
                suppressedFinalization = true;
                OnCancel();
            }
        }
    
        /// <devdoc>
        ///     Commits this transaction.  Once a transaction has
        ///     been committed, further calls to this method
        ///     will do nothing.  You should always call this
        ///     method after creating a transaction to ensure
        ///     that the transaction is closed properly.
        /// </devdoc>
        public void Commit() {
            if (!committed && !canceled) {
                committed = true;
                GC.SuppressFinalize(this);
                suppressedFinalization = true;
                OnCommit();
            }
        }
        
          /// <devdoc>
        ///     User code should implement this method to perform
        ///     the actual work of committing a transaction.
        /// </devdoc>
        protected abstract void OnCancel(); 
        
        /// <devdoc>
        ///     User code should implement this method to perform
        ///     the actual work of committing a transaction.
        /// </devdoc>
        protected abstract void OnCommit();
        
        /// <devdoc>
        ///     Overrides Object to commit this transaction
        ///     in case the user forgot.
        /// </devdoc>
        ~DesignerTransaction() {
            Dispose(false);
        }
        
        /// <internalonly/>
        /// <devdoc>
        /// Private implementation of IDisaposable.
        /// When a transaction is disposed it is
        /// committed.
        /// </devdoc>
        void IDisposable.Dispose() {
            Dispose(true);

            // note - Dispose calls Cancel which sets this bit, so
            //        this should never be hit.
            //
            if (!suppressedFinalization) {
                System.Diagnostics.Debug.Fail("Invalid state. Dispose(true) should have called cancel which does the SuppressFinalize");
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing) {
            System.Diagnostics.Debug.Assert(disposing, "Designer transaction garbage collected, unable to cancel, please Cancel, Close, or Dispose your transaction.");
            System.Diagnostics.Debug.Assert(disposing || canceled || committed, "Disposing DesignerTransaction that has not been comitted or canceled; forcing Cancel" );
            Cancel();
        }
    }
}

