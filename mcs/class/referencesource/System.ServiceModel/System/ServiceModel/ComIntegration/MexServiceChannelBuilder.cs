//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using ConfigNS = System.ServiceModel.Configuration;
    using DiscoNS = System.Web.Services.Discovery;
    using WsdlNS = System.Web.Services.Description;

    class MexServiceChannelBuilder : IProxyCreator, IProvideChannelBuilderSettings
    {

        ContractDescription contractDescription = null;
        ServiceChannelFactory serviceChannelFactory = null;
        Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile ServiceChannel serviceChannel = null;
        ServiceEndpoint serviceEndpoint = null;
        KeyedByTypeCollection<IEndpointBehavior> behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
        bool useXmlSerializer = false;

        //Suppressing PreSharp warning that property get methods should not throw
#pragma warning disable 6503
        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadWrite
        {
            get
            {
                if (serviceChannel != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.TooLate), HR.RPC_E_TOO_LATE));
                return serviceChannelFactory;
            }
        }
#pragma warning restore 6503

        ServiceChannel IProvideChannelBuilderSettings.ServiceChannel
        {
            get
            {
                return CreateChannel();
            }
        }
        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadOnly
        {
            get
            {
                return serviceChannelFactory;
            }
        }

        //Suppressing PreSharp warning that property get methods should not throw
#pragma warning disable 6503
        KeyedByTypeCollection<IEndpointBehavior> IProvideChannelBuilderSettings.Behaviors
        {
            get
            {
                if (serviceChannel != null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.TooLate), HR.RPC_E_TOO_LATE));
                return behaviors;
            }
        }
