//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Transactions;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Runtime;

    class MsmqReceiveContextSettings : IReceiveContextSettings
    {
        public MsmqReceiveContextSettings()
        {
            ValidityDuration = MsmqDefaults.ValidityDuration;
        }

        public MsmqReceiveContextSettings(IReceiveContextSettings toBeCloned)
        {
            Enabled = toBeCloned.Enabled;
            ValidityDuration = toBeCloned.ValidityDuration;
        }

        public TimeSpan ValidityDuration
        {
            get;
            private set;
        }

        public bool Enabled
        {
            get;
            set;
        }

        internal void SetValidityDuration(TimeSpan validityDuration)
        {
            ValidityDuration = validityDuration;
        }
    }

    class MsmqReceiveContext : ReceiveContext
    {
        long lookupId;
        DateTime expiryTime;
        MsmqReceiveContextLockManager manager;

        public MsmqReceiveContext(long lookupId, DateTime expiryTime, MsmqReceiveContextLockManager manager)
            : base()
        {
            this.manager = manager;
            this.lookupId = lookupId;
            this.expiryTime = expiryTime;
        }

        public long LookupId
        {
            get
            {
                return this.lookupId;
            }
        }

        public DateTime ExpiryTime
        {
            get
            {
                return this.expiryTime;
            }
        }

        public MsmqReceiveContextLockManager Manager
        {
            get
            {
                return this.manager;
            }
        }

        public void MarkContextExpired()
        {
            base.Fault();
        }

        protected override void OnComplete(TimeSpan timeout)
        {
            this.manager.DeleteMessage(this, timeout);
        }

        protected override void OnAbandon(TimeSpan timeout)
        {
            this.manager.UnlockMessage(this, timeout);
        }

        protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveContextAsyncResult.CreateComplete(this, timeout, callback, state);
        }

        protected override void OnEndComplete(IAsyncResult result)
        {
            ReceiveContextAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveContextAsyncResult.CreateAbandon(this, timeout, callback, state);
        }

        protected override void OnEndAbandon(IAsyncResult result)
        {
            ReceiveContextAsyncResult.End(result);
        }

        class ReceiveContextAsyncResult : AsyncResult
        {
            MsmqReceiveContext receiver;
            TimeoutHelper timeoutHelper;
            static Action<object> onComplete;
            static Action<object> onAbandon;
            Transaction associatedTransaction;

            ReceiveContextAsyncResult(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state, Action<object> target)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.receiver = receiver;

                if (Transaction.Current != null)
                {
                    this.associatedTransaction = Transaction.Current;
                }

                ActionItem.Schedule(target, this);
            }

            public static IAsyncResult CreateComplete(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (onComplete == null)
                {
                    onComplete = new Action<object>(OnComplete);
                }
                return new ReceiveContextAsyncResult(receiver, timeout, callback, state, onComplete);
            }

            public static IAsyncResult CreateAbandon(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (onAbandon == null)
                {
                    onAbandon = new Action<object>(OnAbandon);
                }
                return new ReceiveContextAsyncResult(receiver, timeout, callback, state, onAbandon);
            }

            static void OnComplete(object parameter)
            {
                ReceiveContextAsyncResult result = parameter as ReceiveContextAsyncResult;
                Exception completionException = null;
                Transaction savedTransaction = null;
                try
                {
                    // set the current transaction object for this worker thread as this operation could
                    // have been scheduled by another worker thread. We do not want to complete in
                    // worker threads ambient transaction. associatedTransaction can be null. 
                    savedTransaction = Transaction.Current;
                    Transaction.Current = result.associatedTransaction;
                    result.receiver.OnComplete(result.timeoutHelper.RemainingTime());
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completionException = e;
                }
                finally
                {
                    Transaction.Current = savedTransaction;
                }
                result.Complete(false, completionException);
            }

            static void OnAbandon(object parameter)
            {
                ReceiveContextAsyncResult result = parameter as ReceiveContextAsyncResult;
                Exception completionException = null;
                try
                {
                    result.receiver.OnAbandon(result.timeoutHelper.RemainingTime());
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    completionException = e;
                }
                result.Complete(false, completionException);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReceiveContextAsyncResult>(result);
            }
        }
    }
}
