//
// FederatedMessageSecurityOverHttp.cs
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
using System.Collections.ObjectModel;
using System.Net.Security;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	[MonoTODO]
	public sealed class FederatedMessageSecurityOverHttp
	{
		SecurityAlgorithmSuite algorithm =
			SecurityAlgorithmSuite.Default;
		SecurityKeyType issued_key_type;
		string issued_token_type;
		EndpointAddress issuer_address, metadata_address;
		Binding issuer_binding;
		bool establish_sec_ctx = true, negotiate = true;
		Collection<ClaimTypeRequirement> claim_type_reqs =
			new Collection<ClaimTypeRequirement> ();
		Collection<XmlElement> request_params = new Collection<XmlElement> ();

		internal FederatedMessageSecurityOverHttp ()
		{
		}

		public SecurityAlgorithmSuite AlgorithmSuite {
			get { return algorithm; }
			set { algorithm = value; }
		}

		public bool EstablishSecurityContext {
			get { return establish_sec_ctx; }
			set { establish_sec_ctx = value; }
		}

		public SecurityKeyType IssuedKeyType {
			get { return issued_key_type; }
			set { issued_key_type = value; }
		}

		public string IssuedTokenType {
			get { return issued_token_type; }
			set { issued_token_type = value; }
		}

		public EndpointAddress IssuerAddress {
			get { return issuer_address; }
			set { issuer_address = value; }
		}

		public Binding IssuerBinding {
			get { return issuer_binding; }
			set { issuer_binding = value; }
		}

		public EndpointAddress IssuerMetadataAddress {
			get { return metadata_address; }
			set { metadata_address = value; }
		}

		public bool NegotiateServiceCredential {
			get { return negotiate; }
			set { negotiate = value; }
		}

		public Collection<ClaimTypeRequirement> ClaimTypeRequirements {
			get { return claim_type_reqs; }
		}

		public Collection<XmlElement> TokenRequestParameters {
			get { return request_params; }
		}
	}
}
