//
// MessageSecurityGenerator.cs
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
	internal class InitiatorMessageSecurityGenerator : MessageSecurityGenerator
	{
		EndpointAddress message_to;
		InitiatorMessageSecurityBindingSupport security;

		public InitiatorMessageSecurityGenerator (
			Message msg,
			InitiatorMessageSecurityBindingSupport security,
			EndpointAddress messageTo)
			: base (msg, security)
		{
			// FIXME: I believe it should be done at channel
			// creation phase, but WinFX does not.
//			if (!security.InitiatorParameters.InternalHasAsymmetricKey)
//				throw new InvalidOperationException ("Wrong security token parameters: it must have an asymmetric key (HasAsymmetricKey). There is likely a misconfiguration in the custom security binding element.");

			this.security = security;
			this.message_to = messageTo;
		}

		public override SecurityTokenParameters Parameters {
			get { return security.InitiatorParameters; }
		}

		public override SecurityTokenParameters CounterParameters {
			get { return security.RecipientParameters; }
		}

		public override MessageDirection Direction {
			get { return MessageDirection.Input; }
		}

		public override EndpointAddress MessageTo {
			get { return message_to; }
		}

		public override bool ShouldIncludeToken (SecurityTokenInclusionMode mode, bool isInitialized)
		{
			switch (mode) {
			case SecurityTokenInclusionMode.Never:
			case SecurityTokenInclusionMode.AlwaysToInitiator:
				return false;
			case SecurityTokenInclusionMode.AlwaysToRecipient:
				return true;
			case SecurityTokenInclusionMode.Once:
				return !isInitialized;
			}
			throw new Exception ("Internal Error: should not happen.");
		}

		public override ScopedMessagePartSpecification SignatureParts { 
			get { return Security.ChannelRequirements.IncomingSignatureParts; }
		}

		public override ScopedMessagePartSpecification EncryptionParts { 
			get { return Security.ChannelRequirements.IncomingEncryptionParts; }
		}
	}

	internal class RecipientMessageSecurityGenerator : MessageSecurityGenerator
	{
		RecipientMessageSecurityBindingSupport security;

		public RecipientMessageSecurityGenerator (
			Message msg,
			SecurityMessageProperty requestSecProp,
			RecipientMessageSecurityBindingSupport security)
			: base (msg, security)
		{
			this.security = security;
			SecurityMessageProperty secprop =
				(SecurityMessageProperty) requestSecProp.CreateCopy ();
			msg.Properties.Security = secprop;
		}

		public override SecurityTokenParameters Parameters {
			get { return security.RecipientParameters; }
		}

		public override SecurityTokenParameters CounterParameters {
			get { return security.InitiatorParameters; }
		}

		public override MessageDirection Direction {
			get { return MessageDirection.Output; }
		}

		public override EndpointAddress MessageTo {
			get { return null; }
		}

		public override bool ShouldIncludeToken (SecurityTokenInclusionMode mode, bool isInitialized)
		{
			switch (mode) {
			case SecurityTokenInclusionMode.Never:
			case SecurityTokenInclusionMode.AlwaysToRecipient:
				return false;
			case SecurityTokenInclusionMode.AlwaysToInitiator:
				return true;
			case SecurityTokenInclusionMode.Once:
				return !isInitialized;
			}
			throw new Exception ("Internal Error: should not happen.");
		}

		public override ScopedMessagePartSpecification SignatureParts { 
			get { return Security.ChannelRequirements.OutgoingSignatureParts; }
		}

		public override ScopedMessagePartSpecification EncryptionParts { 
			get { return Security.ChannelRequirements.OutgoingEncryptionParts; }
		}
	}

	internal abstract class MessageSecurityGenerator
	{
		Message msg;
		SecurityMessageProperty secprop;
		MessageSecurityBindingSupport security;
		int idbase;

		public MessageSecurityGenerator (Message msg, 
			MessageSecurityBindingSupport security)
		{
			this.msg = msg;
			this.security = security;
		}

		public Message Message {
			get { return msg; }
		}

		public MessageSecurityBindingSupport Security {
			get { return security; }
		}

		public abstract SecurityTokenParameters Parameters { get; }

		public abstract SecurityTokenParameters CounterParameters { get; }

		public abstract MessageDirection Direction { get; }

		public abstract EndpointAddress MessageTo { get; }

		public abstract ScopedMessagePartSpecification SignatureParts { get; }

		public abstract ScopedMessagePartSpecification EncryptionParts { get; }

		public MessagePartSpecification SignaturePart {
			get {
				MessagePartSpecification spec;
				if (!SignatureParts.TryGetParts (GetAction (), false, out spec))
					spec = SignatureParts.ChannelParts;
				return spec;
			}
		}

		public MessagePartSpecification EncryptionPart {
			get {
				MessagePartSpecification spec;
				if (!EncryptionParts.TryGetParts (GetAction (), false, out spec))
					spec = EncryptionParts.ChannelParts;
				return spec;
			}
		}

		public abstract bool ShouldIncludeToken (SecurityTokenInclusionMode mode, bool isInitialized);

		public bool ShouldOutputEncryptedKey {
			get { return Direction == MessageDirection.Input || secprop.ProtectionToken == null; } //security.Element is AsymmetricSecurityBindingElement; }
		}

		public Message SecureMessage ()
		{
			secprop = Message.Properties.Security ?? new SecurityMessageProperty ();

			SecurityToken encToken =
				secprop.InitiatorToken != null ? secprop.InitiatorToken.SecurityToken : security.EncryptionToken;
			// FIXME: it might be still incorrect.
			SecurityToken signToken =
				Parameters == CounterParameters ? null :
				security.SigningToken;
			MessageProtectionOrder protectionOrder =
				security.MessageProtectionOrder;
			SecurityTokenSerializer serializer =
				security.TokenSerializer;
			SecurityBindingElement element =
				security.Element;
			SecurityAlgorithmSuite suite = element.DefaultAlgorithmSuite;

			string messageId = "uuid-" + Guid.NewGuid ();
			int identForMessageId = 1;
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			// FIXME: get correct ReplyTo value
			if (Direction == MessageDirection.Input)
				msg.Headers.ReplyTo = new EndpointAddress (Constants.WsaAnonymousUri);

			if (MessageTo != null)
				msg.Headers.To = MessageTo.Uri;

			// wss:Security
			WSSecurityMessageHeader header =
				new WSSecurityMessageHeader (serializer);
			msg.Headers.Add (header);
			// 1. [Timestamp]
			if (element.IncludeTimestamp) {
				WsuTimestamp timestamp = new WsuTimestamp ();
				timestamp.Id = messageId + "-" + identForMessageId++;
				timestamp.Created = DateTime.Now;
				// FIXME: on service side, use element.LocalServiceSettings.TimestampValidityDuration
				timestamp.Expires = timestamp.Created.Add (element.LocalClientSettings.TimestampValidityDuration);
				header.AddContent (timestamp);
			}

			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", msg.Version.Envelope.Namespace);
			nsmgr.AddNamespace ("o", Constants.WssNamespace);
			nsmgr.AddNamespace ("u", Constants.WsuNamespace);
			nsmgr.AddNamespace ("o11", Constants.Wss11Namespace);

			/*WrappedKey*/SecurityToken primaryToken = null;
			DerivedKeySecurityToken dkeyToken = null;
			SecurityToken actualToken = null;
			SecurityKeyIdentifierClause actualClause = null;
			Signature sig = null;

			List<DerivedKeySecurityToken> derivedKeys =
				new List<DerivedKeySecurityToken> ();

			SymmetricAlgorithm masterKey = new RijndaelManaged ();
			masterKey.KeySize = suite.DefaultSymmetricKeyLength;
			masterKey.Mode = CipherMode.CBC;
			masterKey.Padding = PaddingMode.ISO10126;
			SymmetricAlgorithm actualKey = masterKey;

			// 2. [Encryption Token]

			// SecurityTokenInclusionMode
			// - Initiator or Recipient
			// - done or notyet. FIXME: not implemented yet
			// It also affects on key reference output

			bool includeEncToken = // /* FIXME: remove this hack */Parameters is SslSecurityTokenParameters ? false :
						ShouldIncludeToken (
				Security.RecipientParameters.InclusionMode, false);
			bool includeSigToken = // /* FIXME: remove this hack */ Parameters is SslSecurityTokenParameters ? false :
						ShouldIncludeToken (
				Security.InitiatorParameters.InclusionMode, false);

			SecurityKeyIdentifierClause encClause = ShouldOutputEncryptedKey ?
				CounterParameters.CallCreateKeyIdentifierClause (encToken, !ShouldOutputEncryptedKey ? SecurityTokenReferenceStyle.Internal : includeEncToken ? Parameters.ReferenceStyle : SecurityTokenReferenceStyle.External) : null;

			MessagePartSpecification sigSpec = SignaturePart;
			MessagePartSpecification encSpec = EncryptionPart;

			// encryption key (possibly also used for signing)
			// FIXME: get correct SymmetricAlgorithm according to the algorithm suite
			if (secprop.EncryptionKey != null)
				actualKey.Key = secprop.EncryptionKey;

// FIXME: remove thid hack
if (!ShouldOutputEncryptedKey)
primaryToken = secprop.ProtectionToken.SecurityToken as WrappedKeySecurityToken;
else
			primaryToken =
				// FIXME: remove this hack?
				encToken is SecurityContextSecurityToken ? encToken :
				new WrappedKeySecurityToken (messageId + "-" + identForMessageId++,
				actualKey.Key,
				// security.DefaultKeyWrapAlgorithm,
				Parameters.InternalHasAsymmetricKey ?
					suite.DefaultAsymmetricKeyWrapAlgorithm :
					suite.DefaultSymmetricKeyWrapAlgorithm,
				encToken,
				encClause != null ? new SecurityKeyIdentifier (encClause) : null);

			// If it reuses request's encryption key, do not output.
			if (ShouldOutputEncryptedKey)
				header.AddContent (primaryToken);

			actualToken = primaryToken;

			// FIXME: I doubt it is correct...
			WrappedKeySecurityToken requestEncKey = ShouldOutputEncryptedKey ? null : primaryToken as WrappedKeySecurityToken;
			actualClause = requestEncKey == null ? (SecurityKeyIdentifierClause)
				new LocalIdKeyIdentifierClause (actualToken.Id, typeof (WrappedKeySecurityToken)) :
				new InternalEncryptedKeyIdentifierClause (SHA1.Create ().ComputeHash (requestEncKey.GetWrappedKey ()));

			// generate derived key if needed
			if (CounterParameters.RequireDerivedKeys) {
				RijndaelManaged deriv = new RijndaelManaged ();
				deriv.KeySize = suite.DefaultEncryptionKeyDerivationLength;
				deriv.Mode = CipherMode.CBC;
				deriv.Padding = PaddingMode.ISO10126;
				deriv.GenerateKey ();
				dkeyToken = new DerivedKeySecurityToken (
					GenerateId (doc),
					null, // algorithm
					actualClause,
					new InMemorySymmetricSecurityKey (actualKey.Key),
					null, // name
					null, // generation
					null, // offset
					deriv.Key.Length,
					null, // label
					deriv.Key);
				derivedKeys.Add (dkeyToken);
				actualToken = dkeyToken;
				actualKey.Key = ((SymmetricSecurityKey) dkeyToken.SecurityKeys [0]).GetSymmetricKey ();
				actualClause = new LocalIdKeyIdentifierClause (dkeyToken.Id);
				header.AddContent (dkeyToken);
			}

			ReferenceList refList = new ReferenceList ();
			// When encrypted with DerivedKeyToken, put references
			// immediately after the derived token (not inside the
			// primary token).
			// Similarly, when we do not output EncryptedKey,
			// output ReferenceList in the same way.
			if (CounterParameters.RequireDerivedKeys ||
			    !ShouldOutputEncryptedKey)
				header.AddContent (refList);
			else
				((WrappedKeySecurityToken) primaryToken).ReferenceList = refList;

			// [Signature Confirmation]
			if (security.RequireSignatureConfirmation && secprop.ConfirmedSignatures.Count > 0)
				foreach (string value in secprop.ConfirmedSignatures)
					header.AddContent (new Wss11SignatureConfirmation (GenerateId (doc), value));

			SupportingTokenInfoCollection tokenInfos =
				Direction == MessageDirection.Input ?
				security.CollectSupportingTokens (GetAction ()) :
				new SupportingTokenInfoCollection (); // empty

			foreach (SupportingTokenInfo tinfo in tokenInfos)
				header.AddContent (tinfo.Token);

			// populate DOM to sign.
			XPathNavigator nav = doc.CreateNavigator ();
			using (XmlWriter w = nav.AppendChild ()) {
				msg.WriteMessage (w);
			}

			XmlElement body = doc.SelectSingleNode ("/s:Envelope/s:Body/*", nsmgr) as XmlElement;
			string bodyId = null;
			XmlElement secElem = null;
			Collection<WSSignedXml> endorsedSignatures =
				new Collection<WSSignedXml> ();
			bool signatureProtection = (protectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature);

			// Below are o:Security contents that are not signed...
			if (includeSigToken && signToken != null)
				header.AddContent (signToken);

			switch (protectionOrder) {
			case MessageProtectionOrder.EncryptBeforeSign:
				// FIXME: implement
				throw new NotImplementedException ();
			case MessageProtectionOrder.SignBeforeEncrypt:
			case MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature:

				// sign
				// see clause 8 of WS-SecurityPolicy C.2.2
				WSSignedXml sxml = new WSSignedXml (doc);
				SecurityTokenReferenceKeyInfo sigKeyInfo;

				sig = sxml.Signature;
				sig.SignedInfo.CanonicalizationMethod =
					suite.DefaultCanonicalizationAlgorithm;
				foreach (XmlElement elem in doc.SelectNodes ("/s:Envelope/s:Header/o:Security/u:Timestamp", nsmgr))
					CreateReference (sig, elem, elem.GetAttribute ("Id", Constants.WsuNamespace));
				foreach (XmlElement elem in doc.SelectNodes ("/s:Envelope/s:Header/o:Security/o11:SignatureConfirmation", nsmgr))
					CreateReference (sig, elem, elem.GetAttribute ("Id", Constants.WsuNamespace));
				foreach (SupportingTokenInfo tinfo in tokenInfos)
					if (tinfo.Mode != SecurityTokenAttachmentMode.Endorsing) {
						XmlElement el = sxml.GetIdElement (doc, tinfo.Token.Id);
						CreateReference (sig, el, el.GetAttribute ("Id", Constants.WsuNamespace));
					}
				XmlNodeList nodes = doc.SelectNodes ("/s:Envelope/s:Header/*", nsmgr);
				for (int i = 0; i < msg.Headers.Count; i++) {
					MessageHeaderInfo h = msg.Headers [i];
					if (h.Name == "Security" && h.Namespace == Constants.WssNamespace)
						secElem = nodes [i] as XmlElement;
					else if (sigSpec.HeaderTypes.Count == 0 ||
					    sigSpec.HeaderTypes.Contains (new XmlQualifiedName (h.Name, h.Namespace))) {
						string id = GenerateId (doc);
						h.Id = id;
						CreateReference (sig, nodes [i] as XmlElement, id);
					}
				}
				if (sigSpec.IsBodyIncluded) {
					bodyId = GenerateId (doc);
					CreateReference (sig, body.ParentNode as XmlElement, bodyId);
				}

				if (security.DefaultSignatureAlgorithm == SignedXml.XmlDsigHMACSHA1Url) {
					// FIXME: use appropriate hash algorithm
					sxml.ComputeSignature (new HMACSHA1 (actualKey.Key));
					sigKeyInfo = new SecurityTokenReferenceKeyInfo (actualClause, serializer, doc);
				}
				else {
					SecurityKeyIdentifierClause signClause =
						CounterParameters.CallCreateKeyIdentifierClause (signToken, includeSigToken ? CounterParameters.ReferenceStyle : SecurityTokenReferenceStyle.External);
					AsymmetricSecurityKey signKey = (AsymmetricSecurityKey) signToken.ResolveKeyIdentifierClause (signClause);
					sxml.SigningKey = signKey.GetAsymmetricAlgorithm (security.DefaultSignatureAlgorithm, true);
					sxml.ComputeSignature ();
					sigKeyInfo = new SecurityTokenReferenceKeyInfo (signClause, serializer, doc);
				}

				sxml.KeyInfo = new KeyInfo ();
				sxml.KeyInfo.AddClause (sigKeyInfo);

				if (!signatureProtection)
					header.AddContent (sig);

				// endorse the signature with (signed)endorsing
				// supporting tokens.

				foreach (SupportingTokenInfo tinfo in tokenInfos) {
					switch (tinfo.Mode) {
					case SecurityTokenAttachmentMode.Endorsing:
					case SecurityTokenAttachmentMode.SignedEndorsing:
						if (sxml.Signature.Id == null) {
							sig.Id = GenerateId (doc);
							secElem.AppendChild (sxml.GetXml ());
						}
						WSSignedXml ssxml = new WSSignedXml (doc);
						ssxml.Signature.SignedInfo.CanonicalizationMethod = suite.DefaultCanonicalizationAlgorithm;
						CreateReference (ssxml.Signature, doc, sig.Id);
						SecurityToken sst = tinfo.Token;
						SecurityKey ssk = sst.SecurityKeys [0]; // FIXME: could be different?
						SecurityKeyIdentifierClause tclause = new LocalIdKeyIdentifierClause (sst.Id); // FIXME: could be different?
						if (ssk is SymmetricSecurityKey) {
							SymmetricSecurityKey signKey = (SymmetricSecurityKey) ssk;
							ssxml.ComputeSignature (signKey.GetKeyedHashAlgorithm (suite.DefaultSymmetricSignatureAlgorithm));
						} else {
							AsymmetricSecurityKey signKey = (AsymmetricSecurityKey) ssk;
							ssxml.SigningKey = signKey.GetAsymmetricAlgorithm (suite.DefaultAsymmetricSignatureAlgorithm, true);
							ssxml.ComputeSignature ();
						}
						ssxml.KeyInfo.AddClause (new SecurityTokenReferenceKeyInfo (tclause, serializer, doc));
						if (!signatureProtection)
							header.AddContent (ssxml.Signature);
						endorsedSignatures.Add (ssxml);

						break;
					}
				}

				// encrypt

				WSEncryptedXml exml = new WSEncryptedXml (doc);

				EncryptedData edata = Encrypt (body, actualKey, actualToken.Id, refList, actualClause, exml, doc);
				EncryptedXml.ReplaceElement (body, edata, false);

				// encrypt signature
				if (signatureProtection) {
					XmlElement sigxml = sig.GetXml ();
					edata = Encrypt (sigxml, actualKey, actualToken.Id, refList, actualClause, exml, doc);
					header.AddContent (edata);

					foreach (WSSignedXml ssxml in endorsedSignatures) {
						sigxml = ssxml.GetXml ();
						edata = Encrypt (sigxml, actualKey, actualToken.Id, refList, actualClause, exml, doc);
						header.AddContent (edata);
					}

					if (security.RequireSignatureConfirmation) {
						Collection<Wss11SignatureConfirmation> confs = header.FindAll<Wss11SignatureConfirmation> ();
						int count = 0;
						foreach (XmlElement elem in doc.SelectNodes ("/s:Envelope/s:Header/o:Security/o11:SignatureConfirmation", nsmgr)) {
							edata = Encrypt (elem, actualKey, confs [count].Id, refList, actualClause, exml, doc);
							EncryptedXml.ReplaceElement (elem, edata, false);
							header.Contents.Insert (header.Contents.IndexOf (confs [count]), edata);
							header.Contents.Remove (confs [count++]);
						}
					}
				}

				// encrypt Encrypted supporting tokens
				foreach (SupportingTokenInfo tinfo in tokenInfos) {
					if (tinfo.Mode == SecurityTokenAttachmentMode.SignedEncrypted) {
						XmlElement el = exml.GetIdElement (doc, tinfo.Token.Id);
						tinfo.Encrypted = Encrypt (el, actualKey, actualToken.Id, refList, actualClause, exml, doc);
						EncryptedXml.ReplaceElement (el, tinfo.Encrypted, false);
						header.Contents.Insert (header.Contents.IndexOf (tinfo.Token), tinfo.Encrypted);
						header.Contents.Remove (tinfo.Token);
					}
				}
				break;
			}

			Message ret = new WSSecurityMessage (Message.CreateMessage (msg.Version, msg.Headers.Action, new XmlNodeReader (doc.SelectSingleNode ("/s:Envelope/s:Body/*", nsmgr) as XmlElement)), bodyId);
			ret.Properties.Security = (SecurityMessageProperty) secprop.CreateCopy ();
			ret.Properties.Security.EncryptionKey = masterKey.Key;

			// FIXME: can we support TransportToken here?
			if (element is AsymmetricSecurityBindingElement) {
				ret.Properties.Security.InitiatorToken = new SecurityTokenSpecification (encToken, null); // FIXME: second argument
				ret.Properties.Security.InitiatorToken = new SecurityTokenSpecification (signToken, null); // FIXME: second argument
			}
			else
				ret.Properties.Security.ProtectionToken = new SecurityTokenSpecification (primaryToken, null);

			ret.Headers.Clear ();
			ret.Headers.CopyHeadersFrom (msg);

			// Header contents are:
			//	- Timestamp
			//	- SignatureConfirmation if required
			//	- EncryptionToken if included
			//	- derived key token for EncryptionToken
			//	- ReferenceList for encrypted items
			//	- signed supporting tokens
			//	- signed endorsing supporting tokens
			//	(i.e. Signed/SignedEncrypted/SignedEndorsing)
			//	- Signature Token if different from enc token.
			//	- derived key token for sig token if different
			//	- Signature for:
			//		- Timestamp
			//		- supporting tokens (regardless of
			//		  its inclusion)
			//		- message parts in SignedParts
			//		- SignatureToken if TokenProtection
			//		  (regardless of its inclusion)
			//	- Signatures for the main signature (above),
			//	  for every endorsing token and signed
			//	  endorsing token.
			//	

//MessageBuffer zzz = ret.CreateBufferedCopy (100000);
//ret = zzz.CreateMessage ();
//Console.WriteLine (zzz.CreateMessage ());
			return ret;
		}

		void CreateReference (Signature sig, XmlElement el, string id)
		{
			CreateReference (sig, el.OwnerDocument, id);

			if (el.GetAttribute ("Id", Constants.WsuNamespace) != id) {
				XmlAttribute a = el.SetAttributeNode ("Id", Constants.WsuNamespace);
				a.Prefix = "u";
				a.Value = id;
			}
		}

		void CreateReference (Signature sig, XmlDocument doc, string id)
		{
			SecurityAlgorithmSuite suite = security.Element.DefaultAlgorithmSuite;
			if (id == String.Empty)
				id = GenerateId (doc);
			Reference r = new Reference ("#" + id);
			r.AddTransform (CreateTransform (suite.DefaultCanonicalizationAlgorithm));
			r.DigestMethod = suite.DefaultDigestAlgorithm;
			sig.SignedInfo.AddReference (r);
		}

		Transform CreateTransform (string url)
		{
			switch (url) {
			case SignedXml.XmlDsigC14NTransformUrl:
				return new XmlDsigC14NTransform ();
			case SignedXml.XmlDsigC14NWithCommentsTransformUrl:
				return new XmlDsigC14NWithCommentsTransform ();
			case SignedXml.XmlDsigExcC14NTransformUrl:
				return new XmlDsigExcC14NTransform ();
			case SignedXml.XmlDsigExcC14NWithCommentsTransformUrl:
				return new XmlDsigExcC14NWithCommentsTransform ();
			}
			throw new Exception (String.Format ("INTERNAL ERROR: Invalid canonicalization URL: {0}", url));
		}

		EncryptedData Encrypt (XmlElement target, SymmetricAlgorithm actualKey, string ekeyId, ReferenceList refList, SecurityKeyIdentifierClause encClause, EncryptedXml exml, XmlDocument doc)
		{
			SecurityAlgorithmSuite suite = security.Element.DefaultAlgorithmSuite;
			SecurityTokenSerializer serializer = security.TokenSerializer;

			byte [] encrypted = exml.EncryptData (target, actualKey, false);
			EncryptedData edata = new EncryptedData ();
			edata.Id = GenerateId (doc);
			edata.Type = EncryptedXml.XmlEncElementContentUrl;
			edata.EncryptionMethod = new EncryptionMethod (suite.DefaultEncryptionAlgorithm);
			// FIXME: here wsse:DigestMethod should be embedded 
			// inside EncryptionMethod. Since it is not possible 
			// with S.S.C.Xml.EncryptionMethod, we will have to
			// build our own XML encryption classes.

			edata.CipherData.CipherValue = encrypted;

			DataReference dr = new DataReference ();
			dr.Uri = "#" + edata.Id;
			refList.Add (dr);

			if (ShouldOutputEncryptedKey && !CounterParameters.RequireDerivedKeys)
				edata.KeyInfo = null;
			else {
				edata.KeyInfo = new KeyInfo ();
				edata.KeyInfo.AddClause (new SecurityTokenReferenceKeyInfo (encClause, serializer, doc));
			}

			return edata;
		}

		string GenerateId (XmlDocument doc)
		{
			idbase++;
			return secprop.SenderIdPrefix + idbase;
		}

		public string GetAction ()
		{
			string ret = msg.Headers.Action;
			if (ret == null) {
				HttpRequestMessageProperty reqprop =
					msg.Properties ["Action"] as HttpRequestMessageProperty;
				if (reqprop != null)
					ret = reqprop.Headers ["Action"];
			}
			return ret;
		}
	}

	internal class WSSecurityMessage : Message
	{
		Message msg;
		string body_id;

		public WSSecurityMessage (Message msg, string bodyId)
		{
			this.msg = msg;
			this.body_id = bodyId;
		}

		public override MessageVersion Version {
			get { return msg.Version; }
		}

		public override MessageHeaders Headers {
			get { return msg.Headers; }
		}

		public override MessageProperties Properties {
			get { return msg.Properties; }
		}

		protected override MessageBuffer OnCreateBufferedCopy (int maxBufferSize)
		{
			return new WSSecurityMessageBuffer (msg.CreateBufferedCopy (maxBufferSize), body_id);
		}

		protected override string OnGetBodyAttribute (string localName, string ns)
		{
			if (localName == "Id" && ns == Constants.WsuNamespace)
				return body_id;
			return msg.GetBodyAttribute (localName, ns);
		}

		protected override void OnWriteStartBody (
			XmlDictionaryWriter writer)
		{
			var dic = Constants.SoapDictionary;
			writer.WriteStartElement ("s", dic.Add ("Body"), dic.Add (Version.Envelope.Namespace));
			if (body_id != null)
				writer.WriteAttributeString ("Id", Constants.WsuNamespace, body_id);
		}

		protected override void OnWriteBodyContents (XmlDictionaryWriter w)
		{
			msg.WriteBodyContents (w);
		}
	}
	
	internal class WSSecurityMessageBuffer : MessageBuffer
	{
		public WSSecurityMessageBuffer (MessageBuffer mb, string bodyId)
		{
			buffer = mb;
			body_id = bodyId;
		}
		
		MessageBuffer buffer;
		string body_id;
		
		public override int BufferSize {
			get { return buffer.BufferSize; }
		}
		
		public override void Close ()
		{
			buffer.Close ();
		}
		
		public override Message CreateMessage ()
		{
			return new WSSecurityMessage (buffer.CreateMessage (), body_id);
		}
	}
}
