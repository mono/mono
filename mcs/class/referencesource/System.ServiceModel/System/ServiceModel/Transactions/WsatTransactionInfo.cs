//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Security;
    using System.Transactions;
    using Microsoft.Transactions.Wsat.Messaging;

    class WsatTransactionInfo : TransactionInfo
    {
        WsatProxy wsatProxy;
        CoordinationContext context;
        RequestSecurityTokenResponse issuedToken;

        public WsatTransactionInfo(WsatProxy wsatProxy, 
                                   CoordinationContext context,
                                   RequestSecurityTokenResponse issuedToken)
        {
            this.wsatProxy = wsatProxy;
            this.context = context;
            this.issuedToken = issuedToken;
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls into CoordinationContext are safe.")]
        public override Transaction UnmarshalTransaction()
        {
            Transaction tx;

            if (WsatIncomingTransactionCache.Find(this.context.Identifier, out tx))
                return tx;

            tx = this.wsatProxy.UnmarshalTransaction(this);

            // Cache extended information for subsequent marshal operations
            WsatExtendedInformation info = new WsatExtendedInformation(context.Identifier, context.Expires);
            info.TryCache(tx);

            // Cache the unmarshalled transaction for subsequent unmarshal operations
            WsatIncomingTransactionCache.Cache(this.context.Identifier, tx);

            return tx;
        }

        public CoordinationContext Context
        {
            get { return this.context; }
        }

        public RequestSecurityTokenResponse IssuedToken
        {
            get { return this.issuedToken; }
        }
    }
}
