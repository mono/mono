//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Transactions;
    using System.Runtime;

    internal class MsmqReceiveContextLockManager : IDisposable
    {
        MsmqReceiveContextSettings receiveContextSettings;
        IOThreadTimer messageExpiryTimer;
        TimeSpan messageTimeoutInterval = TimeSpan.FromSeconds(60);
        Dictionary<long, MsmqReceiveContext> messageExpiryMap;
        Dictionary<Guid, List<MsmqReceiveContext>> transMessages;
        MsmqQueue queue;
        TransactionCompletedEventHandler transactionCompletedHandler;

        bool disposed;

        object internalStateLock = new object();

        public MsmqReceiveContextLockManager(MsmqReceiveContextSettings receiveContextSettings, MsmqQueue queue)
        {
            Fx.Assert(queue is ILockingQueue, "Queue must be ILockingQueue");

            this.disposed = false;
            this.queue = queue;
            this.receiveContextSettings = receiveContextSettings;
            this.messageExpiryMap = new Dictionary<long, MsmqReceiveContext>();
            this.transMessages = new Dictionary<Guid, List<MsmqReceiveContext>>();
            transactionCompletedHandler = new TransactionCompletedEventHandler(OnTransactionCompleted);

            this.messageExpiryTimer = new IOThreadTimer(new Action<object>(CleanupExpiredLocks), null, false);
            this.messageExpiryTimer.Set(messageTimeoutInterval);
        }

        public MsmqQueue Queue
        {
            get
            {
                return queue;
            }
        }

        public MsmqReceiveContext CreateMsmqReceiveContext(long lookupId)
        {
            DateTime expiryTime = TimeoutHelper.Add(DateTime.UtcNow, receiveContextSettings.ValidityDuration);

            MsmqReceiveContext receiveContext = new MsmqReceiveContext(lookupId, expiryTime, this);
            receiveContext.Faulted += new EventHandler(OnReceiveContextFaulted);
            lock (this.internalStateLock)
            {
                this.messageExpiryMap.Add(lookupId, receiveContext);
            }
            return receiveContext;
        }

        // tx aborts can ---- with DeleteMessage but this ---- is harmless because 
        //  - internal state changes are protected via the internalStateLock
        //  - we do not have an ordering requirement between DeleteMessage and a tx abort
        //
        // tx commits cannot ---- with DeleteMessage as the ReceiveContext state machine does not allow
        // DeleteMessage calls if the tx holding this lock committed 
        public void DeleteMessage(MsmqReceiveContext receiveContext, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            long lookupId = receiveContext.LookupId;
            lock (this.internalStateLock)
            {
                // Expiry map is first checked before calling ReceiveContextExists as we need to throw
                // validity expired exception if the lookup id is not in the map.
                if (this.messageExpiryMap.ContainsKey(lookupId))
                {
                    Fx.Assert(ReceiveContextExists(receiveContext), "Mismatch between the receive context object stored in the map and the object passed to the method");
                    MsmqReceiveContext entry = this.messageExpiryMap[lookupId];
                    if (DateTime.UtcNow > entry.ExpiryTime)
                    {
                        entry.MarkContextExpired();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MessageValidityExpired, lookupId)));
                    }
                    else
                    {
                        ((ILockingQueue)this.queue).DeleteMessage(lookupId, helper.RemainingTime());

                        if (Transaction.Current != null)
                        {
                            List<MsmqReceiveContext> transMsgs;
                            if (!this.transMessages.TryGetValue(Transaction.Current.TransactionInformation.DistributedIdentifier, out transMsgs))
                            {
                                transMsgs = new List<MsmqReceiveContext>();
                                this.transMessages.Add(Transaction.Current.TransactionInformation.DistributedIdentifier, transMsgs);
                                // only need to attach the tx complete handler once per transaction
                                Transaction.Current.TransactionCompleted += this.transactionCompletedHandler;
                            }
                            transMsgs.Add(entry);
                        }
                        else
                        {
                            this.messageExpiryMap.Remove(lookupId);
                        }
                    }
                }
                else
                {
                    // it was cleaned up by the expiry timer
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MessageValidityExpired, lookupId)));
                }
            }
        }

        // tx aborts can ---- with UnlockMessage but this ---- is harmless because 
        //  - internal state changes are protected via the internalStateLock
        //  - we do not have an ordering requirement between UnlockMessage and a tx abort
        //
        // tx commits cannot ---- with UnlockMessage as the ReceiveContext state machine does not allow
        // UnlockMessage calls if the tx holding this lock committed 
        public void UnlockMessage(MsmqReceiveContext receiveContext, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            long lookupId = receiveContext.LookupId;
            lock (this.internalStateLock)
            {
                if (ReceiveContextExists(receiveContext))
                {
                    ((ILockingQueue)this.queue).UnlockMessage(lookupId, helper.RemainingTime());
                    this.messageExpiryMap.Remove(lookupId);
                }
            }
        }

        bool ReceiveContextExists(MsmqReceiveContext receiveContext)
        {
            Fx.Assert((receiveContext != null), "Receive context object cannot be null");

            MsmqReceiveContext receiveContextFromMap = null;
            if (messageExpiryMap.TryGetValue(receiveContext.LookupId, out receiveContextFromMap))
            {
                if (object.ReferenceEquals(receiveContext, receiveContextFromMap))
                {
                    return true;
                }
            }

            return false;
        }

        void OnTransactionCompleted(object sender, TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= this.transactionCompletedHandler;
            lock (this.internalStateLock)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                {
                    List<MsmqReceiveContext> toRemove;
                    if (this.transMessages.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out toRemove))
                    {
                        foreach (MsmqReceiveContext entry in toRemove)
                        {
                            this.messageExpiryMap.Remove(entry.LookupId);
                        }
                    }
                }
                // on abort the messages stay locked, we just remove the transaction info from our collection
                this.transMessages.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
            }
        }

        void CleanupExpiredLocks(object state)
        {
            lock (this.internalStateLock)
            {
                if (this.disposed)
                {
                    return;
                }

                if ((this.messageExpiryMap.Count < 1))
                {
                    this.messageExpiryTimer.Set(this.messageTimeoutInterval);
                    return;
                }

                List<MsmqReceiveContext> expiredLockList = new List<MsmqReceiveContext>();
                try
                {
                    foreach (KeyValuePair<long, MsmqReceiveContext> msgEntry in this.messageExpiryMap)
                    {
                        if (DateTime.UtcNow > msgEntry.Value.ExpiryTime)
                        {
                            expiredLockList.Add(msgEntry.Value);
                        }
                    }
                    try
                    {
                        foreach (MsmqReceiveContext entry in expiredLockList)
                        {
                            entry.MarkContextExpired();
                        }
                    }
                    catch (MsmqException ex)
                    {
                        MsmqDiagnostics.ExpectedException(ex);
                    }
                }
                finally
                {
                    this.messageExpiryTimer.Set(this.messageTimeoutInterval);
                }
            }
        }

        void OnReceiveContextFaulted(object sender, EventArgs e)
        {
            try
            {
                MsmqReceiveContext receiveContext = (MsmqReceiveContext)sender;
                UnlockMessage(receiveContext, TimeSpan.Zero);
            }
            catch (MsmqException ex)
            {
                // ReceiveContext is already faulted and best effort was made to cleanup the lock queue.
                MsmqDiagnostics.ExpectedException(ex);
            }
        }

        public void Dispose()
        {
            lock (this.internalStateLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.messageExpiryTimer.Cancel();
                    this.messageExpiryTimer = null;
                }
            }
        }
    }
}
