//
// BindingTestAssertions.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Net;
using System.Net.Security;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;

using QName = System.Xml.XmlQualifiedName;
using WS = System.Web.Services.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public static class BindingTestAssertions {

		const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";
		const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
		const string MsbNamespace = "http://schemas.microsoft.com/ws/06/2004/mspolicy/netbinary1";
		const string WsawNamespace = "http://www.w3.org/2006/05/addressing/wsdl";
		const string MsfNamespace = "http://schemas.microsoft.com/ws/2006/05/framing/policy";
		const string SpNamespace = "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";
		const string WsrmNamespace = "http://schemas.xmlsoap.org/ws/2005/02/rm/policy";
		const string HttpNamespace = "http://schemas.microsoft.com/ws/06/2004/policy/http";
		const string WsomaNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization";
		const string Wsa10Namespace = "http://www.w3.org/2005/08/addressing";

		static readonly QName BinaryEncodingQName = new QName ("BinaryEncoding", MsbNamespace);
		static readonly QName UsingAddressingQName = new QName ("UsingAddressing", WsawNamespace);
		static readonly QName StreamedTransferQName = new QName ("Streamed", MsfNamespace);
		static readonly QName ReliableSessionQName = new QName ("RMAssertion", WsrmNamespace);
		static readonly QName TransportBindingQName = new QName ("TransportBinding", SpNamespace);
		static readonly QName AsymmetricBindingQName = new QName ("AsymmetricBinding", SpNamespace);
		static readonly QName SymmetricBindingQName = new QName ("SymmetricBinding", SpNamespace);
		static readonly QName EndorsingSupportingQName = new QName ("EndorsingSupportingTokens", SpNamespace);
		static readonly QName SignedSupportingQName = new QName ("SignedSupportingTokens", SpNamespace);
		static readonly QName Wss10QName = new QName ("Wss10", SpNamespace);
		static readonly QName Wss11QName = new QName ("Wss11", SpNamespace);
		static readonly QName Trust10QName = new QName ("Trust10", SpNamespace);
		static readonly QName NtlmAuthenticationQName = new QName ("NtlmAuthentication", HttpNamespace);
		static readonly QName MtomEncodingQName = new QName ("OptimizedMimeSerialization", WsomaNamespace);

		public static void CheckImportErrors (WsdlImporter importer, TestLabel label)
		{
			bool foundErrors = false;
			foreach (var error in importer.Errors) {
				if (error.IsWarning)
					Console.WriteLine ("WARNING ({0}): {1}", label, error.Message);
				else {
					Console.WriteLine ("ERROR ({0}): {1}", label, error.Message);
					foundErrors = true;
				}
			}

			if (foundErrors)
				Assert.Fail ("Found import errors", label);
		}

		static void CheckSoapBinding (object extension, string transport, TestLabel label)
		{
			label.EnterScope ("soap");
			Assert.That (extension, Is.AssignableTo<WS.SoapBinding>(), label.Get ());
			var soap = (WS.SoapBinding)extension;
			Assert.That (soap.Style, Is.EqualTo (WS.SoapBindingStyle.Document), label.Get ());
			Assert.That (soap.Transport, Is.EqualTo (transport), label.Get ());
			Assert.That (soap.Required, Is.False, label.Get ());
			label.LeaveScope ();
		}

		public static void CheckBasicHttpBinding (
			Binding binding, string scheme, BasicHttpSecurityMode security,
			WSMessageEncoding encoding, HttpClientCredentialType clientCred,
			AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("http");

			if (security == BasicHttpSecurityMode.Message) {
				Assert.IsInstanceOfType (typeof(CustomBinding), binding, label.Get ());
			} else {
				Assert.IsInstanceOfType (typeof(BasicHttpBinding), binding, label.Get ());
				var basicHttp = (BasicHttpBinding)binding;
				Assert.That (basicHttp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap11), label.Get ());
				Assert.That (basicHttp.MessageVersion, Is.EqualTo (MessageVersion.Soap11), label.Get ());
				Assert.That (basicHttp.Scheme, Is.EqualTo (scheme), label.Get ());
				Assert.That (basicHttp.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());
				Assert.That (basicHttp.MessageEncoding, Is.EqualTo (encoding), label.Get ());
				Assert.That (basicHttp.Security, Is.Not.Null, label.Get ());
				Assert.That (basicHttp.Security.Mode, Is.EqualTo (security), label.Get ());
				Assert.That (basicHttp.Security.Transport.ClientCredentialType, Is.EqualTo (clientCred), label.Get ());
				Assert.That (basicHttp.Security.Message.AlgorithmSuite, Is.EqualTo (SecurityAlgorithmSuite.Basic256), label.Get ());
			}

			label.EnterScope ("elements");

			var elements = binding.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());
			if ((security == BasicHttpSecurityMode.Message) ||
				(security == BasicHttpSecurityMode.TransportWithMessageCredential))
				Assert.That (elements.Count, Is.EqualTo (3), label.Get ());
			else
				Assert.That (elements.Count, Is.EqualTo (2), label.Get ());
			
			TextMessageEncodingBindingElement textElement = null;
			TransportSecurityBindingElement securityElement = null;
			HttpTransportBindingElement transportElement = null;
			AsymmetricSecurityBindingElement asymmSecurityElement = null;
			MtomMessageEncodingBindingElement mtomElement = null;
			
			foreach (var element in elements) {
				if (element is TextMessageEncodingBindingElement)
					textElement = (TextMessageEncodingBindingElement)element;
				else if (element is HttpTransportBindingElement)
					transportElement = (HttpTransportBindingElement)element;
				else if (element is TransportSecurityBindingElement)
					securityElement = (TransportSecurityBindingElement)element;
				else if (element is AsymmetricSecurityBindingElement)
					asymmSecurityElement = (AsymmetricSecurityBindingElement)element;
				else if (element is MtomMessageEncodingBindingElement)
					mtomElement = (MtomMessageEncodingBindingElement)element;
				else
					Assert.Fail (string.Format (
						"Unknown element: {0}", element.GetType ()), label.Get ());
			}

			label.EnterScope ("text");
			if (encoding == WSMessageEncoding.Text) {
				Assert.That (textElement, Is.Not.Null, label.Get ());
				Assert.IsInstanceOfType (typeof(UTF8Encoding), textElement.WriteEncoding, label.Get ());
			} else {
				Assert.That (textElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("mtom");
			if (encoding == WSMessageEncoding.Mtom) {
				Assert.That (mtomElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (mtomElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("security");
			if (security == BasicHttpSecurityMode.TransportWithMessageCredential) {
				Assert.That (securityElement, Is.Not.Null, label.Get ());
				Assert.That (securityElement.SecurityHeaderLayout,
				             Is.EqualTo (SecurityHeaderLayout.Lax), label.Get ());
			} else {
				Assert.That (securityElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("asymmetric");
			if (security == BasicHttpSecurityMode.Message) {
				Assert.That (asymmSecurityElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (asymmSecurityElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());
			
			Assert.That (transportElement.Realm, Is.Empty, label.Get ());
			Assert.That (transportElement.Scheme, Is.EqualTo (scheme), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());

			label.EnterScope ("auth");
			Assert.That (transportElement.AuthenticationScheme, Is.EqualTo (authScheme), label.Get ());
			label.LeaveScope (); // auth
			label.LeaveScope (); // transport
			label.LeaveScope (); // elements
			label.LeaveScope (); // http
		}

		static void CheckEndpoint (ServiceEndpoint endpoint, string uri, TestLabel label)
		{
			label.EnterScope ("endpoint");
			Assert.That (endpoint.ListenUri, Is.EqualTo (new Uri (uri)), label.Get ());
			Assert.That (endpoint.ListenUriMode, Is.EqualTo (ListenUriMode.Explicit), label.Get ());
			Assert.That (endpoint.Contract, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Contract.Name, Is.EqualTo ("MyContract"), label.Get ());
			Assert.That (endpoint.Address, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Address.Uri, Is.EqualTo (new Uri (uri)), label.Get ());
			Assert.That (endpoint.Address.Identity, Is.Null, label.Get ());
			Assert.That (endpoint.Address.Headers, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Address.Headers.Count, Is.EqualTo (0), label.Get ());
			label.LeaveScope ();
		}

		public static void BasicHttpBinding (
			TestContext context, MetadataSet doc, WSMessageEncoding encoding, TestLabel label)
		{
			BasicHttpBinding (
				context, doc, BasicHttpSecurityMode.None, encoding,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}

		public static void BasicHttpBinding (
			TestContext context, MetadataSet doc, BasicHttpSecurityMode security, TestLabel label)
		{
			BasicHttpBinding (
				context, doc, security, WSMessageEncoding.Text,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}
		
		public static void BasicHttpBinding (
			TestContext context, MetadataSet doc, BasicHttpSecurityMode security,
			WSMessageEncoding encoding, HttpClientCredentialType clientCred,
			AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("basicHttpBinding");
			BasicHttpBinding_inner (
				context, doc, security, encoding, clientCred,
				authScheme, false, label);
			label.LeaveScope ();
		}

		public static void BasicHttpsBinding (
			TestContext context, MetadataSet doc, BasicHttpSecurityMode security,
			WSMessageEncoding encoding, HttpClientCredentialType clientCred,
			AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("basicHttpsBinding");
			BasicHttpBinding_inner (
				context, doc, security, encoding, clientCred,
				authScheme, true, label);
			label.LeaveScope ();
		}
		
		static void BasicHttpBinding_inner (
			TestContext context, MetadataSet doc, BasicHttpSecurityMode security,
			WSMessageEncoding encoding, HttpClientCredentialType clientCred,
			AuthenticationSchemes authScheme, bool isHttps, TestLabel label)
		{
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");
			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());

			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());

			bool hasPolicyXml;

			switch (security) {
			case BasicHttpSecurityMode.None:
				if (isHttps)
					throw new InvalidOperationException ();
				hasPolicyXml = encoding == WSMessageEncoding.Mtom;
				break;
			case BasicHttpSecurityMode.Message:
			case BasicHttpSecurityMode.Transport:
			case BasicHttpSecurityMode.TransportWithMessageCredential:
				if (encoding == WSMessageEncoding.Mtom)
					throw new InvalidOperationException ();
				hasPolicyXml = true;
				break;
			case BasicHttpSecurityMode.TransportCredentialOnly:
				if (isHttps)
					throw new InvalidOperationException ();
				hasPolicyXml = true;
				break;
			default:
				throw new InvalidOperationException ();
			}
			label.LeaveScope ();

			WS.SoapBinding soap = null;
			XmlElement xml = null;

			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}

			CheckSoapBinding (soap, WS.SoapBinding.HttpTransport, label);
			label.LeaveScope ();

			label.EnterScope ("policy-xml");
			if (!hasPolicyXml)
				Assert.That (xml, Is.Null, label.Get ());
			else {
				Assert.That (xml, Is.Not.Null, label.Get ());
				var assertions = AssertPolicy (sd, xml, label);
				Assert.That (assertions, Is.Not.Null, label.Get ());
				if (clientCred == HttpClientCredentialType.Ntlm)
					AssertPolicy (assertions, NtlmAuthenticationQName, label);
				if (encoding == WSMessageEncoding.Mtom)
					AssertPolicy (assertions, MtomEncodingQName, label);
				switch (security) {
				case BasicHttpSecurityMode.Message:
					AssertPolicy (assertions, AsymmetricBindingQName, label);
					AssertPolicy (assertions, Wss10QName, label);
					break;
				case BasicHttpSecurityMode.Transport:
					AssertPolicy (assertions, TransportBindingQName, label);
					break;
				case BasicHttpSecurityMode.TransportWithMessageCredential:
					AssertPolicy (assertions, SignedSupportingQName, label);
					AssertPolicy (assertions, TransportBindingQName, label);
					AssertPolicy (assertions, Wss10QName, label);
					break;
				default:
					break;
				}
				Assert.That (assertions.Count, Is.EqualTo (0), label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("services");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			var port = service.Ports [0];
			
			label.EnterScope ("port");
			Assert.That (port.Extensions, Is.Not.Null, label.Get ());
			Assert.That (port.Extensions.Count, Is.EqualTo (1), label.Get ());
			
			WS.SoapAddressBinding soap_addr_binding = null;
			foreach (var extension in port.Extensions) {
				if (extension is WS.SoapAddressBinding)
					soap_addr_binding = (WS.SoapAddressBinding)extension;
				else
					Assert.Fail (label.Get ());
			}
			Assert.That (soap_addr_binding, Is.Not.Null, label.Get ());
			label.LeaveScope ();

			label.LeaveScope (); // wsdl

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);

			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			string scheme;
			if ((security == BasicHttpSecurityMode.Transport) ||
			    (security == BasicHttpSecurityMode.TransportWithMessageCredential))
				scheme = "https";
			else
				scheme = "http";

			CheckBasicHttpBinding (
				bindings [0], scheme, security, encoding, clientCred,
				authScheme, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);

			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());

			var uri = isHttps ? MetadataSamples.HttpsUri : MetadataSamples.HttpUri;

			CheckEndpoint (endpoints [0], uri, label);
			label.LeaveScope ();
		}

		public static void CheckNetTcpBinding (
			Binding binding, SecurityMode security, bool reliableSession,
			TransferMode transferMode, TestLabel label)
		{
			label.EnterScope ("net-tcp");
			if (security == SecurityMode.Message) {
				Assert.IsInstanceOfType (typeof(CustomBinding), binding, label.Get ());
			} else {
				Assert.IsInstanceOfType (typeof(NetTcpBinding), binding, label.Get ());
				var netTcp = (NetTcpBinding)binding;
				Assert.That (netTcp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap12), label.Get ());
				Assert.That (netTcp.MessageVersion, Is.EqualTo (MessageVersion.Soap12WSAddressing10), label.Get ());
				Assert.That (netTcp.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
				Assert.That (netTcp.TransferMode, Is.EqualTo (transferMode), label.Get ());

				label.EnterScope ("security");
				Assert.That (netTcp.Security, Is.Not.Null, label.Get ());
				Assert.That (netTcp.Security.Mode, Is.EqualTo (security), label.Get ());

				Assert.That (netTcp.Security.Transport, Is.Not.Null, label.Get ());
				Assert.That (netTcp.Security.Transport.ProtectionLevel, Is.EqualTo (ProtectionLevel.EncryptAndSign), label.Get ());
				Assert.That (netTcp.Security.Transport.ClientCredentialType, Is.EqualTo (TcpClientCredentialType.Windows), label.Get ());
				label.LeaveScope ();
			}

			label.EnterScope ("elements");
			
			var elements = binding.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());

			TcpTransportBindingElement transportElement = null;
			TransactionFlowBindingElement transactionFlowElement = null;
			BinaryMessageEncodingBindingElement encodingElement = null;
			WindowsStreamSecurityBindingElement windowsStreamElement = null;
			ReliableSessionBindingElement reliableSessionElement = null;
			TransportSecurityBindingElement transportSecurityElement = null;
			SslStreamSecurityBindingElement sslStreamElement = null;
			SymmetricSecurityBindingElement symmSecurityElement = null;
			
			foreach (var element in elements) {
				if (element is TcpTransportBindingElement)
					transportElement = (TcpTransportBindingElement)element;
				else if (element is TransactionFlowBindingElement)
					transactionFlowElement = (TransactionFlowBindingElement)element;
				else if (element is BinaryMessageEncodingBindingElement)
					encodingElement = (BinaryMessageEncodingBindingElement)element;
				else if (element is WindowsStreamSecurityBindingElement)
					windowsStreamElement = (WindowsStreamSecurityBindingElement)element;
				else if (element is ReliableSessionBindingElement)
					reliableSessionElement = (ReliableSessionBindingElement)element;
				else if (element is TransportSecurityBindingElement)
					transportSecurityElement = (TransportSecurityBindingElement)element;
				else if (element is SslStreamSecurityBindingElement)
					sslStreamElement = (SslStreamSecurityBindingElement)element;
				else if (element is SymmetricSecurityBindingElement)
					symmSecurityElement = (SymmetricSecurityBindingElement)element;
				else
					Assert.Fail (string.Format (
						"Unknown element `{0}'.", element.GetType ()), label.Get ());
			}

			label.EnterScope ("windows-stream");
			if (security == SecurityMode.Transport) {
				Assert.That (windowsStreamElement, Is.Not.Null, label.Get ());
				Assert.That (windowsStreamElement.ProtectionLevel, Is.EqualTo (ProtectionLevel.EncryptAndSign), label.Get ());
			} else {
				Assert.That (windowsStreamElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("reliable-session");
			if (reliableSession) {
				Assert.That (reliableSessionElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (reliableSessionElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("encoding");
			Assert.That (encodingElement, Is.Not.Null, label.Get ());
			label.LeaveScope ();

			label.EnterScope ("transaction");
			if (security == SecurityMode.Message) {
				Assert.That (transactionFlowElement, Is.Null, label.Get ());
			} else {
				Assert.That (transactionFlowElement, Is.Not.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());

			Assert.That (transportElement.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (transferMode), label.Get ());
			label.LeaveScope (); // transport

			label.EnterScope ("security");
			switch (security) {
			case SecurityMode.None:
			case SecurityMode.Transport:
				Assert.That (transportSecurityElement, Is.Null, label.Get ());
				Assert.That (sslStreamElement, Is.Null, label.Get ());
				Assert.That (symmSecurityElement, Is.Null, label.Get ());
				break;
			case SecurityMode.TransportWithMessageCredential:
				Assert.That (transportSecurityElement, Is.Not.Null, label.Get ());
				Assert.That (sslStreamElement, Is.Not.Null, label.Get ());
				Assert.That (symmSecurityElement, Is.Null, label.Get ());
				break;
			case SecurityMode.Message:
				Assert.That (transportSecurityElement, Is.Null, label.Get ());
				Assert.That (sslStreamElement, Is.Null, label.Get ());
				Assert.That (symmSecurityElement, Is.Not.Null, label.Get ());
				break;
			default:
				throw new InvalidOperationException ();
			}
			label.LeaveScope ();

			label.LeaveScope (); // elements
			label.LeaveScope (); // net-tcp
		}

		public static void NetTcpBinding (
			TestContext context, MetadataSet doc, SecurityMode security,
			bool reliableSession, TransferMode transferMode, TestLabel label)
		{
			label.EnterScope ("netTcpBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");

			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());

			WS.Soap12Binding soap = null;
			XmlElement xml = null;
			
			foreach (var ext in binding.Extensions) {
				if (ext is WS.Soap12Binding)
					soap = (WS.Soap12Binding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}
			
			CheckSoapBinding (soap, "http://schemas.microsoft.com/soap/tcp", label);

			label.EnterScope ("policy-xml");
			Assert.That (xml, Is.Not.Null, label.Get ());
			var assertions = AssertPolicy (sd, xml, label);
			Assert.That (assertions, Is.Not.Null, label.Get ());
			AssertPolicy (assertions, BinaryEncodingQName, label);
			AssertPolicy (assertions, UsingAddressingQName, label);
			if (transferMode == TransferMode.Streamed)
				AssertPolicy (assertions, StreamedTransferQName, label);
			switch (security) {
			case SecurityMode.Message:
				AssertPolicy (assertions, SymmetricBindingQName, label);
				AssertPolicy (assertions, Wss11QName, label);
				AssertPolicy (assertions, Trust10QName, label);
				break;
			case SecurityMode.Transport:
				AssertPolicy (assertions, TransportBindingQName, label);
				break;
			case SecurityMode.TransportWithMessageCredential:
				AssertPolicy (assertions, TransportBindingQName, label);
				AssertPolicy (assertions, EndorsingSupportingQName, label);
				AssertPolicy (assertions, Wss11QName, label);
				AssertPolicy (assertions, Trust10QName, label);
				break;
			default:
				break;
			}
			if (reliableSession)
				AssertPolicy (assertions, ReliableSessionQName, label);
			Assert.That (assertions.Count, Is.EqualTo (0), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("services");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			var port = service.Ports [0];

			label.EnterScope ("port");
			Assert.That (port.Extensions, Is.Not.Null, label.Get ());
			Assert.That (port.Extensions.Count, Is.EqualTo (2), label.Get ());

			WS.Soap12AddressBinding soap_addr_binding = null;
			XmlElement port_xml = null;
			foreach (var extension in port.Extensions) {
				if (extension is WS.Soap12AddressBinding)
					soap_addr_binding = (WS.Soap12AddressBinding)extension;
				else if (extension is XmlElement)
					port_xml = (XmlElement)extension;
				else
					Assert.Fail (label.Get ());
			}
			Assert.That (soap_addr_binding, Is.Not.Null, label.Get ());
			Assert.That (port_xml, Is.Not.Null, label.Get ());
			Assert.That (port_xml.NamespaceURI, Is.EqualTo (Wsa10Namespace), label.Get ());
			Assert.That (port_xml.LocalName, Is.EqualTo ("EndpointReference"), label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();

			label.LeaveScope (); // wsdl

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());
			
			CheckNetTcpBinding (
				bindings [0], security, reliableSession,
				transferMode, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], MetadataSamples.NetTcpUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
		}

		public static void Dump (PolicyAssertionCollection assertions)
		{
			foreach (var assertion in assertions)
				Console.WriteLine ("ASSERTION: {0}", assertion.OuterXml);
		}

		public static void AssertPolicy (
			PolicyAssertionCollection assertions, QName qname, TestLabel label)
		{
			var assertion = assertions.Find (qname.Name, qname.Namespace);
			label.EnterScope (qname.Name);
			Assert.That (assertion, Is.Not.Null, label.ToString ());
			assertions.Remove (assertion);
			label.LeaveScope ();
		}

		static XmlElement ResolvePolicy (WS.ServiceDescription sd, XmlElement policy)
		{
			if (policy.LocalName.Equals ("Policy"))
				return policy;

			var uri = policy.GetAttribute ("URI");
			if (!uri.StartsWith ("#"))
				return null;
			
			foreach (var sext in sd.Extensions) {
				var sxml = sext as XmlElement;
				if (sxml == null)
					continue;
				if (!sxml.NamespaceURI.Equals (WspNamespace))
					continue;
				if (!sxml.LocalName.Equals ("Policy"))
					continue;
				var id = sxml.GetAttribute ("Id", WsuNamespace);
				if (uri.Substring (1).Equals (id))
					return sxml;
			}

			return null;
		}

		public static PolicyAssertionCollection AssertPolicy (
			WS.Binding binding, TestLabel label)
		{
			label.EnterScope ("FindPolicy");
			XmlElement policy = null;
		
			foreach (var extension in binding.Extensions) {
				var xml = extension as XmlElement;
				if (xml == null)
					continue;
				Assert.That (policy, Is.Null, label.Get ());
				policy = xml;
			}
			Assert.That (policy, Is.Not.Null, label.Get ());
			try {
				return AssertPolicy (binding.ServiceDescription, policy, label);
			} finally {
				label.LeaveScope ();
			}
		}

		static XmlElement AssertExactlyOneChildElement (XmlElement element)
		{
			XmlElement found = null;
			foreach (var node in element.ChildNodes) {
				if (node is XmlWhitespace)
					continue;
				var e = node as XmlElement;
				if (e == null)
					return null;
				if (found != null)
					return null;
				found = e;
			}

			return found;
		}
		
		public static PolicyAssertionCollection AssertPolicy (
			WS.ServiceDescription sd, XmlElement element, TestLabel label)
		{
			label.EnterScope ("wsp:Policy");
			Assert.That (element.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (element.LocalName, Is.EqualTo ("Policy") | Is.EqualTo ("PolicyReference"), label.Get ());

			var policy = ResolvePolicy (sd, element);
			Assert.That (policy, Is.Not.Null, label.Get ());

			label.EnterScope ("wsp:ExactlyOne");
			var exactlyOne = AssertExactlyOneChildElement (policy);
			Assert.That (exactlyOne, Is.Not.Null, label.Get ());
			Assert.That (exactlyOne.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (exactlyOne.LocalName, Is.EqualTo ("ExactlyOne"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("wsp:Any");
			var all = AssertExactlyOneChildElement (exactlyOne);
			Assert.That (all, Is.Not.Null, label.Get ());
			Assert.That (all.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (all.LocalName, Is.EqualTo ("All"), label.Get ());
			label.LeaveScope ();

			var collection = new PolicyAssertionCollection ();

			label.EnterScope ("assertions");
			foreach (var node in all.ChildNodes) {
				if (node is XmlWhitespace)
					continue;
				Assert.IsInstanceOfType (typeof (XmlElement), node, label.ToString ());
				collection.Add ((XmlElement)node);
			}
			label.LeaveScope ();

			label.LeaveScope ();

			return collection;
		}

		public static void TestOperation (MetadataSet metadata, bool soap12, TestLabel label)
		{
			label.EnterScope ("TestOperation");

			label.EnterScope ("metadata");
			WS.ServiceDescription sd = null;
			foreach (var ms in metadata.MetadataSections) {
				if (!ms.Dialect.Equals ("http://schemas.xmlsoap.org/wsdl/"))
					continue;
				sd = ms.Metadata as WS.ServiceDescription;
			}
			Assert.That (sd, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			label.LeaveScope ();

			label.EnterScope ("operation");
			Assert.That (binding.Operations, Is.Not.Null, label.Get ());
			Assert.That (binding.Operations.Count, Is.EqualTo (1), label.Get ());
			var op = binding.Operations [0];

			Assert.That (op.Name, Is.EqualTo ("Hello"), label.Get ());
			Assert.That (op.ExtensibleAttributes, Is.Null, label.Get ());

			label.EnterScope ("extensions");
			Assert.That (op.Extensions, Is.Not.Null, label.Get ());
			Assert.That (op.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (op.Extensions [0], Is.AssignableTo<WS.SoapOperationBinding>(), label.Get ());
			var soap = (WS.SoapOperationBinding)op.Extensions [0];
			TestSoap (soap, soap12, label);
			label.LeaveScope ();

			TestSoapMessage (op.Input, soap12, label);
			TestSoapMessage (op.Output, soap12, label);
			label.LeaveScope (); // operation

			label.LeaveScope ();
		}

		static void TestSoap (WS.SoapOperationBinding soap, bool soap12, TestLabel label)
		{
			label.EnterScope ("soap");
			var type = soap12 ? typeof (WS.Soap12OperationBinding) : typeof (WS.SoapOperationBinding);
			Assert.That (soap.GetType (), Is.EqualTo (type), label.Get ());
			Assert.That (soap.Style, Is.EqualTo (WS.SoapBindingStyle.Document), label.Get ());
			Assert.That (soap.SoapAction, Is.EqualTo ("http://tempuri.org/IMyContract/Hello"), label.Get ());
			Assert.That (soap.Required, Is.False, label.Get ());
			label.LeaveScope ();
		}

		static void TestSoapMessage (WS.MessageBinding binding, bool soap12, TestLabel label)
		{
			label.EnterScope (binding is WS.InputBinding ? "input" : "output");

			Assert.That (binding, Is.Not.Null, label.Get ());
			Assert.That (binding.Name, Is.Null, label.Get ());
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());
			Assert.That (binding.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (binding.Extensions [0], Is.AssignableTo<WS.SoapBodyBinding> (), label.Get ());
			var body = (WS.SoapBodyBinding)binding.Extensions [0];
			TestSoapBody (body, soap12, label);
			label.LeaveScope ();
		}

		static void TestSoapBody (WS.SoapBodyBinding soap, bool soap12, TestLabel label)
		{
			label.EnterScope ("soap-body");
			var type = soap12 ? typeof (WS.Soap12BodyBinding) : typeof (WS.SoapBodyBinding);
			Assert.That (soap.GetType (), Is.EqualTo (type), label.Get ());
			Assert.That (soap.Encoding, Is.Empty, label.Get ());
			Assert.That (soap.Namespace, Is.Empty, label.Get ());
			Assert.That (soap.Parts, Is.Null, label.Get ());
			Assert.That (soap.Use, Is.EqualTo (WS.SoapBindingUse.Literal), label.Get ());
			label.LeaveScope ();
		}

		public static void AssertConfig (MetadataSet metadata, XmlDocument xml, TestLabel label)
		{
			label.EnterScope ("import");
			var importer = new WsdlImporter (metadata);
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);
			Assert.That (endpoints.Count, Is.AtLeast (1), label.Get ());
			label.LeaveScope ();

			var nav = xml.CreateNavigator ();

			// FIXME: Check endpoints.

			label.EnterScope ("endpoints");
			var endpointIter = nav.Select ("/configuration/system.serviceModel/client/endpoint");
			Assert.That (endpointIter.Count, Is.EqualTo (endpoints.Count), label.Get ());
			
			label.LeaveScope ();
		}
	}
}
#endif

