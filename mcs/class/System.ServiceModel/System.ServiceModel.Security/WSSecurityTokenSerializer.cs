//
// WSSecurityTokenSerializer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Security
{
	public class WSSecurityTokenSerializer : SecurityTokenSerializer
	{
		static WSSecurityTokenSerializer default_instance =
			new WSSecurityTokenSerializer ();

		public static WSSecurityTokenSerializer DefaultInstance {
			get { return default_instance; }
		}

		const int defaultOffset = 64,
			defaultLabelLength = 128,
			defaultNonceLength = 128;

		public WSSecurityTokenSerializer ()
			: this (false)
		{
		}

		public WSSecurityTokenSerializer (bool emitBspRequiredAttributes)
			: this (SecurityVersion.WSSecurity11, emitBspRequiredAttributes)
		{
		}

		public WSSecurityTokenSerializer (SecurityVersion securityVersion)
			: this (securityVersion, false)
		{
		}

		public WSSecurityTokenSerializer (SecurityVersion securityVersion, bool emitBspRequiredAttributes)
			: this (securityVersion, emitBspRequiredAttributes, new SamlSerializer ())
		{
		}

		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer)
			: this (securityVersion, emitBspRequiredAttributes, 
				samlSerializer, null, null)
		{
		}

		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer,
			SecurityStateEncoder securityStateEncoder,
			IEnumerable<Type> knownTypes)
			: this (securityVersion, emitBspRequiredAttributes, 
				samlSerializer, securityStateEncoder,
				knownTypes, defaultOffset, defaultLabelLength,
				defaultNonceLength)
		{
		}
		
		public WSSecurityTokenSerializer (
			SecurityVersion securityVersion,
			bool emitBspRequiredAttributes,
			SamlSerializer samlSerializer,
			SecurityStateEncoder securityStateEncoder,
			IEnumerable<Type> knownTypes,
			int maximumKeyDerivationOffset,
			int maximumKeyDerivationLabelLength,
			int maximumKeyDerivationNonceLength)
		{
			security_version = securityVersion;
			emit_bsp = emitBspRequiredAttributes;
			saml_serializer = samlSerializer;
			encoder = securityStateEncoder;
			known_types = new List<Type> (knownTypes ?? Type.EmptyTypes);
			max_offset = maximumKeyDerivationOffset;
			max_label_length = maximumKeyDerivationLabelLength;
			max_nonce_length = maximumKeyDerivationNonceLength;

			if (encoder == null)
				encoder = new DataProtectionSecurityStateEncoder ();
		}

		SecurityVersion security_version;
		bool emit_bsp;
		SamlSerializer saml_serializer;
		SecurityStateEncoder encoder;
		List<Type> known_types;
		int max_offset, max_label_length, max_nonce_length;

		bool WSS1_0 {
			get { return SecurityVersion == SecurityVersion.WSSecurity10; }
		}

		public bool EmitBspRequiredAttributes {
			get { return emit_bsp; }
		}

		public SecurityVersion SecurityVersion {
			get { return security_version; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationOffset {
			get { return max_offset; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationLabelLength {
			get { return max_label_length; }
		}

		[MonoTODO]
		public int MaximumKeyDerivationNonceLength {
			get { return max_nonce_length; }
		}

		protected virtual string GetTokenTypeUri (Type tokenType)
		{
			if (tokenType == typeof (WrappedKeySecurityToken))
				return Constants.WSSEncryptedKeyToken;
			if (tokenType == typeof (X509SecurityToken))
				return Constants.WSSX509Token;
//			if (tokenType == typeof (RsaSecurityToken))
//				return null;
			if (tokenType == typeof (SamlSecurityToken))
				return Constants.WSSSamlToken;
			if (tokenType == typeof (SecurityContextSecurityToken))
				return Constants.WsscContextToken;
//			if (tokenType == typeof (huh))
//				return ServiceModelSecurityTokenTypes.SecureConversation;
//			if (tokenType == typeof (hah))
//				return ServiceModelSecurityTokenTypes.MutualSslnego;
//			if (tokenType == typeof (whoa))
//				return ServiceModelSecurityTokenTypes.AnonymousSslnego;
			if (tokenType == typeof (UserNameSecurityToken))
				return Constants.WSSUserNameToken;
//			if (tokenType == typeof (uhoh))
//				return ServiceModelSecurityTokenTypes.Spnego;
//			if (tokenType == typeof (SspiSecurityToken))
//				return ServiceModelSecurityTokenTypes.SspiCredential;
			if (tokenType == typeof (KerberosRequestorSecurityToken))
				return Constants.WSSKerberosToken;
			return null;
		}

		[MonoTODO]
		protected override bool CanReadKeyIdentifierClauseCore (XmlReader reader)
		{
			reader.MoveToContent ();
			switch (reader.NamespaceURI) {
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedKey":
					return true;
				}
				break;
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "SecurityTokenReference":
					return true;
				}
				break;
			}

			return false;
		}

		[MonoTODO]
		protected override bool CanReadKeyIdentifierCore (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanReadTokenCore (XmlReader reader)
		{
			reader.MoveToContent ();

			switch (reader.NamespaceURI) {
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "BinarySecurityToken":
				case "BinarySecret":
				case "UsernameToken":
					return true;
				}
				break;
			case Constants.WsscNamespace:
				switch (reader.LocalName) {
				case "DerivedKeyToken":
				case "SecurityContextToken":
					return true;
				}
				break;
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedKey":
					return true;
				}
				break;
			}
			return false;
		}

		[MonoTODO]
		public virtual SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml (
			XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override SecurityKeyIdentifier ReadKeyIdentifierCore (
			XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore (XmlReader reader)
		{
			reader.MoveToContent ();
			switch (reader.NamespaceURI) {
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedKey":
					return ReadEncryptedKeyIdentifierClause (reader);
				}
				break;
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "SecurityTokenReference":
					return ReadSecurityTokenReference (reader);
				}
				break;
			}

			throw new NotImplementedException (String.Format ("Security key identifier clause element '{0}' in namespace '{1}' is either not implemented or not supported.", reader.LocalName, reader.NamespaceURI));
		}

		SecurityKeyIdentifierClause ReadSecurityTokenReference (XmlReader reader)
		{
			reader.ReadStartElement ();
			reader.MoveToContent ();
			if (reader.NamespaceURI == SignedXml.XmlDsigNamespaceUrl) {
				KeyInfoX509Data x509 = new KeyInfoX509Data ();
				x509.LoadXml (new XmlDocument ().ReadNode (reader) as XmlElement);
				if (x509.IssuerSerials.Count == 0)
					throw new XmlException ("'X509IssuerSerial' element is expected inside 'X509Data' element");
				X509IssuerSerial s = (X509IssuerSerial) x509.IssuerSerials [0];
				reader.MoveToContent ();
				reader.ReadEndElement ();
				return new X509IssuerSerialKeyIdentifierClause (s.IssuerName, s.SerialNumber);
			}
			if (reader.NamespaceURI != Constants.WssNamespace)
				throw new XmlException (String.Format ("Unexpected SecurityTokenReference content: expected local name 'Reference' and namespace URI '{0}' but found local name '{1}' and namespace '{2}'.", Constants.WssNamespace, reader.LocalName, reader.NamespaceURI));

			switch (reader.LocalName) {
			case "Reference":
				Type ownerType = null;
				// FIXME: there could be more token types.
				if (reader.MoveToAttribute ("ValueType")) {
					switch (reader.Value) {
					case Constants.WSSEncryptedKeyToken:
						ownerType = typeof (WrappedKeySecurityToken);
						break;
					case Constants.WSSX509Token:
						ownerType = typeof (X509SecurityToken);
						break;
					case Constants.WsscContextToken:
						ownerType = typeof (SecurityContextSecurityToken);
						break;
					default:
						throw new XmlException (String.Format ("Unexpected ValueType in 'Reference' element: '{0}'", reader.Value));
					}
				}
				reader.MoveToElement ();
				string uri = reader.GetAttribute ("URI");
				if (String.IsNullOrEmpty (uri))
					uri = "#";
				SecurityKeyIdentifierClause ic = null;
				if (ownerType == typeof (SecurityContextSecurityToken) && uri [0] != '#')
					// FIXME: Generation?
					ic = new SecurityContextKeyIdentifierClause (new UniqueId (uri));
				else
				 ic = new LocalIdKeyIdentifierClause (uri.Substring (1), ownerType);
				reader.Skip ();
				reader.MoveToContent ();
				reader.ReadEndElement ();
				return ic;
			case "KeyIdentifier":
				string valueType = reader.GetAttribute ("ValueType");
				string value = reader.ReadElementContentAsString ();
				reader.MoveToContent ();
				reader.ReadEndElement (); // consume </Reference>
				switch (valueType) {
				case Constants.WssKeyIdentifierX509Thumbptint:
					return new X509ThumbprintKeyIdentifierClause (Convert.FromBase64String (value));
				case Constants.WssKeyIdentifierEncryptedKey:
					return new InternalEncryptedKeyIdentifierClause (Convert.FromBase64String (value));
				case Constants.WssKeyIdentifierSamlAssertion:
					return new SamlAssertionKeyIdentifierClause (value);
				default:
					// It is kinda weird but it throws XmlException here ...
					throw new XmlException (String.Format ("KeyIdentifier type '{0}' is not supported in WSSecurityTokenSerializer.", valueType));
				}
			default:
				throw new XmlException (String.Format ("Unexpected SecurityTokenReference content: expected local name 'Reference' and namespace URI '{0}' but found local name '{1}' and namespace '{2}'.", Constants.WssNamespace, reader.LocalName, reader.NamespaceURI));
			}
		}

		EncryptedKeyIdentifierClause ReadEncryptedKeyIdentifierClause (
			XmlReader reader)
		{
			string encNS = EncryptedXml.XmlEncNamespaceUrl;

			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			reader.Read ();
			reader.MoveToContent ();
			string encMethod = reader.GetAttribute ("Algorithm");
			bool isEmpty = reader.IsEmptyElement;
			reader.ReadStartElement ("EncryptionMethod", encNS);
			string digMethod = null;
			if (!isEmpty) {
				reader.MoveToContent ();
				if (reader.LocalName == "DigestMethod" && reader.NamespaceURI == SignedXml.XmlDsigNamespaceUrl)
					digMethod = reader.GetAttribute ("Algorithm");
				while (reader.NodeType != XmlNodeType.EndElement) {
					reader.Skip ();
					reader.MoveToContent ();
				}
				reader.ReadEndElement ();
			}
			reader.MoveToContent ();
			SecurityKeyIdentifier ki = null;
			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
				reader.MoveToContent ();
				SecurityKeyIdentifierClause kic = ReadKeyIdentifierClauseCore (reader);
				ki = new SecurityKeyIdentifier ();
				ki.Add (kic);
				reader.MoveToContent ();
				reader.ReadEndElement (); // </ds:KeyInfo>
				reader.MoveToContent ();
			}
			byte [] keyValue = null;
			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ("CipherData", encNS);
				reader.MoveToContent ();
				keyValue = Convert.FromBase64String (reader.ReadElementContentAsString ("CipherValue", encNS));
				reader.MoveToContent ();
				reader.ReadEndElement (); // CipherData
			}
			string carriedKeyName = null;
			if (!reader.IsEmptyElement && reader.LocalName == "CarriedKeyName" && reader.NamespaceURI == encNS) {
				carriedKeyName = reader.ReadElementContentAsString ();
				reader.MoveToContent ();
			}
			// FIXME: handle derived keys??
			return new EncryptedKeyIdentifierClause (keyValue, encMethod, ki, carriedKeyName);
		}

		[MonoTODO]
		protected override SecurityToken ReadTokenCore (
			XmlReader reader,
			SecurityTokenResolver tokenResolver)
		{
			if (!CanReadToken (reader))
				throw new XmlException (String.Format ("Cannot read security token from {0} node of name '{1}' and namespace URI '{2}'", reader.NodeType, reader.LocalName, reader.NamespaceURI));

			switch (reader.NamespaceURI) {
			case Constants.WssNamespace:
				switch (reader.LocalName) {
				case "BinarySecurityToken":
					return ReadX509TokenCore (reader, tokenResolver);
				case "BinarySecret":
					return ReadBinarySecretTokenCore (reader, tokenResolver);
				case "UsernameToken":
					return ReadUserNameTokenCore (reader, tokenResolver);
				}
				break;
			case Constants.WsscNamespace:
				if (reader.LocalName == "DerivedKeyToken")
					return ReadDerivedKeyToken (reader, tokenResolver);
				if (reader.LocalName == "SecurityContextToken")
					return ReadSecurityContextToken (reader, tokenResolver);
				break;
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedKey":
					return ReadWrappedKeySecurityTokenCore (reader, tokenResolver);
				}
				break;
			}

			throw new NotImplementedException ();
		}

		DerivedKeySecurityToken ReadDerivedKeyToken (
			XmlReader reader, SecurityTokenResolver tokenResolver)
		{
			try {
				return ReadDerivedKeyTokenCore (reader, tokenResolver);
			} catch (XmlException) {
				throw;
			} catch (Exception ex) {
				throw new XmlException ("Cannot read DerivedKeyToken", ex);
			}
		}
		
		DerivedKeySecurityToken ReadDerivedKeyTokenCore (
			XmlReader reader, SecurityTokenResolver tokenResolver)
		{
			if (tokenResolver == null)
				throw new ArgumentNullException ("tokenResolver");
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			string algorithm = reader.MoveToAttribute ("Algorithm") ? reader.Value : null;
			reader.MoveToElement ();
			reader.ReadStartElement ();
			reader.MoveToContent ();
			SecurityKeyIdentifierClause kic = ReadKeyIdentifierClause (reader);
			int? generation = null, offset = null, length = null;
			byte [] nonce = null;
			string name = null, label = null;
			for (reader.MoveToContent ();
			       reader.NodeType != XmlNodeType.EndElement;
			       reader.MoveToContent ())
				switch (reader.LocalName) {
				case "Properties":
					reader.ReadStartElement ("Properties", Constants.WsscNamespace);
					for (reader.MoveToContent ();
					       reader.NodeType != XmlNodeType.EndElement;
					       reader.MoveToContent ())
						switch (reader.LocalName) {
						case "Name":
							name = reader.ReadElementContentAsString ("Name", Constants.WsscNamespace);
							break;
						case "Label":
							label = reader.ReadElementContentAsString ("Label", Constants.WsscNamespace);
							break;
						case "Nonce":
							nonce = Convert.FromBase64String (reader.ReadElementContentAsString ("Nonce", Constants.WsscNamespace));
							break;
						}
					reader.ReadEndElement ();
					break;
				case "Offset":
					offset = reader.ReadElementContentAsInt ("Offset", Constants.WsscNamespace);
					break;
				case "Length":
					length = reader.ReadElementContentAsInt ("Length", Constants.WsscNamespace);
					break;
				case "Nonce":
					nonce = Convert.FromBase64String (reader.ReadElementContentAsString ("Nonce", Constants.WsscNamespace));
					break;
				case "Label":
					label = reader.ReadElementContentAsString ("Label", Constants.WsscNamespace);
					break;
				}
			reader.ReadEndElement ();

			// resolve key reference
			SymmetricSecurityKey key = tokenResolver.ResolveSecurityKey (kic) as SymmetricSecurityKey;
			if (key == null)
				throw new XmlException ("Cannot resolve the security key referenced by the DerivedKeyToken as a symmetric key");

			return new DerivedKeySecurityToken (id, algorithm, kic, key, name, generation, offset, length, label, nonce);
		}

		// since it cannot consume RequestSecurityTokenResponse,
		// the token information cannot be complete.
		SecurityContextSecurityToken ReadSecurityContextToken (
			XmlReader reader, SecurityTokenResolver tokenResolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			reader.Read ();

			// The input dnse:Cookie value is encrypted by the
			// server's SecurityStateEncoder
			// (setting error-raising encoder to ServiceCredentials.
			// SecureConversationAuthentication.SecurityStateEncoder
			// shows it).
			UniqueId cid = null;
			byte [] cookie = null;
			while (true) {
				reader.MoveToContent ();
				if (reader.NodeType != XmlNodeType.Element)
					break;
				switch (reader.NamespaceURI) {
				case Constants.WsscNamespace:
					switch (reader.LocalName) {
					case "Identifier":
						cid = new UniqueId (reader.ReadElementContentAsString ());
						continue;
					}
					break;
				case Constants.MSTlsnegoTokenContent:
					switch (reader.LocalName) {
					case "Cookie":
						cookie = Convert.FromBase64String (reader.ReadElementContentAsString ());
						continue;
					}
					break;
				}
				throw new XmlException (String.Format ("Unexpected element {0} in namespace {1}", reader.LocalName, reader.NamespaceURI));
			}
			reader.ReadEndElement ();

			// LAMESPEC: at client side there is no way to specify
			// SecurityStateEncoder, so it must be guessed from
			// its cookie content itself.
			if (encoder == null) throw new Exception ();
			byte [] decoded =
				cookie != null && cookie.Length > 154 ?
				encoder.DecodeSecurityState (cookie) :
				cookie;
			return SslnegoCookieResolver.ResolveCookie (decoded, cookie);
		}

		WrappedKeySecurityToken ReadWrappedKeySecurityTokenCore (
			XmlReader reader, SecurityTokenResolver tokenResolver)
		{
			if (tokenResolver == null)
				throw new ArgumentNullException ("tokenResolver");
			EncryptedKey ek = new EncryptedKey ();
			ek.LoadXml (new XmlDocument ().ReadNode (reader) as XmlElement);
			SecurityKeyIdentifier ki = new SecurityKeyIdentifier ();
			foreach (KeyInfoClause kic in ek.KeyInfo)
				ki.Add (ReadKeyIdentifierClause (new XmlNodeReader (kic.GetXml ())));
			SecurityToken token = tokenResolver.ResolveToken (ki);
			string alg = ek.EncryptionMethod.KeyAlgorithm;
			foreach (SecurityKey skey in token.SecurityKeys)
				if (skey.IsSupportedAlgorithm (alg)) {
					byte [] key = skey.DecryptKey (alg, ek.CipherData.CipherValue);
					WrappedKeySecurityToken wk =
						new WrappedKeySecurityToken (ek.Id, key, alg, token, ki);
					// FIXME: This should not be required.
					wk.SetWrappedKey (ek.CipherData.CipherValue);
					wk.ReferenceList = ek.ReferenceList;
					return wk;
				}
			throw new InvalidOperationException (String.Format ("Cannot resolve security key with the resolved SecurityToken specified by the key identifier in the EncryptedKey XML. The key identifier is: {0}", ki));
		}

		X509SecurityToken ReadX509TokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			byte [] raw = Convert.FromBase64String (reader.ReadElementContentAsString ());
			return new X509SecurityToken (new X509Certificate2 (raw), id);
		}

		UserNameSecurityToken ReadUserNameTokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			if (reader.IsEmptyElement)
				throw new XmlException ("At least UsernameToken must contain Username");
			reader.Read ();
			reader.MoveToContent ();
			string user = reader.ReadElementContentAsString ("Username", Constants.WssNamespace);
			reader.MoveToContent ();
			string pass = null;
			if (reader.LocalName == "Password" && reader.NamespaceURI == Constants.WssNamespace) {
				pass = reader.ReadElementContentAsString ("Password", Constants.WssNamespace);
				reader.MoveToContent ();
			}
			reader.ReadEndElement ();
			return id != null ?
				new UserNameSecurityToken (user, pass, id) :
				new UserNameSecurityToken (user, pass);
		}

		BinarySecretSecurityToken ReadBinarySecretTokenCore (
			XmlReader reader, SecurityTokenResolver resolver)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			byte [] data = Convert.FromBase64String (reader.ReadElementContentAsString ());
			return new BinarySecretSecurityToken (id, data);
		}

		[MonoTODO]
		protected override bool CanWriteKeyIdentifierCore (
			SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool CanWriteKeyIdentifierClauseCore (
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			if (keyIdentifierClause is LocalIdKeyIdentifierClause ||
			    keyIdentifierClause is SecurityContextKeyIdentifierClause ||
			    keyIdentifierClause is X509IssuerSerialKeyIdentifierClause ||
			    (keyIdentifierClause is X509ThumbprintKeyIdentifierClause && !WSS1_0) ||
			    keyIdentifierClause is EncryptedKeyIdentifierClause ||
			    keyIdentifierClause is BinarySecretKeyIdentifierClause ||
			    keyIdentifierClause is InternalEncryptedKeyIdentifierClause ||
			    keyIdentifierClause is SamlAssertionKeyIdentifierClause)
				return true;
			else
				return false;
		}

		[MonoTODO]
		protected override bool CanWriteTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WriteKeyIdentifierCore (
			XmlWriter writer,
			SecurityKeyIdentifier keyIdentifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WriteKeyIdentifierClauseCore (
			XmlWriter writer,
			SecurityKeyIdentifierClause keyIdentifierClause)
		{
			string errorReason = null;

			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			if (keyIdentifierClause is LocalIdKeyIdentifierClause)
				WriteLocalIdKeyIdentifierClause (writer, (LocalIdKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is SecurityContextKeyIdentifierClause)
				WriteSecurityContextKeyIdentifierClause (writer, (SecurityContextKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is X509IssuerSerialKeyIdentifierClause)
				WriteX509IssuerSerialKeyIdentifierClause (writer, (X509IssuerSerialKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is X509ThumbprintKeyIdentifierClause) {
				if (WSS1_0)
					errorReason = String.Format ("Security key identifier clause '{0}' is not supported in this serializer.", keyIdentifierClause.GetType ());
				else
					WriteX509ThumbprintKeyIdentifierClause (writer, (X509ThumbprintKeyIdentifierClause) keyIdentifierClause);
			}
			else if (keyIdentifierClause is EncryptedKeyIdentifierClause)
				WriteEncryptedKeyIdentifierClause (writer, (EncryptedKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is BinarySecretKeyIdentifierClause)
				WriteBinarySecretKeyIdentifierClause (writer, (BinarySecretKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is InternalEncryptedKeyIdentifierClause)
				WriteInternalEncryptedKeyIdentifierClause (writer, (InternalEncryptedKeyIdentifierClause) keyIdentifierClause);
			else if (keyIdentifierClause is SamlAssertionKeyIdentifierClause)
				WriteSamlAssertionKeyIdentifierClause (writer, (SamlAssertionKeyIdentifierClause) keyIdentifierClause);
			else
				throw new NotImplementedException (String.Format ("Security key identifier clause '{0}' is not either implemented or supported.", keyIdentifierClause.GetType ()));

			if (errorReason != null)
				throw new InvalidOperationException (errorReason);
		}

		void WriteX509IssuerSerialKeyIdentifierClause (
			XmlWriter w, X509IssuerSerialKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("X509Data", Constants.XmlDsig);
			w.WriteStartElement ("X509IssuerSerial", Constants.XmlDsig);
			w.WriteStartElement ("X509IssuerName", Constants.XmlDsig);
			w.WriteString (ic.IssuerName);
			w.WriteEndElement ();
			w.WriteStartElement ("X509SerialNumber", Constants.XmlDsig);
			w.WriteString (ic.IssuerSerialNumber);
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteX509ThumbprintKeyIdentifierClause (
			XmlWriter w, X509ThumbprintKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "KeyIdentifier", Constants.WssNamespace);
			w.WriteAttributeString ("ValueType", Constants.WssKeyIdentifierX509Thumbptint);
			if (EmitBspRequiredAttributes)
				w.WriteAttributeString ("EncodingType", Constants.WssBase64BinaryEncodingType);
			w.WriteString (Convert.ToBase64String (ic.GetX509Thumbprint ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteLocalIdKeyIdentifierClause (
			XmlWriter w, LocalIdKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "Reference", Constants.WssNamespace);
			if (EmitBspRequiredAttributes && ic.OwnerType != null) {
				string vt = GetTokenTypeUri (ic.OwnerType);
				if (vt != null)
					w.WriteAttributeString ("ValueType", vt);
			}
			w.WriteAttributeString ("URI", "#" + ic.LocalId);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteSecurityContextKeyIdentifierClause (
			XmlWriter w, SecurityContextKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "Reference", Constants.WssNamespace);
			w.WriteAttributeString ("URI", ic.ContextId.ToString ());
			string vt = GetTokenTypeUri (typeof (SecurityContextSecurityToken));
			w.WriteAttributeString ("ValueType", vt);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteEncryptedKeyIdentifierClause (
			XmlWriter w, EncryptedKeyIdentifierClause ic)
		{
			w.WriteStartElement ("e", "EncryptedKey", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteStartElement ("EncryptionMethod", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteAttributeString ("Algorithm", ic.EncryptionMethod);
			w.WriteEndElement ();
			if (ic.EncryptingKeyIdentifier != null) {
				w.WriteStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
				foreach (SecurityKeyIdentifierClause ckic in ic.EncryptingKeyIdentifier)
					WriteKeyIdentifierClause (w, ckic);
				w.WriteEndElement ();
			}
			w.WriteStartElement ("CipherData", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteStartElement ("CipherValue", EncryptedXml.XmlEncNamespaceUrl);
			w.WriteString (Convert.ToBase64String (ic.GetEncryptedKey ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
			if (ic.CarriedKeyName != null)
				w.WriteElementString ("CarriedKeyName", EncryptedXml.XmlEncNamespaceUrl, ic.CarriedKeyName);
			w.WriteEndElement ();
		}

		void WriteBinarySecretKeyIdentifierClause (
			XmlWriter w, BinarySecretKeyIdentifierClause ic)
		{
			w.WriteStartElement ("t", "BinarySecret", Constants.WstNamespace);
			w.WriteString (Convert.ToBase64String (ic.GetBuffer ()));
			w.WriteEndElement ();
		}

		void WriteInternalEncryptedKeyIdentifierClause (
			XmlWriter w, InternalEncryptedKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "KeyIdentifier", Constants.WssNamespace);
			w.WriteAttributeString ("ValueType", Constants.WssKeyIdentifierEncryptedKey);
			w.WriteString (Convert.ToBase64String (ic.GetBuffer ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteSamlAssertionKeyIdentifierClause (XmlWriter w, SamlAssertionKeyIdentifierClause ic)
		{
			w.WriteStartElement ("o", "SecurityTokenReference", Constants.WssNamespace);
			w.WriteStartElement ("o", "KeyIdentifier", Constants.WssNamespace);
			w.WriteAttributeString ("ValueType", Constants.WssKeyIdentifierSamlAssertion);
			w.WriteString (ic.AssertionId);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		[MonoTODO]
		protected override void WriteTokenCore (
			XmlWriter writer, SecurityToken token)
		{
			// WSSecurity supports:
			//	- UsernameToken : S.IM.T.UserNameSecurityToken
			//	- X509SecurityToken : S.IM.T.X509SecurityToken
			//	- SAML Assertion : S.IM.T.SamlSecurityToken
			//	- Kerberos : S.IM.T.KerberosRequestorSecurityToken
			//	- Rights Expression Language (REL) : N/A
			//	- SOAP with Attachments : N/A
			// they are part of standards support:
			//	- WrappedKey (EncryptedKey)
			//	- BinarySecret (WS-Trust)
			//	- SecurityContext (WS-SecureConversation)
			// additionally there are extra token types in WCF:
			//	- GenericXml
			//	- Windows
			//	- Sspi
			// not supported in this class:
			//	- Rsa

			if (token is UserNameSecurityToken)
				WriteUserNameSecurityToken (writer, ((UserNameSecurityToken) token));
			else if (token is X509SecurityToken)
				WriteX509SecurityToken (writer, ((X509SecurityToken) token));
			else if (token is BinarySecretSecurityToken)
				WriteBinarySecretSecurityToken (writer, ((BinarySecretSecurityToken) token));
			else if (token is SamlSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is GenericXmlSecurityToken)
				((GenericXmlSecurityToken) token).TokenXml.WriteTo (writer);
			else if (token is WrappedKeySecurityToken)
				WriteWrappedKeySecurityToken (writer, (WrappedKeySecurityToken) token);
			else if (token is DerivedKeySecurityToken)
				WriteDerivedKeySecurityToken (writer, (DerivedKeySecurityToken) token);
			else if (token is SecurityContextSecurityToken)
				WriteSecurityContextSecurityToken (writer, (SecurityContextSecurityToken) token);
			else if (token is SspiSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is KerberosRequestorSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else if (token is WindowsSecurityToken)
				throw new NotImplementedException ("WriteTokenCore() is not implemented for " + token);
			else
				throw new InvalidOperationException (String.Format ("This SecurityTokenSerializer does not support security token '{0}'.", token));
		}

		void WriteUserNameSecurityToken (XmlWriter w, UserNameSecurityToken token)
		{
			w.WriteStartElement ("o", "UsernameToken", Constants.WssNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteStartElement ("o", "Username", Constants.WssNamespace);
			w.WriteString (token.UserName);
			w.WriteEndElement ();
			w.WriteStartElement ("o", "Password", Constants.WssNamespace);
			w.WriteString (token.Password);
			w.WriteEndElement ();
			w.WriteEndElement ();
		}

		void WriteX509SecurityToken (XmlWriter w, X509SecurityToken token)
		{
			w.WriteStartElement ("o", "BinarySecurityToken", Constants.WssNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteAttributeString ("ValueType", Constants.WSSX509Token);
			w.WriteString (Convert.ToBase64String (token.Certificate.RawData));
			w.WriteEndElement ();
		}

		void WriteBinarySecretSecurityToken (XmlWriter w, BinarySecretSecurityToken token)
		{
			w.WriteStartElement ("t", "BinarySecret", Constants.WstNamespace);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteString (Convert.ToBase64String (token.GetKeyBytes ()));
			w.WriteEndElement ();
		}

		void WriteDerivedKeySecurityToken (XmlWriter w, DerivedKeySecurityToken token)
		{
			string ns = Constants.WsscNamespace;
			w.WriteStartElement ("c", "DerivedKeyToken", ns);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			WriteKeyIdentifierClause (w, token.TokenReference);
			if (token.Name != null) {
				w.WriteStartElement ("Properties", ns);
				w.WriteElementString ("Name", ns, token.Name);
				w.WriteEndElement ();
			}
			if (token.Offset != null)
				w.WriteElementString ("Offset", ns, Convert.ToString (token.Offset));
			if (token.Length != null)
				w.WriteElementString ("Length", ns, Convert.ToString (token.Length));
			if (token.Label != null)
				w.WriteElementString ("Label", ns, token.Label);
			w.WriteElementString ("Nonce", ns, Convert.ToBase64String (token.Nonce));
			w.WriteEndElement ();
		}

		void WriteWrappedKeySecurityToken (XmlWriter w, WrappedKeySecurityToken token)
		{
			string encNS = EncryptedXml.XmlEncNamespaceUrl;
			w.WriteStartElement ("e", "EncryptedKey", encNS);
			w.WriteAttributeString ("Id", token.Id);
			w.WriteStartElement ("EncryptionMethod", encNS);
			w.WriteAttributeString ("Algorithm", token.WrappingAlgorithm);
			w.WriteStartElement ("DigestMethod", SignedXml.XmlDsigNamespaceUrl);
			w.WriteAttributeString ("Algorithm", SignedXml.XmlDsigSHA1Url);
			w.WriteEndElement ();
			w.WriteEndElement ();

			w.WriteStartElement ("KeyInfo", SignedXml.XmlDsigNamespaceUrl);
			if (token.WrappingTokenReference != null)
				foreach (SecurityKeyIdentifierClause kic in token.WrappingTokenReference)
					WriteKeyIdentifierClause (w, kic);
			w.WriteEndElement ();
			w.WriteStartElement ("CipherData", encNS);
			w.WriteStartElement ("CipherValue", encNS);
			w.WriteString (Convert.ToBase64String (token.GetWrappedKey ()));
			w.WriteEndElement ();
			w.WriteEndElement ();
			if (token.ReferenceList != null) {
				w.WriteStartElement ("e", "ReferenceList", encNS);
				foreach (DataReference er in token.ReferenceList) {
					w.WriteStartElement ("DataReference", encNS);
					w.WriteAttributeString ("URI", er.Uri);
					w.WriteEndElement ();
				}
				w.WriteEndElement ();
			}
			w.WriteEndElement ();
		}

		void WriteSecurityContextSecurityToken (XmlWriter w, SecurityContextSecurityToken token)
		{
			string ns = Constants.WsscNamespace;
			w.WriteStartElement ("c", "SecurityContextToken", ns);
			w.WriteAttributeString ("u", "Id", Constants.WsuNamespace, token.Id);
			w.WriteElementString ("Identifier", ns, token.ContextId.ToString ());
			// FIXME: add Cookie output (from CreateCookieSecurityContextToken() method)
			if (token.Cookie != null)
				w.WriteElementString ("dnse", "Cookie", Constants.MSTlsnegoTokenContent, Convert.ToBase64String (token.Cookie));
			w.WriteEndElement ();
		}
	}
}
