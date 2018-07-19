//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    static class ProtocolStrings
    {
        public const string VersionNameDefault = Version11.Name;

        public const string XsNamespace = "http://www.w3.org/2001/XMLSchema";
        public const string WsaNamespaceAugust2004 = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
        public const string WsaNamespace10 = "http://www.w3.org/2005/08/addressing";

        public static class VersionApril2005
        {
            public const string Name = "WSDiscoveryApril2005";
            public const string Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery";

            public const string AdhocAddress = "urn:schemas-xmlsoap-org:ws:2005:04:discovery";

            public const string HelloAction = Namespace + "/Hello";
            public const string ByeAction = Namespace + "/Bye";
            public const string ProbeAction = Namespace + "/Probe";
            public const string ProbeMatchesAction = Namespace + "/ProbeMatches";
            public const string ResolveAction = Namespace + "/Resolve";
            public const string ResolveMatchesAction = Namespace + "/ResolveMatches";

            public const string ScopeMatchByExact = Namespace + "/strcmp0";
            public const string ScopeMatchByLdap = Namespace + "/ldap";
            public const string ScopeMatchByPrefix = Namespace + "/rfc2396";
            public const string ScopeMatchByUuid = Namespace + "/uuid";
            public const string ScopeMatchByNone = ProtocolStrings.Version11.Namespace + "/none";
        }

        public static class VersionCD1
        {
            public const string Name = "WSDiscoveryCD1";
            public const string Namespace = "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09";                                             

            public const string AdhocAddress = "urn:docs-oasis-open-org:ws-dd:discovery:2008:09";

            public const string HelloAction = Namespace + "/Hello";
            public const string ByeAction = Namespace + "/Bye";
            public const string ProbeAction = Namespace + "/Probe";
            public const string ProbeMatchesAction = Namespace + "/ProbeMatches";
            public const string ResolveAction = Namespace + "/Resolve";
            public const string ResolveMatchesAction = Namespace + "/ResolveMatches";

            public const string ScopeMatchByExact = Namespace + "/strcmp0";
            public const string ScopeMatchByLdap = Namespace + "/ldap";
            public const string ScopeMatchByPrefix = Namespace + "/rfc3986";
            public const string ScopeMatchByUuid = Namespace + "/uuid";
            public const string ScopeMatchByNone = ProtocolStrings.Version11.Namespace + "/none";
        }

        public static class Version11
        {
            public const string Name = "WSDiscovery11";
            public const string Namespace = "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01";

            public const string AdhocAddress = "urn:docs-oasis-open-org:ws-dd:ns:discovery:2009:01";

            public const string HelloAction = Namespace + "/Hello";
            public const string ByeAction = Namespace + "/Bye";
            public const string ProbeAction = Namespace + "/Probe";
            public const string ProbeMatchesAction = Namespace + "/ProbeMatches";
            public const string ResolveAction = Namespace + "/Resolve";
            public const string ResolveMatchesAction = Namespace + "/ResolveMatches";

            public const string ScopeMatchByExact = Namespace + "/strcmp0";
            public const string ScopeMatchByLdap = Namespace + "/ldap";
            public const string ScopeMatchByPrefix = Namespace + "/rfc3986";
            public const string ScopeMatchByUuid = Namespace + "/uuid";
            public const string ScopeMatchByNone = Namespace + "/none";
        }

        public static class VersionInternal
        {
            public const string Namespace = "http://schemas.microsoft.com/ws/2008/06/discovery";

            public const string AdhocAddress = "urn:schemas-microsoft-org:ws:2008:07:discovery";

            public const string ScopeMatchByExact = Namespace + "/strcmp0";
            public const string ScopeMatchByLdap = Namespace + "/ldap";
            public const string ScopeMatchByPrefix = Namespace + "/rfc";
            public const string ScopeMatchByUuid = Namespace + "/uuid";
            public const string ScopeMatchByNone = Namespace + "/none";
        }

        public static class SchemaNames
        {
            public const string AppSequenceElement = "AppSequence";
            public const string AppSequenceInstanceId = "InstanceId";
            public const string AppSequenceMessageNumber = "MessageNumber";
            public const string AppSequenceSequenceId = "SequenceId";
            public const string AppSequenceType = "AppSequenceType";
            public const string ByeElement = "Bye";
            public const string DefaultPrefix = "d";
            public const string DurationElement = "Duration";
            public const string EprElement = "EndpointReference";
            public const string HelloElement = "Hello";
            public const string MatchByAttribute = "MatchBy";
            public const string MaxResultsElement = "MaxResults";
            public const string MetadataVersionElement = "MetadataVersion";
            public const string ProbeElement = "Probe";
            public const string ProbeMatchElement = "ProbeMatch";
            public const string ProbeMatchesElement = "ProbeMatches";
            public const string ProbeMatchType = "ProbeMatchType";
            public const string ProbeType = "ProbeType";
            public const string QNameListType = "QNameListType";
            public const string ResolveElement = "Resolve";
            public const string ResolveMatchElement = "ResolveMatch";
            public const string ResolveMatchesElement = "ResolveMatches";
            public const string ResolveType = "ResolveType";
            public const string ScopesElement = "Scopes";
            public const string ScopesType = "ScopesType";
            public const string TypesElement = "Types";
            public const string UriListType = "UriListType";
            public const string XAddrsElement = "XAddrs";
        }

        public static class TracingStrings
        {
            public const string Bye = SchemaNames.ByeElement;                        
            public const string FindOperation = "Find";
            public const string Hello = SchemaNames.HelloElement;
            public const string Probe = SchemaNames.ProbeElement;
            public const string ProbeMatches = SchemaNames.ProbeMatchesElement;
            public const string Resolve = SchemaNames.ResolveElement;
            public const string ResolveMatches = SchemaNames.ResolveMatchesElement;
            public const string ResolveOperation = "Resolve";
        }

        public static class ContractNames
        {
            public const string AnnouncementContractName = "Client";
            public const string DiscoveryManagedContractName = "DiscoveryProxy";
            public const string DiscoveryAdhocContractName = "TargetService";
            public const string DiscoveryAdhocResposeContractName = "TargetServiceResponse";
        }

        public static class Udp
        {
            public const string MulticastIPv4Address = "soap.udp://239.255.255.250:3702";
            public const string MulticastIPv6Address = "soap.udp://[FF02::C]:3702";
        }
    }
}
