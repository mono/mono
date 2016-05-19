//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel;
    using System.Transactions;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Runtime.InteropServices;
    using SR = System.ServiceModel.SR;

    class TransactionProxyBuilder : IProxyCreator
    {
        ComProxy comProxy = null;
        TransactionProxy txProxy = null;

        private TransactionProxyBuilder(TransactionProxy proxy)
        {
            this.txProxy = proxy;
        }
        void IDisposable.Dispose()
        {

        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if ((riid != typeof(ITransactionProxy).GUID))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(SR.GetString(SR.NoInterface, riid)));
            if (outer == IntPtr.Zero)
            {
                // transactions require failfasts to prevent corruption
                DiagnosticUtility.FailFast("OuterProxy cannot be null");
            }

            if (comProxy == null)
            {
                comProxy = ComProxy.Create(outer, txProxy, null);
                return comProxy;

            }
            else
                return comProxy.Clone();
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if ((riid != typeof(ITransactionProxy).GUID))
                return false;
            else
                return true;

        }

        bool IProxyCreator.SupportsDispatch()
        {
            return false;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return false;
        }

        public static IntPtr CreateTransactionProxyTearOff(TransactionProxy txProxy)
        {
            IProxyCreator txProxyBuilder = new TransactionProxyBuilder(txProxy);
            IProxyManager proxyManager = new ProxyManager(txProxyBuilder);
            Guid iid = typeof(ITransactionProxy).GUID;
            return OuterProxyWrapper.CreateOuterProxyInstance(proxyManager, ref iid);
        }

    }

    class TransactionProxy : ITransactionProxy,
                             IExtension<InstanceContext>
    {
        Transaction currentTransaction;
        VoterBallot currentVoter;
        object syncRoot;
        Guid appid;
        Guid clsid;
        int instanceID = 0;

        public TransactionProxy(Guid appid, Guid clsid)
        {
            this.syncRoot = new object();
            this.appid = appid;
            this.clsid = clsid;
        }

        public Transaction CurrentTransaction
        {
            get
            {
                return this.currentTransaction;
            }
        }
        public Guid AppId
        {
            get
            {
                return this.appid;
            }
        }
        public Guid Clsid
        {
            get
            {
                return this.clsid;
            }
        }
        public int InstanceID
        {
            get
            {
                return this.instanceID;
            }
            set
            {
                this.instanceID = value;
            }
        }
        public void SetTransaction(Transaction transaction)
        {
            lock (this.syncRoot)
            {
                if (transaction == null)
                {
                    // transactions require failfasts to prevent corruption
                    DiagnosticUtility.FailFast("Attempting to set transaction to NULL");
                }

                if (this.currentTransaction == null)
                {
                    ProxyEnlistment enlistment;
                    enlistment = new ProxyEnlistment(this, transaction);
                    transaction.EnlistVolatile(enlistment, EnlistmentOptions.None);
                    this.currentTransaction = transaction;
                    if (this.currentVoter != null)
                    {
                        this.currentVoter.SetTransaction(this.currentTransaction);
                    }
                }
                else if (this.currentTransaction != transaction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.TransactionMismatch());
                }
            }
        }

        // IExtension<ServiceInstance>
        public void Attach(InstanceContext owner) { }
        public void Detach(InstanceContext owner) { }

        // ITransactionProxy
        public void Commit(Guid guid)
        {
            // transactions require failfasts to prevent corruption
            DiagnosticUtility.FailFast("Commit not supported: BYOT only!");
        }

        public void Abort()
        {
            if (this.currentTransaction != null)
            {
                this.currentTransaction.Rollback();
            }
        }

        public IDtcTransaction Promote()
        {
            EnsureTransaction();
            return TransactionInterop.GetDtcTransaction(
                this.currentTransaction);
        }

        public void CreateVoter(
            ITransactionVoterNotifyAsync2 voterNotification,
            IntPtr voterBallot)
        {
            if (IntPtr.Zero == voterBallot)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("voterBallot");

            lock (this.syncRoot)
            {
                if (this.currentVoter != null)
                {
                    // transactions require failfasts to prevent corruption
                    DiagnosticUtility.FailFast("Assumption: proxy only needs one voter");
                }

                VoterBallot voter = new VoterBallot(voterNotification, this);
                if (this.currentTransaction != null)
                {
                    voter.SetTransaction(this.currentTransaction);
                }

                this.currentVoter = voter;

                IntPtr ppv = InterfaceHelper.GetInterfacePtrForObject(typeof(ITransactionVoterBallotAsync2).GUID, this.currentVoter);

                Marshal.WriteIntPtr(voterBallot, ppv);
            }
        }

        public DtcIsolationLevel GetIsolationLevel()
        {
            DtcIsolationLevel retVal;
            switch (this.currentTransaction.IsolationLevel)
            {
                case IsolationLevel.Serializable:
                    retVal = DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
                    break;
                case IsolationLevel.RepeatableRead:
                    retVal = DtcIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD;
                    break;
                case IsolationLevel.ReadCommitted:
                    retVal = DtcIsolationLevel.ISOLATIONLEVEL_READCOMMITTED;
                    break;
                case IsolationLevel.ReadUncommitted:
                    retVal = DtcIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED;
                    break;
                default:
                    retVal = DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
                    break;
            }
            return retVal;
        }

        public Guid GetIdentifier()
        {
            return this.currentTransaction.TransactionInformation.DistributedIdentifier;
        }

        // ITransactionProxy2
        public bool IsReusable()
        {
            return true;
        }

        void ClearTransaction(ProxyEnlistment enlistment)
        {
            lock (this.syncRoot)
            {
                if (this.currentTransaction == null)
                {
                    // transactions require failfasts to prevent corruption
                    DiagnosticUtility.FailFast("Clearing inactive TransactionProxy");
                }
                if (enlistment.Transaction != this.currentTransaction)
                {
                    // transactions require failfasts to prevent corruption
                    DiagnosticUtility.FailFast("Incorrectly working on multiple transactions");
                }
                this.currentTransaction = null;
                this.currentVoter = null;
            }
        }

        void EnsureTransaction()
        {
            lock (this.syncRoot)
            {
                if (this.currentTransaction == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(null, HR.CONTEXT_E_NOTRANSACTION));

            }
        }

        class ProxyEnlistment : IEnlistmentNotification
        {
            TransactionProxy proxy;
            Transaction transaction;

            public ProxyEnlistment(TransactionProxy proxy,
                                   Transaction transaction)
            {
                this.proxy = proxy;
                this.transaction = transaction;
            }

            public Transaction Transaction
            {
                get { return this.transaction; }
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.proxy.ClearTransaction(this);
                this.proxy = null;
                preparingEnlistment.Done();

            }

            public void Commit(Enlistment enlistment)
            {
                // transactions require failfasts to prevent corruption
                DiagnosticUtility.FailFast("Should have voted read only");
            }

            public void Rollback(Enlistment enlistment)
            {
                this.proxy.ClearTransaction(this);
                this.proxy = null;
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                // transactions require failfasts to prevent corruption
                DiagnosticUtility.FailFast("Should have voted read only");
            }
        }

        class VoterBallot : ITransactionVoterBallotAsync2, IEnlistmentNotification
        {
            const int S_OK = 0;

            ITransactionVoterNotifyAsync2 notification;
            Transaction transaction;
            Enlistment enlistment;
            PreparingEnlistment preparingEnlistment;
            TransactionProxy proxy;

            public VoterBallot(ITransactionVoterNotifyAsync2 notification, TransactionProxy proxy)
            {
                this.notification = notification;
                this.proxy = proxy;
            }

            public void SetTransaction(Transaction transaction)
            {
                if (this.transaction != null)
                {
                    // transactions require failfasts to prevent corruption
                    DiagnosticUtility.FailFast("Already have a transaction in the ballot!");
                }
                this.transaction = transaction;
                this.enlistment = transaction.EnlistVolatile(
                    this,
                    EnlistmentOptions.None);
            }


            public void Prepare(PreparingEnlistment enlistment)
            {
                this.preparingEnlistment = enlistment;
                this.notification.VoteRequest();
            }

            public void Rollback(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.Aborted(0, false, 0, S_OK);
                ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTxProxyTxAbortedByTM,
                        SR.TraceCodeComIntegrationTxProxyTxAbortedByTM, proxy.AppId, proxy.Clsid, transaction.TransactionInformation.DistributedIdentifier, proxy.InstanceID);
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.Committed(false, 0, S_OK);
                ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTxProxyTxCommitted,
                            SR.TraceCodeComIntegrationTxProxyTxCommitted, proxy.AppId, proxy.Clsid, transaction.TransactionInformation.DistributedIdentifier, proxy.InstanceID);
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
                this.notification.InDoubt();
                Marshal.ReleaseComObject(this.notification);
                this.notification = null;
            }

            public void VoteRequestDone(int hr, int reason)
            {
                if (this.preparingEnlistment == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.NoVoteIssued)));
                }

                if (S_OK == hr)
                {
                    this.preparingEnlistment.Prepared();
                }
                else
                {

                    this.preparingEnlistment.ForceRollback();
                    ComPlusTxProxyTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationTxProxyTxAbortedByContext,
                            SR.TraceCodeComIntegrationTxProxyTxAbortedByContext, proxy.AppId, proxy.Clsid, transaction.TransactionInformation.DistributedIdentifier, proxy.InstanceID);
                }
            }
        }
    }
}
