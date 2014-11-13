//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;

    struct OperationIdentifier : IEquatable<OperationIdentifier>
    {
        public string ContractName;
        public string ContractNamespace;
        public string OperationName;

        public OperationIdentifier(string contractName, string contractNamespace, string operationName)
        {
            this.ContractName = contractName;
            this.ContractNamespace = contractNamespace;
            this.OperationName = operationName;
        }

        public bool Equals(OperationIdentifier other)
        {
            return this.ContractName == other.ContractName
                && this.ContractNamespace == other.ContractNamespace
                && this.OperationName == other.OperationName;
        }
    }
}
