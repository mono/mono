//
// ExtensionsSection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public class ExtensionsSection
		 : ConfigurationSection
	{
		ConfigurationPropertyCollection _properties;

		// Properties

		[ConfigurationProperty ("behaviorExtensions",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtensionElementCollection BehaviorExtensions {
			get { return (ExtensionElementCollection) base ["behaviorExtensions"]; }
		}

		[ConfigurationProperty ("bindingElementExtensions",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtensionElementCollection BindingElementExtensions {
			get { return (ExtensionElementCollection) base ["bindingElementExtensions"]; }
		}

		[ConfigurationProperty ("bindingExtensions",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtensionElementCollection BindingExtensions {
			get { return (ExtensionElementCollection) base ["bindingExtensions"]; }
		}

#if NET_4_0
		[ConfigurationProperty ("endpointExtensions",
			 Options = ConfigurationPropertyOptions.None)]
		public ExtensionElementCollection EndpointExtensions {
			get { return (ExtensionElementCollection) base ["endpointExtensions"]; }
		}
#endif

		protected override ConfigurationPropertyCollection Properties {
			get {
				if (_properties == null) {
					_properties = new ConfigurationPropertyCollection ();
					_properties.Add (new ConfigurationProperty ("behaviorExtensions", typeof (ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("bindingElementExtensions", typeof (ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
					_properties.Add (new ConfigurationProperty ("bindingExtensions", typeof (ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
#if NET_4_0
					_properties.Add (new ConfigurationProperty ("endpointExtensions", typeof (ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
#endif
				}
				return _properties;
			}
		}

		protected override void InitializeDefault () {
			InitializeBehaviorExtensionsDefault ();
			InitializeBindingElementExtensionsDefault ();
			InitializeBindingExtensionsDefault ();
#if NET_4_0
			InitializeEndpointExtensionsDefault ();
#endif
		}

		void InitializeBindingExtensionsDefault () {
			BindingExtensions.Add (new ExtensionElement ("basicHttpBinding", typeof (BasicHttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("customBinding", typeof (CustomBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("mexHttpBinding", typeof (MexHttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("mexHttpsBinding", typeof (MexHttpsBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("mexNamedPipeBinding", typeof (MexNamedPipeBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("mexTcpBinding", typeof (MexTcpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("msmqIntegrationBinding", typeof (MsmqIntegrationBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("netMsmqBinding", typeof (NetMsmqBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("netNamedPipeBinding", typeof (NetNamedPipeBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("netPeerTcpBinding", typeof (NetPeerTcpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("netTcpBinding", typeof (NetTcpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("ws2007FederationHttpBinding", typeof (WS2007FederationHttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("ws2007HttpBinding", typeof (WS2007HttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("wsDualHttpBinding", typeof (WSDualHttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("wsFederationHttpBinding", typeof (WSFederationHttpBindingCollectionElement).AssemblyQualifiedName));
			BindingExtensions.Add (new ExtensionElement ("wsHttpBinding", typeof (WSHttpBindingCollectionElement).AssemblyQualifiedName));
		}

		void InitializeBindingElementExtensionsDefault () {
			BindingElementExtensions.Add (new ExtensionElement ("binaryMessageEncoding", typeof (BinaryMessageEncodingElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("compositeDuplex", typeof (CompositeDuplexElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("httpTransport", typeof (HttpTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("httpsTransport", typeof (HttpsTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("msmqIntegration", typeof (MsmqIntegrationElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("msmqTransport", typeof (MsmqTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("mtomMessageEncoding", typeof (MtomMessageEncodingElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("namedPipeTransport", typeof (NamedPipeTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("oneWay", typeof (OneWayElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("peerTransport", typeof (PeerTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("pnrpPeerResolver", typeof (PnrpPeerResolverElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("privacyNoticeAt", typeof (PrivacyNoticeElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("reliableSession", typeof (ReliableSessionElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("security", typeof (SecurityElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("sslStreamSecurity", typeof (SslStreamSecurityElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("tcpTransport", typeof (TcpTransportElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("textMessageEncoding", typeof (TextMessageEncodingElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("transactionFlow", typeof (TransactionFlowElement).AssemblyQualifiedName));
			//BindingElementExtensions.Add (new ExtensionElement ("unrecognizedPolicyAssertion", typeof (UnrecognizedPolicyAssertionElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("useManagedPresentation", typeof (UseManagedPresentationElement).AssemblyQualifiedName));
			BindingElementExtensions.Add (new ExtensionElement ("windowsStreamSecurity", typeof (WindowsStreamSecurityElement).AssemblyQualifiedName));
		}

		void InitializeBehaviorExtensionsDefault () {
			BehaviorExtensions.Add (new ExtensionElement ("callbackDebug", typeof (CallbackDebugElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("callbackTimeouts", typeof (CallbackTimeoutsElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("clientCredentials", typeof (ClientCredentialsElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("clientVia", typeof (ClientViaElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("dataContractSerializer", typeof (DataContractSerializerElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceAuthorization", typeof (ServiceAuthorizationElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceCredentials", typeof (ServiceCredentialsElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceDebug", typeof (ServiceDebugElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceMetadata", typeof (ServiceMetadataPublishingElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceSecurityAudit", typeof (ServiceSecurityAuditElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceThrottling", typeof (ServiceThrottlingElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("serviceTimeouts", typeof (ServiceTimeoutsElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("synchronousReceive", typeof (SynchronousReceiveElement).AssemblyQualifiedName));
			BehaviorExtensions.Add (new ExtensionElement ("transactedBatching", typeof (TransactedBatchingElement).AssemblyQualifiedName));
		}

#if NET_4_0
		void InitializeEndpointExtensionsDefault () {
			EndpointExtensions.Add (new ExtensionElement ("mexEndpoint", typeof (ServiceMetadataEndpointCollectionElement).AssemblyQualifiedName));
		}
#endif
	}

}
