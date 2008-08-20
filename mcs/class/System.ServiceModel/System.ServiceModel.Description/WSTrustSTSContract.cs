//
// WSTrustSTSContract.cs
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

using System.Collections.ObjectModel;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;

namespace System.ServiceModel.Description
{
	internal class WSTrustSecurityTokenServiceProxy
		: ClientBase<IWSTrustSecurityTokenService>, IWSTrustSecurityTokenService
	{
		public WSTrustSecurityTokenServiceProxy (Binding binding, EndpointAddress address)
			: base (binding, address)
		{
		}

		public Message Issue (Message request)
		{
			return Channel.Issue (request);
		}

		public Message IssueReply (Message request)
		{
			return Channel.IssueReply (request);
		}

		public Message Renew (Message request)
		{
			return Channel.Issue (request);
		}

		public Message Cancel (Message request)
		{
			return Channel.Issue (request);
		}

		public Message Validate (Message request)
		{
			return Channel.Issue (request);
		}
	}

	[ServiceContract]
	internal interface IWSTrustSecurityTokenService
	{
		[OperationContract (Action = Constants.WstIssueAction, ReplyAction = Constants.WstIssueReplyAction, ProtectionLevel = ProtectionLevel.EncryptAndSign)]
		Message Issue (Message request);

		[OperationContract (Action = Constants.WstIssueReplyAction, ReplyAction = Constants.WstIssueReplyAction, ProtectionLevel = ProtectionLevel.EncryptAndSign)] // needed for token negotiation reply
		Message IssueReply (Message request);

		[OperationContract (Action = Constants.WstRenewAction, ReplyAction = Constants.WstRenewReplyAction, ProtectionLevel = ProtectionLevel.EncryptAndSign)]
		Message Renew (Message request);

		[OperationContract (Action = Constants.WstCancelAction, ReplyAction = Constants.WstCancelReplyAction, ProtectionLevel = ProtectionLevel.EncryptAndSign)]
		Message Cancel (Message request);

		[OperationContract (Action = Constants.WstValidateAction, ReplyAction = Constants.WstValidateReplyAction, ProtectionLevel = ProtectionLevel.EncryptAndSign)]
		Message Validate (Message request);

		// FIXME: do we need KET here?
	}

	class WstRequestSecurityToken : WstRequestSecurityTokenBase
	{
		protected override void OnWriteBodyContents (XmlDictionaryWriter w)
		{
			w.WriteStartElement ("t", "RequestSecurityToken", Constants.WstNamespace);
			WriteBodyContentsCore (w);
			w.WriteEndElement ();
		}
	}

	class WstRequestSecurityTokenResponse : WstRequestSecurityTokenBase
	{
		SecurityTokenSerializer serializer;

		public WstRequestSecurityTokenResponse (SecurityTokenSerializer serializer)
		{
			this.serializer = serializer;
		}

		public string TokenType;

		public SecurityContextSecurityToken RequestedSecurityToken;

		public SecurityKeyIdentifierClause RequestedAttachedReference;

		public SecurityKeyIdentifierClause RequestedUnattachedReference;

		public object RequestedProofToken;

		public WstLifetime Lifetime;

		public byte [] Authenticator;

		// it only supports negotiation so far ...
		protected override void OnWriteBodyContents (XmlDictionaryWriter w)
		{
			string ns = Constants.WstNamespace;
			string nsu = Constants.WsuNamespace;
			w.WriteStartElement ("t", "RequestSecurityTokenResponse", ns);
			w.WriteXmlnsAttribute ("u", nsu);
			w.WriteAttributeString ("Context", Context);
			if (Authenticator != null) {
				w.WriteStartElement ("t", "Authenticator", ns);
				w.WriteStartElement ("t", "CombinedHash", ns);
				w.WriteBase64 (Authenticator, 0, Authenticator.Length);
				w.WriteEndElement ();
				w.WriteEndElement ();
			}
			if (TokenType != null)
				w.WriteElementString ("t", "TokenType", ns, TokenType);
			if (RequestedSecurityToken != null) {
				w.WriteStartElement ("t", "RequestedSecurityToken", ns);
				serializer.WriteToken (w, RequestedSecurityToken);
				w.WriteEndElement ();
			}
			if (RequestedAttachedReference != null) {
				w.WriteStartElement ("t", "RequestedAttachedReference", ns);
				serializer.WriteKeyIdentifierClause (w, RequestedAttachedReference);
				w.WriteEndElement ();
			}
			if (RequestedUnattachedReference != null) {
				w.WriteStartElement ("t", "RequestedUnattachedReference", ns);
				serializer.WriteKeyIdentifierClause (w, RequestedUnattachedReference);
				w.WriteEndElement ();
			}
			if (RequestedProofToken != null) {
				w.WriteStartElement ("t", "RequestedProofToken", ns);
				if (RequestedProofToken is SecurityToken)
					serializer.WriteToken (w, (SecurityToken) RequestedProofToken);
				else if (RequestedProofToken is SecurityKeyIdentifierClause)
					serializer.WriteKeyIdentifierClause (w, (SecurityKeyIdentifierClause) RequestedProofToken);
				else {
					string ens = EncryptedXml.XmlEncNamespaceUrl;
					w.WriteStartElement ("e", "EncryptedKey", ens);
					w.WriteStartElement ("EncryptionMethod", ens);
					w.WriteAttributeString ("Algorithm", Constants.WstTlsnegoProofTokenType);
					w.WriteEndElement ();
					w.WriteStartElement ("CipherData", ens);
					w.WriteStartElement ("CipherValue", ens);
					byte [] base64 = (byte []) RequestedProofToken;
					w.WriteBase64 (base64, 0, base64.Length);
					w.WriteEndElement ();
					w.WriteEndElement ();
					w.WriteEndElement ();
				}
				w.WriteEndElement ();
			}
			if (Lifetime != null) {
				w.WriteStartElement ("t", "Lifetime", ns);
				if (Lifetime.Created != DateTime.MinValue)
					w.WriteElementString ("Created", nsu, XmlConvert.ToString (Lifetime.Created.ToUniversalTime (), Constants.LifetimeFormat));
				if (Lifetime.Expires != DateTime.MaxValue)
					w.WriteElementString ("Expires", nsu, XmlConvert.ToString (Lifetime.Expires.ToUniversalTime (), Constants.LifetimeFormat));
				w.WriteEndElement ();
			}
			//w.WriteElementString ("t", "KeySize", ns, XmlConvert.ToString (KeySize));
			if (BinaryExchange != null)
				BinaryExchange.WriteTo (w);
			w.WriteEndElement ();
		}
	}

