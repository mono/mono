//
// CardSelectorClient.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors
{
	public abstract class CardSelectorClient
	{
		public abstract void Manage ();

		#region Import

		// This must be implemented unless Import() is overriden.
		public virtual string ReceivePassword ()
		{
			throw new NotImplementedException ("Import is not implemented by this identity selector client");
		}

		public virtual void Import (string filename)
		{
			string password = ReceivePassword ();
			if (password == null)
				return;
			IdentityCard card = ProcessImport (filename, password);
			IdentityStore.GetDefaultStore ().StoreCard (card, password);
		}

		protected IdentityCard ProcessImport (string filename, string password)
		{
			string xml = new IdentityCardEncryption ().Decrypt (
				new StreamReader (filename).ReadToEnd (), password);
			IdentityCard card = new IdentityCard ();
			card.Load (XmlReader.Create (new StringReader (xml)));
			return card;
		}

		#endregion

		// This is virtual since it might not be required when
		// GetToken() is overriden.
		public virtual IdentityCard SelectCardToSend (CardSelectionContext context)
		{
			throw new NotSupportedException ();
		}

		#region Default self-issued card processor
		// They are used to indicate a service URL when there is no
		// overriden behavior of RequestSelfIssuedToken().

		string self_identity_issuer = Environment.GetEnvironmentVariable ("MONO_IDENTITY_SERVICE_URL") ?? "localhost:7450";
		string self_identity_issuer_cert = Environment.GetEnvironmentVariable ("MONO_IDENTITY_SERVICE_CERTIFICATE");

		public virtual string SelfIdentityIssuerUrl {
			get { return self_identity_issuer; }
		}

		public virtual string SelfIdentityIssuerCertificate {
			get { return self_identity_issuer_cert; }
		}
		#endregion

		public virtual GenericXmlSecurityToken GetToken (
			CardSpacePolicyElement [] policyChain,
			SecurityTokenSerializer serializer)
		{
			// FIXME: sort out what is supposed to be done here.
			foreach (CardSpacePolicyElement policy in policyChain)
				return GetToken (policy.Target, policy.Issuer,
					  policy.Parameters,
					  policy.PolicyNoticeLink,
					  policy.PolicyNoticeVersion);
			throw new Exception ("INTERNAL ERROR: no policy to process");
		}

		GenericXmlSecurityToken GetToken (
			XmlElement target, XmlElement issuer,
			Collection<XmlElement> parameters,
			Uri policyNoticeLink, int policyNoticeVersion)
		{
			Collection<ClaimTypeRequirement> reqs = new Collection<ClaimTypeRequirement> ();
			Collection<XmlElement> alist = new Collection<XmlElement> ();
			foreach (XmlElement el in parameters) {
				if (el.LocalName == "Claims" && el.NamespaceURI == Constants.WstNamespace)
					foreach (XmlElement c in el.ChildNodes)
						reqs.Add (new ClaimTypeRequirement (c.GetAttribute ("Uri"), c.GetAttribute ("Optional") == "true"));
				else
					alist.Add (el);
			}

			CardSelectionContext ctx = new CardSelectionContext (
				EndpointAddress.ReadFrom (XmlDictionaryReader.CreateDictionaryReader (new XmlNodeReader (target))),
				EndpointAddress.ReadFrom (XmlDictionaryReader.CreateDictionaryReader (new XmlNodeReader (issuer))),
				reqs,
				alist,
				policyNoticeLink,
				policyNoticeVersion);

			IdentityCard card = SelectCardToSend (ctx);

			if (card.Issuer != null)
				// process WS-Trust RST
				return RequestTrustedToken (ctx, card);
			else
				return RequestSelfIssuedToken (ctx, card);
		}

		public virtual GenericXmlSecurityToken RequestTrustedToken (CardSelectionContext ctx, IdentityCard card)
		{
			X509Certificate2 cert = new X509Certificate2 (card.Certificate);
			EndpointAddress issuer = new EndpointAddress (card.Issuer, new X509CertificateEndpointIdentity (cert));
			return RequestToken (issuer, ctx);
		}

		public virtual GenericXmlSecurityToken RequestSelfIssuedToken (CardSelectionContext ctx, IdentityCard card)
		{
			Uri issuerUri = card.Issuer ?? new Uri (SelfIdentityIssuerUrl);
			X509Certificate2 cert = new X509Certificate2 (SelfIdentityIssuerCertificate);
			EndpointAddress issuer = new EndpointAddress (issuerUri, new X509CertificateEndpointIdentity (cert));
			return RequestToken (issuer, ctx);
		}

		// This must be implemented unless other depending methods
		// are overriden.
		public virtual GenericXmlSecurityToken RequestToken (EndpointAddress issuer, CardSelectionContext ctx)
		{
			return null;
		}

		/* This will be used if we have to implement unmanaged foo.

		public string GetToken (
			string targetXml,
			string issuerXml,
			string claimTypeRequirementsXml,
			string policyNoticeLink,
			int policyNoticeVersion,
			bool isManagedIssuer)
		{
			EndpointAddress target = EndpointAddress.ReadFrom (
				XmlDictionaryReader.CreateDictionaryReader (
					XmlReader.Create (new StringReader (targetXml))));
			EndpointAddress issuer = isManagedIssuer ?EndpointAddress.ReadFrom (
				XmlDictionaryReader.CreateDictionaryReader (
					XmlReader.Create (new StringReader (issuerXml)))) : null;
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;
			Collection<ClaimTypeRequirement> reqs = new Collection<ClaimTypeRequirement> ();
			Collection<XmlElement> parameters = new Collection<XmlElement> ();
			XmlDictionaryReader dr = XmlDictionaryReader.CreateDictionaryReader (
				XmlReader.Create (new StringReader (claimTypeRequirementsXml)));
			XmlDocument doc = new XmlDocument ();
			for (dr.MoveToContent (); !dr.EOF; dr.MoveToContent ()) {
				XmlElement el = doc.ReadNode (dr) as XmlElement;
				if (el.LocalName == "Claims" && el.NamespaceURI == Constants.WstNamespace)
					foreach (XmlElement c in el.ChildNodes)
						reqs.Add (new ClaimTypeRequirement (c.GetAttribute ("Uri"), c.GetAttribute ("Optional") == "true"));
				else
					parameters.Add (el);
			}

			GenericXmlSecurityToken token = GetToken (target, issuer, reqs, parameters, new Uri (policyNoticeLink), policyNoticeVersion);
			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw)) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (xw, token);
			}
			return sw.ToString ();
		}
		*/
	}
}
