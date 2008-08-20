//
// WSTrustMessageConverters.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc.
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
using System.Reflection;
using System.Security.Cryptography;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using System.Xml.Serialization;

namespace System.ServiceModel.Description
{
	class WSTrustRequestSecurityTokenReader : IDisposable
	{
		WstRequestSecurityToken req = new WstRequestSecurityToken ();
		XmlDictionaryReader reader;
		SecurityTokenSerializer serializer;

		public WSTrustRequestSecurityTokenReader (XmlDictionaryReader reader, SecurityTokenSerializer serializer)
		{
			this.reader = reader;
			this.serializer = serializer;
		}

		public WstRequestSecurityToken Value {
			get { return req; }
		}

		string LineInfo ()
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			return li != null && li.HasLineInfo () ?
				String.Format ("({0},{1})", li.LineNumber, li.LinePosition) : String.Empty;
		}

		public void Dispose ()
		{
			reader.Close ();
		}

		public WstRequestSecurityToken Read ()
		{
			reader.MoveToContent ();
			req.Context = reader.GetAttribute ("Context");
			reader.ReadStartElement ("RequestSecurityToken", Constants.WstNamespace);
			do {
				reader.MoveToContent ();
				switch (reader.NodeType) {
				case XmlNodeType.EndElement:
					reader.Read (); // consume RequestSecurityToken end element.
					return req;
				case XmlNodeType.Element:
					ReadTokenContent ();
					break;
				default:
					throw new XmlException (String.Format ("Unexpected request XML {0} node, name {1}{2}", reader.NodeType, reader.Name, LineInfo ()));
				}
			} while (true);
		}

		void ReadTokenContent ()
		{
			switch (reader.NamespaceURI) {
			case Constants.WstNamespace:
				switch (reader.LocalName) {
				case "RequestType":
					req.RequestType = reader.ReadElementContentAsString ();
					return;
				case "Entropy":
					ReadEntropy ();
					return;
				case "KeySize":
					req.KeySize = reader.ReadElementContentAsInt ();
					return;
				case "KeyType":
					req.KeyType = reader.ReadElementContentAsString ();
					return;
				case "TokenType":
					string tokenType = reader.ReadElementContentAsString ();
					if (tokenType != Constants.WsscContextToken)
						throw new SecurityTokenException (String.Format ("Unexpected TokenType: {0}", tokenType));
					return;
				case "ComputedKeyAlgorithm":
					req.ComputedKeyAlgorithm = reader.ReadElementContentAsString ();
					return;
				case "BinaryExchange":
					ReadBinaryExchange ();
					return;
				}
				break;
			case Constants.WspNamespace:
				switch (reader.LocalName) {
				case "AppliesTo":
					ReadAppliesTo ();
					return;
				}
				break;
			}
			throw new XmlException (String.Format ("Unexpected RequestSecurityToken content element. Name is {0} and namespace URI is {1}{2}", reader.Name, reader.NamespaceURI, LineInfo ()));
		}

