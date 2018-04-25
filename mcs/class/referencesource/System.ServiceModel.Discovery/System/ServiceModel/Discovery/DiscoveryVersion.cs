//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Discovery.Version11;
    using System.ServiceModel.Discovery.VersionApril2005;
    using System.ServiceModel.Discovery.VersionCD1;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public sealed class DiscoveryVersion
    {
        static DiscoveryVersion wsDiscoveryApril2005;
        static DiscoveryVersion wsDiscoveryCD1;
        static DiscoveryVersion wsDiscovery11;

        [Fx.Tag.SynchronizationObject()]
        static object staticLock = new object();

        string name;
        string discoveryNamespace;
        IDiscoveryVersionImplementation discoveryVersionImplementation;

        DiscoveryVersion(string name, string discoveryNamespace,
            IDiscoveryVersionImplementation discoveryVersionImplementation)
        {
            this.name = name;
            this.discoveryNamespace = discoveryNamespace;
            this.discoveryVersionImplementation = discoveryVersionImplementation;
        }

        public static DiscoveryVersion WSDiscoveryApril2005
        {
            get
            {
                if (DiscoveryVersion.wsDiscoveryApril2005 == null)
                {
                    lock (staticLock)
                    {
                        if (DiscoveryVersion.wsDiscoveryApril2005 == null)
                        {
                            DiscoveryVersion.wsDiscoveryApril2005 = new DiscoveryVersion(
                                ProtocolStrings.VersionApril2005.Name,
                                ProtocolStrings.VersionApril2005.Namespace,
                                new DiscoveryVersionApril2005Implementation());
                        }
                    }
                }
                return DiscoveryVersion.wsDiscoveryApril2005;
            }
        }

        public static DiscoveryVersion WSDiscoveryCD1
        {
            get
            {
                if (DiscoveryVersion.wsDiscoveryCD1 == null)
                {
                    lock (staticLock)
                    {
                        if (DiscoveryVersion.wsDiscoveryCD1 == null)
                        {
                            DiscoveryVersion.wsDiscoveryCD1 = new DiscoveryVersion(
                                ProtocolStrings.VersionCD1.Name,
                                ProtocolStrings.VersionCD1.Namespace,
                                new DiscoveryVersionCD1Implementation());
                        }
                    }
                }
                return DiscoveryVersion.wsDiscoveryCD1;
            }
        }

        public static DiscoveryVersion WSDiscovery11
        {
            get
            {
                if (DiscoveryVersion.wsDiscovery11 == null)
                {
                    lock (staticLock)
                    {
                        if (DiscoveryVersion.wsDiscovery11 == null)
                        {
                            DiscoveryVersion.wsDiscovery11 = new DiscoveryVersion(
                                ProtocolStrings.Version11.Name,
                                ProtocolStrings.Version11.Namespace,
                                new DiscoveryVersion11Implementation());
                        }
                    }
                }
                return DiscoveryVersion.wsDiscovery11;
            }
        }

        public string Namespace
        {
            get
            {
                return this.discoveryNamespace;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.discoveryVersionImplementation.MessageVersion;
            }
        }

        [SuppressMessage(
            FxCop.Category.Naming,
            FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
            Justification = "Adhoc is a valid name.")]
        public Uri AdhocAddress
        {
            get
            {
                return this.discoveryVersionImplementation.DiscoveryAddress;
            }
        }

        internal static DiscoveryVersion DefaultDiscoveryVersion
        {
            get
            {
                return DiscoveryVersion.FromName(ProtocolStrings.VersionNameDefault);
            }
        }

        internal IDiscoveryVersionImplementation Implementation
        {
            get
            {
                return this.discoveryVersionImplementation;
            }
        }

        public static DiscoveryVersion FromName(string name)
        {
            if (name == null)
            {
                throw FxTrace.Exception.ArgumentNull("name");
            }

            if (WSDiscovery11.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return WSDiscovery11;
            }
            else if (WSDiscoveryCD1.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return WSDiscoveryCD1;
            }
            else if (WSDiscoveryApril2005.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return WSDiscoveryApril2005;
            }

            throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException(
                SR2.DiscoveryIncorrectVersion(name, WSDiscovery11.Name, WSDiscoveryCD1.Name, WSDiscoveryApril2005.Name)));
        }

        public override string ToString()
        {
            return SR.DiscoveryVersionToString(this.Name, this.Namespace);
        }

        internal class SchemaQualifiedNames
        {
            public readonly XmlQualifiedName AppSequenceType;
            public readonly XmlQualifiedName AnyType;
            public readonly XmlQualifiedName AnyUriType;
            public readonly XmlQualifiedName EprElement;
            public readonly XmlQualifiedName MetadataVersionElement;
            public readonly XmlQualifiedName ProbeMatchType;
            public readonly XmlQualifiedName ProbeType;
            public readonly XmlQualifiedName QNameListType;
            public readonly XmlQualifiedName QNameType;
            public readonly XmlQualifiedName ResolveType;
            public readonly XmlQualifiedName ScopesElement;
            public readonly XmlQualifiedName ScopesType;
            public readonly XmlQualifiedName TypesElement;
            public readonly XmlQualifiedName UnsignedIntType;
            public readonly XmlQualifiedName UriListType;
            public readonly XmlQualifiedName XAddrsElement;

            internal SchemaQualifiedNames(string versionNameSpace, string wsaNameSpace)
            {
                this.AppSequenceType = new XmlQualifiedName(ProtocolStrings.SchemaNames.AppSequenceType, versionNameSpace);
                this.AnyType = new XmlQualifiedName("anyType", ProtocolStrings.XsNamespace);
                this.AnyUriType = new XmlQualifiedName("anyURI", ProtocolStrings.XsNamespace);
                this.EprElement = new XmlQualifiedName(ProtocolStrings.SchemaNames.EprElement, wsaNameSpace);
                this.MetadataVersionElement = new XmlQualifiedName(ProtocolStrings.SchemaNames.MetadataVersionElement, versionNameSpace);
                this.ProbeMatchType = new XmlQualifiedName(ProtocolStrings.SchemaNames.ProbeMatchType, versionNameSpace);
                this.ProbeType = new XmlQualifiedName(ProtocolStrings.SchemaNames.ProbeType, versionNameSpace);
                this.QNameListType = new XmlQualifiedName(ProtocolStrings.SchemaNames.QNameListType, versionNameSpace);
                this.QNameType = new XmlQualifiedName("QName", ProtocolStrings.XsNamespace);
                this.ResolveType = new XmlQualifiedName(ProtocolStrings.SchemaNames.ResolveType, versionNameSpace);
                this.ScopesElement = new XmlQualifiedName(ProtocolStrings.SchemaNames.ScopesElement, versionNameSpace);
                this.ScopesType = new XmlQualifiedName(ProtocolStrings.SchemaNames.ScopesType, versionNameSpace);
                this.TypesElement = new XmlQualifiedName(ProtocolStrings.SchemaNames.TypesElement, versionNameSpace);
                this.UnsignedIntType = new XmlQualifiedName("unsignedInt", ProtocolStrings.XsNamespace);
                this.UriListType = new XmlQualifiedName(ProtocolStrings.SchemaNames.UriListType, versionNameSpace);
                this.XAddrsElement = new XmlQualifiedName(ProtocolStrings.SchemaNames.XAddrsElement, versionNameSpace);
            }
        }
    }
}
