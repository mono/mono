//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml.Schema;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using WsdlNS = System.Web.Services.Description;

    // implemented by Indigo Transports
    interface ITransportPolicyImport
    {
        void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext);
    }

    public class TransportBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension
    {

        void IWsdlImportExtension.BeforeImport(WsdlNS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas, ICollection<XmlElement> policy)
        {
            WsdlImporter.SoapInPolicyWorkaroundHelper.InsertAdHocTransportPolicy(wsdlDocuments);
        }

        void IWsdlImportExtension.ImportContract(WsdlImporter importer, WsdlContractConversionContext context) { }

        void IWsdlImportExtension.ImportEndpoint(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, these properties cannot be null in this context
            if (context.Endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context.Endpoint.Binding");
            }

#pragma warning suppress 56506 // Microsoft, CustomBinding.Elements never be null
            TransportBindingElement transportBindingElement = GetBindingElements(context).Find<TransportBindingElement>();

            bool transportHandledExternaly = (transportBindingElement != null) && !StateHelper.IsRegisteredTransportBindingElement(importer, context);
            if (transportHandledExternaly)
                return;

#pragma warning suppress 56506 // Microsoft, these properties cannot be null in this context
            WsdlNS.SoapBinding soapBinding = (WsdlNS.SoapBinding)context.WsdlBinding.Extensions.Find(typeof(WsdlNS.SoapBinding));
            if (soapBinding != null && transportBindingElement == null)
            {
                CreateLegacyTransportBindingElement(importer, soapBinding, context);
            }

            // Try to import WS-Addressing address from the port
            if (context.WsdlPort != null)
            {
                ImportAddress(context, transportBindingElement);
            }

        }

        static BindingElementCollection GetBindingElements(WsdlEndpointConversionContext context)
        {
            Binding binding = context.Endpoint.Binding;
            BindingElementCollection elements = binding is CustomBinding ? ((CustomBinding)binding).Elements : binding.CreateBindingElements();
            return elements;
        }

        static CustomBinding ConvertToCustomBinding(WsdlEndpointConversionContext context)
        {
            CustomBinding customBinding = context.Endpoint.Binding as CustomBinding;
            if (customBinding == null)
            {
                customBinding = new CustomBinding(context.Endpoint.Binding);
                context.Endpoint.Binding = customBinding;
            }
            return customBinding;
        }

        static void ImportAddress(WsdlEndpointConversionContext context, TransportBindingElement transportBindingElement)
        {
            EndpointAddress address = context.Endpoint.Address = WsdlImporter.WSAddressingHelper.ImportAddress(context.WsdlPort);
            if (address != null)
            {
                context.Endpoint.Address = address;

                // Replace the http BE with https BE only if the uri scheme is https and the transport binding element is a HttpTransportBindingElement but not HttpsTransportBindingElement
                if (address.Uri.Scheme == Uri.UriSchemeHttps && transportBindingElement is HttpTransportBindingElement && !(transportBindingElement is HttpsTransportBindingElement))
                {
                    BindingElementCollection elements = ConvertToCustomBinding(context).Elements;
                    elements.Remove(transportBindingElement);
                    elements.Add(CreateHttpsFromHttp(transportBindingElement as HttpTransportBindingElement));
                }
            }
        }

        static void CreateLegacyTransportBindingElement(WsdlImporter importer, WsdlNS.SoapBinding soapBinding, WsdlEndpointConversionContext context)
        {
            // We create a transportBindingElement based on the SoapBinding's Transport
            TransportBindingElement transportBindingElement = CreateTransportBindingElements(soapBinding.Transport, null);
            if (transportBindingElement != null)
            {
                ConvertToCustomBinding(context).Elements.Add(transportBindingElement);
                StateHelper.RegisterTransportBindingElement(importer, context);
            }
        }

        static HttpsTransportBindingElement CreateHttpsFromHttp(HttpTransportBindingElement http)
        {
            if (http == null) return new HttpsTransportBindingElement();

            HttpsTransportBindingElement https = HttpsTransportBindingElement.CreateFromHttpBindingElement(http);

            return https;
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlQualifiedName wsdlBindingQName;
            string transportUri = WsdlImporter.SoapInPolicyWorkaroundHelper.FindAdHocTransportPolicy(policyContext, out wsdlBindingQName);

            if (transportUri != null && !policyContext.BindingElements.Contains(typeof(TransportBindingElement)))
            {
                TransportBindingElement transportBindingElement = CreateTransportBindingElements(transportUri, policyContext);

                if (transportBindingElement != null)
                {
                    ITransportPolicyImport transportPolicyImport = transportBindingElement as ITransportPolicyImport;
                    if (transportPolicyImport != null)
                        transportPolicyImport.ImportPolicy(importer, policyContext);

                    policyContext.BindingElements.Add(transportBindingElement);
                    StateHelper.RegisterTransportBindingElement(importer, wsdlBindingQName);
                }
            }
        }

        static TransportBindingElement CreateTransportBindingElements(string transportUri, PolicyConversionContext policyContext)
        {
            TransportBindingElement transportBindingElement = null;
            // Try and Create TransportBindingElement
            switch (transportUri)
            {
                case TransportPolicyConstants.HttpTransportUri:
                    transportBindingElement = GetHttpTransportBindingElement(policyContext);
                    break;
                case TransportPolicyConstants.TcpTransportUri:
                    transportBindingElement = new TcpTransportBindingElement();
                    break;
                case TransportPolicyConstants.NamedPipeTransportUri:
                    transportBindingElement = new NamedPipeTransportBindingElement();
                    break;
                case TransportPolicyConstants.MsmqTransportUri:
                    transportBindingElement = new MsmqTransportBindingElement();
                    break;
                case TransportPolicyConstants.PeerTransportUri:
#pragma warning disable 0618
                    transportBindingElement = new PeerTransportBindingElement();
#pragma warning restore 0618					
                    break;
                case TransportPolicyConstants.WebSocketTransportUri:
                    HttpTransportBindingElement httpTransport = GetHttpTransportBindingElement(policyContext);
                    httpTransport.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
                    httpTransport.WebSocketSettings.SubProtocol = WebSocketTransportSettings.SoapSubProtocol;
                    transportBindingElement = httpTransport;
                    break;
                default:
                    // There may be another registered converter that can handle this transport.
                    break;
            }

            return transportBindingElement;
        }

        static HttpTransportBindingElement GetHttpTransportBindingElement(PolicyConversionContext policyContext)
        {
            if (policyContext != null)
            {
                WSSecurityPolicy sp = null;
                ICollection<XmlElement> policyCollection = policyContext.GetBindingAssertions();
                if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyCollection, out sp) && sp.ContainsWsspHttpsTokenAssertion(policyCollection))
                {
                    HttpsTransportBindingElement httpsBinding = new HttpsTransportBindingElement();
                    httpsBinding.MessageSecurityVersion = sp.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity11);
                    return httpsBinding;
                }
            }

            return new HttpTransportBindingElement();
        }
    }

    internal static class StateHelper
    {
        readonly static object StateBagKey = new object();

        static Dictionary<XmlQualifiedName, XmlQualifiedName> GetGeneratedTransportBindingElements(MetadataImporter importer)
        {
            object retValue;
            if (!importer.State.TryGetValue(StateHelper.StateBagKey, out retValue))
            {
                retValue = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
                importer.State.Add(StateHelper.StateBagKey, retValue);
            }
            return (Dictionary<XmlQualifiedName, XmlQualifiedName>)retValue;
        }

        internal static void RegisterTransportBindingElement(MetadataImporter importer, XmlQualifiedName wsdlBindingQName)
        {
            GetGeneratedTransportBindingElements(importer)[wsdlBindingQName] = wsdlBindingQName;
        }

        internal static void RegisterTransportBindingElement(MetadataImporter importer, WsdlEndpointConversionContext context)
        {
            XmlQualifiedName wsdlBindingQName = new XmlQualifiedName(context.WsdlBinding.Name, context.WsdlBinding.ServiceDescription.TargetNamespace);
            GetGeneratedTransportBindingElements(importer)[wsdlBindingQName] = wsdlBindingQName;
        }

        internal static bool IsRegisteredTransportBindingElement(WsdlImporter importer, WsdlEndpointConversionContext context)
        {
            XmlQualifiedName key = new XmlQualifiedName(context.WsdlBinding.Name, context.WsdlBinding.ServiceDescription.TargetNamespace);
            return GetGeneratedTransportBindingElements(importer).ContainsKey(key);
        }
    }


    static class TransportPolicyConstants
    {
        public const string BasicHttpAuthenticationName = "BasicAuthentication";
        public const string CompositeDuplex = "CompositeDuplex";
        public const string CompositeDuplexNamespace = "http://schemas.microsoft.com/net/2006/06/duplex";
        public const string CompositeDuplexPrefix = "cdp";
        public const string DigestHttpAuthenticationName = "DigestAuthentication";
        public const string DotNetFramingNamespace = FramingEncodingString.NamespaceUri + "/policy";
        public const string DotNetFramingPrefix = "msf";
        public const string HttpTransportNamespace = "http://schemas.microsoft.com/ws/06/2004/policy/http";
        public const string HttpTransportPrefix = "http";
        public const string HttpTransportUri = "http://schemas.xmlsoap.org/soap/http";
        public const string MsmqBestEffort = "MsmqBestEffort";
        public const string MsmqSession = "MsmqSession";
        public const string MsmqTransportNamespace = "http://schemas.microsoft.com/ws/06/2004/mspolicy/msmq";
        public const string MsmqTransportPrefix = "msmq";
        public const string MsmqTransportUri = "http://schemas.microsoft.com/soap/msmq";
        public const string MsmqVolatile = "MsmqVolatile";
        public const string MsmqAuthenticated = "Authenticated";
        public const string MsmqWindowsDomain = "WindowsDomain";
        public const string NamedPipeTransportUri = "http://schemas.microsoft.com/soap/named-pipe";
        public const string NegotiateHttpAuthenticationName = "NegotiateAuthentication";
        public const string NtlmHttpAuthenticationName = "NtlmAuthentication";
        public const string PeerTransportUri = "http://schemas.microsoft.com/soap/peer";
        public const string ProtectionLevelName = "ProtectionLevel";
        public const string RequireClientCertificateName = "RequireClientCertificate";
        public const string SslTransportSecurityName = "SslTransportSecurity";
        public const string StreamedName = "Streamed";
        public const string TcpTransportUri = "http://schemas.microsoft.com/soap/tcp";
        public const string WebSocketPolicyPrefix = "mswsp";
        public const string WebSocketPolicyNamespace = "http://schemas.microsoft.com/soap/websocket/policy";
        public const string WebSocketTransportUri = "http://schemas.microsoft.com/soap/websocket";
        public const string WebSocketEnabled = "WebSocketEnabled";
        public const string WindowsTransportSecurityName = "WindowsTransportSecurity";
    }
}
