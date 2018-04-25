//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Globalization;

    class DiscoveryVersion11Implementation : IDiscoveryVersionImplementation
    {
        static readonly Uri ScopeMatchByExact = new Uri(ProtocolStrings.Version11.ScopeMatchByExact);
        static readonly Uri ScopeMatchByLdap = new Uri(ProtocolStrings.Version11.ScopeMatchByLdap);
        static readonly Uri ScopeMatchByPrefix = new Uri(ProtocolStrings.Version11.ScopeMatchByPrefix);
        static readonly Uri ScopeMatchByUuid = new Uri(ProtocolStrings.Version11.ScopeMatchByUuid);
        static readonly Uri ScopeMatchByNone = new Uri(ProtocolStrings.Version11.ScopeMatchByNone);

        Uri discoveryAddress;
        DataContractSerializer eprSerializer;
        DiscoveryVersion.SchemaQualifiedNames qualifiedNames;

        ContractDescription adhocDiscoveryContract;
        ContractDescription managedDiscoveryContract;
        ContractDescription announcementContract;

        [Fx.Tag.SynchronizationObject()]
        object contractLock;

        public DiscoveryVersion11Implementation()
        {
            this.contractLock = new object();
        }

        public string WsaNamespace
        {
            get
            {
                return ProtocolStrings.WsaNamespace10;
            }
        }

        public Uri DiscoveryAddress
        {
            get
            {
                if (this.discoveryAddress == null)
                {
                    this.discoveryAddress = new Uri(ProtocolStrings.Version11.AdhocAddress);
                }
                return this.discoveryAddress;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return MessageVersion.Soap12WSAddressing10;
            }
        }

        public DiscoveryVersion.SchemaQualifiedNames QualifiedNames
        {
            get
            {
                if (this.qualifiedNames == null)
                {
                    this.qualifiedNames = new DiscoveryVersion.SchemaQualifiedNames(ProtocolStrings.Version11.Namespace, this.WsaNamespace);
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
                    this.eprSerializer = new DataContractSerializer(typeof(EndpointAddress10));
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
                            this.adhocDiscoveryContract = DiscoveryUtility.GetContract(typeof(IDiscoveryContractAdhoc11));
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
                            this.managedDiscoveryContract = DiscoveryUtility.GetContract(typeof(IDiscoveryContractManaged11));
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
                        this.announcementContract = DiscoveryUtility.GetContract(typeof(IAnnouncementContract11));
                    }
                }
            }
            return this.announcementContract;
        }

        public IDiscoveryInnerClient CreateDiscoveryInnerClient(DiscoveryEndpoint discoveryEndpoint, IDiscoveryInnerClientResponse responseReceiver)
        {
            if (discoveryEndpoint.DiscoveryMode == ServiceDiscoveryMode.Adhoc)
            {
                return new DiscoveryInnerClientAdhoc11(discoveryEndpoint, responseReceiver);
            }
            else if (discoveryEndpoint.DiscoveryMode == ServiceDiscoveryMode.Managed)
            {
                return new DiscoveryInnerClientManaged11(discoveryEndpoint, responseReceiver);
            }
            else
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.DiscoveryIncorrectMode(discoveryEndpoint.DiscoveryMode)));
            }
        }

        public IAnnouncementInnerClient CreateAnnouncementInnerClient(AnnouncementEndpoint announcementEndpoint)
        {
            return new AnnouncementInnerClient11(announcementEndpoint);
        }

        public Uri ToVersionIndependentScopeMatchBy(Uri versionDependentScopeMatchBy)
        {
            Uri scopeMatchBy = versionDependentScopeMatchBy;

            if (versionDependentScopeMatchBy == DiscoveryVersion11Implementation.ScopeMatchByExact)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByExact;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersion11Implementation.ScopeMatchByPrefix)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByPrefix;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersion11Implementation.ScopeMatchByLdap)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByLdap;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersion11Implementation.ScopeMatchByUuid)
            {
                scopeMatchBy = FindCriteria.ScopeMatchByUuid;
            }
            else if (versionDependentScopeMatchBy == DiscoveryVersion11Implementation.ScopeMatchByNone)
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
                scopeMatchBy = DiscoveryVersion11Implementation.ScopeMatchByExact;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByPrefix)
            {
                scopeMatchBy = DiscoveryVersion11Implementation.ScopeMatchByPrefix;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByLdap)
            {
                scopeMatchBy = DiscoveryVersion11Implementation.ScopeMatchByLdap;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByUuid)
            {
                scopeMatchBy = DiscoveryVersion11Implementation.ScopeMatchByUuid;
            }
            else if (versionIndependentScopeMatchBy == FindCriteria.ScopeMatchByNone)
            {
                scopeMatchBy = DiscoveryVersion11Implementation.ScopeMatchByNone;
            }

            return scopeMatchBy;
        }
    }
}
