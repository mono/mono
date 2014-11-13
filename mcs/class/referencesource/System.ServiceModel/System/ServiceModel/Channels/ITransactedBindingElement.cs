//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    public interface ITransactedBindingElement
    {
        bool TransactedReceiveEnabled { get; }
    }
}
