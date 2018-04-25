//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.Collections.Generic;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    class MsmqSubqueueLockingQueue : MsmqQueue, ILockingQueue
    {
        string lockQueueName;

        MsmqQueue mainQueueForMove;
        MsmqQueue lockQueueForMove;
        MsmqQueue lockQueueForReceive;

        IOThreadTimer lockCollectionTimer;
        TimeSpan lockCollectionInterval = TimeSpan.FromMinutes(5);
        object timerLock = new object();
        bool disposed;
        string hostname;
        bool validHostName;

        private const string LockSubqueuePrefix = "lock_";

        public MsmqSubqueueLockingQueue(string formatName, string hostname, int accessMode)
            : base(formatName, accessMode)
        {
            // The hostname will be empty for MsmqIntegrationBinding 
            if (string.Compare(hostname, string.Empty, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.validHostName = MsmqSubqueueLockingQueue.TryGetHostName(formatName, out hostname);
            }
            else
            {
                this.validHostName = true;
            }

            this.disposed = false;
            this.lockQueueName = this.formatName + ";" + MsmqSubqueueLockingQueue.GenerateLockQueueName();
            this.lockQueueForReceive = new MsmqQueue(this.lockQueueName, UnsafeNativeMethods.MQ_RECEIVE_ACCESS, UnsafeNativeMethods.MQ_DENY_RECEIVE_SHARE);
            this.lockQueueForMove = new MsmqQueue(this.lockQueueName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
            this.mainQueueForMove = new MsmqQueue(this.formatName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
            this.lockCollectionTimer = new IOThreadTimer(new Action<object>(OnCollectionTimer), null, false);

            if (string.Compare(hostname, "localhost", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.hostname = null;
            }
            else
            {
                this.hostname = hostname;
            }
        }

        private static string GenerateLockQueueName()
        {
            string lockGuid = Guid.NewGuid().ToString();
            return MsmqSubqueueLockingQueue.LockSubqueuePrefix + lockGuid.Substring(lockGuid.Length - 8, 8);
        }

        public MsmqQueue LockQueueForReceive
        {
            get
            {
                return this.lockQueueForReceive;
            }
        }

        internal override MsmqQueueHandle OpenQueue()
        {
            if (!this.validHostName)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqOpenError,
                    MsmqError.GetErrorString(UnsafeNativeMethods.MQ_ERROR_UNSUPPORTED_FORMATNAME_OPERATION)),
                    UnsafeNativeMethods.MQ_ERROR_UNSUPPORTED_FORMATNAME_OPERATION));
            }

            this.EnsureLockQueuesOpen();
            this.mainQueueForMove.EnsureOpen();
            // first time collection
            this.OnCollectionTimer(null);
            return base.OpenQueue();
        }

        internal void EnsureLockQueuesOpen()
        {
            int attempts = 0;

            // handle lock queue name collisions, if we fail three times in a row it is probably not the name 
            // collision that is causing the open to fail
            while (true)
            {
                try
                {
                    this.lockQueueForReceive.EnsureOpen();
                    break;
                }
                catch (MsmqException ex)
                {
                    if (attempts >= 3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);
                    }
                    MsmqDiagnostics.ExpectedException(ex);
                }

                this.lockQueueForReceive.Dispose();
                this.lockQueueForMove.Dispose();

                this.lockQueueName = this.formatName + ";" + MsmqSubqueueLockingQueue.GenerateLockQueueName();
                this.lockQueueForReceive = new MsmqQueue(this.lockQueueName, UnsafeNativeMethods.MQ_RECEIVE_ACCESS, UnsafeNativeMethods.MQ_DENY_RECEIVE_SHARE);
                this.lockQueueForMove = new MsmqQueue(this.lockQueueName, UnsafeNativeMethods.MQ_MOVE_ACCESS);
                attempts++;
            }
            this.lockQueueForMove.EnsureOpen();
        }

        public override ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            // we ignore transaction mode for receive context receives
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            bool receivedMessage = false;
            long lookupId = 0;

            // peek for new message, move it to the lock queue and then receive the full message
            // if move fails because another thread moved it ahead of us then peek again
            while (!receivedMessage)
            {
                ReceiveResult result;
                MoveReceiveResult moveResult;

                using (MsmqMessageLookupId emptyMessage = new MsmqMessageLookupId())
                {
                    result = this.TryPeek(emptyMessage, timeoutHelper.RemainingTime());
                    if (result != ReceiveResult.MessageReceived)
                    {
                        return result;
                    }
                    lookupId = emptyMessage.lookupId.Value;
                }

                try
                {
                    moveResult = this.TryMoveMessage(lookupId, this.lockQueueForMove, MsmqTransactionMode.None);
                    if (moveResult == MoveReceiveResult.Succeeded)
                    {
                        receivedMessage = true;
                    }
                }
                catch (MsmqException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                }
            }

            MoveReceiveResult lookupIdReceiveResult;
            try
            {
                lookupIdReceiveResult = this.lockQueueForReceive.TryReceiveByLookupId(lookupId, message, MsmqTransactionMode.None, UnsafeNativeMethods.MQ_LOOKUP_PEEK_CURRENT);
            }
            catch (MsmqException ex)
            {
                this.UnlockMessage(lookupId, TimeSpan.Zero);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);
            }

            if (lookupIdReceiveResult == MoveReceiveResult.Succeeded)
            {
                return ReceiveResult.MessageReceived;
            }
            else
            {
                this.UnlockMessage(lookupId, TimeSpan.Zero);
                return ReceiveResult.OperationCancelled;
            }
        }

        public void DeleteMessage(long lookupId, TimeSpan timeout)
        {
            // operations on the lock subqueue need to be protected from ---- with close
            MoveReceiveResult receiveResult;
            IPostRollbackErrorStrategy postRollBack = new SimplePostRollbackErrorStrategy(lookupId);
            do
            {
                using (MsmqEmptyMessage emptyMessage = new MsmqEmptyMessage())
                {
                    receiveResult = this.lockQueueForReceive.TryReceiveByLookupId(lookupId, emptyMessage, MsmqTransactionMode.CurrentOrNone);
                }

                if (receiveResult != MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction)
                    break;

                // We could have failed because of ---- with transaction.abort() for the transaction 
                // that had this message locked previously. We will retry in these cases.
            } while (postRollBack.AnotherTryNeeded());

            // We could have failed because of
            //  a) failure in the underlying queue manager
            //  b) expiration of the native message timer
            //  c) ---- with Channel.Close()
            // ..not much we can do in any of these cases
            if (receiveResult != MoveReceiveResult.Succeeded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqReceiveContextMessageNotReceived, lookupId.ToString(CultureInfo.InvariantCulture))));
            }
        }

        public void UnlockMessage(long lookupId, TimeSpan timeout)
        {
            MoveReceiveResult moveResult;
            IPostRollbackErrorStrategy postRollBack = new SimplePostRollbackErrorStrategy(lookupId);
            do
            {
                moveResult = this.lockQueueForReceive.TryMoveMessage(lookupId, this.mainQueueForMove, MsmqTransactionMode.None);
                if (moveResult != MsmqQueue.MoveReceiveResult.MessageLockedUnderTransaction)
                    break;

                // We could have failed because of ---- with transaction.abort() for the transaction 
                // that had this message locked previously. We will retry in these cases.
            } while (postRollBack.AnotherTryNeeded());

            if (moveResult != MoveReceiveResult.Succeeded)
            {
                // We could have failed because of
                //  a) failure in the underlying queue manager
                //  b) expiration of the native message timer
                //  c) ---- with Channel.Close()
                // ..not much we can do in any of these cases

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqReceiveContextMessageNotMoved, lookupId.ToString(CultureInfo.InvariantCulture))));
            }
        }

        public override void CloseQueue()
        {
            lock (this.timerLock)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.lockCollectionTimer.Cancel();
                    this.lockCollectionTimer = null;
                }
            }
            this.CollectLocks(this.lockQueueForReceive);
            this.mainQueueForMove.CloseQueue();
            this.lockQueueForMove.CloseQueue();
            this.lockQueueForReceive.CloseQueue();
            base.CloseQueue();
        }

        private void OnCollectionTimer(object state)
        {
            lock (this.timerLock)
            {
                if (this.disposed)
                {
                    return;
                }

                List<string> subqueues;
                if (TryEnumerateSubqueues(out subqueues))
                {
                    foreach (string subqueueName in subqueues)
                    {
                        if (subqueueName.StartsWith(MsmqSubqueueLockingQueue.LockSubqueuePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            MsmqQueue collectQueue;
                            if (TryOpenLockQueueForCollection(subqueueName, out collectQueue))
                            {
                                this.CollectLocks(collectQueue);
                            }
                        }
                    }
                }
                this.lockCollectionTimer.Set(this.lockCollectionInterval);
            }
        }

        private bool TryOpenLockQueueForCollection(string subqueueName, out MsmqQueue lockQueue)
        {
            lockQueue = null;
            string formatName = this.formatName + ";" + subqueueName;
            int accessMode = UnsafeNativeMethods.MQ_RECEIVE_ACCESS;
            int shareMode = UnsafeNativeMethods.MQ_DENY_RECEIVE_SHARE;

            try
            {
                int error = 0;
                if (MsmqQueue.IsQueueOpenable(formatName, accessMode, shareMode, out error))
                {
                    lockQueue = new MsmqQueue(formatName, accessMode, shareMode);
                    lockQueue.EnsureOpen();
                }
                else
                {
                    // The lock subqueue is either being actively used by a channel or is not available.
                    // So, we do not have to collect this lock queue.
                    if (error == UnsafeNativeMethods.MQ_ERROR_SHARING_VIOLATION ||
                        error == UnsafeNativeMethods.MQ_ERROR_QUEUE_NOT_FOUND)
                    {
                        return false;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqOpenError, MsmqError.GetErrorString(error)), error));
                    }
                }
            }
            catch (MsmqException)
            {
                // The error has already been logged. Since this function is to decide whether to collect
                // the lock queue, we return false.
                return false;
            }

            return true;
        }

        private void CollectLocks(MsmqQueue lockQueue)
        {
            ReceiveResult result = ReceiveResult.MessageReceived;

            while (result == ReceiveResult.MessageReceived)
            {
                using (MsmqMessageLookupId message = new MsmqMessageLookupId())
                {
                    try
                    {
                        result = lockQueue.TryPeek(message, TimeSpan.FromSeconds(0));
                        if (result == ReceiveResult.MessageReceived)
                        {
                            lockQueue.TryMoveMessage(message.lookupId.Value, this.mainQueueForMove, MsmqTransactionMode.None);
                        }
                    }
                    catch (MsmqException ex)
                    {
                        // we will retry the collection in the next cleanup round
                        MsmqDiagnostics.ExpectedException(ex);
                        result = ReceiveResult.Unknown;
                    }
                }
            }
        }

        private bool TryEnumerateSubqueues(out List<String> subqueues)
        {
            subqueues = new List<string>();
            int[] ids = new int[1];
            UnsafeNativeMethods.MQMSGPROPS props = new UnsafeNativeMethods.MQMSGPROPS();
            UnsafeNativeMethods.MQPROPVARIANT prop = new UnsafeNativeMethods.MQPROPVARIANT();
            UnsafeNativeMethods.MQPROPVARIANT retProp;

            GCHandle propsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            GCHandle nativePropertyIdsHandle = GCHandle.Alloc(null, GCHandleType.Pinned);
            GCHandle propHandle = GCHandle.Alloc(null, GCHandleType.Pinned);

            props.status = IntPtr.Zero;
            props.count = 1;
            ids[0] = UnsafeNativeMethods.PROPID_MGMT_QUEUE_SUBQUEUE_NAMES;
            prop.vt = UnsafeNativeMethods.VT_NULL;

            try
            {
                // pin
                propsHandle.Target = props;
                nativePropertyIdsHandle.Target = ids;
                propHandle.Target = prop;

                props.variants = propHandle.AddrOfPinnedObject();
                props.ids = nativePropertyIdsHandle.AddrOfPinnedObject();

                if (UnsafeNativeMethods.MQMgmtGetInfo(this.hostname, "queue=" + this.formatName, propsHandle.AddrOfPinnedObject()) == 0)
                {
                    retProp = (UnsafeNativeMethods.MQPROPVARIANT)Marshal.PtrToStructure(props.variants, typeof(UnsafeNativeMethods.MQPROPVARIANT));

                    IntPtr[] stringArrays = new IntPtr[retProp.stringArraysValue.count];
                    Marshal.Copy(retProp.stringArraysValue.stringArrays, stringArrays, 0, retProp.stringArraysValue.count);

                    for (int i = 0; i < retProp.stringArraysValue.count; i++)
                    {
                        subqueues.Add(Marshal.PtrToStringUni(stringArrays[i]));
                        UnsafeNativeMethods.MQFreeMemory(stringArrays[i]);
                    }
                    UnsafeNativeMethods.MQFreeMemory(retProp.stringArraysValue.stringArrays);
                }
                else
                {
                    return false;
                }

            }
            finally
            {
                // unpin
                nativePropertyIdsHandle.Target = null;
                propsHandle.Target = null;
                propHandle.Target = null;
            }
            return true;
        }

        private class MsmqMessageLookupId : NativeMsmqMessage
        {
            public LongProperty lookupId;

            public MsmqMessageLookupId()
                : base(1)
            {
                this.lookupId = new LongProperty(this, UnsafeNativeMethods.PROPID_M_LOOKUPID);
            }
        }

        private static bool TryGetHostName(string formatName, out string hostName)
        {
            string directFormatNamePrefix = "DIRECT=";
            string tcpProtocolPrefix = "TCP:";
            string osProtocolPrefix = "OS:";
            hostName = null;

            if (formatName.StartsWith(directFormatNamePrefix, StringComparison.OrdinalIgnoreCase))
            {
                // The direct format name of the form DIRECT=OS:.\sampleq is parsed here
                string formatNameWithProtocol = formatName.Substring(directFormatNamePrefix.Length,
                    formatName.Length - directFormatNamePrefix.Length);

                int addressStartPos = formatNameWithProtocol.IndexOf(':') + 1;
                string address = formatNameWithProtocol.Substring(addressStartPos,
                    formatNameWithProtocol.IndexOf('\\') - addressStartPos);

                if (formatNameWithProtocol.StartsWith(tcpProtocolPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    // formatNameWithProtocol is TCP:<tcp-address>\<queue-type>\<queue-name>
                    hostName = address;
                    return true;
                }
                else if (formatNameWithProtocol.StartsWith(osProtocolPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (address.Equals("."))
                    {
                        // formatNameWithProtocol is OS:.\<queue-type>\<queue-name>
                        hostName = "localhost";
                    }
                    else
                    {
                        // formatNameWithProtocol is OS:<machine-name>\<queue-type>\<queue-name>
                        hostName = address;
                    }

                    return true;
                }
                else
                {
                    // Other protocols not supported. IPX is valid only on NT, w2k
                    // HTTP/HTTPS: can be used only to send messages. If support changes in future,
                    // use Dns.GetHostEntry to obtain the IP address
                    return false;
                }
            }
            else
            {
                // Other format names are not supported
                return false;
            }
        }
    }
}

