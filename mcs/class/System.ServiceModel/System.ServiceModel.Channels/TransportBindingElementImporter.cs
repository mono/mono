//
// TransportBindingElementImporter.cs
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
using System.Xml.Schema;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using WS = System.Web.Services.Description;
using QName = System.Xml.XmlQualifiedName;

namespace System.ServiceModel.Channels {

	public class TransportBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension {
		#region IWsdlImportExtension implementation

		public void BeforeImport (WS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas,
		                          ICollection<XmlElement> policy)
		{
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext contractContext)
		{
		}

		public void ImportEndpoint (WsdlImporter importer, WsdlEndpointConversionContext context)
		{
			// Only import the binding, not the endpoint.
			if (context.WsdlPort == null)
				return;
			
			DoImportEndpoint (context);
		}

		bool DoImportEndpoint (WsdlEndpointConversionContext context)
		{
			WS.SoapAddressBinding address = null;
			foreach (var extension in context.WsdlPort.Extensions) {
				var check = extension as WS.SoapAddressBinding;
				if (check != null) {
					address = check;
					break;
				}
			}
			
			if (address == null)
				return false;
			
			context.Endpoint.Address = new EndpointAddress (address.Location);
			context.Endpoint.ListenUri = new Uri (address.Location);
			context.Endpoint.ListenUriMode = ListenUriMode.Explicit;
			return true;
		}

		#endregion

		#region IPolicyImportExtension implementation

		public void ImportPolicy (MetadataImporter importer, PolicyConversionContext context)
		{
			var customCtx = context as CustomPolicyConversionContext;
			var customBinding = context.Endpoint.Binding as CustomBinding;
			if ((customCtx == null) || (customBinding == null))
				// FIXME: Should we allow this ?
				throw new InvalidOperationException ();

			var soapHttp = StandardBindingImporter.GetHttpSoapBinding (customCtx.WsdlBinding);
			if (soapHttp != null) {
				if (!ImportHttpPolicy (importer, customCtx, soapHttp))
					context.BindingElements.Add (new HttpTransportBindingElement ());
				return;
			}

			var soapTcp = StandardBindingImporter.GetTcpSoapBinding (customCtx.WsdlBinding);
			if (soapTcp != null) {
				if (!ImportTcpPolicy (importer, customCtx, soapTcp))
					context.BindingElements.Add (new TcpTransportBindingElement ());
				return;
			}
		}

		#endregion

		bool ImportHttpAuthScheme (MetadataImporter importer,
		                           HttpTransportBindingElement bindingElement,
		                           PolicyConversionContext context)
		{
			var assertions = context.GetBindingAssertions ();
			var authSchemes = AuthenticationSchemes.None;

			var httpsTransport = bindingElement as HttpsTransportBindingElement;
			bool certificate = httpsTransport != null ?
				httpsTransport.RequireClientCertificate : false;

			var authElements = PolicyImportHelper.FindAssertionByNS (
				assertions, PolicyImportHelper.HttpAuthNS);
			foreach (XmlElement authElement in authElements) {
				assertions.Remove (authElement);

				if (certificate) {
					importer.AddWarning (
						"Invalid authentication assertion while " +
						"using client certificate: {0}", authElement.OuterXml);
					return false;
				}

				switch (authElement.LocalName) {
				case "BasicAuthentication":
					authSchemes |= AuthenticationSchemes.Basic;
					break;
				case "NtlmAuthentication":
					authSchemes |= AuthenticationSchemes.Ntlm;
					break;
				case "DigestAuthentication":
					authSchemes |= AuthenticationSchemes.Digest;
					break;
				case "NegotiateAuthentication":
					authSchemes |= AuthenticationSchemes.Negotiate;
					break;
				default:
					importer.AddWarning (
						"Invalid policy assertion: {0}", authElement.OuterXml);
					return false;
				}
			}

			bindingElement.AuthenticationScheme = authSchemes;
			return true;
		}

		bool ImportWindowsTransportSecurity (MetadataImporter importer,
		                                     PolicyConversionContext context,
		                                     XmlElement policyElement)
		{
			var protectionLevel = PolicyImportHelper.GetElement (
				importer, policyElement, "ProtectionLevel",
				PolicyImportHelper.FramingPolicyNS, true);
			if (protectionLevel == null) {
				importer.AddWarning (
					"Invalid policy assertion: {0}", policyElement.OuterXml);
				return false;
			}

			var element = new WindowsStreamSecurityBindingElement ();

			switch (protectionLevel.InnerText.ToLowerInvariant ()) {
			case "none":
				element.ProtectionLevel = ProtectionLevel.None;
				break;
			case "sign":
				element.ProtectionLevel = ProtectionLevel.Sign;
				break;
			case "encryptandsign":
				element.ProtectionLevel = ProtectionLevel.EncryptAndSign;
				break;
			default:
				importer.AddWarning (
					"Invalid policy assertion: {0}", protectionLevel.OuterXml);
				return false;
			}

			context.BindingElements.Add (element);
			return true;
		}

