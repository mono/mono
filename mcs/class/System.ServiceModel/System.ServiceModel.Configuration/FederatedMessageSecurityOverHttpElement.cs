//
// FederatedMessageSecurityOverHttpElement.cs
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
	[MonoTODO]
	public sealed partial class FederatedMessageSecurityOverHttpElement
		 : ConfigurationElement
	{
		// Static Fields
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty algorithm_suite;
		static ConfigurationProperty claim_type_requirements;
		static ConfigurationProperty establish_security_context;
		static ConfigurationProperty issued_key_type;
		static ConfigurationProperty issued_token_type;
		static ConfigurationProperty issuer;
		static ConfigurationProperty issuer_metadata;
		static ConfigurationProperty negotiate_service_credential;
		static ConfigurationProperty token_request_parameters;

		static FederatedMessageSecurityOverHttpElement ()
		{
			properties = new ConfigurationPropertyCollection ();
			algorithm_suite = new ConfigurationProperty ("algorithmSuite",
				typeof (SecurityAlgorithmSuite), "Default", new SecurityAlgorithmSuiteConverter (), null,
				ConfigurationPropertyOptions.None);

			claim_type_requirements = new ConfigurationProperty ("claimTypeRequirements",
				typeof (ClaimTypeElementCollection), null, null/* FIXME: get converter for ClaimTypeElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			establish_security_context = new ConfigurationProperty ("establishSecurityContext",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			issued_key_type = new ConfigurationProperty ("issuedKeyType",
				typeof (SecurityKeyType), "SymmetricKey", null/* FIXME: get converter for SecurityKeyType*/, null,
				ConfigurationPropertyOptions.None);

			issued_token_type = new ConfigurationProperty ("issuedTokenType",
				typeof (string), "", new StringConverter (), null,
				ConfigurationPropertyOptions.None);

			issuer = new ConfigurationProperty ("issuer",
				typeof (IssuedTokenParametersEndpointAddressElement), null, null/* FIXME: get converter for IssuedTokenParametersEndpointAddressElement*/, null,
				ConfigurationPropertyOptions.None);

			issuer_metadata = new ConfigurationProperty ("issuerMetadata",
				typeof (EndpointAddressElementBase), null, null/* FIXME: get converter for EndpointAddressElementBase*/, null,
				ConfigurationPropertyOptions.None);

			negotiate_service_credential = new ConfigurationProperty ("negotiateServiceCredential",
				typeof (bool), "true", new BooleanConverter (), null,
				ConfigurationPropertyOptions.None);

			token_request_parameters = new ConfigurationProperty ("tokenRequestParameters",
				typeof (XmlElementElementCollection), null, null/* FIXME: get converter for XmlElementElementCollection*/, null,
				ConfigurationPropertyOptions.None);

			properties.Add (algorithm_suite);
			properties.Add (claim_type_requirements);
			properties.Add (establish_security_context);
			properties.Add (issued_key_type);
			properties.Add (issued_token_type);
			properties.Add (issuer);
			properties.Add (issuer_metadata);
			properties.Add (negotiate_service_credential);
			properties.Add (token_request_parameters);
		}

		public FederatedMessageSecurityOverHttpElement ()
		{
		}


		// Properties

		[TypeConverter (typeof (SecurityAlgorithmSuiteConverter))]
		[ConfigurationProperty ("algorithmSuite",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "Default")]
		public SecurityAlgorithmSuite AlgorithmSuite {
			get { return (SecurityAlgorithmSuite) base [algorithm_suite]; }
			set { base [algorithm_suite] = value; }
		}

		[ConfigurationProperty ("claimTypeRequirements",
			 Options = ConfigurationPropertyOptions.None)]
		public ClaimTypeElementCollection ClaimTypeRequirements {
			get { return (ClaimTypeElementCollection) base [claim_type_requirements]; }
		}

		[ConfigurationProperty ("establishSecurityContext",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool EstablishSecurityContext {
			get { return (bool) base [establish_security_context]; }
			set { base [establish_security_context] = value; }
		}

		[ConfigurationProperty ("issuedKeyType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "SymmetricKey")]
		public SecurityKeyType IssuedKeyType {
			get { return (SecurityKeyType) base [issued_key_type]; }
			set { base [issued_key_type] = value; }
		}

		[StringValidator ( MinLength = 0,
			MaxLength = int.MaxValue,
			 InvalidCharacters = null)]
		[ConfigurationProperty ("issuedTokenType",
			 Options = ConfigurationPropertyOptions.None,
			 DefaultValue = "")]
		public string IssuedTokenType {
			get { return (string) base [issued_token_type]; }
			set { base [issued_token_type] = value; }
		}

		[ConfigurationProperty ("issuer",
			 Options = ConfigurationPropertyOptions.None)]
		public IssuedTokenParametersEndpointAddressElement Issuer {
			get { return (IssuedTokenParametersEndpointAddressElement) base [issuer]; }
		}

		[ConfigurationProperty ("issuerMetadata",
			 Options = ConfigurationPropertyOptions.None)]
		public EndpointAddressElementBase IssuerMetadata {
			get { return (EndpointAddressElementBase) base [issuer_metadata]; }
		}

		[ConfigurationProperty ("negotiateServiceCredential",
			 Options = ConfigurationPropertyOptions.None,
			DefaultValue = true)]
		public bool NegotiateServiceCredential {
			get { return (bool) base [negotiate_service_credential]; }
			set { base [negotiate_service_credential] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		[ConfigurationProperty ("tokenRequestParameters",
			 Options = ConfigurationPropertyOptions.None)]
		public XmlElementElementCollection TokenRequestParameters {
			get { return (XmlElementElementCollection) base [token_request_parameters]; }
		}

		// Methods
		internal void ApplyConfiguration (FederatedMessageSecurityOverHttp s)
		{
			s.AlgorithmSuite = AlgorithmSuite;
			foreach (ClaimTypeElement cte in ClaimTypeRequirements)
				s.ClaimTypeRequirements.Add (cte.Create ());
			s.EstablishSecurityContext = EstablishSecurityContext;
			s.IssuedKeyType = IssuedKeyType;
			s.IssuedTokenType = IssuedTokenType;
			if (Issuer.Address != null)
				s.IssuerAddress = new EndpointAddress (Issuer.Address, Issuer.Identity.Create (), Issuer.Headers.Headers);
			if (!String.IsNullOrEmpty (Issuer.Binding))
				s.IssuerBinding = ConfigUtil.CreateBinding (Issuer.Binding, Issuer.BindingConfiguration);
			if (IssuerMetadata.Address != null)
				s.IssuerMetadataAddress = new EndpointAddress (IssuerMetadata.Address, IssuerMetadata.Identity.Create (), IssuerMetadata.Headers.Headers);
			s.NegotiateServiceCredential = NegotiateServiceCredential;
			foreach (XmlElementElement xee in TokenRequestParameters)
				s.TokenRequestParameters.Add (xee.XmlElement);
		}
	}

}