	abstract class WstRequestSecurityTokenBase : BodyWriter
	{
		protected WstRequestSecurityTokenBase ()
			: base (true)
		{
		}

		protected void WriteBodyContentsCore (XmlDictionaryWriter w)
		{
			string ns = Constants.WstNamespace;
			w.WriteAttributeString ("Context", Context);
			w.WriteElementString ("t", "TokenType", ns, Constants.WsscContextToken);
			w.WriteElementString ("t", "RequestType", ns, Constants.WstIssueRequest);
			w.WriteElementString ("t", "KeySize", ns, XmlConvert.ToString (KeySize));
			if (BinaryExchange != null)
				BinaryExchange.WriteTo (w);
		}

		public string Context = "uuid-" + Guid.NewGuid (); // UniqueId()-"urn:"
		public string RequestType;
		public WspAppliesTo AppliesTo;

		public SecurityToken Entropy;

		public int KeySize = 256;
		public string KeyType;
		public string ComputedKeyAlgorithm;

		public WstBinaryExchange BinaryExchange;
	}

	class WstRequestSecurityTokenResponseCollection : BodyWriter
	{
		public WstRequestSecurityTokenResponseCollection ()
			: base (true)
		{
		}

		Collection<WstRequestSecurityTokenResponse> responses =
			new Collection<WstRequestSecurityTokenResponse> ();

		public Collection<WstRequestSecurityTokenResponse> Responses {
			get { return responses; }
		}

		protected override void OnWriteBodyContents (XmlDictionaryWriter w)
		{
			w.WriteStartElement ("t", "RequestSecurityTokenResponseCollection", Constants.WstNamespace);
			foreach (WstRequestSecurityTokenResponse r in Responses)
				r.WriteBodyContents (w);
			w.WriteEndElement ();
		}

		public void Read (string negotiationType, XmlDictionaryReader r, SecurityTokenSerializer serializer, SecurityTokenResolver resolver)
		{
			r.MoveToContent ();
			r.ReadStartElement ("RequestSecurityTokenResponseCollection", Constants.WstNamespace);
			while (true) {
				r.MoveToContent ();
				if (r.NodeType != XmlNodeType.Element)
					break;
				WSTrustRequestSecurityTokenResponseReader rstrr = new WSTrustRequestSecurityTokenResponseReader (negotiationType, r, serializer, resolver);
				rstrr.Read ();
				responses.Add (rstrr.Value);
			}
			r.ReadEndElement ();
		}
	}

	class WstLifetime
	{
		public DateTime Created = DateTime.MinValue;

		public DateTime Expires = DateTime.MaxValue;
	}

	class WstEntropy
	{
		public object Token;
	}

	class WspAppliesTo
	{
		public WsaEndpointReference EndpointReference;
	}

	class WsaEndpointReference
	{
		public string Address;
	}

	class WstBinarySecret
	{
		public string Id;

		public string Type;

		public string Value;
	}

	class WstBinaryExchange
	{
		public WstBinaryExchange (string valueType)
		{
			ValueType = valueType;
		}

		public string ValueType;

		public string EncodingType = Constants.WssBase64BinaryEncodingType;

		public byte [] Value;

		public void WriteTo (XmlWriter w)
		{
			w.WriteStartElement ("t", "BinaryExchange", Constants.WstNamespace);
			w.WriteAttributeString ("ValueType", ValueType);
			w.WriteAttributeString ("EncodingType", EncodingType);
			w.WriteString (Convert.ToBase64String (Value));
			w.WriteEndElement ();
		}
	}
}