		bool ImportTransport (MetadataImporter importer, TransportBindingElement bindingElement,
		                      XmlElement transportPolicy)
		{
			XmlElement algorithmSuite, layout;
			if (!PolicyImportHelper.FindPolicyElement (
				importer, transportPolicy,
				new QName ("AlgorithmSuite", PolicyImportHelper.SecurityPolicyNS),
				false, true, out algorithmSuite) ||
			    !PolicyImportHelper.FindPolicyElement (
				importer, transportPolicy,
				new QName ("Layout", PolicyImportHelper.SecurityPolicyNS),
				false, true, out layout))
				return false;

			bool foundUnknown = false;
			foreach (var node in transportPolicy.ChildNodes) {
				var e = node as XmlElement;
				if (e == null)
					continue;
				importer.AddWarning ("Unknown policy assertion: {0}", e.OuterXml);
				foundUnknown = true;
			}

			return !foundUnknown;
		}

		bool GetTransportToken (MetadataImporter importer, XmlElement transportPolicy,
		                        out XmlElement transportToken)
		{
			return PolicyImportHelper.FindPolicyElement (
				importer, transportPolicy,
				new QName ("TransportToken", PolicyImportHelper.SecurityPolicyNS),
				false, true, out transportToken);
		}

		bool ImportHttpTransport (MetadataImporter importer, PolicyConversionContext context,
		                          XmlElement transportPolicy,
		                          out HttpTransportBindingElement bindingElement)
		{
			XmlElement transportToken;
			if (!GetTransportToken (importer, transportPolicy, out transportToken)) {
				bindingElement = null;
				return false;
			}

			if (transportToken == null) {
				bindingElement = new HttpTransportBindingElement ();
				return true;
			}
			
			bool error;
			var tokenElementList = PolicyImportHelper.GetPolicyElements (transportToken, out error);
			if (error || (tokenElementList.Count != 1)) {
				importer.AddWarning ("Invalid policy assertion: {0}", transportToken.OuterXml);
				bindingElement = null;
				return false;
			}

			var tokenElement = tokenElementList [0];
			if (!PolicyImportHelper.SecurityPolicyNS.Equals (tokenElement.NamespaceURI) ||
				!tokenElement.LocalName.Equals ("HttpsToken")) {
				importer.AddWarning ("Invalid policy assertion: {0}", tokenElement.OuterXml);
				bindingElement = null;
				return false;
			}

			var httpsTransport = new HttpsTransportBindingElement ();
			bindingElement = httpsTransport;

			var certAttr = tokenElement.GetAttribute ("RequireClientCertificate");
			if (!String.IsNullOrEmpty (certAttr))
				httpsTransport.RequireClientCertificate = Boolean.Parse (certAttr);
			return true;
		}

		bool ImportTcpTransport (MetadataImporter importer, PolicyConversionContext context,
		                         XmlElement transportPolicy)
		{
			XmlElement transportToken;
			if (!GetTransportToken (importer, transportPolicy, out transportToken))
				return false;

			if (transportToken == null)
				return true;

			bool error;
			var tokenElementList = PolicyImportHelper.GetPolicyElements (transportToken, out error);
			if (error || (tokenElementList.Count != 1)) {
				importer.AddWarning ("Invalid policy assertion: {0}", transportToken.OuterXml);
				return false;
			}

			var tokenElement = tokenElementList [0];
			if (!PolicyImportHelper.FramingPolicyNS.Equals (tokenElement.NamespaceURI)) {
				importer.AddWarning ("Invalid policy assertion: {0}", tokenElement.OuterXml);
				return false;
			}

			if (tokenElement.LocalName.Equals ("WindowsTransportSecurity")) {
				if (!ImportWindowsTransportSecurity (importer, context, tokenElement))
					return false;
			} else if (tokenElement.LocalName.Equals ("SslTransportSecurity")) {
				context.BindingElements.Add (new SslStreamSecurityBindingElement ());
			}

			return true;
		}

		bool ImportHttpPolicy (MetadataImporter importer, PolicyConversionContext context,
		                       WS.SoapBinding soap)
		{
			HttpTransportBindingElement httpTransport;
			var assertions = context.GetBindingAssertions ();
			var transportPolicy = PolicyImportHelper.GetTransportBindingPolicy (assertions);
			if (transportPolicy != null) {
				if (!ImportHttpTransport (importer, context, transportPolicy, out httpTransport))
					return false;
				if (!ImportTransport (importer, httpTransport, transportPolicy))
					return false;
			} else {
				httpTransport = new HttpTransportBindingElement ();
			}

			if (!ImportHttpAuthScheme (importer, httpTransport, context))
				return false;

			context.BindingElements.Add (httpTransport);
			return true;
		}

		bool ImportTcpPolicy (MetadataImporter importer, PolicyConversionContext context,
		                      WS.Soap12Binding soap)
		{
			var assertions = context.GetBindingAssertions ();

			var tcpTransport = new TcpTransportBindingElement ();

			var transportPolicy = PolicyImportHelper.GetTransportBindingPolicy (assertions);
			if (transportPolicy != null) {
				if (!ImportTcpTransport (importer, context, transportPolicy))
					return false;
				if (!ImportTransport (importer, tcpTransport, transportPolicy))
					return false;
			}

			var streamed = PolicyImportHelper.GetStreamedMessageFramingPolicy (assertions);
			if (streamed != null)
				tcpTransport.TransferMode = TransferMode.Streamed;
			
			context.BindingElements.Add (tcpTransport);
			return true;
		}
	}
}
