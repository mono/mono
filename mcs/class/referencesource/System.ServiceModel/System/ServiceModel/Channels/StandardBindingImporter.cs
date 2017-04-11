//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml.Schema;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using WsdlNS = System.Web.Services.Description;

    public class StandardBindingImporter : IWsdlImportExtension
    {
        void IWsdlImportExtension.BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy) { }
        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context) { }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext endpointContext)
        {
            if (endpointContext == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext");

#pragma warning suppress 56506 // Microsoft, endpointContext.Endpoint is never null
            if (endpointContext.Endpoint.Binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointContext.Binding");

            if (endpointContext.Endpoint.Binding is CustomBinding)
            {
                BindingElementCollection elements = ((CustomBinding)endpointContext.Endpoint.Binding).Elements;

                Binding binding;
                TransportBindingElement transport = elements.Find<TransportBindingElement>();

                if (transport is HttpTransportBindingElement)
                {
                    if (WSHttpBindingBase.TryCreate(elements, out binding))
                    {
                        SetBinding(endpointContext.Endpoint, binding);
                    }
                    else if (WSDualHttpBinding.TryCreate(elements, out binding))
                    {
                        SetBinding(endpointContext.Endpoint, binding);
                    }
                    else if (BasicHttpBinding.TryCreate(elements, out binding))
                    {
                        SetBinding(endpointContext.Endpoint, binding);
                    }
                    else if (NetHttpBinding.TryCreate(elements, out binding))
                    {
                        SetBinding(endpointContext.Endpoint, binding);
                    }
                }
                else if (transport is MsmqTransportBindingElement && NetMsmqBinding.TryCreate(elements, out binding))
                {
                    SetBinding(endpointContext.Endpoint, binding);
                }
                else if (transport is NamedPipeTransportBindingElement && NetNamedPipeBinding.TryCreate(elements, out binding))
                {
                    SetBinding(endpointContext.Endpoint, binding);
                }
#pragma warning disable 0618				
                else if (transport is PeerTransportBindingElement && NetPeerTcpBinding.TryCreate(elements, out binding))
                {
                    SetBinding(endpointContext.Endpoint, binding);
                }
#pragma warning restore 0618				
                else if (transport is TcpTransportBindingElement && NetTcpBinding.TryCreate(elements, out binding))
                {
                    SetBinding(endpointContext.Endpoint, binding);
                }
            }
        }
        void SetBinding(ServiceEndpoint endpoint, Binding binding)
        {
            binding.Name = endpoint.Binding.Name;
            binding.Namespace = endpoint.Binding.Namespace;
            endpoint.Binding = binding;
        }
    }
}
