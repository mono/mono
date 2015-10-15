//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Runtime;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.Xml;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;

    abstract class WsatTransactionFormatter : TransactionFormatter
    {
        bool initialized;
        WsatConfiguration wsatConfig;
        WsatProxy wsatProxy;
        ProtocolVersion protocolVersion;

        protected WsatTransactionFormatter(ProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
        }

        //=======================================================================================
        void EnsureInitialized()
        {
            if (!this.initialized)
            {
                lock (this)
                {
                    if (!this.initialized)
                    {
                        this.wsatConfig = new WsatConfiguration();
                        this.wsatProxy = new WsatProxy(this.wsatConfig, this.protocolVersion);
                        this.initialized = true;
                    }
                }
            }
        }

        //=======================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we call code from a non-APTCA assembly; WSATs are not supported in partial trust, so customers should not be broken by this demand
        */
        public override void WriteTransaction(Transaction transaction, Message message)
        {
            EnsureInitialized();

            ForcePromotion(transaction);

            // Make a context and add it to the message
            CoordinationContext context;
            RequestSecurityTokenResponse issuedToken;
            MarshalAsCoordinationContext(transaction, out context, out issuedToken);
            if (issuedToken != null)
            {
                CoordinationServiceSecurity.AddIssuedToken(message, issuedToken);
            }

            WsatTransactionHeader header = new WsatTransactionHeader(context, this.protocolVersion);
            message.Headers.Add(header);
        }

        //=======================================================================================
        void ForcePromotion(Transaction transaction)
        {
            // Force promotion. This may throw TransactionException.
            // We used to check the DistributedIdentifier property first, but VSWhidbey 



            TransactionInterop.GetTransmitterPropagationToken(transaction);
        }

        //=======================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we use CoordinationServiceSecurity from a non-APTCA assembly and CoordinationServiceSecurity.GetIssuedToken(..) can call Environment.FailFast.
        // It's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public override TransactionInfo ReadTransaction(Message message)
        {
            EnsureInitialized();

            CoordinationContext context = WsatTransactionHeader.GetCoordinationContext(message, this.protocolVersion);
            if (context == null)
                return null;

            // Incoming transaction tokens are optional
            RequestSecurityTokenResponse issuedToken;
            try
            {
                issuedToken = CoordinationServiceSecurity.GetIssuedToken(message, context.Identifier, this.protocolVersion);
            }
            catch (XmlException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionException(SR.FailedToDeserializeIssuedToken, e));
            }

            return new WsatTransactionInfo(this.wsatProxy, context, issuedToken);
        }

        //=======================================================================================
        public WsatTransactionInfo CreateTransactionInfo(CoordinationContext context,
                                                         RequestSecurityTokenResponse issuedToken)
        {
            return new WsatTransactionInfo(this.wsatProxy, context, issuedToken);
        }

        //=======================================================================================
        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        // We demand full trust because we use CoordinationContext and CoordinationServiceSecurity from a non-APTCA assembly.
        // The CoordinationContext constructor can call Environment.FailFast and it's recommended to not let partially trusted callers to bring down the process.
        // WSATs are not supported in partial trust, so customers should not be broken by this demand.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        */
        public void MarshalAsCoordinationContext(Transaction transaction,
                                                 out CoordinationContext context,
                                                 out RequestSecurityTokenResponse issuedToken)
        {
            Guid transactionId = transaction.TransactionInformation.DistributedIdentifier;
            string nonNativeContextId = null;

            context = new CoordinationContext(this.protocolVersion);

            // Get timeout, description and isolation flags
            uint timeout;
            IsolationFlags isoFlags;
            string description;
            OleTxTransactionFormatter.GetTransactionAttributes(transaction,
                                                               out timeout,
                                                               out isoFlags,
                                                               out description);
            context.IsolationFlags = isoFlags;
            context.Description = description;

            // If we can, use cached extended information
            // Note - it may be worth using outgoing contexts more than once.
            // We'll let performance profiling decide that question
            WsatExtendedInformation info;
            if (WsatExtendedInformationCache.Find(transaction, out info))
            {
                context.Expires = info.Timeout;

                // The extended info cache only contains an identifier when it's non-native
                if (!string.IsNullOrEmpty(info.Identifier))
                {
                    context.Identifier = info.Identifier;
                    nonNativeContextId = info.Identifier;
                }
            }
            else
            {
                context.Expires = timeout;
                if (context.Expires == 0)
                {
                    // If the timeout is zero, there are two possibilities:
                    // 1) This is a root transaction with an infinite timeout.
                    // 2) This is a subordinate transaction whose timeout was not flowed.
                    // We have no mechanism for distinguishing between the two cases.
                    //
                    // We could always return zero here, instead of using the local max timeout.
                    // The problem is that the 2004/08 WS-C spec does not specify the meaning
                    // of a zero expires field. While we accept zero to mean "as large as possible"
                    // it would be risky to expect others to do the same.  So we only propagate
                    // zero in the expires field if the local max timeout has been disabled.
                    //
                    // This is MB 34596: how can we flow the real timeout?
                    context.Expires = (uint)TimeoutHelper.ToMilliseconds(this.wsatConfig.MaxTimeout);
                }
            }

            if (context.Identifier == null)
            {
                context.Identifier = CoordinationContext.CreateNativeIdentifier(transactionId);
                nonNativeContextId = null;
            }

            string tokenId;
            if (!this.wsatConfig.IssuedTokensEnabled)
            {
                tokenId = null;
                issuedToken = null;
            }
            else
            {
                CoordinationServiceSecurity.CreateIssuedToken(transactionId,
                                                              context.Identifier,
                                                              this.protocolVersion,
                                                              out issuedToken,
                                                              out tokenId);
            }

            AddressHeader refParam = new WsatRegistrationHeader(transactionId, nonNativeContextId, tokenId);
            context.RegistrationService = wsatConfig.CreateRegistrationService(refParam, this.protocolVersion);
            context.IsolationLevel = transaction.IsolationLevel;
            context.LocalTransactionId = transactionId;

            if (this.wsatConfig.OleTxUpgradeEnabled)
            {
                context.PropagationToken = TransactionInterop.GetTransmitterPropagationToken(transaction);
            }
        }
    }

    //------------------------------------------------------------------------------------------
    //                          Versioned Wsat transaction formatters
    //------------------------------------------------------------------------------------------

    class WsatTransactionFormatter10 : WsatTransactionFormatter
    {
        static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version10);

        public WsatTransactionFormatter10() : base(ProtocolVersion.Version10) { }

        //=======================================================================================
        public override MessageHeader EmptyTransactionHeader
        {
            get { return emptyTransactionHeader; }
        }
    }

    class WsatTransactionFormatter11 : WsatTransactionFormatter
    {
        static WsatTransactionHeader emptyTransactionHeader = new WsatTransactionHeader(null, ProtocolVersion.Version11);

        public WsatTransactionFormatter11() : base(ProtocolVersion.Version11) { }

        //=======================================================================================
        public override MessageHeader EmptyTransactionHeader
        {
            get { return emptyTransactionHeader; }
        }
    }
}
