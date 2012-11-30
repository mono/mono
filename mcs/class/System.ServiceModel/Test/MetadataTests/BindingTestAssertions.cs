//
// Test.cs
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

using System;
using System.Net;
using System.Net.Security;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using WS = System.Web.Services.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public static class BindingTestAssertions {

		const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";

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
			Assert.That (extension, Is.InstanceOfType (typeof (WS.SoapBinding)), label.Get ());
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
				Assert.That (binding, Is.InstanceOfType (typeof(CustomBinding)), label.Get ());
			} else {
				Assert.That (binding, Is.InstanceOfType (typeof(BasicHttpBinding)), label.Get ());
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
				Assert.That (textElement.WriteEncoding, Is.InstanceOfType (typeof(UTF8Encoding)), label.Get ());
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
			else if (context.CheckPolicyXml) {
				Assert.That (xml, Is.Not.Null, label.Get ());
				
				Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
				Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference") | Is.EqualTo ("Policy"), label.Get ());
			}
			label.LeaveScope ();

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

			CheckEndpoint (endpoints [0], MetadataSamples.HttpUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
		}

		public static void BasicHttpsBinding (
			TestContext context, MetadataSet doc, BasicHttpSecurityMode security,
			WSMessageEncoding encoding, HttpClientCredentialType clientCred,
			AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("basicHttpsBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");

			Assert.That (sd.Extensions, Is.Not.Null, label.Get ());
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof(XmlElement)), label.Get ());

			label.EnterScope ("extensions");
			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());
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

			if (context.CheckPolicyXml) {
				label.EnterScope ("policy-xml");
				Assert.That (xml, Is.Not.Null, label.Get ());
				Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
				Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference") | Is.EqualTo ("Policy"), label.Get ());
				label.LeaveScope ();
			}

			label.LeaveScope (); // wsdl

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			CheckBasicHttpBinding (
				bindings [0], "https", security, encoding,
				clientCred, authScheme, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], MetadataSamples.HttpsUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
		}

		public static void CheckNetTcpBinding (
			Binding binding, SecurityMode security, bool reliableSession,
			TransferMode transferMode, TestLabel label)
		{
			label.EnterScope ("net-tcp");
			if (security == SecurityMode.Message) {
				Assert.That (binding, Is.InstanceOfType (typeof(CustomBinding)), label.Get ());
			} else {
				Assert.That (binding, Is.InstanceOfType (typeof(NetTcpBinding)), label.Get ());
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

			label.EnterScope ("extensions");
			Assert.That (sd.Extensions, Is.Not.Null, label.Get ());
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof(XmlElement)), label.Get ());
			
			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());

			WS.SoapBinding soap = null;
			XmlElement xml = null;
			
			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}
			
			CheckSoapBinding (soap, "http://schemas.microsoft.com/soap/tcp", label);

			if (context.CheckPolicyXml) {
				label.EnterScope ("policy-xml");
				Assert.That (xml, Is.Not.Null, label.Get ());
			
				Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
				Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference") | Is.EqualTo ("Policy"), label.Get ());
				label.LeaveScope ();
			}

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

	}
}
