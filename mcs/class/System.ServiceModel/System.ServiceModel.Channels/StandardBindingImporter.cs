//
// StandardBindingImporter.cs
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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using WS = System.Web.Services.Description;
using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Channels {

	public class StandardBindingImporter : IWsdlImportExtension {
		#region IWsdlImportExtension implementation

		public void BeforeImport (WS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas,
		                          ICollection<XmlElement> policy)
		{
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext contractContext)
		{
		}

		WS.Port LookupPort (WsdlImporter importer, QName name)
		{
			foreach (WS.ServiceDescription doc in importer.WsdlDocuments) {
				foreach (WS.Service service in doc.Services) {
					foreach (WS.Port port in service.Ports) {
						if (!name.Namespace.Equals (port.Binding.Namespace))
							continue;
						if (!name.Name.Equals (port.Binding.Name))
							continue;
						return port;
					}
				}
			}

			return null;
		}

		public void ImportEndpoint (WsdlImporter importer, WsdlEndpointConversionContext context)
		{
			var custom = context.Endpoint.Binding as CustomBinding;
			if (custom == null)
				return;

			var soapHttp = GetHttpSoapBinding (context.WsdlBinding);
			if (soapHttp != null) {
				ImportBasicHttpBinding (importer, context, custom, soapHttp);
				return;
			}

			var soapTcp = GetTcpSoapBinding (context.WsdlBinding);
			if (soapTcp != null) {
				ImportNetTcpBinding (importer, context, custom, soapTcp);
				return;
			}
		}

		internal static WS.SoapBinding GetHttpSoapBinding (WS.Binding binding)
		{
			WS.SoapBinding soap = null;
			foreach (var extension in binding.Extensions) {
				var check = extension as WS.SoapBinding;
				if (check != null) {
					soap = check;
					break;
				}
			}
			
			if (soap == null)
				return null;
			if (soap.Transport != WS.SoapBinding.HttpTransport)
				return null;
			if (soap.Style != WS.SoapBindingStyle.Document)
				return null;
			return soap;
		}

		const string TcpTransport = "http://schemas.microsoft.com/soap/tcp";
		
		internal static WS.Soap12Binding GetTcpSoapBinding (WS.Binding binding)
		{
			WS.Soap12Binding soap = null;
			foreach (var extension in binding.Extensions) {
				var check = extension as WS.Soap12Binding;
				if (check != null) {
					soap = check;
					break;
				}
			}
			
			if (soap == null)
				return null;
			if (soap.Transport != TcpTransport)
				return null;
			if (soap.Style != WS.SoapBindingStyle.Document)
				return null;
			return soap;
		}

		bool ImportBasicHttpBinding (
			WsdlImporter importer, WsdlEndpointConversionContext context,
			CustomBinding custom, WS.SoapBinding soap)
		{
			TransportBindingElement transportElement = null;
			MtomMessageEncodingBindingElement mtomElement = null;
			TextMessageEncodingBindingElement textElement = null;
			bool foundUnknownElement = false;

			foreach (var element in custom.Elements) {
				if (element is TransportBindingElement)
					transportElement = (TransportBindingElement)element;
				else if (element is MtomMessageEncodingBindingElement)
					mtomElement = (MtomMessageEncodingBindingElement)element;
				else if (element is TextMessageEncodingBindingElement)
					textElement = (TextMessageEncodingBindingElement)element;
				else {
					importer.AddWarning (
						"Found unknown binding element `{0}' while attempting " +
						"to import binding `{0}'.", element.GetType (),
						custom.Name);
					foundUnknownElement = true;
				}
			}

			if (foundUnknownElement)
				return false;

			if ((mtomElement != null) && (textElement != null)) {
				// FIXME: Should never happen
				importer.AddWarning (
					"Found both MtomMessageEncodingBindingElement and " +
					"TextMessageEncodingBindingElement while attempting to " +
					"import binding `{0}'.", custom.Name);
				return false;
			}

			BasicHttpBinding httpBinding;
			AuthenticationSchemes authScheme;

			/*
			 * FIXME: Maybe make the BasicHttpBinding use the transport element
			 * that we created with the TransportBindingElementImporter ?
			 * 
			 * There seems to be no public API to do that, so maybe add a private .ctor ?
			 * 
			 */

			var httpsTransport = transportElement as HttpsTransportBindingElement;
			var httpTransport = transportElement as HttpTransportBindingElement;

			if (httpsTransport != null) {
				httpBinding = new BasicHttpBinding (BasicHttpSecurityMode.Transport);
				authScheme = httpsTransport.AuthenticationScheme;
			} else if (httpTransport != null) {
				authScheme = httpTransport.AuthenticationScheme;
				if ((authScheme != AuthenticationSchemes.None) &&
					(authScheme != AuthenticationSchemes.Anonymous))
					httpBinding = new BasicHttpBinding (
						BasicHttpSecurityMode.TransportCredentialOnly);
				else
					httpBinding = new BasicHttpBinding ();
			} else {
				httpBinding = new BasicHttpBinding ();
				authScheme = AuthenticationSchemes.Anonymous;
			}

			if (mtomElement != null)
				httpBinding.MessageEncoding = WSMessageEncoding.Mtom;
			else if (textElement != null)
				httpBinding.MessageEncoding = WSMessageEncoding.Text;
			else {
				importer.AddWarning (
					"Found neither MtomMessageEncodingBindingElement nor " +
					"TextMessageEncodingBindingElement while attempting to " +
					"import binding `{0}'.", custom.Name);
				return false;
			}

			httpBinding.Name = context.Endpoint.Binding.Name;
			httpBinding.Namespace = context.Endpoint.Binding.Namespace;

			switch (authScheme) {
			case AuthenticationSchemes.None:
			case AuthenticationSchemes.Anonymous:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				break;
			case AuthenticationSchemes.Basic:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
				break;
			case AuthenticationSchemes.Digest:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Digest;
				break;
			case AuthenticationSchemes.Ntlm:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
				break;
			case AuthenticationSchemes.Negotiate:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
				break;
			default:
				importer.AddWarning ("Invalid auth scheme: {0}", authScheme);
				return false;
			}

			if ((httpsTransport != null) && httpsTransport.RequireClientCertificate) {
				if (httpBinding.Security.Transport.ClientCredentialType != HttpClientCredentialType.None) {
					importer.AddWarning ("Cannot use both client certificate and explicit auth type.");
					return false;
				}
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			}

			context.Endpoint.Binding = httpBinding;
			return true;
		}

		bool ImportNetTcpBinding (
			WsdlImporter importer, WsdlEndpointConversionContext context,
			CustomBinding custom, WS.Soap12Binding soap)
		{
			TcpTransportBindingElement transportElement = null;
			BinaryMessageEncodingBindingElement binaryElement = null;
			TransactionFlowBindingElement transactionFlowElement = null;
			WindowsStreamSecurityBindingElement windowsStreamElement = null;
			SslStreamSecurityBindingElement sslStreamElement = null;
			bool foundUnknownElement = false;
			
			foreach (var element in custom.Elements) {
				if (element is TcpTransportBindingElement)
					transportElement = (TcpTransportBindingElement)element;
				else if (element is BinaryMessageEncodingBindingElement)
					binaryElement = (BinaryMessageEncodingBindingElement)element;
				else if (element is TransactionFlowBindingElement)
					transactionFlowElement = (TransactionFlowBindingElement)element;
				else if (element is WindowsStreamSecurityBindingElement)
					windowsStreamElement = (WindowsStreamSecurityBindingElement)element;
				else if (element is SslStreamSecurityBindingElement)
					sslStreamElement = (SslStreamSecurityBindingElement)element;
				else {
					importer.AddWarning (
						"Found unknown binding element `{0}' while importing " +
						"binding `{1}'.", element.GetType (), custom.Name);
					foundUnknownElement = true;
				}
			}

			if (foundUnknownElement)
				return false;

			if (transportElement == null) {
				importer.AddWarning (
					"Missing TcpTransportBindingElement while importing " +
					"binding `{0}'.", custom.Name);
				return false;
			}
			if (binaryElement == null) {
				importer.AddWarning (
					"Missing BinaryMessageEncodingBindingElement while importing " +
					"binding `{0}'.", custom.Name);
				return false;
			}

			if ((windowsStreamElement != null) && (sslStreamElement != null)) {
				importer.AddWarning (
					"Found both WindowsStreamSecurityBindingElement and " +
					"SslStreamSecurityBindingElement while importing binding `{0}.",
					custom.Name);
				return false;
			}

			NetTcpSecurity security;
			if (windowsStreamElement != null) {
				security = new NetTcpSecurity (SecurityMode.Transport);
				security.Transport.ProtectionLevel = windowsStreamElement.ProtectionLevel;
			} else if (sslStreamElement != null) {
				security = new NetTcpSecurity (SecurityMode.TransportWithMessageCredential);
			} else {
				security = new NetTcpSecurity (SecurityMode.None);
			}

			var netTcp = new NetTcpBinding (transportElement, security, false);

			netTcp.Name = context.Endpoint.Binding.Name;
			netTcp.Namespace = context.Endpoint.Binding.Namespace;

			context.Endpoint.Binding = netTcp;
			return true;
		}

		#endregion
	}
}