		protected void ReadBinaryExchange ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("Binary content is expected in 'BinaryExchange' element.{0}", LineInfo ()));
			WstBinaryExchange b = new WstBinaryExchange (reader.GetAttribute ("ValueType"));
			b.EncodingType = reader.GetAttribute ("EncodingType");
			b.Value = Convert.FromBase64String (reader.ReadElementContentAsString ());
			req.BinaryExchange = b;
		}

		void ReadEntropy ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("WS-Trust Entropy element is empty.{2}", LineInfo ()));
			reader.ReadStartElement ("Entropy", Constants.WstNamespace);
			reader.MoveToContent ();

			req.Entropy = serializer.ReadToken (reader, null);
			// after reading a token, </Entropy> should follow.
			reader.MoveToContent ();
			reader.ReadEndElement ();
		}

		void ReadAppliesTo ()
		{
			WspAppliesTo aTo = new WspAppliesTo ();

			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("WS-Policy AppliesTo element is empty.{2}", LineInfo ()));
			reader.ReadStartElement ();
			reader.MoveToContent ();
			reader.ReadStartElement ("EndpointReference", Constants.WsaNamespace);
			reader.MoveToContent ();
			WsaEndpointReference er = new WsaEndpointReference ();
			er.Address = reader.ReadElementContentAsString ("Address", Constants.WsaNamespace);
			reader.MoveToContent ();
			reader.ReadEndElement (); // </EndpointReference>
			aTo.EndpointReference = er;
			reader.MoveToContent ();
			reader.ReadEndElement (); // </wsp:AppliesTo>
			req.AppliesTo = aTo;
		}
	}

	// FIXME: it might be extraneous - currently used only for IssuedToken
	class WstRequestSecurityTokenWriter : BodyWriter
	{
		WstRequestSecurityToken value;
		SecurityTokenSerializer serializer;

		public WstRequestSecurityTokenWriter (WstRequestSecurityToken value, SecurityTokenSerializer serializer)
			: base (true)
		{
			this.value = value;
			this.serializer = serializer;
		}

		protected override void OnWriteBodyContents (XmlDictionaryWriter w)
		{
			w.WriteStartElement ("RequestSecurityToken", Constants.WstNamespace);
			w.WriteEndElement ();
		}
	}

	class WSTrustRequestSecurityTokenResponseReader : IDisposable
	{
		string negotiation_type;
		WstRequestSecurityTokenResponse res;
		XmlDictionaryReader reader;
		SecurityTokenSerializer serializer;
		SecurityTokenResolver resolver;

		public WSTrustRequestSecurityTokenResponseReader (
			string negotiationType,
			XmlDictionaryReader reader,
			SecurityTokenSerializer serializer,
			SecurityTokenResolver resolver)
		{
			this.negotiation_type = negotiationType;
			this.reader = reader;
			this.serializer = serializer;
			this.resolver = resolver;
			res = new WstRequestSecurityTokenResponse (serializer);
		}

		public WstRequestSecurityTokenResponse Value {
			get { return res; }
		}

		string LineInfo ()
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			return li != null && li.HasLineInfo () ?
				String.Format ("({0},{1})", li.LineNumber, li.LinePosition) : String.Empty;
		}

		public void Dispose ()
		{
			reader.Close ();
		}

		public WstRequestSecurityTokenResponse Read ()
		{
			reader.MoveToContent ();
			res.Context = reader.GetAttribute ("Context");
			reader.ReadStartElement ("RequestSecurityTokenResponse", Constants.WstNamespace);
			do {
				reader.MoveToContent ();
				switch (reader.NodeType) {
				case XmlNodeType.EndElement:
					reader.Read (); // consume RequestSecurityTokenResponse end element.
					return res;
				case XmlNodeType.Element:
					ReadTokenContent ();
					break;
				default:
					throw new XmlException (String.Format ("Unexpected request XML {0} node, name {1}{2}", reader.NodeType, reader.Name, LineInfo ()));
				}
			} while (true);
		}

		void ReadTokenContent ()
		{
			switch (reader.NamespaceURI) {
			case Constants.WstNamespace:
				switch (reader.LocalName) {
				case "RequestedSecurityToken":
					res.RequestedSecurityToken = (SecurityContextSecurityToken) ReadToken ();
					return;
				case "RequestedProofToken":
#if true // FIXME: we can't handle it right now
					string ens = EncryptedXml.XmlEncNamespaceUrl;
					reader.Read ();
					reader.ReadStartElement ("EncryptedKey", ens);
					string alg = reader.GetAttribute ("Algorithm");
					bool isEmpty = reader.IsEmptyElement;
					reader.ReadStartElement ("EncryptionMethod", ens);
					if (alg != negotiation_type)
						throw new XmlException (String.Format ("EncryptionMethod '{0}' is not supported in RequestedProofToken.", alg));
					if (!isEmpty)
						reader.ReadEndElement ();
					reader.ReadStartElement ("CipherData", ens);
					reader.ReadStartElement ("CipherValue", ens);
					byte [] pt = reader.ReadContentAsBase64 ();
					res.RequestedProofToken = pt;
					reader.ReadEndElement ();
					reader.ReadEndElement ();
					reader.ReadEndElement ();// EncryptedKey
					reader.ReadEndElement ();// RPT
#else
					reader.Read ();
					reader.MoveToContent ();
					if (serializer.CanReadToken (reader))
						res.RequestedProofToken = serializer.ReadToken (reader, resolver);
					else
						res.RequestedProofToken = serializer.ReadKeyIdentifierClause (reader);
					reader.ReadEndElement ();
#endif
					return;
				case "BinaryExchange":
					ReadBinaryExchange ();
					return;
				case "TokenType":
					res.TokenType = reader.ReadElementContentAsString ();
					return;
				case "Lifetime":
					ReadLifetime ();
					return;
				case "KeySize":
					res.KeySize = reader.ReadElementContentAsInt ();
					return;
				case "RequestedAttachedReference":
					res.RequestedAttachedReference = ReadTokenReference ();
					return;
				case "RequestedUnattachedReference":
					res.RequestedUnattachedReference = ReadTokenReference ();
					return;
				case "Authenticator":
					ReadAuthenticator ();
					return;
				}
				break;
			}
			throw new XmlException (String.Format ("Unexpected RequestSecurityTokenResponse content element. Name is {0} and namespace URI is {1} {2}", reader.Name, reader.NamespaceURI, LineInfo ()));
		}

		void ReadAuthenticator ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("WS-Trust 'Authenticator' element is expected to have contents. {0}", LineInfo ()));
			reader.Read ();
			reader.MoveToContent ();
			res.Authenticator = Convert.FromBase64String (reader.ReadElementContentAsString ("CombinedHash", Constants.WstNamespace));
			reader.ReadEndElement ();
		}

		void ReadLifetime ()
		{
			WstLifetime lt = new WstLifetime ();
			res.Lifetime = lt;
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("WS-Trust 'Lifetime' element is expected to have contents. {0}", LineInfo ()));
			reader.Read ();
			while (true) {
				reader.MoveToContent ();
				if (reader.NodeType != XmlNodeType.Element)
					break;
				if (reader.NamespaceURI == Constants.WsuNamespace) {
					switch (reader.LocalName) {
					case "Created":
						lt.Created = XmlConvert.ToDateTime (reader.ReadElementContentAsString (), XmlDateTimeSerializationMode.RoundtripKind);
						continue;
					case "Expires":
						lt.Expires = XmlConvert.ToDateTime (reader.ReadElementContentAsString (), XmlDateTimeSerializationMode.RoundtripKind);
						continue;
					}
				}
				throw new XmlException (String.Format ("Unexpected Lifetime content. Name is {0} and namespace URI is {1} {2}", reader.Name, reader.NamespaceURI, LineInfo ()));
			}
			reader.ReadEndElement ();
		}

		SecurityToken ReadToken ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("Security token body is expected in '{0}' element. {1}", reader.LocalName, LineInfo ()));
			reader.Read ();
			reader.MoveToContent ();
			SecurityToken token = serializer.ReadToken (reader, resolver);
			reader.ReadEndElement ();
			return token;
		}

		SecurityKeyIdentifierClause ReadTokenReference ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("Content is expected in 'RequestedAttachedReference' element. {0}", LineInfo ()));
			reader.Read ();
			SecurityKeyIdentifierClause ret = serializer.ReadKeyIdentifierClause (reader);
			reader.ReadEndElement ();
			return ret;
		}

		void ReadBinaryExchange ()
		{
			if (reader.IsEmptyElement)
				throw new XmlException (String.Format ("Binary content is expected in 'BinaryExchange' element.{0}", LineInfo ()));
			WstBinaryExchange b = new WstBinaryExchange (reader.GetAttribute ("ValueType"));
			b.EncodingType = reader.GetAttribute ("EncodingType");
			b.Value = Convert.FromBase64String (reader.ReadElementContentAsString ());
			res.BinaryExchange = b;
		}
	}

	// FIXME: it might be extraneous - currently unused
	internal class WstRequestSecurityTokenResponseWriter : BodyWriter
	{
		WstRequestSecurityTokenResponse res;
		SecurityTokenSerializer serializer;

		public WstRequestSecurityTokenResponseWriter (WstRequestSecurityTokenResponse res, SecurityTokenSerializer serializer)
			: base (true)
		{
			this.res = res;
			this.serializer = serializer;
		}

		protected override void OnWriteBodyContents (XmlDictionaryWriter writer)
		{
try {
			writer.WriteStartElement ("RequestSecurityTokenResponse", Constants.WstNamespace);

			// RequestedSecurityToken
			writer.WriteStartElement ("RequestedSecurityToken", Constants.WstNamespace);

			serializer.WriteToken (writer, res.RequestedSecurityToken);

			writer.WriteEndElement ();

/*
			// Entropy
			writer.WriteStartElement ("Entropy", Constants.WstNamespace);
			// FIXME: keep generated key
			serializer.WriteToken (writer,
				new BinarySecretSecurityToken (Rijndael.Create ().Key));
			writer.WriteEndElement ();
*/

			writer.WriteEndElement ();
} catch (Exception ex) {
Console.WriteLine (ex);
throw;
}
		}
	}
}

