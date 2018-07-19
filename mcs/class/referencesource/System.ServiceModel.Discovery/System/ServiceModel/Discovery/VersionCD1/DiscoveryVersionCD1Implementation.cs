//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Globalization;

    class DiscoveryVersionCD1Implementation : IDiscoveryVersionImplementation
    {
        static readonly Uri ScopeMatchByExact = new Uri(ProtocolStrings.VersionCD1.ScopeMatchByExact);
        static readonly Uri ScopeMatchByLdap = new Uri(ProtocolStrings.VersionCD1.ScopeMatchByLdap);
        static readonly Uri ScopeMatchByPrefix = new Uri(ProtocolStrings.VersionCD1.ScopeMatchByPrefix);
        static readonly Uri ScopeMatchByUuid = new Uri(ProtocolStrings.VersionCD1.ScopeMatchByUuid);
        static readonly Uri ScopeMatchByNone = new Uri(ProtocolStrings.VersionCD1.ScopeMatchByNone);

        Uri discoveryAddress;
        DataContractSerializer eprSerializer;
        DiscoveryVersion.SchemaQualifiedNames qualifiedNames;

        ContractDescription adhocDiscoveryContract;
        ContractDescription managedDiscoveryContract;
        ContractDescription announcementContract;

        [Fx.Tag.SynchronizationObject()]
        object contractLock;

        public DiscoveryVersionCD1Implementation()
        {
            this.contractLock = new object();
        }

        public string WsaNamespace
        {
            get
            {
                return ProtocolStrings.WsaNamespaceAugust2004;
            }
        }

        public Uri DiscoveryAddress
        {
            get
            {
                if (this.discoveryAddress == null)
                {
                    this.discoveryAddress = new Uri(ProtocolStrings.VersionCD1.AdhocAddress);
                }
                return this.discoveryAddress;
            }
        }                

        public MessageVersion MessageVersion
        {
            get
            {                    
                return MessageVersion.Soap12WSAddressingAugust2004;
            }
        }

        public DiscoveryVersion.SchemaQualifiedNames QualifiedNames
        {
            get
            {
                if (this.qualifiedNames == null)
                {
                    this.qualifiedNames = new DiscoveryVersion.SchemaQualifiedNames(ProtocolStrings.VersionCD1.Namespace, this.WsaNamespace);
                }
                return this.qualifiedNames;
            }
        }

        public DataContractSerializer EprSerializer
        {
            get
            {
                if (this.eprSerializer == null)
                {
                    this.eprSerializer = new DataContractSerializer(typeof(EndpointAddressAugust2004));
                }
                return this.eprSerializer;
            }
        }

        public ContractDescription GetDiscoveryContract(ServiceDiscoveryMode discoveryMode)
        {
            if (discoveryMode == ServiceDiscoveryMode.Adhoc)
            {
                if (this.adhocDiscoveryContract == null)
                {
                    lock (this.contractLock)
                    {
                        if (this.adhocDiscoveryContract == null)
                        {
                            this.adhocDiscoveryContract = DiscoveryUtility.GetContract(typeof(IDiscoveryContractAdhocCD1));
                        }
                    }
                }
                return this.adhocDiscoveryContract;
            }
            else if (discoveryMode == ServiceDiscoveryMode.Managed)
            {
                if (this.managedDiscoveryContract == null)
                {
                    lock (this.contractLock)
                    {
                        if (this.managedDiscoveryContract == null)
                        {
                            this.managedDiscoveryContract = DiscoveryUtility.GetContract(typeof(IDiscoveryContractManagedCD1));
                        }
                    }
                }
                return this.managedDiscoveryContract;
            }
            else
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.DiscoveryIncorrectMode(discoveryMode)));
            }
        }

        public ContractDescription GetAnnouncementContract()
        {
            if (this.announcementContract == null)
            {
                lock (this.contractLock)
                {
                    if (this.announcementContract == null)
                    {
                        this.announcementContract = DiscoveryUtility.GetContract(typeof(IAnnouncementContractCD1));
                    }
                }
            }
            return this.announcementContract;
        }

        public IDiscoveryInnerClient CreateDiscoveryInnerClient(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
        {
            if (discoveryEndpoint.DiscoveryMode == ServiceDiscoveryMode.Adhoc)
            {
                return new DiscoveryInnerClientAdhocCD1(discoveryEndpoint, responseReceiver);
            }
            else if (discoveryEndpoint.DiscoveryMode == ServiceDiscoveryMode.Managed)
            {
                return new DiscoveryInnerClientManagedCD1(discoveryEndpoint, responseReceiver);
            }
            else
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.DiscoveryIncorrectMode(discoveryEndpoint.DiscoveryMode)));
            }
        }

        public IAnnouncementInnerClient CreateAnnouncementInnerClient(AnnouncementEndpoint announcementEndpoint)
        {
            return new AnnouncementInnerClientCD1(announcementEndpoint);
        }

        public Uri ToVersionIndependentScopeMatchBy(Uri versionDependentScopeMatchBy)
        {
            Uri scopeMatchBy = versionDependentScopeMatchBy;

            if (versionDependentScopeMatchBy == DiscoveryVersionCD1Implementation.ScopeMatchByExact)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByExact;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersionCD1Implementation.ScopeMatchByPrefix)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByPrefix;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersionCD1Implementation.ScopeMatchByLdap)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByLdap;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersionCD1Implementation.ScopeMatchByUuid)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByUuid;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersionCD1Implementation.ScopeMatchByNone)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByNone;
            }

            return scopeMatchBy;
        }

        public Uri ToVersionDependentScopeMatchBy(Uri versionIndependentScopeMatchBy)
        {
            Uri scopeMatchBy = versionIndependentScopeMatchBy;

            if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByExact)
            {
                scopeMatchBy = DiscoveryVersionCD1Implementation.ScopeMatchByExact;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByPrefix)
            {
                scopeMatchBy = DiscoveryVersionCD1Implementation.ScopeMatchByPrefix;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByLdap)
            {
                scopeMatchBy = DiscoveryVersionCD1Implementation.ScopeMatchByLdap;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByUuid)
            {
                scopeMatchBy = DiscoveryVersionCD1Implementation.ScopeMatchByUuid;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByNone)
            {
                scopeMatchBy = DiscoveryVersionCD1Implementation.ScopeMatchByNone;
            }

            return scopeMatchBy;
        }
    }
}
