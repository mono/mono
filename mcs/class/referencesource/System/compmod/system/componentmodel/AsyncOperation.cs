//------------------------------------------------------------------------------
// <copyright file="AsyncOperation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel
{
    using System.Security.Permissions;
    using System.Threading;
    
    [HostProtection(SharedState = true)]
    public sealed class AsyncOperation
    {
        private SynchronizationContext syncContext;
        private object userSuppliedState; 
        private bool alreadyCompleted;

        /// <summary>
        ///     Constructor. Protected to avoid unwitting usage - AsyncOperation objects
        ///     are typically created by AsyncOperationManager calling CreateOperation.
        /// </summary>
        private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext)
        {
            this.userSuppliedState = userSuppliedState;
            this.syncContext = syncContext;
            this.alreadyCompleted = false;
            this.syncContext.OperationStarted();
        }

        /// <summary>
        ///     Destructor. Guarantees that [....] context will always get notified of completion.
        /// </summary>
        ~AsyncOperation()
        {
            if (!alreadyCompleted && syncContext != null)
            {
                syncContext.OperationCompleted();
            }
        }

        public object UserSuppliedState
        {
            get { return userSuppliedState; }
        }

        /// <include file='doc\AsyncOperation.uex' path='docs/doc[@for="AsyncOperation.SynchronizationContext"]/*' />
        public SynchronizationContext SynchronizationContext
        {
            get
            {
                return syncContext;
            }
        }

        public void Post(SendOrPostCallback d, object arg)
        {
            VerifyNotCompleted();
            VerifyDelegateNotNull(d);
            syncContext.Post(d, arg);
        }

        public void PostOperationCompleted(SendOrPostCallback d, object arg)
        {
            Post(d, arg);
            OperationCompletedCore();
        }

        public void OperationCompleted()
        {
            VerifyNotCompleted();
            OperationCompletedCore();
        }

        private void OperationCompletedCore()
        {
            try
            {
                syncContext.OperationCompleted();
            }
            finally
            {
                alreadyCompleted = true;
                GC.SuppressFinalize(this);
            }
        }

        private void VerifyNotCompleted()
        {
            if (alreadyCompleted)
            {
                throw new InvalidOperationException(SR.GetString(SR.Async_OperationAlreadyCompleted));
            }
        }

        private void VerifyDelegateNotNull(SendOrPostCallback d)
        {
            if (d == null)
            {
                throw new ArgumentNullException(SR.GetString(SR.Async_NullDelegate), "d");
            }
        }

        /// <summary>
        ///     Only for use by AsyncOperationManager to create new AsyncOperation objects
        /// </summary>
        internal static AsyncOperation CreateOperation(object userSuppliedState, SynchronizationContext syncContext)
        {
            AsyncOperation newOp = new AsyncOperation(userSuppliedState, syncContext); 
            return newOp;
        }
    }
}

