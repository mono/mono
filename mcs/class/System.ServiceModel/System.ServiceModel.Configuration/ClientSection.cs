//
// ClientSection.cs
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
	public sealed class ClientSection
		 : ConfigurationSection
	{
		static Type [] _defaultPolicyImporters = new Type [] {
				typeof(CompositeDuplexBindingElementImporter),
				typeof(MessageEncodingBindingElementImporter),
				typeof(OneWayBindingElementImporter),
				typeof(PrivacyNoticeBindingElementImporter),
				typeof(ReliableSessionBindingElementImporter),
				typeof(SecurityBindingElementImporter),
				typeof(TransactionFlowBindingElementImporter),
				typeof(TransportBindingElementImporter),
				typeof(UseManagedPresentationBindingElementImporter)
			};

		static Type [] _defaultWsdlImporters = new Type [] { 
				typeof(MessageEncodingBindingElementImporter),
				typeof(StandardBindingImporter),
				typeof(TransportBindingElementImporter),
				typeof(DataContractSerializerMessageContractImporter),
				typeof(XmlSerializerMessageContractImporter)
			};

		public ClientSection () {
		}

		// Properties

		[ConfigurationProperty ("",
			 Options = ConfigurationPropertyOptions.IsDefaultCollection,
			IsDefaultCollection = true)]
		public ChannelEndpointElementCollection Endpoints {
			get { return (ChannelEndpointElementCollection) this [String.Empty]; }
		}

		[ConfigurationProperty ("metadata",
			 Options = ConfigurationPropertyOptions.None)]
		public MetadataElement Metadata {
			get { return (MetadataElement) this ["metadata"]; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}

		protected override void InitializeDefault () {
			base.InitializeDefault ();

			PolicyImporterElementCollection policyImporters = Metadata.PolicyImporters;
			for (int i = 0; i < _defaultPolicyImporters.Length; i++)
				policyImporters.Add (new PolicyImporterElement (_defaultPolicyImporters [i]));

			WsdlImporterElementCollection wsdlImporters = Metadata.WsdlImporters;
			for (int i = 0; i < _defaultWsdlImporters.Length; i++)
				wsdlImporters.Add (new WsdlImporterElement (_defaultWsdlImporters [i]));
		}

		protected override void PostDeserialize () {
			base.PostDeserialize ();
		}
	}

}
