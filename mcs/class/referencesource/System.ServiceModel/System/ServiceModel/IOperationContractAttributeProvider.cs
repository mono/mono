//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Security.Principal;
    using System.Reflection;
    using System.Transactions;
    using System.ServiceModel.Security;
    using System.Net.Security;

    interface IOperationContractAttributeProvider
    {
        OperationContractAttribute GetOperationContractAttribute();
    }
}
