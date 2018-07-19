//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Transactions;

    sealed class PersistenceContextEnlistment : IEnlistmentNotification
    {
        PreparingEnlistment preparingEnlistment;
        Enlistment enlistment;

        // This will be true if we have received either a Prepare or Rollback
        // notification from the transaction manager. If this is true, it is too
        // late to try to add more entries to the undo collection.
        bool tooLateForMoreUndo;
        Transaction transaction;
        object ThisLock = new object();

        List<PersistenceContext> enlistedContexts;

        static Action<object> prepareCallback;
        static Action<object> commitCallback;
        static Action<object> rollbackCallback;
        static Action<object> indoubtCallback;

        internal PersistenceContextEnlistment(PersistenceContext context, Transaction transaction)
        {
            this.transaction = transaction;

            this.enlistedContexts = new List<PersistenceContext>();
            this.enlistedContexts.Add(context);
        }

        internal void AddToEnlistment(PersistenceContext context)
        {
            lock (this.ThisLock)
            {
                if (tooLateForMoreUndo)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PersistenceTooLateToEnlist));                    
                }
                
                this.enlistedContexts.Add(context);
            }
        }

        internal static Action<object> PrepareCallback
        {
            get
            {
                if (prepareCallback == null)
                {
                    prepareCallback = new Action<object>(DoPrepare);
                }
                return prepareCallback;
            }
        }

        internal static Action<object> CommitCallback
        {
            get
            {
                if (commitCallback == null)
                {
                    commitCallback = new Action<object>(DoCommit);
                }
                return commitCallback;
            }
        }

        internal static Action<object> RollbackCallback
        {
            get
            {
                if (rollbackCallback == null)
                {
                    rollbackCallback = new Action<object>(DoRollback);
                }
                return rollbackCallback;
            }
        }

        internal static Action<object> IndoubtCallback
        {
            get
            {
                if (indoubtCallback == null)
                {
                    indoubtCallback = new Action<object>(DoIndoubt);
                }
                return indoubtCallback;
            }
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            // We don't want to try to grab one of our locks while executing on the
            // System.Transactions notification thread because that will block all
            // the other notifications that need to be made. So schedule the
            // processing of this on another thread. If we decide that the locks
            // aren't necessary, we can get rid of this.
            this.preparingEnlistment = preparingEnlistment;
            ActionItem.Schedule(PrepareCallback, this);
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            // We don't want to try to grab one of our locks while executing on the
            // System.Transactions notification thread because that will block all
            // the other notifications that need to be made. So schedule the
            // processing of this on another thread. If we decide that the locks
            // aren't necessary, we can get rid of this.
            this.enlistment = enlistment;
            ActionItem.Schedule(CommitCallback, this);
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            // We don't want to try to grab one of our locks while executing on the
            // System.Transactions notification thread because that will block all
            // the other notifications that need to be made. So schedule the
            // processing of this on another thread. If we decide that the locks
            // aren't necessary, we can get rid of this.
            this.enlistment = enlistment;
            ActionItem.Schedule(RollbackCallback, this);
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            // We don't want to try to grab one of our locks while executing on the
            // System.Transactions notification thread because that will block all
            // the other notifications that need to be made. So schedule the
            // processing of this on another thread. If we decide that the locks
            // aren't necessary, we can get rid of this.
            this.enlistment = enlistment;
            ActionItem.Schedule(IndoubtCallback, this);
        }

        internal static void DoPrepare(object state)
        {
            PersistenceContextEnlistment pcEnlist = state as PersistenceContextEnlistment;
            Fx.Assert(null != pcEnlist, "PersistenceContextEnlistment.DoPrepare called with an object that is not a PersistenceContext.");

            lock (pcEnlist.ThisLock)
            {
                pcEnlist.tooLateForMoreUndo = true;
            }

            // This needs to be done outside of the lock because it could induce System.Transactions
            // to do a whole bunch of other work inline, including issuing the Commit call and doing
            // Completion notifications for hte transaction. We don't want to be holding the lock
            // during all of that.
            pcEnlist.preparingEnlistment.Prepared();
        }

        internal static void DoCommit(object state)
        {
            PersistenceContextEnlistment pcEnlist = state as PersistenceContextEnlistment;
            Fx.Assert(null != pcEnlist, "PersistenceContextEnlistment.DoCommit called with an object that is not a PersistenceContext.");

            lock (pcEnlist.ThisLock)
            {
                // Wake up the next waiter for the pc, if any.
                foreach (PersistenceContext context in pcEnlist.enlistedContexts)
                {
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(pcEnlist.transaction.GetHashCode());
            }
            // This needs to be outside the lock because SysTx might do other stuff on the thread.
            pcEnlist.enlistment.Done();
        }

        internal static void DoRollback(object state)
        {
            PersistenceContextEnlistment pcEnlist = state as PersistenceContextEnlistment;
            Fx.Assert(null != pcEnlist, "PersistenceContextEnlistment.DoRollback called with an object that is not a PersistenceContext.");

            lock (pcEnlist.ThisLock)
            {
                pcEnlist.tooLateForMoreUndo = true;
               
                foreach (PersistenceContext context in pcEnlist.enlistedContexts)
                {
                    context.Abort();
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(pcEnlist.transaction.GetHashCode());
            }
            // This needs to be outside the lock because SysTx might do other stuff on the thread.
            pcEnlist.enlistment.Done();
        }

        internal static void DoIndoubt(object state)
        {
            PersistenceContextEnlistment pcEnlist = state as PersistenceContextEnlistment;
            Fx.Assert(null != pcEnlist, "PersistenceContextEnlistment.DoIndoubt called with an object that is not a PersistenceContext.");

            lock (pcEnlist.ThisLock)
            {
                pcEnlist.tooLateForMoreUndo = true;

                foreach (PersistenceContext context in pcEnlist.enlistedContexts)
                {
                    context.Abort();
                    context.ScheduleNextTransactionWaiter();
                }
            }
            lock (PersistenceContext.Enlistments)
            {
                PersistenceContext.Enlistments.Remove(pcEnlist.transaction.GetHashCode());
            }
            // This needs to be outside the lock because SysTx might do other stuff on the thread.
            pcEnlist.enlistment.Done();
        }
    }
}
