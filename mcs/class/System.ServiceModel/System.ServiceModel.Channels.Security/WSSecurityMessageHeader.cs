//
// WSSecurityMessageHeader.cs
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
using System.IO;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels.Security
{
	internal class WSSecurityMessageHeaderReader
	{
		public WSSecurityMessageHeaderReader (WSSecurityMessageHeader header, SecurityTokenSerializer serializer, SecurityTokenResolver resolver, XmlDocument doc, XmlNamespaceManager nsmgr, List<SecurityToken> tokens)
		{
			this.header = header;
			this.serializer = serializer;
			this.resolver = resolver;
			this.doc = doc;
			this.nsmgr = nsmgr;
			this.tokens = tokens;
		}

		WSSecurityMessageHeader header;
		SecurityTokenSerializer serializer;
		SecurityTokenResolver resolver;
		XmlDocument doc;
		XmlNamespaceManager nsmgr;
		List<SecurityToken> tokens;
		Dictionary<string, EncryptedData> encryptedDataList =
			new Dictionary<string, EncryptedData> ();

		public void ReadContents (XmlReader reader)
		{
			DerivedKeySecurityToken currentToken = null;

			reader.MoveToContent ();
			reader.ReadStartElement ("Security", Constants.WssNamespace);
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				object o = ReadContent (reader);
				if (o is EncryptedData) {
					EncryptedData ed = (EncryptedData) o;
					encryptedDataList [ed.Id] = ed;
				}
				else if (o is ReferenceList && currentToken != null)
					currentToken.ReferenceList = (ReferenceList) o;
				else if (o is SecurityToken) {
					if (o is DerivedKeySecurityToken)
						currentToken = o as DerivedKeySecurityToken;
					tokens.Add ((SecurityToken) o);
				}
				header.Contents.Add (o);
			} while (true);
			reader.ReadEndElement ();
		}

		object ReadContent (XmlReader reader)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element)
				throw new XmlException (String.Format ("Node type {0} is not expected as a WS-Security message header content.", reader.NodeType));
			switch (reader.NamespaceURI) {
			case Constants.WsuNamespace:
				switch (reader.LocalName) {
				case "Timestamp":
					return ReadTimestamp (reader);
				}
				break;
			//case Constants.WstNamespace:
			case Constants.Wss11Namespace:
				if (reader.LocalName == "SignatureConfirmation") {
					return ReadSignatureConfirmation (reader, doc);
				}
				break;
			case SignedXml.XmlDsigNamespaceUrl:
				switch (reader.LocalName) {
				case "Signature":
					WSSignedXml sxml = new WSSignedXml (doc);
					sxml.Signature.LoadXml ((XmlElement) doc.ReadNode (reader));
					UpdateSignatureKeyInfo (sxml.Signature, doc, serializer);
					return sxml;
				}
				break;
			case EncryptedXml.XmlEncNamespaceUrl:
				switch (reader.LocalName) {
				case "EncryptedData":
					XmlElement el = (XmlElement) doc.ReadNode (reader);
					return CreateEncryptedData (el);
				case "ReferenceList":
					ReferenceList rl = new ReferenceList ();
					reader.Read ();
					for (reader.MoveToContent ();
					     reader.NodeType != XmlNodeType.EndElement;
					     reader.MoveToContent ()) {
						switch (reader.LocalName) {
						case "DataReference":
							DataReference dref = new DataReference ();
							dref.LoadXml ((XmlElement) doc.ReadNode (reader));
							rl.Add (dref);
							continue;
						case "KeyReference":
							KeyReference kref = new KeyReference ();
							kref.LoadXml ((XmlElement) doc.ReadNode (reader));
							rl.Add (kref);
							continue;
						}
						throw new XmlException (String.Format ("Unexpected {2} node '{0}' in namespace '{1}' in ReferenceList.", reader.Name, reader.NamespaceURI, reader.NodeType));
					}
					reader.ReadEndElement ();
					return rl;
				}
				break;
			}
			// SecurityTokenReference will be handled here.
			// This order (Token->KeyIdentifierClause) is
			// important because WrappedKey could be read
			// in both context (but must be a token here).
			if (serializer.CanReadToken (reader))
				return serializer.ReadToken (reader, resolver);
			else if (serializer.CanReadKeyIdentifierClause (reader))
				return serializer.ReadKeyIdentifierClause (reader);
			else
				throw new XmlException (String.Format ("Unexpected element '{0}' in namespace '{1}' as a WS-Security message header content.", reader.Name, reader.NamespaceURI));
		}

		void UpdateSignatureKeyInfo (Signature sig, XmlDocument doc, SecurityTokenSerializer serializer)
		{
			KeyInfo ki = new KeyInfo ();
			ki.Id = sig.KeyInfo.Id;
			foreach (KeyInfoClause kic in sig.KeyInfo) {
				SecurityTokenReferenceKeyInfo r = new SecurityTokenReferenceKeyInfo (serializer, doc);
				r.LoadXml (kic.GetXml ());
				ki.AddClause (r);
			}
			sig.KeyInfo = ki;
		}

		#region Decryption

		// returns the protection token
		public void DecryptSecurity (SecureMessageDecryptor decryptor, SymmetricSecurityKey sym, byte [] dummyEncKey)
		{
			WSEncryptedXml encXml = new WSEncryptedXml (doc);

			// default, unless overriden by the default DerivedKeyToken.
			Rijndael aes = RijndaelManaged.Create (); // it is reused with every key
			aes.Mode = CipherMode.CBC;

			if (sym == null)
				throw new MessageSecurityException ("Cannot find the encryption key in this message and context");

			// decrypt the body with the decrypted key
			Collection<string> references = new Collection<string> ();

			foreach (ReferenceList rlist in header.FindAll<ReferenceList> ())
				foreach (EncryptedReference encref in rlist)
					references.Add (StripUri (encref.Uri));

			foreach (WrappedKeySecurityToken wk in header.FindAll<WrappedKeySecurityToken> ())
				foreach (EncryptedReference er in wk.ReferenceList)
					references.Add (StripUri (er.Uri));

			Collection<XmlElement> list = new Collection<XmlElement> ();
			foreach (string uri in references) {
				XmlElement el = encXml.GetIdElement (doc, uri);
				if (el != null)
					list.Add (el);
				else
					throw new MessageSecurityException (String.Format ("On decryption, EncryptedData with Id '{0}', referenced by ReferenceData, was not found.", uri));
			}

			foreach (XmlElement el in list) {
				EncryptedData ed2 = CreateEncryptedData (el);
				byte [] key = GetEncryptionKeyForData (ed2, encXml, dummyEncKey);
				aes.Key = key != null ? key : sym.GetSymmetricKey ();
				byte [] decrypted = DecryptData (encXml, ed2, aes);
				encXml.ReplaceData (el, decrypted);
				EncryptedData existing;
				// if it was a header content, replace 
				// corresponding one.
				if (encryptedDataList.TryGetValue (ed2.Id, out existing)) {
					// FIXME: it is kind of extraneous and could be replaced by XmlNodeReader
//Console.WriteLine ("DECRYPTED EncryptedData:");
//Console.WriteLine (Encoding.UTF8.GetString (decrypted));
					object o = ReadContent (XmlReader.Create (new MemoryStream (decrypted)));
					header.Contents.Remove (existing);
					header.Contents.Add (o);
				}
			}
/*
Console.WriteLine ("======== Decrypted Document ========");
doc.PreserveWhitespace = false;
doc.Save (Console.Out);
doc.PreserveWhitespace = true;
*/
		}

		EncryptedData CreateEncryptedData (XmlElement el)
		{
			EncryptedData ed = new EncryptedData ();
			ed.LoadXml (el);
			if (ed.Id == null)
				ed.Id = el.GetAttribute ("Id", Constants.WsuNamespace);
			return ed;
		}

		byte [] GetEncryptionKeyForData (EncryptedData ed2, EncryptedXml encXml, byte [] dummyEncKey)
		{
			// Since ReferenceList could be embedded directly in wss_header without
			// key indication, it must iterate all the derived keys to find out
			// appropriate one.
			foreach (DerivedKeySecurityToken dk in header.FindAll<DerivedKeySecurityToken> ()) {
				if (dk.ReferenceList == null)
					continue;
				foreach (DataReference dr in dk.ReferenceList)
					if (StripUri (dr.Uri) == ed2.Id)
						return ((SymmetricSecurityKey) dk.SecurityKeys [0]).GetSymmetricKey ();
			}
			foreach (WrappedKeySecurityToken wk in header.FindAll<WrappedKeySecurityToken> ()) {
				if (wk.ReferenceList == null)
					continue;
				foreach (DataReference dr in wk.ReferenceList)
					if (StripUri (dr.Uri) == ed2.Id)
						return ((SymmetricSecurityKey) wk.SecurityKeys [0]).GetSymmetricKey ();
			}

			if (ed2.KeyInfo == null)
				return null;
			foreach (KeyInfoClause kic in ed2.KeyInfo) {
				SecurityKeyIdentifierClause skic = serializer.ReadKeyIdentifierClause (new XmlNodeReader (kic.GetXml ()));

				SecurityKey skey = null;
				if (!resolver.TryResolveSecurityKey (skic, out skey))
					throw new MessageSecurityException (String.Format ("The signing key could not be resolved from {0}", skic));
				SymmetricSecurityKey ssk = skey as SymmetricSecurityKey;
				if (ssk != null)
					return ssk.GetSymmetricKey ();
			}
			return null; // no applicable key info clause.
		}

		// Probably it is a bug in .NET, but sometimes it does not contain
		// proper padding bytes. For such cases, use PaddingMode.None
		// instead. It must not be done in EncryptedXml class as it
		// correctly rejects improper ISO10126 padding.
		byte [] DecryptData (EncryptedXml encXml, EncryptedData ed, SymmetricAlgorithm symAlg)
		{
			PaddingMode bak = symAlg.Padding;
			try {
				byte [] bytes = ed.CipherData.CipherValue;

				if (encXml.Padding != PaddingMode.None &&
				    encXml.Padding != PaddingMode.Zeros &&
				    bytes [bytes.Length - 1] > symAlg.BlockSize / 8)
					symAlg.Padding = PaddingMode.None;
				return encXml.DecryptData (ed, symAlg);
			} finally {
				symAlg.Padding = bak;
			}
		}

		string StripUri (string src)
		{
			if (src == null || src.Length == 0)
				return String.Empty;
			if (src [0] != '#')
				throw new NotSupportedException (String.Format ("Non-fragment URI in DataReference and KeyReference is not supported: '{0}'", src));
			return src.Substring (1);
		}
		#endregion

		static Wss11SignatureConfirmation ReadSignatureConfirmation (XmlReader reader, XmlDocument doc)
		{
			string id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			string value = reader.GetAttribute ("Value");
			reader.Skip ();
			return new Wss11SignatureConfirmation (id, value);
		}

		static WsuTimestamp ReadTimestamp (XmlReader reader)
		{
			WsuTimestamp ret = new WsuTimestamp ();
			ret.Id = reader.GetAttribute ("Id", Constants.WsuNamespace);
			reader.ReadStartElement ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NodeType != XmlNodeType.Element)
					throw new XmlException (String.Format ("Node type {0} is not expected as a WS-Security 'Timestamp' content.", reader.NodeType));
				switch (reader.NamespaceURI) {
				case Constants.WsuNamespace:
					switch (reader.LocalName) {
					case "Created":
						ret.Created = (DateTime) reader.ReadElementContentAs (typeof (DateTime), null);
						continue;
					case "Expires":
						ret.Expires = (DateTime) reader.ReadElementContentAs (typeof (DateTime), null);
						continue;
					}
					break;
				}
				throw new XmlException (String.Format ("Unexpected element '{0}' in namespace '{1}' as a WS-Security message header content.", reader.Name, reader.NamespaceURI));
			} while (true);

			reader.ReadEndElement (); // </u:Timestamp>
			return ret;
		}
	}

	internal class WSSecurityMessageHeader : MessageHeader
	{
		public WSSecurityMessageHeader (SecurityTokenSerializer serializer)
		{
			this.serializer = serializer;
		}

		SecurityTokenSerializer serializer;
		Collection<object> contents = new Collection<object> ();

		// Timestamp, BinarySecurityToken, EncryptedKey,
		// [DerivedKeyToken]*, ReferenceList, EncryptedData
		public Collection<object> Contents {
			get { return contents; }
		}

		public override bool MustUnderstand {
			get { return true; }
		}

		public override string Name {
			get { return "Security"; }
		}

		public override string Namespace {
			get { return Constants.WssNamespace; }
		}

		public void AddContent (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			Contents.Add (obj);
		}

		public T Find<T> ()
		{
			foreach (object o in Contents)
				if (typeof (T).IsAssignableFrom (o.GetType ()))
					return (T) o;
			return default (T);
		}

		public Collection<T> FindAll<T> ()
		{
			Collection<T> c = new Collection<T> ();
			foreach (object o in Contents)
				if (typeof (T).IsAssignableFrom (o.GetType ()))
					c.Add ((T) o);
			return c;
		}

		protected override void OnWriteStartHeader (XmlDictionaryWriter writer, MessageVersion version)
		{
			writer.WriteStartElement ("o", this.Name, this.Namespace);
			WriteHeaderAttributes (writer, version);
		}

		protected override void OnWriteHeaderContents (XmlDictionaryWriter writer, MessageVersion version)
		{
			// FIXME: it should use XmlDictionaryWriter that CanCanonicalize the output (which is not possible in any built-in writer types, so we'll have to hack it).

			foreach (object obj in Contents) {
				if (obj is WsuTimestamp) {
					WsuTimestamp ts = (WsuTimestamp) obj;
					ts.WriteTo (writer);
				} else if (obj is SecurityToken) {
					serializer.WriteToken (writer, (SecurityToken) obj);
				} else if (obj is EncryptedKey) {
					((EncryptedKey) obj).GetXml ().WriteTo (writer);
				} else if (obj is ReferenceList) {
					writer.WriteStartElement ("ReferenceList", EncryptedXml.XmlEncNamespaceUrl);
					foreach (EncryptedReference er in (ReferenceList) obj)
						er.GetXml ().WriteTo (writer);
					writer.WriteEndElement ();
				} else if (obj is EncryptedData) {
					((EncryptedData) obj).GetXml ().WriteTo (writer);
				} else if (obj is Signature) {
					((Signature) obj).GetXml ().WriteTo (writer);
				} else if (obj is Wss11SignatureConfirmation) {
					Wss11SignatureConfirmation sc = (Wss11SignatureConfirmation) obj;
					writer.WriteStartElement ("k", "SignatureConfirmation", Constants.Wss11Namespace);
					writer.WriteAttributeString ("u", "Id", Constants.WsuNamespace, sc.Id);
					writer.WriteAttributeString ("Value", sc.Value);
					writer.WriteEndElement ();
				}
				else
					throw new ArgumentException (String.Format ("Unrecognized header item {0}", obj ?? "(null)"));
			}
		}
	}

	internal class WsuTimestamp
	{
		string id;
		DateTime created, expires;

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public DateTime Created {
			get { return created; }
			set { created = value; }
		}

		public DateTime Expires {
			get { return expires; }
			set { expires = value; }
		}

		public void WriteTo (XmlWriter writer)
		{
			writer.WriteStartElement ("u", "Timestamp", Constants.WsuNamespace);
			writer.WriteAttributeString ("u", "Id", Constants.WsuNamespace, Id);
			writer.WriteStartElement ("u", "Created", Constants.WsuNamespace);
			writer.WriteValue (FormatAsUtc (Created));
			writer.WriteEndElement ();
			writer.WriteStartElement ("u", "Expires", Constants.WsuNamespace);
			writer.WriteValue (FormatAsUtc (Expires));
			writer.WriteEndElement ();
			writer.WriteEndElement ();
		}

		string FormatAsUtc (DateTime date)
		{
			return date.ToUniversalTime ().ToString (
				"yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
				CultureInfo.InvariantCulture);
		}
	}

	internal class SecurityTokenReferenceKeyInfo : KeyInfoClause
	{
		SecurityKeyIdentifierClause clause;
		SecurityTokenSerializer serializer;
		XmlDocument doc;

		// for LoadXml()
		public SecurityTokenReferenceKeyInfo (
			SecurityTokenSerializer serializer,
			XmlDocument doc)
			: this (null, serializer, doc)
		{
		}

		// for GetXml()
		public SecurityTokenReferenceKeyInfo (
			SecurityKeyIdentifierClause clause,
			SecurityTokenSerializer serializer,
			XmlDocument doc)
		{
			this.clause = clause;
			this.serializer = serializer;
			if (doc == null)
				doc = new XmlDocument ();
			this.doc = doc;
		}

		public SecurityKeyIdentifierClause Clause {
			get { return clause; }
		}

		public override XmlElement GetXml ()
		{
			XmlDocumentFragment df = doc.CreateDocumentFragment ();
			XmlWriter w = df.CreateNavigator ().AppendChild ();
			serializer.WriteKeyIdentifierClause (w, clause);
			w.Close ();
			return (XmlElement) df.FirstChild;
		}

		public override void LoadXml (XmlElement element)
		{
			clause = serializer.ReadKeyIdentifierClause (new XmlNodeReader (element));
		}
	}

	internal class Wss11SignatureConfirmation
	{
		string id, value;

		public Wss11SignatureConfirmation (string id, string value)
		{
			this.id = id;
			this.value = value;
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public string Value {
			get { return value; }
			set { this.value = value; }
		}
	}
}
