//
// SecureMessageDecryptor.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels.Security
{
	internal class RecipientSecureMessageDecryptor : SecureMessageDecryptor
	{
		RecipientMessageSecurityBindingSupport security;

		public RecipientSecureMessageDecryptor (
			Message source, RecipientMessageSecurityBindingSupport security)
			: base (source, security)
		{
			this.security = security;
		}

		public override MessageDirection Direction {
			get { return MessageDirection.Input; }
		}

		public override SecurityMessageProperty RequestSecurity {
			get { return null; }
		}

		public override SecurityTokenParameters Parameters {
			get { return security.RecipientParameters; }
		}

		public override SecurityTokenParameters CounterParameters {
			get { return security.InitiatorParameters; }
		}
	}

	internal class InitiatorSecureMessageDecryptor : SecureMessageDecryptor
	{
		InitiatorMessageSecurityBindingSupport security;
		SecurityMessageProperty request_security;

		public InitiatorSecureMessageDecryptor (
			Message source, SecurityMessageProperty secprop, InitiatorMessageSecurityBindingSupport security)
			: base (source, security)
		{
			this.security = security;
			request_security = secprop;
		}

		public override SecurityMessageProperty RequestSecurity {
			get { return request_security; }
		}

		public override MessageDirection Direction {
			get { return MessageDirection.Output; }
		}

		public override SecurityTokenParameters Parameters {
			get { return security.InitiatorParameters; }
		}

		public override SecurityTokenParameters CounterParameters {
			get { return security.RecipientParameters; }
		}
	}

	internal abstract class SecureMessageDecryptor
	{
		Message source_message;
		MessageBuffer buf;
		MessageSecurityBindingSupport security;

		XmlDocument doc;
		XmlNamespaceManager nsmgr; // for XPath query

		SecurityMessageProperty sec_prop =
			new SecurityMessageProperty ();
		WSSecurityMessageHeader wss_header = null;
		WSSecurityMessageHeaderReader wss_header_reader;
		List<MessageHeaderInfo> headers = new List<MessageHeaderInfo> ();
		SecurityTokenResolver token_resolver;
		List<SecurityToken> tokens;

		protected SecureMessageDecryptor (
			Message source, MessageSecurityBindingSupport security)
		{
			source_message = source;
			this.security = security;

			// FIXME: use proper max buffer
			buf = source.CreateBufferedCopy (int.MaxValue);
Console.WriteLine ("####### " + buf.CreateMessage ());

			doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
			nsmgr.AddNamespace ("c", Constants.WsscNamespace);
			nsmgr.AddNamespace ("o", Constants.WssNamespace);
			nsmgr.AddNamespace ("e", EncryptedXml.XmlEncNamespaceUrl);
			nsmgr.AddNamespace ("u", Constants.WsuNamespace);
			nsmgr.AddNamespace ("dsig", SignedXml.XmlDsigNamespaceUrl);

		}

		public abstract MessageDirection Direction { get; }
		public abstract SecurityTokenParameters Parameters { get; }
		public abstract SecurityTokenParameters CounterParameters { get; }
		public abstract SecurityMessageProperty RequestSecurity { get; }

		public SecurityTokenResolver TokenResolver {
			get { return token_resolver; }
		}

		public Message DecryptMessage ()
		{
			Message srcmsg = buf.CreateMessage ();
			if (srcmsg.Version.Envelope == EnvelopeVersion.None)
				throw new ArgumentException ("The message to decrypt is not an expected SOAP envelope.");

			string action = GetAction ();
			if (action == null)
				throw new ArgumentException ("SOAP action could not be retrieved from the message to decrypt.");

			XPathNavigator nav = doc.CreateNavigator ();
			using (XmlWriter writer = nav.AppendChild ()) {
				buf.CreateMessage ().WriteMessage (writer);
			}
/*
doc.PreserveWhitespace = false;
doc.Save (Console.Out);
doc.PreserveWhitespace = true;
*/

			// read and store headers, wsse:Security and setup in-band resolver.
			ReadHeaders (srcmsg);

			ExtractSecurity ();

			Message msg = Message.CreateMessage (new XmlNodeReader (doc), srcmsg.Headers.Count, srcmsg.Version);
			for (int i = 0; i < srcmsg.Headers.Count; i++) {
				MessageHeaderInfo header = srcmsg.Headers [i];
				if (header == wss_header) {
					msg.Headers.RemoveAt (i);
					msg.Headers.Add (wss_header);
				}
			}

			// FIXME: when Local[Client|Service]SecuritySettings.DetectReplays
			// is true, reject such messages which don't have <wsu:Timestamp>

			msg.Properties.Add ("Security", sec_prop);

			return msg;
		}

		void ReadHeaders (Message srcmsg)
		{
			SecurityTokenSerializer serializer =
				security.TokenSerializer;

			tokens = new List<SecurityToken> ();
			token_resolver = SecurityTokenResolver.CreateDefaultSecurityTokenResolver (
				new ReadOnlyCollection <SecurityToken> (tokens),
				true);
			token_resolver = new UnionSecurityTokenResolver (token_resolver, security.OutOfBandTokenResolver);

			// Add relevant protection token and supporting tokens.
			tokens.Add (security.EncryptionToken);
			// FIXME: this is just a workaround for symmetric binding to not require extra client certificate.
			if (security.Element is AsymmetricSecurityBindingElement)
				tokens.Add (security.SigningToken);
			if (RequestSecurity != null && RequestSecurity.ProtectionToken != null)
				tokens.Add (RequestSecurity.ProtectionToken.SecurityToken);
			// FIXME: handle supporting tokens

			for (int i = 0; i < srcmsg.Headers.Count; i++) {
				MessageHeaderInfo header = srcmsg.Headers [i];
				// FIXME: check SOAP Actor.
				// MessageHeaderDescription.Actor needs to be accessible from here.
				if (header.Namespace == Constants.WssNamespace &&
				    header.Name == "Security") {
					wss_header = new WSSecurityMessageHeader (null);
					wss_header_reader = new WSSecurityMessageHeaderReader (wss_header, serializer, token_resolver, doc, nsmgr, tokens);
					wss_header_reader.ReadContents (srcmsg.Headers.GetReaderAtHeader (i));
					headers.Add (wss_header);
				}
				else
					headers.Add (header);
			}
			if (wss_header == null)
				throw new InvalidOperationException ("In this service contract, a WS-Security header is required in the Message, but was not found.");
		}

		void ExtractSecurity ()
		{
			if (security.MessageProtectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature &&
			    wss_header.Find<SignedXml> () != null)
				throw new MessageSecurityException ("The security binding element expects that the message signature is encrypted, while it isn't.");

			WrappedKeySecurityToken wk = wss_header.Find<WrappedKeySecurityToken> ();
			DerivedKeySecurityToken dk = wss_header.Find<DerivedKeySecurityToken> ();
			if (wk != null) {
				if (Parameters.RequireDerivedKeys && dk == null)
					throw new MessageSecurityException ("DerivedKeyToken is required in this contract, but was not found in the message");
			}
			else
				// FIXME: this is kind of hack for symmetric reply processing.
				wk = RequestSecurity.ProtectionToken != null ? RequestSecurity.ProtectionToken.SecurityToken as WrappedKeySecurityToken : null;

			SymmetricSecurityKey wkkey = wk != null ? wk.SecurityKeys [0] as SymmetricSecurityKey : null;

			wss_header_reader.DecryptSecurity (this, wkkey, RequestSecurity != null ? RequestSecurity.EncryptionKey : null);

			// signature confirmation
			WSSignedXml sxml = wss_header.Find<WSSignedXml> ();
			if (sxml == null)
				throw new MessageSecurityException ("The the message signature is expected but not found.");

			bool confirmed = false;

			SecurityKeyIdentifierClause sigClause = null;
			foreach (KeyInfoClause kic in sxml.KeyInfo) {
				SecurityTokenReferenceKeyInfo r = kic as SecurityTokenReferenceKeyInfo;
				if (r != null)
					sigClause = r.Clause;
			}
			if (sigClause == null)
				throw new MessageSecurityException ("SecurityTokenReference was not found in dsig:Signature KeyInfo.");

			SecurityToken signToken;
			SecurityKey signKey;

			signToken = TokenResolver.ResolveToken (sigClause);
			signKey = signToken.ResolveKeyIdentifierClause (sigClause);
			SymmetricSecurityKey symkey = signKey as SymmetricSecurityKey;
			if (symkey != null) {
				confirmed = sxml.CheckSignature (new HMACSHA1 (symkey.GetSymmetricKey ()));
				if (wk != null)
					// FIXME: authenticate token
					sec_prop.ProtectionToken = new SecurityTokenSpecification (wk, null);
			} else {
				AsymmetricAlgorithm alg = ((AsymmetricSecurityKey) signKey).GetAsymmetricAlgorithm (security.DefaultSignatureAlgorithm, false);
				confirmed = sxml.CheckSignature (alg);
				sec_prop.InitiatorToken = new SecurityTokenSpecification (
					signToken,
					security.TokenAuthenticator.ValidateToken (signToken));
			}
			if (!confirmed)
				throw new MessageSecurityException ("Message signature is invalid.");

			// token authentication
			// FIXME: it might not be limited to recipient
			if (Direction == MessageDirection.Input)
				ProcessSupportingTokens (sxml);

			sec_prop.EncryptionKey = ((SymmetricSecurityKey) wk.SecurityKeys [0]).GetSymmetricKey ();
			sec_prop.ConfirmedSignatures.Add (Convert.ToBase64String (sxml.SignatureValue));
		}

		#region supporting token processing

		// authenticate and map supporting tokens to proper SupportingTokenSpecification list.
		void ProcessSupportingTokens (SignedXml sxml)
		{
			List<SupportingTokenInfo> tokens = new List<SupportingTokenInfo> ();
		
			// First, categorize those tokens in the Security
			// header:
			// - Endorsing		signing
			// - Signed			signed
			// - SignedEncrypted		signed	encrypted
			// - SignedEndorsing	signing	signed

			foreach (object obj in wss_header.Contents) {
				SecurityToken token = obj as SecurityToken;
				if (token == null)
					continue;
				bool signed = false, endorsing = false, encrypted = false;
				// signed
				foreach (Reference r in sxml.SignedInfo.References)
					if (r.Uri.Substring (1) == token.Id) {
						signed = true;
						break;
					}
				// FIXME: how to get 'encrypted' state?
				// FIXME: endorsing

				SecurityTokenAttachmentMode mode =
					signed ? encrypted ? SecurityTokenAttachmentMode.SignedEncrypted :
					endorsing ? SecurityTokenAttachmentMode.SignedEndorsing :
					SecurityTokenAttachmentMode.Signed :
					SecurityTokenAttachmentMode.Endorsing;
				tokens.Add (new SupportingTokenInfo (token, mode, false));
			}

			// then,
			// 1. validate every mandatory supporting token
			// parameters (Endpoint-, Operation-). To do that,
			// iterate all tokens in the header against every
			// parameter in the mandatory list.
			// 2. validate every token that is not validated.
			// To do that, iterate all supporting token parameters
			// and check if any of them can validate it.
			SupportingTokenParameters supp;
			string action = GetAction ();
			ValidateTokensByParameters (security.Element.EndpointSupportingTokenParameters, tokens, false);
			if (security.Element.OperationSupportingTokenParameters.TryGetValue (action, out supp))
				ValidateTokensByParameters (supp, tokens, false);
			ValidateTokensByParameters (security.Element.OptionalEndpointSupportingTokenParameters, tokens, true);
			if (security.Element.OptionalOperationSupportingTokenParameters.TryGetValue (action, out supp))
				ValidateTokensByParameters (supp, tokens, true);
		}

		void ValidateTokensByParameters (SupportingTokenParameters supp, List<SupportingTokenInfo> tokens, bool optional)
		{
			ValidateTokensByParameters (supp.Endorsing, tokens, optional, SecurityTokenAttachmentMode.Endorsing);
			ValidateTokensByParameters (supp.Signed, tokens, optional, SecurityTokenAttachmentMode.Signed);
			ValidateTokensByParameters (supp.SignedEndorsing, tokens, optional, SecurityTokenAttachmentMode.SignedEndorsing);
			ValidateTokensByParameters (supp.SignedEncrypted, tokens, optional, SecurityTokenAttachmentMode.SignedEncrypted);
		}

		void ValidateTokensByParameters (IEnumerable<SecurityTokenParameters> plist, List<SupportingTokenInfo> tokens, bool optional, SecurityTokenAttachmentMode attachMode)
		{
			foreach (SecurityTokenParameters p in plist) {
				SecurityTokenResolver r;
				SecurityTokenAuthenticator a =
					security.CreateTokenAuthenticator (p, out r);
				SupportingTokenSpecification spec = ValidateTokensByParameters (a, r, tokens);
				if (spec == null) {
					if (optional)
						continue;
					else
						throw new MessageSecurityException (String.Format ("No security token could be validated for authenticator '{0}' which is indicated by the '{1}' supporting token parameters", a, attachMode));
				} else {
					// For endorsing tokens, verify corresponding signatures.
					switch (attachMode) {
					case SecurityTokenAttachmentMode.Endorsing:
					case SecurityTokenAttachmentMode.SignedEndorsing:
						WSSignedXml esxml = GetSignatureForToken (spec.SecurityToken);
						if (esxml == null)
							throw new MessageSecurityException (String.Format ("The '{1}' token '{0}' is expected to endorse the primary signature but no corresponding signature is found.", spec.SecurityToken, attachMode));

						bool confirmed;
						SecurityAlgorithmSuite suite = security.Element.DefaultAlgorithmSuite;
						foreach (SecurityTokenReferenceKeyInfo kic in esxml.KeyInfo) {
							SecurityKey signKey = spec.SecurityToken.ResolveKeyIdentifierClause (kic.Clause);
							SymmetricSecurityKey symkey = signKey as SymmetricSecurityKey;
							if (symkey != null) {
								confirmed = esxml.CheckSignature (symkey.GetKeyedHashAlgorithm (suite.DefaultSymmetricSignatureAlgorithm));
							} else {
								AsymmetricAlgorithm alg = ((AsymmetricSecurityKey) signKey).GetAsymmetricAlgorithm (suite.DefaultAsymmetricSignatureAlgorithm, false);
								confirmed = esxml.CheckSignature (alg);
							}
							if (!confirmed)
								throw new MessageSecurityException (String.Format ("Signature for '{1}' token '{0}' is invalid.", spec.SecurityToken, attachMode));
							break;
						}

						sec_prop.ConfirmedSignatures.Insert (0, Convert.ToBase64String (esxml.SignatureValue));
						break;
					}
				}

				sec_prop.IncomingSupportingTokens.Add (spec);
			}
		}

		WSSignedXml GetSignatureForToken (SecurityToken token)
		{
			int count = 0;
			foreach (WSSignedXml sxml in wss_header.FindAll<WSSignedXml> ()) {
				if (count++ == 0)
					continue; // primary signature
				foreach (SecurityTokenReferenceKeyInfo r in sxml.KeyInfo)
					if (token.MatchesKeyIdentifierClause (r.Clause))
						return sxml;
			}
			return null;
		}

		SupportingTokenSpecification ValidateTokensByParameters (SecurityTokenAuthenticator a, SecurityTokenResolver r, List<SupportingTokenInfo> tokens)
		{
			foreach (SupportingTokenInfo info in tokens)
				if (a.CanValidateToken (info.Token))
					return new SupportingTokenSpecification (
						info.Token,
						a.ValidateToken (info.Token),
						info.Mode);
			return null;
		}

		#endregion

		string GetAction ()
		{
			string ret = source_message.Headers.Action;
			if (ret == null) {
				HttpRequestMessageProperty reqprop =
					source_message.Properties ["Action"] as HttpRequestMessageProperty;
				if (reqprop != null)
					ret = reqprop.Headers ["Action"];
			}
			return ret;
		}
	}
}
