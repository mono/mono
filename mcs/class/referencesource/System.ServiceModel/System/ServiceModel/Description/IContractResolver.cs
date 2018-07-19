//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;

    interface IContractResolver
    {
        ContractDescription ResolveContract(string contractName);
    }

}
