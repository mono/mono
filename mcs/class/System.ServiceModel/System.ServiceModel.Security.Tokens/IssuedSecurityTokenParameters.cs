//
// IssuedSecurityTokenParameters.cs
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
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.XPath;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Security.Tokens
{
	public class IssuedSecurityTokenParameters : SecurityTokenParameters
	{
		public IssuedSecurityTokenParameters ()
		{
		}

		public IssuedSecurityTokenParameters (string tokenType)
			: this (tokenType, null)
		{
		}

		public IssuedSecurityTokenParameters (string tokenType, EndpointAddress issuerAddress)
			: this (tokenType, issuerAddress, null)
		{
		}

		public IssuedSecurityTokenParameters (string tokenType,
			EndpointAddress issuerAddress, Binding issuerBinding)
		{
			token_type = tokenType;
			issuer_address = issuerAddress;
			binding = issuerBinding;
		}

		protected IssuedSecurityTokenParameters (IssuedSecurityTokenParameters source)
			: base (source)
		{
			binding = source.binding;
			issuer_address = source.issuer_address;
			issuer_meta_address = source.issuer_meta_address;
			key_size = source.key_size;
			key_type = source.key_type;
			token_type = source.token_type;
			reqs = new Collection<ClaimTypeRequirement> (source.reqs);
			additional_reqs = new Collection<XmlElement> (source.additional_reqs);
		}

		Binding binding;
		EndpointAddress issuer_address, issuer_meta_address;
		int key_size;
		SecurityKeyType key_type;
		string token_type;
		Collection<ClaimTypeRequirement> reqs =
			new Collection<ClaimTypeRequirement> ();
		Collection<XmlElement> additional_reqs =
			new Collection<XmlElement> ();

		public override string ToString ()
		{
			return base.ToString ();
		}

		public Collection<XmlElement> AdditionalRequestParameters {
			get { return additional_reqs; }
		}

		public Collection<ClaimTypeRequirement> ClaimTypeRequirements { 
			get { return reqs; }
		}

		protected override bool HasAsymmetricKey {
			get { return false; }
		}

		public EndpointAddress IssuerAddress {
			get { return issuer_address; }
			set { issuer_address = value; }
		}

		public Binding IssuerBinding {
			get { return binding; }
			set { binding = value; }
		}

		public EndpointAddress IssuerMetadataAddress {
			get { return issuer_meta_address; }
			set { issuer_meta_address = value; }
		}

		public int KeySize {
			get { return key_size; }
			set { key_size = value; }
		}

		public SecurityKeyType KeyType {
			get { return key_type; }
			set { key_type = value; }
		}

		public string TokenType {
			get { return token_type; }
			set { token_type = value; }
 		}

		protected override bool SupportsClientAuthentication {
			get { return true; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return false; }
		}

		protected override bool SupportsServerAuthentication {
			get { return true; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new IssuedSecurityTokenParameters (this);
		}

		[MonoTODO]
		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			throw new NotImplementedException ();
		}

		public Collection<XmlElement> CreateRequestParameters (
			MessageSecurityVersion messageSecurityVersion,
			SecurityTokenSerializer securityTokenSerializer)
		{
			XmlDocument doc = new XmlDocument ();
			Collection<XmlElement> ret = new Collection<XmlElement> ();
			// KeyType
			string keyTypeUri =
				KeyType == SecurityKeyType.SymmetricKey ?
				Constants.WstSymmetricKeyTypeUri :
				Constants.WstAsymmetricKeyTypeUri;
			XmlElement kt = doc.CreateElement ("t", "KeyType", Constants.WstNamespace);
			kt.AppendChild (doc.CreateTextNode (keyTypeUri));
			ret.Add (kt);

			// ClaimTypes
			XmlElement cts = doc.CreateElement ("t", "Claims", Constants.WstNamespace);
			foreach (ClaimTypeRequirement req in ClaimTypeRequirements) {
				XmlElement el = doc.CreateElement ("wsid", "ClaimType", Constants.WsidNamespace);
				el.SetAttribute ("Uri", req.ClaimType);
				if (req.IsOptional)
					el.SetAttribute ("Optional", "true");
				cts.AppendChild (el);
			}
			ret.Add (cts);

			// Additional parameters
			foreach (XmlElement el in AdditionalRequestParameters)
				ret.Add (el);
			return ret;
		}

		protected internal override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			if (requirement == null)
				throw new ArgumentNullException ("requirement");
			requirement.TokenType = TokenType;
			requirement.Properties [ReqType.IssuedSecurityTokenParametersProperty] = this.Clone ();
			requirement.Properties [ReqType.IssuerAddressProperty] = IssuerAddress;
			requirement.Properties [ReqType.IssuerBindingProperty] = IssuerBinding;
			requirement.RequireCryptographicToken = true;
			requirement.KeyType = KeyType;
		}
	}
}
