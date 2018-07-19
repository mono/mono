//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    interface IDiscoveryVersionImplementation
    {
        string WsaNamespace { get; }
        Uri DiscoveryAddress { get; }        
        MessageVersion MessageVersion { get; }
        DiscoveryVersion.SchemaQualifiedNames QualifiedNames { get; }
        DataContractSerializer EprSerializer { get; }

        ContractDescription GetDiscoveryContract(ServiceDiscoveryMode discoveryMode);
        ContractDescription GetAnnouncementContract();

        IDiscoveryInnerClient CreateDiscoveryInnerClient(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver);
        IAnnouncementInnerClient CreateAnnouncementInnerClient(AnnouncementEndpoint announcementEndpoint);

        Uri ToVersionIndependentScopeMatchBy(Uri versionDependentScopeMatchBy);
        Uri ToVersionDependentScopeMatchBy(Uri versionIndependentScopeMatchBy);
    }
}
