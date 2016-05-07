//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;
    using System.Xml.Linq;
    using SR2 = System.ServiceModel.Discovery.SR;
    
    [Fx.Tag.XamlVisible(false)]
    public class EndpointDiscoveryMetadata
    {        
        static XmlQualifiedName metadataContractName;

        EndpointAddress endpointAddress;
        OpenableContractTypeNameCollection contractTypeNames;
        OpenableScopeCollection scopes;
        OpenableCollection<Uri> listenUris;
        OpenableCollection<XElement> extensions;        
        int metadataVersion;
        string[] compiledScopes;
        bool isOpen;

        public EndpointDiscoveryMetadata()
        {
            this.endpointAddress = new EndpointAddress(EndpointAddress.AnonymousUri);
        }

        public Collection<XmlQualifiedName> ContractTypeNames
        {
            get
            {
                if (this.contractTypeNames == null)
                {
                    this.contractTypeNames = new OpenableContractTypeNameCollection(this.isOpen);
                }

                return this.contractTypeNames;
            }
        }

        public EndpointAddress Address
        {
            get
            {
                return this.endpointAddress;
            }

            set
            {
                ThrowIfOpen();
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.endpointAddress = value;
            }
        }

        public Collection<XElement> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new OpenableCollection<XElement>(this.isOpen);
                }

                return this.extensions;
            }
        }

        public Collection<Uri> ListenUris
        {
            get
            {
                if (this.listenUris == null)
                {
                    this.listenUris = new OpenableCollection<Uri>(this.isOpen);
                }

                return this.listenUris;
            }
        }

        public Collection<Uri> Scopes
        {
            get
            {
                if (this.scopes == null)
                {
                    this.scopes = new OpenableScopeCollection(this.isOpen);
                }

                return this.scopes;
            }
        }

        public int Version
        {
            get
            {
                return this.metadataVersion;
            }
            set
            {
                ThrowIfOpen();
                if (value < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR2.DiscoveryMetadataVersionLessThanZero);
                }

                this.metadataVersion = value;
            }
        }

        internal static XmlQualifiedName MetadataContractName
        {
            get
            {
                if (metadataContractName == null)
                {
                    ContractDescription metadataContract = ContractDescription.GetContract(typeof(IMetadataExchange));
                    metadataContractName = new XmlQualifiedName(metadataContract.Name, metadataContract.Namespace);
                }

                return metadataContractName;
            }
        }

        internal Collection<XmlQualifiedName> InternalContractTypeNames
        {
            get
            {
                return this.contractTypeNames;
            }
        }

        internal string[] CompiledScopes
        {
            get
            {
                Fx.Assert(IsOpen, "The CompiledScopes property is valid only if this EndpointDiscoveryMetadata instance is open.");
                return this.compiledScopes;
            }
        }

        internal bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        public static EndpointDiscoveryMetadata FromServiceEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpoint");
            }

            return GetEndpointDiscoveryMetadata(endpoint, endpoint.ListenUri);
        }

        public static EndpointDiscoveryMetadata FromServiceEndpoint(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpoint");
            }
            if (endpointDispatcher == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDispatcher");
            }

            EndpointDiscoveryMetadata endpointDiscoveryMetadata;
            if ((endpointDispatcher.ChannelDispatcher != null) &&
                (endpointDispatcher.ChannelDispatcher.Listener != null))
            {
                endpointDiscoveryMetadata = GetEndpointDiscoveryMetadata(endpoint, endpointDispatcher.ChannelDispatcher.Listener.Uri);
            }
            else
            {
                endpointDiscoveryMetadata = GetEndpointDiscoveryMetadata(endpoint, endpoint.ListenUri);
            }

            if ((endpointDiscoveryMetadata != null) &&
                IsMetadataEndpoint(endpoint) &&
                CanHaveMetadataEndpoints(endpointDispatcher))
            {
                AddContractTypeScopes(endpointDiscoveryMetadata, endpointDispatcher.ChannelDispatcher.Host.Description);
            }

            return endpointDiscoveryMetadata;
        }

        static EndpointDiscoveryMetadata GetEndpointDiscoveryMetadata(ServiceEndpoint endpoint, Uri listenUri)
        {
            EndpointDiscoveryMetadata endpointDiscoveryMetadata = new EndpointDiscoveryMetadata();
            endpointDiscoveryMetadata.Address = endpoint.Address;            
            endpointDiscoveryMetadata.ListenUris.Add(listenUri);            

            EndpointDiscoveryBehavior endpointDiscoveryBehavior = endpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
            if (endpointDiscoveryBehavior != null)
            {

                if (!endpointDiscoveryBehavior.Enabled)
                {
                    if (TD.EndpointDiscoverabilityDisabledIsEnabled())
                    {
                        TD.EndpointDiscoverabilityDisabled(endpoint.Address.ToString(), listenUri.ToString());
                    }
                    return null;
                }

                if (TD.EndpointDiscoverabilityEnabledIsEnabled())
                {
                    TD.EndpointDiscoverabilityEnabled(endpoint.Address.ToString(), listenUri.ToString());
                }

                if (endpointDiscoveryBehavior.InternalContractTypeNames != null)
                {
                    foreach (XmlQualifiedName contractTypeName in endpointDiscoveryBehavior.InternalContractTypeNames)
                    {
                        endpointDiscoveryMetadata.ContractTypeNames.Add(contractTypeName);
                    }
                }                

                if (endpointDiscoveryBehavior.InternalScopes != null)
                {
                    foreach (Uri scope in endpointDiscoveryBehavior.InternalScopes)
                    {
                        endpointDiscoveryMetadata.Scopes.Add(scope);
                    }
                }
                if (endpointDiscoveryBehavior.InternalExtensions != null)
                {
                    foreach (XElement xElement in endpointDiscoveryBehavior.InternalExtensions)
                    {
                        endpointDiscoveryMetadata.Extensions.Add(xElement);
                    }
                }
            }

            XmlQualifiedName defaultContractTypeName = new XmlQualifiedName(endpoint.Contract.Name, endpoint.Contract.Namespace);

            if (!endpointDiscoveryMetadata.ContractTypeNames.Contains(defaultContractTypeName))
            {
                endpointDiscoveryMetadata.ContractTypeNames.Add(defaultContractTypeName);
            }

            return endpointDiscoveryMetadata;
        }

        static void AddContractTypeScopes(EndpointDiscoveryMetadata endpointDiscoveryMetadata, ServiceDescription serviceDescription)
        {
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (IsMetadataEndpoint(endpoint) || IsDiscoverySystemEndpoint(endpoint))
                {
                    continue;
                }

                endpointDiscoveryMetadata.Scopes.Add(FindCriteria.GetContractTypeNameScope(
                    new XmlQualifiedName(endpoint.Contract.Name, endpoint.Contract.Namespace)));
            }
        }

        static bool CanHaveMetadataEndpoints(EndpointDispatcher endpointDispatcher)
        {
            if ((endpointDispatcher.ChannelDispatcher == null) || (endpointDispatcher.ChannelDispatcher.Host == null))
            {
                return false;
            }

            ServiceDescription description = endpointDispatcher.ChannelDispatcher.Host.Description;
            if (description.Behaviors != null && description.Behaviors.Find<ServiceMetadataBehavior>() == null)
            {
                return false;
            }

            if (description.ServiceType != null && description.ServiceType.GetInterface(typeof(IMetadataExchange).Name) != null)
            {
                return false;
            }

            return true;
        }

        internal static bool IsDiscoverySystemEndpoint(EndpointDispatcher endpointDispatcher)
        {
            return (endpointDispatcher.IsSystemEndpoint && 
                IsDiscoveryContract(endpointDispatcher.ContractName, endpointDispatcher.ContractNamespace));
        }

        internal static bool IsDiscoverySystemEndpoint(ServiceEndpoint endpoint)
        {
            return (endpoint.IsSystemEndpoint && 
                IsDiscoveryContract(endpoint.Contract.Name, endpoint.Contract.Namespace));
        }

        static bool IsDiscoveryContract(string contractName, string contractNamespace)
        {
            return (IsDiscoveryContractName(contractName) && IsDiscoveryContractNamespace(contractNamespace));
        }

        static bool IsDiscoveryContractName(string contractName)
        {
            return ((string.CompareOrdinal(contractName, ProtocolStrings.ContractNames.DiscoveryAdhocContractName) == 0) ||
                (string.CompareOrdinal(contractName, ProtocolStrings.ContractNames.DiscoveryManagedContractName) == 0));
        }

        static bool IsDiscoveryContractNamespace(string contractNamespace)
        {
            return ((string.CompareOrdinal(contractNamespace, ProtocolStrings.VersionApril2005.Namespace) == 0) ||
                (string.CompareOrdinal(contractNamespace, ProtocolStrings.Version11.Namespace) == 0) ||
                (string.CompareOrdinal(contractNamespace, ProtocolStrings.VersionCD1.Namespace) == 0));
        }

        internal static bool IsMetadataEndpoint(ServiceEndpoint endpoint)
        {
            return ((string.CompareOrdinal(endpoint.Contract.Name, MetadataContractName.Name) == 0) &&
                (string.CompareOrdinal(endpoint.Contract.Namespace, MetadataContractName.Namespace) == 0));
        }

        [Fx.Tag.Throws(typeof(XmlException), "throws on incorrect xml data")]
        internal void ReadFrom(DiscoveryVersion discoveryVersion, XmlReader reader)
        {
            ThrowIfOpen();

            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }

            this.endpointAddress = new EndpointAddress(EndpointAddress.AnonymousUri);
            this.contractTypeNames = null;
            this.scopes = null;
            this.listenUris = null;
            this.metadataVersion = 0;
            this.extensions = null;
            this.isOpen = false;

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlEndpointNull));
            }

            int startDepth = reader.Depth;
            reader.ReadStartElement();

            this.endpointAddress = SerializationUtility.ReadEndpointAddress(discoveryVersion, reader);            

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.TypesElement, discoveryVersion.Namespace))
            {
                this.contractTypeNames = new OpenableContractTypeNameCollection(false);
                SerializationUtility.ReadContractTypeNames(this.contractTypeNames, reader);
            }

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.ScopesElement, discoveryVersion.Namespace))
            {
                this.scopes = new OpenableScopeCollection(false);
                SerializationUtility.ReadScopes(this.scopes, reader);
            }

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.XAddrsElement, discoveryVersion.Namespace))
            {
                this.listenUris = new OpenableCollection<Uri>(false);
                SerializationUtility.ReadListenUris(listenUris, reader);
            }

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.MetadataVersionElement, discoveryVersion.Namespace))
            {
                this.metadataVersion = SerializationUtility.ReadMetadataVersion(reader);
            }

            while (true)
            {
                reader.MoveToContent();

                if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Depth == startDepth))
                {
                    break;
                }
                else if (reader.IsStartElement())
                {
                    this.Extensions.Add(XElement.ReadFrom(reader) as XElement);
                }
                else
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement();            
        }

        internal void WriteTo(DiscoveryVersion discoveryVersion, XmlWriter writer)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (writer == null)
            {
                throw FxTrace.Exception.ArgumentNull("writer");
            }

            SerializationUtility.WriteEndPointAddress(discoveryVersion, this.endpointAddress, writer);

            SerializationUtility.WriteContractTypeNames(discoveryVersion, this.contractTypeNames, writer);


            SerializationUtility.WriteScopes(discoveryVersion, this.scopes, null, writer);

            SerializationUtility.WriteListenUris(discoveryVersion, this.listenUris, writer);

            SerializationUtility.WriteMetadataVersion(discoveryVersion, this.metadataVersion, writer);

            if (this.extensions != null)
            {
                foreach (XElement xElement in Extensions)
                {
                    xElement.WriteTo(writer);
                }
            }
        }

        internal void Open()
        {
            if (this.contractTypeNames != null)
            {
                this.contractTypeNames.Open();
            }
            if (this.scopes != null)
            {
                this.scopes.Open();
                this.compiledScopes = ScopeCompiler.Compile(this.scopes);
            }
            if (this.listenUris != null)
            {
                this.listenUris.Open();
            }
            if (this.extensions != null)
            {
                this.extensions.Open();
            }

            this.isOpen = true;
        }

        void ThrowIfOpen()
        {
            if (this.isOpen)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoveryMetadataAlreadyOpen));
            }
        }

        class OpenableCollection<T> : NonNullItemCollection<T>
        {
            bool isOpen;

            public OpenableCollection(bool opened)
            {
                this.isOpen = opened;
            }

            void ThrowIfOpen()
            {
                if (this.isOpen)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoverySdmCollectionIsOpen(typeof(T).Name)));
                }
            }

            internal void Open()
            {
                this.isOpen = true;
            }

            protected override void ClearItems()
            {
                ThrowIfOpen();
                base.ClearItems();
            }

            protected override void InsertItem(int index, T item)
            {
                ThrowIfOpen();
                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                ThrowIfOpen();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, T item)
            {
                ThrowIfOpen();
                base.SetItem(index, item);
            }
        }

        class OpenableContractTypeNameCollection : OpenableCollection<XmlQualifiedName>
        {

            public OpenableContractTypeNameCollection(bool opened)
                : base(opened)
            {
            }

            protected override void InsertItem(int index, XmlQualifiedName item)
            {
                if ((item != null) && (item.Name == string.Empty))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryArgumentEmptyContractTypeName));
                }
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, XmlQualifiedName item)
            {
                if ((item != null) && (item.Name == string.Empty))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryArgumentEmptyContractTypeName));
                }
                base.SetItem(index, item);
            }
        }

        class OpenableScopeCollection : OpenableCollection<Uri>
        {

            public OpenableScopeCollection(bool opened) : base(opened)
            {
            }

            protected override void InsertItem(int index, Uri item)
            {
                if (item != null && !item.IsAbsoluteUri)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryArgumentInvalidScopeUri(item)));
                }
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, Uri item)
            {
                if (item != null && !item.IsAbsoluteUri)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR2.DiscoveryArgumentInvalidScopeUri(item)));
                }
                base.SetItem(index, item);
            }
        }
    }
}
