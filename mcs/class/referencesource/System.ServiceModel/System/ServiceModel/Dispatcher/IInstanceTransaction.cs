//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Transactions;

    interface IInstanceTransaction
    {
        Transaction GetTransactionForInstance(OperationContext operationContext);
    }
}
