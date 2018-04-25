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
    using System.IO;
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
    using WsdlNS = System.Web.Services.Description;

    internal class WsdlServiceChannelBuilder : IProxyCreator, IProvideChannelBuilderSettings
    {

        ContractDescription contractDescription = null;
        ServiceChannelFactory serviceChannelFactory = null;
        Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable;
        ServiceChannel serviceChannel = null;
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

        internal WsdlServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            this.propertyTable = propertyTable;
            ProcessWsdl();

        }

        private ServiceChannel CreateChannel()
        {
            Thread.MemoryBarrier();
            if (serviceChannel == null)
            {
                lock (this)
                {
                    Thread.MemoryBarrier();
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
                            serviceChannel = serviceChannelFactory.CreateServiceChannel(new EndpointAddress(serviceEndpoint.Address.Uri, serviceEndpoint.Address.Identity, serviceEndpoint.Address.Headers), serviceEndpoint.Address.Uri);
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

        private void ProcessWsdl()
        {
            string wsdlText;
            string portType;
            string bindingName;
            string address;
            string spnIdentity = null;
            string upnIdentity = null;
            string dnsIdentity = null;
            EndpointIdentity identity = null;
            string serializer = null;
            string contractNamespace = null;
            string bindingNamespace = null;

            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out wsdlText);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out portType);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out bindingName);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out address);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out spnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out upnIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out dnsIdentity);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Serializer, out serializer);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out bindingNamespace);
            propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out contractNamespace);

            if (string.IsNullOrEmpty(wsdlText))
            {
                throw Fx.AssertAndThrow("Wsdl should not be null at this point");
            }
            if (string.IsNullOrEmpty(portType) || string.IsNullOrEmpty(bindingName) || string.IsNullOrEmpty(address))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.ContractBindingAddressCannotBeNull)));

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

            TextReader reader = new StringReader(wsdlText);
            try
            {
                try
                {
                    WsdlNS.ServiceDescription wsdl = WsdlNS.ServiceDescription.Read(reader);

                    if (String.IsNullOrEmpty(contractNamespace))
                        contractNamespace = wsdl.TargetNamespace;

                    if (String.IsNullOrEmpty(bindingNamespace))
                        bindingNamespace = wsdl.TargetNamespace;

                    WsdlNS.ServiceDescriptionCollection wsdlDocs = new WsdlNS.ServiceDescriptionCollection();
                    wsdlDocs.Add(wsdl);
                    XmlSchemaSet schemas = new XmlSchemaSet();
                    foreach (XmlSchema schema in wsdl.Types.Schemas)
                        schemas.Add(schema);

                    MetadataSet mds = new MetadataSet(WsdlImporter.CreateMetadataDocuments(wsdlDocs, schemas, null));
                    WsdlImporter importer;

                    if (useXmlSerializer)
                        importer = CreateXmlSerializerImporter(mds);
                    else
                    {
                        if (removeXmlSerializerImporter)
                            importer = CreateDataContractSerializerImporter(mds);
                        else
                            importer = new WsdlImporter(mds);
                    }

                    XmlQualifiedName contractQname = new XmlQualifiedName(portType, contractNamespace);
                    XmlQualifiedName bindingQname = new XmlQualifiedName(bindingName, bindingNamespace);

                    WsdlNS.PortType wsdlPortType = wsdlDocs.GetPortType(contractQname);
                    contractDescription = importer.ImportContract(wsdlPortType);

                    WsdlNS.Binding wsdlBinding = wsdlDocs.GetBinding(bindingQname);
                    Binding binding = importer.ImportBinding(wsdlBinding);

                    EndpointAddress endpointAddress = new EndpointAddress(new Uri(address), identity, (AddressHeaderCollection)null);

                    serviceEndpoint = new ServiceEndpoint(contractDescription, binding, endpointAddress);

                    ComPlusWsdlChannelBuilderTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationWsdlChannelBuilderLoaded,
                        SR.TraceCodeComIntegrationWsdlChannelBuilderLoaded, bindingQname, contractQname, wsdl, contractDescription, binding, wsdl.Types.Schemas);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(SR.GetString(SR.FailedImportOfWsdl, e.Message)));
                }
            }
            finally
            {
                IDisposable disposee = reader;
                disposee.Dispose();
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
          
               
          
