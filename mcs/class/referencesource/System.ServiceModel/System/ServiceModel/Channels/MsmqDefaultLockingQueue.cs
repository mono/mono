//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions;

    class MsmqDefaultLockingQueue : MsmqQueue, ILockingQueue
    {
        Dictionary<long, TransactionLookupEntry> lockMap;
        Dictionary<Guid, List<long>> dtcTransMap;

        object internalStateLock;

        TransactionCompletedEventHandler transactionCompletedHandler;
        object receiveLock = new object();

        public MsmqDefaultLockingQueue(string formatName, int accessMode)
            : base(formatName, accessMode)
        {
            lockMap = new Dictionary<long, TransactionLookupEntry>();
            dtcTransMap = new Dictionary<Guid, List<long>>();
            this.internalStateLock = new object();
            transactionCompletedHandler = new TransactionCompletedEventHandler(Current_TransactionCompleted);
        }

        public override ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            // ignore the transactionMode
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            MsmqQueueHandle handle = GetHandle();

            while (true)
            {
                int error = PeekLockCore(handle, (MsmqInputMessage)message, timeoutHelper.RemainingTime());

                if (error == 0)
                {
                    return ReceiveResult.MessageReceived;
                }

                if (IsReceiveErrorDueToInsufficientBuffer(error))
                {
                    message.GrowBuffers();
                    continue;
                }
                else if (error == UnsafeNativeMethods.MQ_ERROR_IO_TIMEOUT)
                {
                    return ReceiveResult.Timeout;
                }
                else if (error == UnsafeNativeMethods.MQ_ERROR_OPERATION_CANCELLED)
                {
                    return ReceiveResult.OperationCancelled;
                }
                else if (error == UnsafeNativeMethods.MQ_ERROR_INVALID_HANDLE)
                {
                    // should happen only if racing with Close
                    return ReceiveResult.OperationCancelled;
                }
                else if (IsErrorDueToStaleHandle(error))
                {
                    HandleIsStale(handle);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqReceiveError, MsmqError.GetErrorString(error)), error));
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int PeekLockCore(MsmqQueueHandle handle, MsmqInputMessage message, TimeSpan timeout)
        {
            int retCode = 0;
            ITransaction internalTrans;

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            IntPtr nativePropertiesPointer = message.Pin();
            try
            {
                bool receivedMessage = false;

                while (!receivedMessage)
                {
                    retCode = UnsafeNativeMethods.MQBeginTransaction(out internalTrans);
                    if (retCode != 0)
                    {
                        return retCode;
                    }

                    int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime());

                    // no timeout interval if timeout has been set to 0 otherwise a minimum of 100
                    int timeoutIntervalInMilliseconds = (timeoutInMilliseconds == 0) ? 0 : 100;

                    // receive until timeout but let go of receive lock for other contenders periodically
                    while (true)
                    {
                        lock (this.receiveLock)
                        {
                            retCode = UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), timeoutIntervalInMilliseconds,
                                        UnsafeNativeMethods.MQ_ACTION_RECEIVE, nativePropertiesPointer, null, IntPtr.Zero, IntPtr.Zero, internalTrans);
                            if (retCode == UnsafeNativeMethods.MQ_ERROR_IO_TIMEOUT)
                            {
                                // keep trying until we timeout
                                timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeoutHelper.RemainingTime());
                                if (timeoutInMilliseconds == 0)
                                {
                                    return retCode;
                                }
                            }
                            else if (retCode != 0)
                            {
                                BOID boid = new BOID();
                                internalTrans.Abort(
                                    ref boid, // pboidReason
                                    0,  // fRetaining
                                    0   // fAsync
                                    );

                                return retCode;
                                // we don't need to release the ITransaction as MSMQ does not increment the ref counter
                                // in MQBeginTransaction
                            }
                            else
                            {
                                // we got a message within the specified time out
                                break;
                            }
                        }
                    }

                    TransactionLookupEntry entry;

                    lock (this.internalStateLock)
                    {
                        if (!this.lockMap.TryGetValue(message.LookupId.Value, out entry))
                        {
                            this.lockMap.Add(message.LookupId.Value, new TransactionLookupEntry(message.LookupId.Value, internalTrans));
                            receivedMessage = true;
                        }
                        else
                        {
                            // this was a message that was in the process of being handed off
                            // from some app trans to some internal MSMQ transaction
                            // and we grabbed it before the Abort() could finish
                            // need to be a good citizen and finish that Abort() job for it
                            entry.MsmqInternalTransaction = internalTrans;
                        }
                    }
                }
            }
            finally
            {
                message.Unpin();
            }

            return retCode;
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call code from a non-APTCA assembly.
        // MSMQ is not enabled in partial trust, so this demand should not break customers.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public void DeleteMessage(long lookupId, TimeSpan timeout)
        {
            TransactionLookupEntry entry;

            if (Transaction.Current != null && Transaction.Current.TransactionInformation.Status != System.Transactions.TransactionStatus.Active)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqAmbientTransactionInactive)));
            }

            lock (this.internalStateLock)
            {
                if (!this.lockMap.TryGetValue(lookupId, out entry))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MessageNotInLockedState, lookupId)));
                }

                // a failed relock is the same as not having a lock
                if (entry.MsmqInternalTransaction == null)
                {
                    this.lockMap.Remove(entry.LookupId);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MessageNotInLockedState, lookupId)));
                }
            }

            if (Transaction.Current == null)
            {
                entry.MsmqInternalTransaction.Commit(
                                       0, // fRetaining
                                       0, // grfTC
                                       0 // grfRM
                                       );

                lock (this.internalStateLock)
                {
                    this.lockMap.Remove(lookupId);
                }
            }
            else
            {
                // we don't want any thread receiving the message we are trying to re-receive in a new transaction
                lock (this.receiveLock)
                {
                    MsmqQueueHandle handle = GetHandle();

                    // abort internal transaction and re-receive in the ambient transaction
                    BOID boid = new BOID();
                    entry.MsmqInternalTransaction.Abort(
                        ref boid, // pboidReason
                        0,  // fRetaining
                        0   // fAsync
                        );
                    // null indicates that the associated internal tx was aborted and the message is now
                    // unlocked as far as the native queue manager is concerned
                    entry.MsmqInternalTransaction = null;

                    using (MsmqEmptyMessage emptyMessage = new MsmqEmptyMessage())
                    {
                        int error = 0;
                        try
                        {
                            error = base.ReceiveByLookupIdCoreDtcTransacted(handle, lookupId, emptyMessage,
                                MsmqTransactionMode.CurrentOrThrow, UnsafeNativeMethods.MQ_LOOKUP_RECEIVE_CURRENT);
                        }
                        catch (ObjectDisposedException ex)
                        {
                            // ---- with Close
                            MsmqDiagnostics.ExpectedException(ex);

                        }

                        if (error != 0)
                        {
                            if (IsErrorDueToStaleHandle(error))
                            {
                                HandleIsStale(handle);
                            }

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqCannotReacquireLock), error));
                        }
                    }
                }

                List<long> transMsgs;
                lock (this.internalStateLock)
                {
                    if (!this.dtcTransMap.TryGetValue(Transaction.Current.TransactionInformation.DistributedIdentifier, out transMsgs))
                    {
                        transMsgs = new List<long>();
                        this.dtcTransMap.Add(Transaction.Current.TransactionInformation.DistributedIdentifier, transMsgs);
                        // only need to attach the tx complete handler once per transaction
                        Transaction.Current.TransactionCompleted += this.transactionCompletedHandler;
                    }
                    transMsgs.Add(lookupId);
                }
            }
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call code from a non-APTCA assembly.
        // MSMQ is not enabled in partial trust, so this demand should not break customers.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public void UnlockMessage(long lookupId, TimeSpan timeout)
        {
            TransactionLookupEntry entry;

            lock (this.internalStateLock)
            {
                if (this.lockMap.TryGetValue(lookupId, out entry))
                {
                    if (entry.MsmqInternalTransaction != null)
                    {
                        BOID boid = new BOID();

                        entry.MsmqInternalTransaction.Abort(
                            ref boid, // pboidReason
                            0,  // fRetaining
                            0   // fAsync
                            );
                    }
                    this.lockMap.Remove(lookupId);
                }
            }
        }

        void Current_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= this.transactionCompletedHandler;

            if (e.Transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Aborted)
            {
                List<long> transMsgs = null;

                lock (this.internalStateLock)
                {
                    if (this.dtcTransMap.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out transMsgs))
                    {
                        // remove state about all messages locked in this dtc transaction
                        // if we fail to relock the message, the message will simply go back to the 
                        // queue and any subsequent Complete() calls for the message will throw
                        this.dtcTransMap.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
                    }
                }

                if (transMsgs != null)
                {
                    // relock all messages in this transaction
                    foreach (long msgId in transMsgs)
                    {
                        TryRelockMessage(msgId);
                        // not much we can do in case of failures
                    }
                }

            }
            else if (e.Transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Committed)
            {
                List<long> transMsgs = null;

                lock (this.internalStateLock)
                {
                    if (this.dtcTransMap.TryGetValue(e.Transaction.TransactionInformation.DistributedIdentifier, out transMsgs))
                    {
                        // remove state about all messages locked in this dtc transaction
                        this.dtcTransMap.Remove(e.Transaction.TransactionInformation.DistributedIdentifier);
                    }

                    if (transMsgs != null)
                    {
                        foreach (long msgId in transMsgs)
                        {
                            this.lockMap.Remove(msgId);
                        }
                    }
                }
            }
        }

        // attempt to relock the message within an internal transaction
        // the application already has the message so we don't need to recreate the message
        // just need to lock it under an internal transaction and register this transaction
        // in our transaction dictionary
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int TryRelockMessage(long lookupId)
        {
            int retCode = 0;
            ITransaction internalTrans;

            using (MsmqEmptyMessage message = new MsmqEmptyMessage())
            {
                IntPtr nativePropertiesPointer = message.Pin();
                try
                {
                    // don't want other threads receiving the message we want to relock
                    lock (this.receiveLock)
                    {
                        MsmqQueueHandle handle = GetHandle();

                        TransactionLookupEntry entry;

                        lock (this.internalStateLock)
                        {
                            if (!this.lockMap.TryGetValue(lookupId, out entry))
                            {
                                // should never get here
                                return retCode;
                            }

                            if (entry.MsmqInternalTransaction == null)
                            {
                                retCode = UnsafeNativeMethods.MQBeginTransaction(out internalTrans);
                                if (retCode != 0)
                                {
                                    return retCode;
                                }

                                retCode = UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, UnsafeNativeMethods.MQ_LOOKUP_RECEIVE_CURRENT,
                                            nativePropertiesPointer, null, IntPtr.Zero, internalTrans);
                                if (retCode != 0)
                                {
                                    BOID boid = new BOID();
                                    internalTrans.Abort(
                                        ref boid, // pboidReason
                                        0,  // fRetaining
                                        0   // fAsync
                                        );

                                    return retCode;
                                }
                                entry.MsmqInternalTransaction = internalTrans;
                            }
                        }
                    }
                }
                finally
                {
                    message.Unpin();
                }
            }

            return retCode;
        }

        public override void CloseQueue()
        {
            // unlock all messages
            long[] toRemove;

            lock (this.internalStateLock)
            {
                toRemove = new long[this.lockMap.Keys.Count];
                this.lockMap.Keys.CopyTo(toRemove, 0);
            }

            foreach (long lookupId in toRemove)
            {
                this.UnlockMessage(lookupId, TimeSpan.Zero);
            }

            base.CloseQueue();
        }

        class TransactionLookupEntry
        {
            public long LookupId;
            public ITransaction MsmqInternalTransaction;

            public TransactionLookupEntry(long lookupId, ITransaction transaction)
            {
                this.LookupId = lookupId;
                this.MsmqInternalTransaction = transaction;
            }
        }
    }
}

