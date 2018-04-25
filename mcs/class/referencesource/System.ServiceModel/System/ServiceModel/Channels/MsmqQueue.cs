//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    enum MsmqTransactionMode
    {
        None,               // do not use a transaction
        Single,             // create a single message transaction
        CurrentOrSingle,    // use Transaction.Current if set, Single otherwise
        CurrentOrNone,      // use Transaction.Current if set, None otherwise
        CurrentOrThrow,     // use Transaction.Current if set, throw otherwise
    }

    class MsmqQueue : IDisposable
    {
        MsmqQueueHandle handle;
        bool isBoundToCompletionPort;
        bool isAsyncEnabled;

        protected int shareMode;
        protected string formatName;
        protected int accessMode;

        public MsmqQueue(string formatName, int accessMode)
        {
            this.formatName = formatName;
            this.accessMode = accessMode;
            this.shareMode = UnsafeNativeMethods.MQ_DENY_NONE;
        }

        public MsmqQueue(string formatName, int accessMode, int shareMode)
        {
            this.formatName = formatName;
            this.accessMode = accessMode;
            this.shareMode = shareMode;
        }

        protected object ThisLock
        {
            get { return this; }
        }

        public string FormatName
        {
            get { return this.formatName; }
        }

        public override string ToString()
        {
            return this.formatName;
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                CloseQueue();
            }
        }
        
        internal void EnsureOpen()
        {
            GetHandle();
        }

        MsmqQueueHandle GetHandleForAsync(out bool useCompletionPort)
        {
            lock (this.ThisLock)
            {
                if (this.handle == null)
                {
                    this.handle = OpenQueue();
                }
                if (!this.isAsyncEnabled)
                {
                    if (IsCompletionPortSupported(this.handle))
                    {
                        ThreadPool.BindHandle(this.handle);
                        this.isBoundToCompletionPort = true;
                    }
                    this.isAsyncEnabled = true;
                }
                useCompletionPort = this.isBoundToCompletionPort;
                return this.handle;
            }
        }

        protected MsmqQueueHandle GetHandle()
        {
            lock (this.ThisLock)
            {
                if (this.handle == null)
                {
                    this.handle = OpenQueue();
                }
                return this.handle;
            }
        }

        static bool IsCompletionPortSupported(MsmqQueueHandle handle)
        {
            // if it's a kernel handle, then it supports completion ports
            int flags;
#pragma warning suppress 56523
            return UnsafeNativeMethods.GetHandleInformation(handle, out flags) != 0;
        }

        [ResourceConsumption(ResourceScope.Machine)]
        internal virtual MsmqQueueHandle OpenQueue()
        {
            MsmqQueueHandle handle;
            int error = UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode,
                                                        this.shareMode, out handle);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(handle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqOpenError, MsmqError.GetErrorString(error)), error));
            }

            MsmqDiagnostics.QueueOpened(this.formatName);
            return handle;
        }

        public virtual void CloseQueue()
        {
            if (this.handle != null)
            {
                CloseQueue(this.handle);
                this.handle = null;
                this.isBoundToCompletionPort = false;
                this.isAsyncEnabled = false;
                MsmqDiagnostics.QueueClosed(this.formatName);
            }
        }

        void CloseQueue(MsmqQueueHandle handle)
        {
            handle.Dispose();
        }

        protected void HandleIsStale(MsmqQueueHandle handle)
        {
            lock (this.ThisLock)
            {
                if (this.handle == handle)
                {
                    CloseQueue();
                }
            }
        }

        public static void GetMsmqInformation(ref Version version, ref bool activeDirectoryEnabled)
        {
            PrivateComputerProperties properties = new PrivateComputerProperties();
            using (properties)
            {
                IntPtr nativePropertiesPointer = properties.Pin();
                try
                {
                    int error = UnsafeNativeMethods.MQGetPrivateComputerInformation(null,
                                                                                    nativePropertiesPointer);
                    if (error != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(
                                                                                                        SR.MsmqGetPrivateComputerInformationError, MsmqError.GetErrorString(error)), error));
                    }
                    int packedVersion = properties.Version.Value;
                    version = new Version(
                        packedVersion >> 24,
                        (packedVersion & 0x00FF0000) >> 16,
                        packedVersion & 0xFFFF);
                    activeDirectoryEnabled = properties.ActiveDirectory.Value;
                }
                finally
                {
                    properties.Unpin();
                }
            }
        }

        public static bool IsReadable(string formatName, out MsmqException ex)
        {
            return SupportsAccessMode(formatName, UnsafeNativeMethods.MQ_RECEIVE_ACCESS, out ex);
        }

        public static bool IsWriteable(string formatName)
        {
            MsmqException ex;
            return SupportsAccessMode(formatName, UnsafeNativeMethods.MQ_SEND_ACCESS, out ex);
        }

        public static bool IsMoveable(string formatName)
        {
            MsmqException ex;
            return SupportsAccessMode(formatName, UnsafeNativeMethods.MQ_MOVE_ACCESS, out ex);
        }

        internal static bool IsQueueOpenable(string formatName, int accessMode, int shareMode, out int error)
        {
            MsmqQueueHandle handle;
            error = UnsafeNativeMethods.MQOpenQueue(formatName, accessMode,
                                                        shareMode, out handle);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(handle);
                return false;
            }

            handle.Dispose();
            return true;
        }

        static bool SupportsAccessMode(string formatName, int accessType, out MsmqException msmqException)
        {
            msmqException = null;

            try
            {
                using (MsmqQueue msmqQueue = new MsmqQueue(formatName, accessType))
                {
                    msmqQueue.GetHandle();
                }
            }
            catch (Exception ex)
            {
                msmqException = ex as MsmqException;
                if (null != msmqException)
                {
                    return false;
                }
                throw;
            }
            return true;
        }

        public static bool TryGetIsTransactional(string formatName, out bool isTransactional)
        {
            using (QueueTransactionProperties properties = new QueueTransactionProperties())
            {
                IntPtr nativePropertiesPointer = properties.Pin();
                try
                {
                    if (UnsafeNativeMethods.MQGetQueueProperties(formatName,
                                                                 nativePropertiesPointer) == 0)
                    {
                        isTransactional = properties.Transaction.Value != UnsafeNativeMethods.MQ_TRANSACTIONAL_NONE;
                        return true;
                    }
                    else
                    {
                        isTransactional = false;
                        MsmqDiagnostics.QueueTransactionalStatusUnknown(formatName);
                        return false;
                    }
                }
                finally
                {
                    properties.Unpin();
                }
            }
        }

        protected static bool IsErrorDueToStaleHandle(int error)
        {
            switch (error)
            {
                case UnsafeNativeMethods.MQ_ERROR_STALE_HANDLE:
                case UnsafeNativeMethods.MQ_ERROR_INVALID_HANDLE:
                case UnsafeNativeMethods.MQ_ERROR_INVALID_PARAMETER:
                case UnsafeNativeMethods.MQ_ERROR_QUEUE_DELETED:
                    return true;
                default:
                    return false;
            }
        }

        protected static bool IsReceiveErrorDueToInsufficientBuffer(int error)
        {
            switch (error)
            {
                case UnsafeNativeMethods.MQ_ERROR_BUFFER_OVERFLOW:
                case UnsafeNativeMethods.MQ_INFORMATION_FORMATNAME_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_FORMATNAME_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_SENDERID_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_USER_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_SENDER_CERT_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_RESULT_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_LABEL_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_SYMM_KEY_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_SIGNATURE_BUFFER_TOO_SMALL:
                case UnsafeNativeMethods.MQ_ERROR_PROV_NAME_BUFFER_TOO_SMALL:
                    return true;
                default:
                    return false;
            }
        }

        public void MarkMessageRejected(long lookupId)
        {
            MsmqQueueHandle handle = GetHandle();
            int error = 0;
            try
            {
                error = UnsafeNativeMethods.MQMarkMessageRejected(handle, lookupId);
            }
            catch (ObjectDisposedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
            if (error != 0)
            {
                if (IsErrorDueToStaleHandle(error))
                {
                    HandleIsStale(handle);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqSendError, MsmqError.GetErrorString(error)), error));
            }
        }

        int TryMoveMessageDtcTransacted(long lookupId, MsmqQueueHandle sourceQueueHandle, MsmqQueueHandle destinationQueueHandle, MsmqTransactionMode transactionMode)
        {
            IDtcTransaction dtcTransaction = GetNativeTransaction(transactionMode);
            if (dtcTransaction != null)
            {
                try
                {
                    return UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, destinationQueueHandle,
                                                             lookupId, dtcTransaction);
                }
                finally
                {
                    Marshal.ReleaseComObject(dtcTransaction);
                }
            }
            else
            {
                return UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, destinationQueueHandle,
                                                         lookupId, (IntPtr)GetTransactionConstant(transactionMode));
            }
        }

        public MoveReceiveResult TryMoveMessage(long lookupId, MsmqQueue destinationQueue, MsmqTransactionMode transactionMode)
        {
            MsmqQueueHandle sourceQueueHandle = GetHandle();
            MsmqQueueHandle destinationQueueHandle = destinationQueue.GetHandle();
            int error;
            try
            {
                if (RequiresDtcTransaction(transactionMode))
                {
                    error = TryMoveMessageDtcTransacted(lookupId, sourceQueueHandle, destinationQueueHandle, transactionMode);
                }
                else
                {
                    error = UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, destinationQueueHandle, 
                                                              lookupId, (IntPtr)GetTransactionConstant(transactionMode));
                }
            }
            catch (ObjectDisposedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
                return MoveReceiveResult.Succeeded;
            }
            if (error != 0)
            {
                if (error == UnsafeNativeMethods.MQ_ERROR_MESSAGE_NOT_FOUND)
                    return MoveReceiveResult.MessageNotFound;
                else if (error == UnsafeNativeMethods.MQ_ERROR_MESSAGE_LOCKED_UNDER_TRANSACTION)
                    return MoveReceiveResult.MessageLockedUnderTransaction;

                else if (IsErrorDueToStaleHandle(error))
                {
                    HandleIsStale(sourceQueueHandle);
                    destinationQueue.HandleIsStale(destinationQueueHandle);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqSendError, 
                                                                                                         MsmqError.GetErrorString(error)), error));
            }

            return MoveReceiveResult.Succeeded;
        }

        public virtual ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            return TryReceiveInternal(message, timeout, transactionMode, UnsafeNativeMethods.MQ_ACTION_RECEIVE);
        }
        
        ReceiveResult TryReceiveInternal(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            MsmqQueueHandle handle = GetHandle();

            while (true)
            {
                int error = ReceiveCore(handle, message, timeoutHelper.RemainingTime(), transactionMode, action);

                if (0 == error)
                    return ReceiveResult.MessageReceived;

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

        public MoveReceiveResult TryReceiveByLookupId(long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            return this.TryReceiveByLookupId(lookupId, message, transactionMode, UnsafeNativeMethods.MQ_LOOKUP_RECEIVE_CURRENT);
        }

        public MoveReceiveResult TryReceiveByLookupId(long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            MsmqQueueHandle handle = GetHandle();
            int error = 0;

            while (true)
            {
                try
                {
                    error = ReceiveByLookupIdCore(handle, lookupId, message, transactionMode, action);
                }
                catch (ObjectDisposedException ex)
                {
                    // ---- with Close
                    MsmqDiagnostics.ExpectedException(ex);
                    return MoveReceiveResult.Succeeded;
                }

                if (0 == error)
                {
                    return MoveReceiveResult.Succeeded;
                }
                
                if (IsReceiveErrorDueToInsufficientBuffer(error))
                {
                    message.GrowBuffers();
                    continue;
                }
                else if (UnsafeNativeMethods.MQ_ERROR_MESSAGE_NOT_FOUND == error)
                {
                    return MoveReceiveResult.MessageNotFound;
                }
                else if (UnsafeNativeMethods.MQ_ERROR_MESSAGE_LOCKED_UNDER_TRANSACTION == error)
                {
                    return MoveReceiveResult.MessageLockedUnderTransaction;
                }
                else if (IsErrorDueToStaleHandle(error))
                {
                    HandleIsStale(handle);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqReceiveError, MsmqError.GetErrorString(error)), error));
            }
        }
    
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        protected unsafe int ReceiveByLookupIdCoreDtcTransacted(MsmqQueueHandle handle, long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            IDtcTransaction dtcTransaction = GetNativeTransaction(transactionMode);

            IntPtr nativePropertiesPointer = message.Pin();
            try
            {
                if (dtcTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, nativePropertiesPointer, null, IntPtr.Zero, dtcTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(dtcTransaction);
                    }
                }
                else
                {
                    return UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, nativePropertiesPointer, null, IntPtr.Zero, (IntPtr)GetTransactionConstant(transactionMode));
                }
            }
            finally
            {
                message.Unpin();
            }

        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int ReceiveByLookupIdCore(MsmqQueueHandle handle, long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            if (RequiresDtcTransaction(transactionMode))
            {
                return ReceiveByLookupIdCoreDtcTransacted(handle, lookupId, message, transactionMode, action);
            }
            else
            {
                IntPtr nativePropertiesPointer = message.Pin();
                try
                {
                    return UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, nativePropertiesPointer, null, IntPtr.Zero, (IntPtr)GetTransactionConstant(transactionMode));
                }
                finally
                {
                    message.Unpin();
                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int ReceiveCoreDtcTransacted(MsmqQueueHandle handle, NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            IDtcTransaction dtcTransaction = GetNativeTransaction(transactionMode);
            int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeout);

            IntPtr nativePropertiesPointer = message.Pin();
            try
            {
                if (dtcTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), timeoutInMilliseconds,
                                                                    action, nativePropertiesPointer, null, IntPtr.Zero, IntPtr.Zero, dtcTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(dtcTransaction);
                    }
                }
                else
                {
                    return UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), timeoutInMilliseconds,
                                                                action, nativePropertiesPointer, null, IntPtr.Zero, IntPtr.Zero, (IntPtr)GetTransactionConstant(transactionMode));
                }
            }
            finally
            {
                message.Unpin();
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int ReceiveCore(MsmqQueueHandle handle, NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            if (RequiresDtcTransaction(transactionMode))
            {
                return ReceiveCoreDtcTransacted(handle, message, timeout, transactionMode, action);
            }
            else
            {
                int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeout);
                IntPtr nativePropertiesPointer = message.Pin();
                try
                {
                    return UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), timeoutInMilliseconds,
                                                                action, nativePropertiesPointer, null, IntPtr.Zero, IntPtr.Zero, (IntPtr)GetTransactionConstant(transactionMode));
                }
                finally
                {
                    message.Unpin();
                }
            }
        }

        protected IDtcTransaction GetNativeTransaction(MsmqTransactionMode transactionMode)
        {
            Transaction transaction = Transaction.Current;
            if (transaction != null)
            {
                return TransactionInterop.GetDtcTransaction(transaction);
            }
            if (transactionMode == MsmqTransactionMode.CurrentOrThrow)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqTransactionRequired)));
            }
            return null;
        }

        public ReceiveResult TryPeek(NativeMsmqMessage message, TimeSpan timeout)
        {
            return TryReceiveInternal(message, timeout, MsmqTransactionMode.None, 
                              UnsafeNativeMethods.MQ_ACTION_PEEK_CURRENT);
        }

        bool RequiresDtcTransaction(MsmqTransactionMode transactionMode)
        {
            switch (transactionMode)
            {
                case MsmqTransactionMode.None:
                case MsmqTransactionMode.Single:
                    return false;
                case MsmqTransactionMode.CurrentOrSingle:
                case MsmqTransactionMode.CurrentOrNone:
                case MsmqTransactionMode.CurrentOrThrow:
                    return true;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("transactionMode"));
            }
        }

        int GetTransactionConstant(MsmqTransactionMode transactionMode)
        {
            switch (transactionMode)
            {
                case MsmqTransactionMode.CurrentOrNone:
                case MsmqTransactionMode.None:
                    return UnsafeNativeMethods.MQ_NO_TRANSACTION;
                case MsmqTransactionMode.Single:
                case MsmqTransactionMode.CurrentOrSingle:
                    return UnsafeNativeMethods.MQ_SINGLE_MESSAGE;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("transactionMode"));
            }
        }

        int SendDtcTransacted(NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            IDtcTransaction dtcTransaction = GetNativeTransaction(transactionMode);

            MsmqQueueHandle handle = GetHandle();
            IntPtr nativePropertiesPointer = message.Pin();
            try
            {
                if (dtcTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQSendMessage(handle, nativePropertiesPointer,
                                                                 dtcTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(dtcTransaction);
                    }
                }
                else
                {
                    return UnsafeNativeMethods.MQSendMessage(handle, nativePropertiesPointer,
                                                             (IntPtr)GetTransactionConstant(transactionMode));
                }
            }
            finally
            {
                message.Unpin();
            }
        }

        public void Send(NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            int error = 0;
            if (RequiresDtcTransaction(transactionMode))
            {
                error = SendDtcTransacted(message, transactionMode);
            }
            else
            {
                MsmqQueueHandle handle = GetHandle();
                IntPtr nativePropertiesPointer = message.Pin();
                try
                {
                    error = UnsafeNativeMethods.MQSendMessage(handle, nativePropertiesPointer,
                                                              (IntPtr)GetTransactionConstant(transactionMode));
                }
                finally
                {
                    message.Unpin();
                }
            }

            if (error != 0)
            {
                if (IsErrorDueToStaleHandle(error))
                {
                    HandleIsStale(handle);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqSendError, MsmqError.GetErrorString(error)), error));
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int ReceiveCoreAsync(MsmqQueueHandle handle, IntPtr nativePropertiesPointer, TimeSpan timeout,
                                    int action, NativeOverlapped* nativeOverlapped,
                                    UnsafeNativeMethods.MQReceiveCallback receiveCallback)
        {
            int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeout);

            return UnsafeNativeMethods.MQReceiveMessage(handle, timeoutInMilliseconds, action,
                                                        nativePropertiesPointer, nativeOverlapped, receiveCallback,
                                                        IntPtr.Zero, (IntPtr)UnsafeNativeMethods.MQ_NO_TRANSACTION);
        }

        public IAsyncResult BeginTryReceive(NativeMsmqMessage message, TimeSpan timeout, 
                                            AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, message, timeout, 
                                             UnsafeNativeMethods.MQ_ACTION_RECEIVE, callback, state);
        }

        public ReceiveResult EndTryReceive(IAsyncResult result)
        {
            return TryReceiveAsyncResult.End(result);
        }

        public IAsyncResult BeginPeek(NativeMsmqMessage message, TimeSpan timeout, 
                                      AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, message, timeout, 
                                             UnsafeNativeMethods.MQ_ACTION_PEEK_CURRENT, callback, state);
        }

        public ReceiveResult EndPeek(IAsyncResult result)
        {
            return TryReceiveAsyncResult.End(result);
        }

        class TryReceiveAsyncResult : AsyncResult
        {
            MsmqQueue msmqQueue;
            int action;
            TimeoutHelper timeoutHelper;
            NativeMsmqMessage message;
            unsafe NativeOverlapped* nativeOverlapped = null;
            MsmqQueueHandle handle;
            ReceiveResult receiveResult;
            unsafe static IOCompletionCallback onPortedCompletion = Fx.ThunkCallback(new IOCompletionCallback(OnPortedCompletion));
            unsafe static UnsafeNativeMethods.MQReceiveCallback onNonPortedCompletion;

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            public TryReceiveAsyncResult(MsmqQueue msmqQueue, NativeMsmqMessage message, TimeSpan timeout,
                                         int action, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.msmqQueue = msmqQueue;
                this.message = message;
                this.action = action;
                this.timeoutHelper = new TimeoutHelper(timeout);
                StartReceive(true);
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe ~TryReceiveAsyncResult()
            {
                if (null != this.nativeOverlapped
                    && ! Environment.HasShutdownStarted 
                    && ! AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    Overlapped.Free(this.nativeOverlapped);
                }
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe void StartReceive(bool synchronously)
            {
                bool useCompletionPort;
                try
                {
                    this.handle = this.msmqQueue.GetHandleForAsync(out useCompletionPort);
                }
                catch (MsmqException ex)
                {
                    OnCompletion(ex.ErrorCode, synchronously);
                    return;
                }
                if (null != nativeOverlapped)
                {
                    Fx.Assert("---- in StartReceive");
                }

                IntPtr nativePropertiesPointer = this.message.Pin();
                nativeOverlapped = new Overlapped(0, 0, IntPtr.Zero, this).UnsafePack(onPortedCompletion, this.message.GetBuffersForAsync());

                int error;
                try
                {
                    if (useCompletionPort)
                    {
                        error = msmqQueue.ReceiveCoreAsync(this.handle, nativePropertiesPointer, this.timeoutHelper.RemainingTime(), 
                                                           this.action, nativeOverlapped, null);
                    }
                    else
                    {
                        if (onNonPortedCompletion == null)
                        {
                            onNonPortedCompletion = new UnsafeNativeMethods.MQReceiveCallback(OnNonPortedCompletion);
                        }
                        error = msmqQueue.ReceiveCoreAsync(this.handle, nativePropertiesPointer, this.timeoutHelper.RemainingTime(), 
                                                           this.action, nativeOverlapped, onNonPortedCompletion);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    // if Close ----s with the async Receive, it is possible that SafeHandle will throw ObjectDisposedException
                    // the behavior should be same as if operation was just cancelled (the channel will return no message)
                    MsmqDiagnostics.ExpectedException(ex);
                    error = UnsafeNativeMethods.MQ_ERROR_OPERATION_CANCELLED;
                }
                if (error != 0)
                {
                    if (error != UnsafeNativeMethods.MQ_INFORMATION_OPERATION_PENDING)
                    {
                        Overlapped.Free(nativeOverlapped);
                        nativeOverlapped = null;
                        GC.SuppressFinalize(this);
                        OnCompletion(error, synchronously);
                    }
                }
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe static void OnNonPortedCompletion(int error, IntPtr handle, int timeout,
                                                     int action, IntPtr props, NativeOverlapped* nativeOverlapped, IntPtr cursor)
            {
                ThreadPool.UnsafeQueueNativeOverlapped(nativeOverlapped);
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe static void OnPortedCompletion(uint error, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
                TryReceiveAsyncResult result = (TryReceiveAsyncResult)overlapped.AsyncResult;
                if (error != 0)
                {
                    error = (uint)UnsafeNativeMethods.MQGetOverlappedResult(nativeOverlapped);
                }
                Overlapped.Free(nativeOverlapped);
                result.nativeOverlapped = null;
#pragma warning suppress 56508 // Suppression justified. Presharp warning concerns different scenario.
                GC.SuppressFinalize(result);
                result.OnCompletion((int)error, false);
            }

            void OnCompletion(int error, bool completedSynchronously)
            {
                Exception completionException = null;

                this.receiveResult = ReceiveResult.MessageReceived;

                try
                {
                    if (error != 0)
                    {
                        if (error == UnsafeNativeMethods.MQ_ERROR_IO_TIMEOUT)
                        {
                            this.receiveResult = ReceiveResult.Timeout;
                        }
                        else if (error == UnsafeNativeMethods.MQ_ERROR_OPERATION_CANCELLED)
                        {
                            this.receiveResult = ReceiveResult.OperationCancelled;
                        }
                        else
                        {
                            if (IsReceiveErrorDueToInsufficientBuffer(error))
                            {
                                this.message.Unpin();
                                message.GrowBuffers();
                                StartReceive(completedSynchronously);
                                return;
                            }
                            else if (IsErrorDueToStaleHandle(error))
                            {
                                this.msmqQueue.HandleIsStale(this.handle);
                            }

                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(SR.GetString(SR.MsmqReceiveError, MsmqError.GetErrorString(error)), error));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is NullReferenceException || e is SEHException)
                        throw;
                    completionException = e;
                }

                this.message.Unpin();
                Complete(completedSynchronously, completionException);
            }

            public static ReceiveResult End(IAsyncResult result)
            {
                TryReceiveAsyncResult thisPtr = AsyncResult.End<TryReceiveAsyncResult>(result);
                return thisPtr.receiveResult;
            }
        }

        class QueueTransactionProperties : NativeMsmqMessage
        {
            ByteProperty transaction;

            public QueueTransactionProperties() : base(1)
            {
                this.transaction = new ByteProperty(this, UnsafeNativeMethods.PROPID_Q_TRANSACTION);
            }

            public ByteProperty Transaction
            {
                get { return this.transaction; }
            }
        }

        class PrivateComputerProperties : NativeMsmqMessage
        {
            IntProperty version;
            BooleanProperty activeDirectory;

            public PrivateComputerProperties()
                : base(2)
            {
                this.version = new IntProperty(this, UnsafeNativeMethods.PROPID_PC_VERSION);
                this.activeDirectory = new BooleanProperty(this, UnsafeNativeMethods.PROPID_PC_DS_ENABLED);
            }

            public IntProperty Version
            {
                get { return this.version; }
            }

            public BooleanProperty ActiveDirectory
            {
                get { return this.activeDirectory; }
            }
        }

        public enum MoveReceiveResult
        {
            Unknown,
            Succeeded,
            MessageNotFound,
            MessageLockedUnderTransaction
        }

        internal enum ReceiveResult
        {
            Unknown,
            MessageReceived,
            Timeout,
            OperationCancelled
        }
    }

    static class MsmqFormatName
    {
        const string systemMessagingLabelPrefix = "LABEL:";
        const string systemMessagingFormatNamePrefix = "FORMATNAME:";

        public static string ToSystemMessagingQueueName(string formatName)
        {
            return systemMessagingFormatNamePrefix + formatName;
        }

        public static string FromQueuePath(string queuePath)
        {
            int len = 256;
            StringBuilder buffer = new StringBuilder(len);
            int error = UnsafeNativeMethods.MQPathNameToFormatName(queuePath, buffer, ref len);
            if (UnsafeNativeMethods.MQ_ERROR_FORMATNAME_BUFFER_TOO_SMALL == error)
            {
                buffer = new StringBuilder(len);
                error = UnsafeNativeMethods.MQPathNameToFormatName(queuePath, buffer, ref len);
            }

            if (0 != error)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MsmqException(SR.GetString(SR.MsmqPathLookupError, queuePath, MsmqError.GetErrorString(error)), error));
            }
            
            return buffer.ToString();
        }
    }
}

