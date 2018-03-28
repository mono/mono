//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Transactions
{
    using System;
    using System.ServiceModel.Channels;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Transactions;
    using System.ServiceModel.Security;
    using System.ServiceModel.Diagnostics;

    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;

    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;
    using System.Security.Permissions;

    class WsatProxy
    {
        WsatConfiguration wsatConfig;
        ProtocolVersion protocolVersion;

        CoordinationService coordinationService;
        ActivationProxy activationProxy;
        object proxyLock = new object();

        public WsatProxy(WsatConfiguration wsatConfig, ProtocolVersion protocolVersion)
        {
            this.wsatConfig = wsatConfig;
            this.protocolVersion = protocolVersion;
        }

        //=============================================================================================
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to CoordinationContext properties are safe.")]
        public Transaction UnmarshalTransaction(WsatTransactionInfo info)
        {
            if (info.Context.ProtocolVersion != this.protocolVersion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }

            if (wsatConfig.OleTxUpgradeEnabled)
            {
                byte[] propToken = info.Context.PropagationToken;
                if (propToken != null)
                {
                    try
                    {
                        return OleTxTransactionInfo.UnmarshalPropagationToken(propToken);
                    }
                    catch (TransactionException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                    }

                    // Fall back to WS-AT unmarshal
                    if (DiagnosticUtility.ShouldTraceInformation)
                        TraceUtility.TraceEvent(TraceEventType.Information,
                                                                     TraceCode.TxFailedToNegotiateOleTx,
                                                                     SR.GetString(SR.TraceCodeTxFailedToNegotiateOleTx, info.Context.Identifier));
                }
            }

            // Optimization: if the context's registration service points to our local TM, we can
            // skip the CreateCoordinationContext step
            CoordinationContext localContext = info.Context;

            if (!this.wsatConfig.IsLocalRegistrationService(localContext.RegistrationService, this.protocolVersion))
            {
                // Our WS-AT protocol service for the context's protocol version should be enabled
                if (!this.wsatConfig.IsProtocolServiceEnabled(this.protocolVersion))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TransactionException(SR.GetString(SR.WsatProtocolServiceDisabled, this.protocolVersion)));
                }

                // We should have enabled inbound transactions
                if (!this.wsatConfig.InboundEnabled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TransactionException(SR.GetString(SR.InboundTransactionsDisabled)));
                }

                // The sender should have enabled both WS-AT and outbound transactions
                if (this.wsatConfig.IsDisabledRegistrationService(localContext.RegistrationService))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new TransactionException(SR.GetString(SR.SourceTransactionsDisabled)));
                }

                // Ask the WS-AT protocol service to unmarshal the transaction
                localContext = CreateCoordinationContext(info);
            }

            Guid transactionId = localContext.LocalTransactionId;
            if (transactionId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionException(SR.GetString(SR.InvalidCoordinationContextTransactionId)));
            }

            byte[] propagationToken = MarshalPropagationToken(ref transactionId,
                                                              localContext.IsolationLevel,
                                                              localContext.IsolationFlags,
                                                              localContext.Description);

            return OleTxTransactionInfo.UnmarshalPropagationToken(propagationToken);
        }

        //=============================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we use CreateCoordinationContext from a non-APTCA assembly and the CreateCoordinationContext constructor does an Environment.FailFast 
        // if the argument is invalid. It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        CoordinationContext CreateCoordinationContext(WsatTransactionInfo info)
        {
            CreateCoordinationContext cccMessage = new CreateCoordinationContext(this.protocolVersion);
            cccMessage.CurrentContext = info.Context;
            cccMessage.IssuedToken = info.IssuedToken;

            try
            {
                // This was necessary during some portions of WCF 1.0 development
                // It is probably not needed now. However, it seems conceptually 
                // solid to separate this operation from the incoming app message as 
                // much as possible.  There have also been enough ServiceModel bugs in 
                // this area that it does not seem wise to remove this at the moment
                // (2006/3/30, WCF 1.0 RC1 milestone)
                using (new OperationContextScope((OperationContext)null))
                {
                    return Enlist(ref cccMessage).CoordinationContext;
                }
            }
            catch (WsatFaultException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionException(SR.GetString(SR.UnmarshalTransactionFaulted, e.Message), e));
            }
            catch (WsatSendFailureException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerCommunicationException(SR.GetString(SR.TMCommunicationError), e));
            }
        }

        //=============================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we call code from a non-APTCA assembly; WSATs are not supported in partial trust, so customers should not be broken by this demand
        */
        CreateCoordinationContextResponse Enlist(ref CreateCoordinationContext cccMessage)
        {
            int attempts = 0;
            while (true)
            {
                ActivationProxy proxy = GetActivationProxy();
                EndpointAddress address = proxy.To;

                EndpointAddress localActivationService = this.wsatConfig.LocalActivationService(this.protocolVersion);
                EndpointAddress remoteActivationService = this.wsatConfig.RemoteActivationService(this.protocolVersion);

                try
                {
                    return proxy.SendCreateCoordinationContext(ref cccMessage);
                }
                catch (WsatSendFailureException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);

                    // Don't retry if we're not likely to succeed on the next pass
                    Exception inner = e.InnerException;
                    if (inner is TimeoutException ||
                        inner is QuotaExceededException ||
                        inner is FaultException)
                        throw;

                    // Give up after 10 attempts
                    if (attempts > 10)
                        throw;

                    if (attempts > 5 &&
                        remoteActivationService != null &&
                        ReferenceEquals(address, localActivationService))
                    {
                        // Switch over to the remote activation service.
                        // In clustered scenarios this uses the cluster name,
                        // so it should always work if the resource is online
                        // This covers the case where we were using a local cluster
                        // resource which failed over to another node
                        address = remoteActivationService;
                    }
                }
                finally
                {
                    proxy.Release();
                }

                TryStartMsdtcService();

                // We need to refresh our proxy here because the channel is sessionful
                // and may simply decided to enter the faulted state if something fails.
                RefreshActivationProxy(address);

                // Don't spin
                Thread.Sleep(0);
                attempts++;
            }
        }

        //=============================================================================================
        void TryStartMsdtcService()
        {
            try
            {
                TransactionInterop.GetWhereabouts();
            }
            catch (TransactionException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
        }

        //=============================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call ActivationProxy.AddRef(), which is defined in a non-APTCA assembly and can do Environment.FailFast.
        // It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        ActivationProxy GetActivationProxy()
        {
            if (this.activationProxy == null)
            {
                RefreshActivationProxy(null);
            }

            lock (this.proxyLock)
            {
                ActivationProxy proxy = this.activationProxy;
                proxy.AddRef();
                return proxy;
            }
        }

        //=============================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we call ActivationProxy.Release(), which is defined in a non-APTCA assembly and can do Environment.FailFast. 
        // It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        void RefreshActivationProxy(EndpointAddress suggestedAddress)
        {
            // Pick an address in the following order...
            EndpointAddress address = suggestedAddress;

            if (address == null)
            {
                address = this.wsatConfig.LocalActivationService(this.protocolVersion);

                if (address == null)
                {
                    address = this.wsatConfig.RemoteActivationService(this.protocolVersion);
                }
            }

            if (!(address != null))
            {
                // tx processing requires failfast when state is inconsistent
                DiagnosticUtility.FailFast("Must have valid activation service address");
            }

            lock (this.proxyLock)
            {
                ActivationProxy newProxy = CreateActivationProxy(address);
                if (this.activationProxy != null)
                    this.activationProxy.Release();
                this.activationProxy = newProxy;
            }
        }

        //=============================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we call code from a non-APTCA assembly; WSATs are not supported in partial trust, so customers should not be broken by this demand
        */
        ActivationProxy CreateActivationProxy(EndpointAddress address)
        {
            CoordinationService coordination = GetCoordinationService();
            try
            {
                return coordination.CreateActivationProxy(address, false);
            }
            catch (CreateChannelFailureException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionException(SR.GetString(SR.WsatProxyCreationFailed), e));
            }
        }

        //=============================================================================================
        //[SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "We call PartialTrustHelpers.DemandForFullTrust().")]
        CoordinationService GetCoordinationService()
        {
            if (this.coordinationService == null)
            {
                lock (this.proxyLock)
                {
                    if (this.coordinationService == null)
                    {
                        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
                        /*
                        // We demand full trust because CoordinationService is defined in a non-APTCA assembly and can call Environment.FailFast.
                        // It's recommended to not let partially trusted callers to bring down the process.
                        System.Runtime.PartialTrustHelpers.DemandForFullTrust();
                        */

                        try
                        {
                            CoordinationServiceConfiguration config = new CoordinationServiceConfiguration();
                            config.Mode = CoordinationServiceMode.Formatter;
                            config.RemoteClientsEnabled = this.wsatConfig.RemoteActivationService(this.protocolVersion) != null;
                            this.coordinationService = new CoordinationService(config, this.protocolVersion);
                        }
                        catch (MessagingInitializationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new TransactionException(SR.GetString(SR.WsatMessagingInitializationFailed), e));
                        }
                    }
                }
            }

            return this.coordinationService;
        }

        //-------------------------------------------------------------------------------
        //                          Marshal/Unmarshaling related stuff
        //-------------------------------------------------------------------------------

        // Keep a propagation token around as a template for hydrating transactions
        static byte[] fixedPropagationToken;
        static byte[] CreateFixedPropagationToken()
        {
            if (fixedPropagationToken == null)
            {
                CommittableTransaction tx = new CommittableTransaction();
                byte[] token = TransactionInterop.GetTransmitterPropagationToken(tx);

                // Don't abort the transaction. People notice this and do not like it.
                try
                {
                    tx.Commit();
                }
                catch (TransactionException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }

                Interlocked.CompareExchange<byte[]>(ref fixedPropagationToken, token, null);
            }

            byte[] tokenCopy = new byte[fixedPropagationToken.Length];
            Array.Copy(fixedPropagationToken, tokenCopy, fixedPropagationToken.Length);

            return tokenCopy;
        }

        // This is what a propagation token looks like:
        //
        // struct PropagationToken
        // {
        //     DWORD dwVersionMin;
        //     DWORD dwVersionMax;
        //     GUID guidTx;
        //     ISOLATIONLEVEL isoLevel;
        //     ISOFLAG isoFlags;
        //     ULONG cbSourceTmAddr;
        //     char szDesc[40];
        //     [etc]
        // }

        static byte[] MarshalPropagationToken(ref Guid transactionId,
                                              IsolationLevel isoLevel,
                                              IsolationFlags isoFlags,
                                              string description)
        {
            const int offsetof_guidTx = 8;
            const int offsetof_isoLevel = 24;
            const int offsetof_isoFlags = 28;
            const int offsetof_szDesc = 36;

            const int MaxDescriptionLength = 39;

            byte[] token = CreateFixedPropagationToken();

            // Replace transaction id
            byte[] transactionIdBytes = transactionId.ToByteArray();
            Array.Copy(transactionIdBytes, 0, token, offsetof_guidTx, transactionIdBytes.Length);

            // Replace isolation level
            byte[] isoLevelBytes = BitConverter.GetBytes((int)ConvertIsolationLevel(isoLevel));
            Array.Copy(isoLevelBytes, 0, token, offsetof_isoLevel, isoLevelBytes.Length);

            // Replace isolation flags
            byte[] isoFlagsBytes = BitConverter.GetBytes((int)isoFlags);
            Array.Copy(isoFlagsBytes, 0, token, offsetof_isoFlags, isoFlagsBytes.Length);

            // Replace description
            if (!string.IsNullOrEmpty(description))
            {
                byte[] descriptionBytes = Encoding.UTF8.GetBytes(description);
                int copyDescriptionBytes = Math.Min(descriptionBytes.Length, MaxDescriptionLength);

                Array.Copy(descriptionBytes, 0, token, offsetof_szDesc, copyDescriptionBytes);
                token[offsetof_szDesc + copyDescriptionBytes] = 0;
            }

            return token;
        }

        enum ProxyIsolationLevel : int
        {
            Unspecified = -1,
            Chaos = 0x10,
            ReadUncommitted = 0x100,
            Browse = 0x100,
            CursorStability = 0x1000,
            ReadCommitted = 0x1000,
            RepeatableRead = 0x10000,
            Serializable = 0x100000,
            Isolated = 0x100000
        }

        static ProxyIsolationLevel ConvertIsolationLevel(IsolationLevel IsolationLevel)
        {
            ProxyIsolationLevel retVal;
            switch (IsolationLevel)
            {
                case IsolationLevel.Serializable:
                    retVal = ProxyIsolationLevel.Serializable;
                    break;
                case IsolationLevel.RepeatableRead:
                    retVal = ProxyIsolationLevel.RepeatableRead;
                    break;
                case IsolationLevel.ReadCommitted:
                    retVal = ProxyIsolationLevel.ReadCommitted;
                    break;
                case IsolationLevel.ReadUncommitted:
                    retVal = ProxyIsolationLevel.ReadUncommitted;
                    break;
                case IsolationLevel.Unspecified:
                    retVal = ProxyIsolationLevel.Unspecified;
                    break;
                default:
                    retVal = ProxyIsolationLevel.Serializable;
                    break;
            }
            return retVal;
        }
    }
}
