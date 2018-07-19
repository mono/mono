//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.ServiceModel;

    [ServiceContract(
        Name = ProtocolStrings.ContractNames.DiscoveryAdhocContractName,
        Namespace = ProtocolStrings.VersionApril2005.Namespace,
        CallbackContract = typeof(IDiscoveryResponseContractApril2005))]
    interface IDiscoveryContractAdhocApril2005 : IDiscoveryContractApril2005
    {
    }
}