#pragma warning restore 6503

        void IDisposable.Dispose()
        {
            if (serviceChannel != null)
                serviceChannel.Close();

        }

        internal MexServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            this.propertyTable = propertyTable;
            DoMex();
        }

        ServiceChannel CreateChannel()
        {
            if (serviceChannel == null)
            {
                lock (this)
                {
                    if (serviceChannel == null)
                    {
                        try
                        {
                            if (serviceChannelFactory == null)
                            {
                                FaultInserviceChannelFactory();
                            }

                            if (serviceChannelFactory == null)
                            {
                                throw Fx.AssertAndThrow("ServiceChannelFactory cannot be null at this point");
                            }

                            serviceChannelFactory.Open();

                            if (serviceEndpoint == null)
                            {
                                throw Fx.AssertAndThrow("ServiceEndpoint cannot be null");
                            }

                            ServiceChannel localChannel = serviceChannelFactory.CreateServiceChannel(new EndpointAddress(serviceEndpoint.Address.Uri, serviceEndpoint.Address.Identity, serviceEndpoint.Address.Headers), serviceEndpoint.Address.Uri);
                            serviceChannel = localChannel;

                            ComPlusChannelCreatedTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationChannelCreated,
                                SR.TraceCodeComIntegrationChannelCreated, serviceEndpoint.Address.Uri, contractDescription.ContractType);

                            if (serviceChannel == null)
                            {
                                throw Fx.AssertAndThrow("serviceProxy MUST derive from RealProxy");
                            }
                        }
                        finally
                        {
                            if ((serviceChannel == null) && (serviceChannelFactory != null))
                            {
                                serviceChannelFactory.Close();
                            }
                        }
                    }
                }
            }
            return serviceChannel;

        }
        private ServiceChannelFactory CreateServiceChannelFactory()
        {
            serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(serviceEndpoint) as ServiceChannelFactory;
            if (serviceChannelFactory == null)
            {
                throw Fx.AssertAndThrow("We should get a ServiceChannelFactory back");
            }
            FixupProxyBehavior();
            return serviceChannelFactory;
        }

        void FaultInserviceChannelFactory()
        {
            if (propertyTable == null)
            {
                throw Fx.AssertAndThrow("PropertyTable should not be null");
            }
            foreach (IEndpointBehavior behavior in behaviors)
                serviceEndpoint.Behaviors.Add(behavior);
            serviceChannelFactory = CreateServiceChannelFactory();
        }

        void FixupProxyBehavior()
        {
            ClientOperation operation = null;

            if (useXmlSerializer)
                XmlSerializerOperationBehavior.AddBehaviors(contractDescription);

            foreach (OperationDescription opDesc in contractDescription.Operations)
            {
                operation = serviceChannelFactory.ClientRuntime.Operations[opDesc.Name];
                operation.SerializeRequest = true;
                operation.DeserializeReply = true;

                if (useXmlSerializer)
                    operation.Formatter = XmlSerializerOperationBehavior.CreateOperationFormatter(opDesc);
                else
                    operation.Formatter = new DataContractSerializerOperationFormatter(opDesc, TypeLoader.DefaultDataContractFormatAttribute, null);
            }
        }

        private void DoMex()
        {
            string mexAddress;
            string mexBindingSectionName;
            string mexBindingConfiguration;
            string contract;
            string contractNamespace;
            string binding;
            string bindingNamespace;
            string address;
            string spnIdentity = null;
            string upnIdentity = null;
            string dnsIdentity = null;
            string mexSpnIdentity = null;
            string mexUpnIdentity = null;
            string mexDnsIdentity = null;
            string serializer = null;

            EndpointIdentity identity = null;
            EndpointIdentity mexIdentity = null;

            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out contract);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out contractNamespace);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out bindingNamespace);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out binding);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out mexAddress);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBinding, out mexBindingSectionName);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBindingConfiguration, out mexBindingConfiguration);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out address);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out spnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out upnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out dnsIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexSpnIdentity, out mexSpnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexUpnIdentity, out mexUpnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexDnsIdentity, out mexDnsIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Serializer, out serializer);

            if (string.IsNullOrEmpty(mexAddress))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerMexAddressNotSpecified)));

            if (!string.IsNullOrEmpty(mexSpnIdentity))
            {
                if ((!string.IsNullOrEmpty(mexUpnIdentity)) || (!string.IsNullOrEmpty(mexDnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentityForMex)));
                mexIdentity = EndpointIdentity.CreateSpnIdentity(mexSpnIdentity);
            }
            else if (!string.IsNullOrEmpty(mexUpnIdentity))
            {
                if ((!string.IsNullOrEmpty(mexSpnIdentity)) || (!string.IsNullOrEmpty(mexDnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentityForMex)));
                mexIdentity = EndpointIdentity.CreateUpnIdentity(mexUpnIdentity);
            }
            else if (!string.IsNullOrEmpty(mexDnsIdentity))
            {
                if ((!string.IsNullOrEmpty(mexSpnIdentity)) || (!string.IsNullOrEmpty(mexUpnIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentityForMex)));
                mexIdentity = EndpointIdentity.CreateDnsIdentity(mexDnsIdentity);
            }
            else
                mexIdentity = null;

            if (string.IsNullOrEmpty(address))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerAddressNotSpecified)));

            if (string.IsNullOrEmpty(contract))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerContractNotSpecified)));

            if (string.IsNullOrEmpty(binding))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerBindingNotSpecified)));

            if (string.IsNullOrEmpty(bindingNamespace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerBindingNamespacetNotSpecified)));

            if (!string.IsNullOrEmpty(spnIdentity))
            {
                if ((!string.IsNullOrEmpty(upnIdentity)) || (!string.IsNullOrEmpty(dnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateSpnIdentity(spnIdentity);
            }
            else if (!string.IsNullOrEmpty(upnIdentity))
            {
                if ((!string.IsNullOrEmpty(spnIdentity)) || (!string.IsNullOrEmpty(dnsIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateUpnIdentity(upnIdentity);
            }
            else if (!string.IsNullOrEmpty(dnsIdentity))
            {
                if ((!string.IsNullOrEmpty(spnIdentity)) || (!string.IsNullOrEmpty(upnIdentity)))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorrectServerIdentity)));
                identity = EndpointIdentity.CreateDnsIdentity(dnsIdentity);
            }
            else
                identity = null;

            MetadataExchangeClient resolver = null;
            EndpointAddress mexEndpointAddress = new EndpointAddress(new Uri(mexAddress), mexIdentity);

            if (!string.IsNullOrEmpty(mexBindingSectionName))
            {
                Binding mexBinding = null;
                try
                {
                    mexBinding = ConfigLoader.LookupBinding(mexBindingSectionName, mexBindingConfiguration);
                }
                catch (System.Configuration.ConfigurationErrorsException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MexBindingNotFoundInConfig, mexBindingSectionName)));
                }


                if (null == mexBinding)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MexBindingNotFoundInConfig, mexBindingSectionName)));

                resolver = new MetadataExchangeClient(mexBinding);
            }
            else if (string.IsNullOrEmpty(mexBindingConfiguration))
                resolver = new MetadataExchangeClient(mexEndpointAddress);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerMexBindingSectionNameNotSpecified)));

            if (null != mexIdentity)
            {
                // To disable AllowNtlm warning.
#pragma warning disable 618
                resolver.SoapCredentials.Windows.AllowNtlm = false;
#pragma warning restore 618

            }

            bool removeXmlSerializerImporter = false;

            if (!String.IsNullOrEmpty(serializer))
            {
                if ("xml" != serializer && "datacontract" != serializer)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerIncorectSerializer)));

                if ("xml" == serializer)
                    useXmlSerializer = true;
                else
                    removeXmlSerializerImporter = true; // specifying datacontract will explicitly remove the Xml importer
                // if this parameter is not set we will simply use indigo defaults
            }

            ServiceEndpoint endpoint = null;
            ServiceEndpointCollection serviceEndpointsRetrieved = null;

            WsdlImporter importer;

            try
            {
                MetadataSet metadataSet = resolver.GetMetadata(mexEndpointAddress);

                if (useXmlSerializer)
                    importer = CreateXmlSerializerImporter(metadataSet);
                else
                {
                    if (removeXmlSerializerImporter)
                        importer = CreateDataContractSerializerImporter(metadataSet);
                    else
                        importer = new WsdlImporter(metadataSet);
                }

                serviceEndpointsRetrieved = this.ImportWsdlPortType(new XmlQualifiedName(contract, contractNamespace), importer);
                ComPlusMexChannelBuilderMexCompleteTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationMexMonikerMetadataExchangeComplete, SR.TraceCodeComIntegrationMexMonikerMetadataExchangeComplete, serviceEndpointsRetrieved);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                if (UriSchemeSupportsDisco(mexEndpointAddress.Uri))
                {
                    try
                    {
                        DiscoNS.DiscoveryClientProtocol discoClient = new DiscoNS.DiscoveryClientProtocol();
                        discoClient.UseDefaultCredentials = true;
                        discoClient.AllowAutoRedirect = true;

                        discoClient.DiscoverAny(mexEndpointAddress.Uri.AbsoluteUri);
                        discoClient.ResolveAll();
                        MetadataSet metadataSet = new MetadataSet();

                        foreach (object document in discoClient.Documents.Values)
                        {
                            AddDocumentToSet(metadataSet, document);
                        }

                        if (useXmlSerializer)
                            importer = CreateXmlSerializerImporter(metadataSet);
                        else
                        {
                            if (removeXmlSerializerImporter)
                                importer = CreateDataContractSerializerImporter(metadataSet);
                            else
                                importer = new WsdlImporter(metadataSet);
                        }

                        serviceEndpointsRetrieved = this.ImportWsdlPortType(new XmlQualifiedName(contract, contractNamespace), importer);
                        ComPlusMexChannelBuilderMexCompleteTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationMexMonikerMetadataExchangeComplete, SR.TraceCodeComIntegrationMexMonikerMetadataExchangeComplete, serviceEndpointsRetrieved);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                            throw;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerFailedToDoMexRetrieve, ex.Message)));
                    }
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerFailedToDoMexRetrieve, e.Message)));
            }

            if (serviceEndpointsRetrieved.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerContractNotFoundInRetreivedMex)));

            foreach (ServiceEndpoint retrievedEndpoint in serviceEndpointsRetrieved)
            {
                Binding bindingSelected = retrievedEndpoint.Binding;
                if ((bindingSelected.Name == binding) && (bindingSelected.Namespace == bindingNamespace))
                {
                    endpoint = retrievedEndpoint;
                    break;
                }
            }

            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.MonikerNoneOfTheBindingMatchedTheSpecifiedBinding)));

            contractDescription = endpoint.Contract;
            this.serviceEndpoint = new ServiceEndpoint(contractDescription, endpoint.Binding, new EndpointAddress(new Uri(address), identity, (AddressHeaderCollection)null));

            ComPlusMexChannelBuilderTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationMexChannelBuilderLoaded,
                SR.TraceCodeComIntegrationMexChannelBuilderLoaded, endpoint.Contract, endpoint.Binding, address);
        }

        static bool UriSchemeSupportsDisco(Uri serviceUri)
        {
            return (serviceUri.Scheme == Uri.UriSchemeHttp) || (serviceUri.Scheme == Uri.UriSchemeHttps);
        }

        void AddDocumentToSet(MetadataSet metadataSet, object document)
        {
            WsdlNS.ServiceDescription wsdl = document as WsdlNS.ServiceDescription;
            XmlSchema schema = document as XmlSchema;
            XmlElement xmlDoc = document as XmlElement;

            if (wsdl != null)
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromServiceDescription(wsdl));
            }
            else if (schema != null)
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromSchema(schema));
            }
            else if (xmlDoc != null && MetadataSection.IsPolicyElement(xmlDoc))
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromPolicy(xmlDoc, null));
            }
            else
            {
                MetadataSection mexDoc = new MetadataSection();
                mexDoc.Metadata = document;
                metadataSet.MetadataSections.Add(mexDoc);
            }
        }

        public WsdlImporter CreateDataContractSerializerImporter(MetadataSet metaData)
        {
            Collection<IWsdlImportExtension> wsdlImportExtensions = ConfigNS.ClientSection.GetSection().Metadata.LoadWsdlImportExtensions();

            for (int i = 0; i < wsdlImportExtensions.Count; i++)
            {
                if (wsdlImportExtensions[i].GetType() == typeof(XmlSerializerMessageContractImporter))
                    wsdlImportExtensions.RemoveAt(i);
            }

            WsdlImporter importer = new WsdlImporter(metaData, null, wsdlImportExtensions);

            return importer;
        }

        public WsdlImporter CreateXmlSerializerImporter(MetadataSet metaData)
        {
            Collection<IWsdlImportExtension> wsdlImportExtensions = ConfigNS.ClientSection.GetSection().Metadata.LoadWsdlImportExtensions();

            for (int i = 0; i < wsdlImportExtensions.Count; i++)
            {
                if (wsdlImportExtensions[i].GetType() == typeof(DataContractSerializerMessageContractImporter))
                    wsdlImportExtensions.RemoveAt(i);
            }

            WsdlImporter importer = new WsdlImporter(metaData, null, wsdlImportExtensions);

            return importer;
        }

        ServiceEndpointCollection ImportWsdlPortType(XmlQualifiedName portTypeQName, WsdlImporter importer)
        {
            foreach (WsdlNS.ServiceDescription wsdl in importer.WsdlDocuments)
            {
                if (wsdl.TargetNamespace == portTypeQName.Namespace)
                {
                    WsdlNS.PortType wsdlPortType = wsdl.PortTypes[portTypeQName.Name];
                    if (wsdlPortType != null)
                    {
                        ServiceEndpointCollection endpoints = importer.ImportEndpoints(wsdlPortType);
                        return endpoints;
                    }
                }
            }
            return new ServiceEndpointCollection();
        }



        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            IntPtr inner = IntPtr.Zero;
            if (riid != InterfaceID.idIDispatch)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(SR.GetString(SR.NoInterface, riid)));
            if (contractDescription == null)
            {
                throw Fx.AssertAndThrow("ContractDescription should not be null at this point");
            }
            return DispatchProxy.Create(outer, contractDescription, this);

        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if (riid != InterfaceID.idIDispatch)
                return false;
            else
                return true;

        }

        bool IProxyCreator.SupportsDispatch()
        {
            return true;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return true;
        }

    }
}



