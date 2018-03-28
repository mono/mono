//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Transactions;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.EnterpriseServices;
    using SR = System.ServiceModel.SR;
    using System.Globalization;


    class ComPlusThreadInitializer : ICallContextInitializer
    {
        ServiceInfo info;
        ComPlusAuthorization comAuth;
        Guid iid;

        public ComPlusThreadInitializer(ContractDescription contract,
                                        DispatchOperation operation,
                                        ServiceInfo info)
        {
            this.info = info;
            iid = contract.ContractType.GUID;

            if (info.CheckRoles)
            {
                string[] serviceRoleMembers = null;
                string[] contractRoleMembers = null;
                string[] operationRoleMembers = null;

                // Figure out the role members we want...
                //
                serviceRoleMembers = info.ComponentRoleMembers;
                foreach (ContractInfo contractInfo in this.info.Contracts)
                {
                    if (contractInfo.IID == iid)
                    {
                        contractRoleMembers = contractInfo.InterfaceRoleMembers;
                        foreach (OperationInfo opInfo in contractInfo.Operations)
                        {
                            if (opInfo.Name == operation.Name)
                            {
                                operationRoleMembers = opInfo.MethodRoleMembers;
                                break;
                            }
                        }

                        if (operationRoleMembers == null)
                        {
                            // Did not find the operation
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                                SR.GetString(SR.ComOperationNotFound,
                                             contract.Name,
                                             operation.Name)));
                        }
                        break;
                    }
                }

                this.comAuth = new ComPlusAuthorization(serviceRoleMembers,
                                                        contractRoleMembers,
                                                        operationRoleMembers);
            }
        }

        public object BeforeInvoke(
            InstanceContext instanceContext,
            IClientChannel channel,
            Message message)
        {
            ComPlusServerSecurity serverSecurity = null;
            WindowsImpersonationContext impersonateContext = null;
            bool errorTraced = false;
            WindowsIdentity identity = null;
            Uri from = null;
            object instance = null;
            int instanceID = 0;
            string action = null;
            TransactionProxy proxy = null;
            Transaction tx = null;
            Guid incomingTransactionID = Guid.Empty;

            // The outer try block is to comply with FXCOP's WrapVulnerableFinallyClausesInOuterTry rule.
            try
            {
                try
                {

                    identity = MessageUtil.GetMessageIdentity(message);

                    if (message.Headers.From != null)
                        from = message.Headers.From.Uri;

                    instance = instanceContext.GetServiceInstance(message);
                    instanceID = instance.GetHashCode();
                    action = message.Headers.Action;



                    ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInvokingMethod,
                        SR.TraceCodeComIntegrationInvokingMethod, this.info, from, action, identity.Name, iid, instanceID, false);

                    // Security
                    //

                    if (this.info.CheckRoles)
                    {
                        if (!this.comAuth.IsAuthorizedForOperation(identity))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.CallAccessDenied());
                        }
                    }

                    if (this.info.HostingMode != HostingMode.WebHostOutOfProcess)
                    {
                        // NOTE: This has the side effect of setting up
                        //       the COM server security thing, so be sure
                        //       to clear it with Dispose() eventually.
                        //
                        serverSecurity = new ComPlusServerSecurity(identity,
                                                                   this.info.CheckRoles);
                    }

                    // Transactions
                    //
                    proxy = instanceContext.Extensions.Find<TransactionProxy>();
                    if (proxy != null)
                    {
                        // This makes the Tx header Understood.
                        tx = MessageUtil.GetMessageTransaction(message);
                        if (tx != null)
                        {
                            incomingTransactionID = tx.TransactionInformation.DistributedIdentifier;
                        }
                        try
                        {
                            if (tx != null)
                            {
                                proxy.SetTransaction(tx);
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.TransactionMismatch());
                            }
                            ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInvokingMethodNewTransaction,
                            SR.TraceCodeComIntegrationInvokingMethodNewTransaction, this.info, from, action, identity.Name, iid, instanceID, incomingTransactionID);
                        }
                        catch (FaultException e)
                        {
                            Transaction txProxy = proxy.CurrentTransaction;
                            Guid currentTransactionID = Guid.Empty;
                            if (txProxy != null)
                                currentTransactionID = txProxy.TransactionInformation.DistributedIdentifier;

                            string identityName = String.Empty;

                            if (null != identity)
                                identityName = identity.Name;

                            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                                (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                                (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusInvokingMethodFailedMismatchedTransactions,
                                incomingTransactionID.ToString("B").ToUpperInvariant(),
                                currentTransactionID.ToString("B").ToUpperInvariant(),
                                from.ToString(),
                                this.info.AppID.ToString("B").ToUpperInvariant(),
                                this.info.Clsid.ToString("B").ToUpperInvariant(),
                                iid.ToString(),
                                action,
                                instanceID.ToString(CultureInfo.InvariantCulture),
                                System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture),
                                SafeNativeMethods.GetCurrentThreadId().ToString(CultureInfo.InvariantCulture),
                                identityName,
                                e.ToString());
                            errorTraced = true;
                            throw;
                        }
                    }
                    else
                    {
                        ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInvokingMethodContextTransaction,
                        SR.TraceCodeComIntegrationInvokingMethodContextTransaction, this.info, from, action, identity.Name, iid, instanceID, true);
                    }

                    // Impersonation
                    //
                    if (this.info.HostingMode == HostingMode.WebHostOutOfProcess)
                    {
                        impersonateContext = identity.Impersonate();
                    }

                    CorrelationState correlationState;
                    correlationState = new CorrelationState(impersonateContext,
                                                            serverSecurity,
                                                            from,
                                                            action,
                                                            identity.Name,
                                                            instanceID);

                    impersonateContext = null;
                    serverSecurity = null;


                    return correlationState;
                }
                finally
                {
                    if (impersonateContext != null)
                        impersonateContext.Undo();

                    if (serverSecurity != null)
                        ((IDisposable)serverSecurity).Dispose();
                }

            }
            catch (Exception e)
            {
                if (errorTraced == false)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                           (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                           (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusInvokingMethodFailed,
                           from == null ? string.Empty : from.ToString(),
                           this.info.AppID.ToString("B").ToUpperInvariant(),
                           this.info.Clsid.ToString("B").ToUpperInvariant(),
                           iid.ToString("B").ToUpperInvariant(),
                           action,
                           instanceID.ToString(CultureInfo.InvariantCulture),
                           System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture),
                           SafeNativeMethods.GetCurrentThreadId().ToString(CultureInfo.InvariantCulture),
                           identity.Name,
                           e.ToString());
                    }
                }
                throw;
            }

        }

        public void AfterInvoke(object correlationState)
        {
            CorrelationState state = (CorrelationState)correlationState;
            if (state != null)
            {
                ComPlusMethodCallTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInvokedMethod,
                    SR.TraceCodeComIntegrationInvokedMethod, this.info, state.From, state.Action, state.CallerIdentity, iid, state.InstanceID, false);
                state.Cleanup();

            }
        }

        class CorrelationState
        {
            WindowsImpersonationContext impersonationContext;
            ComPlusServerSecurity serverSecurity;
            Uri from;
            string action;
            string callerIdentity;
            int instanceID;

            public CorrelationState(WindowsImpersonationContext context,
                                    ComPlusServerSecurity serverSecurity,
                                    Uri from,
                                    string action,
                                    string callerIdentity,
                                    int instanceID)
            {
                this.impersonationContext = context;
                this.serverSecurity = serverSecurity;
                this.from = from;
                this.action = action;
                this.callerIdentity = callerIdentity;
                this.instanceID = instanceID;
            }
            public Uri From
            {
                get
                {
                    return this.from;
                }
            }

            public string Action
            {
                get
                {
                    return this.action;
                }
            }
            public string CallerIdentity
            {
                get
                {
                    return this.callerIdentity;
                }
            }

            public int InstanceID
            {
                get
                {
                    return this.instanceID;
                }
            }

            public void Cleanup()
            {
                if (this.impersonationContext != null)
                    this.impersonationContext.Undo();

                if (this.serverSecurity != null)
                    ((IDisposable)this.serverSecurity).Dispose();
            }
        }
    }
}
