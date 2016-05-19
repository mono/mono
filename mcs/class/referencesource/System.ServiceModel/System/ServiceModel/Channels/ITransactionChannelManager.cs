//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Transactions;
    using System.ServiceModel.Transactions;
    using System.ServiceModel.Security;

    internal interface ITransactionChannelManager
    {
        TransactionProtocol TransactionProtocol { get; set; }
        TransactionFlowOption FlowIssuedTokens { get; set; }
        IDictionary<DirectionalAction, TransactionFlowOption> Dictionary { get; }
        TransactionFlowOption GetTransaction(MessageDirection direction, string action);
        SecurityStandardsManager StandardsManager { get; }
    }
}
