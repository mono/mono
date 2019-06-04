//
// BinaryMessageEncodingBindingElementTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

using Element = System.ServiceModel.Channels.BinaryMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class BinaryMessageEncodingBindingElementTest
	{
		[Test]
		public void DefaultValues ()
		{
			Element el = new Element ();
			Assert.AreEqual (64, el.MaxReadPoolSize, "#1");
			Assert.AreEqual (16, el.MaxWritePoolSize, "#2");
			Assert.AreEqual (MessageVersion.Default, el.MessageVersion, "#3");
			// FIXME: test ReaderQuotas
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BuildChannelListenerNullArg ()
		{
			new Element ().BuildChannelListener<IReplyChannel> (null);
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			Element el = new Element ();
			Assert.IsTrue (el.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (el.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#2");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			Element el = new Element ();
			IChannelFactory<IRequestChannel> cf =
				el.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelListenerEmptyCustomBinding ()
		{
			CustomBinding cb = new CustomBinding ();
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			new Element ().BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		public void BuildChannelListenerWithTransport ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection (),
				new Uri ("http://localhost:8080"), String.Empty, ListenUriMode.Unique);
			new Element ().BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (ProtocolException))]
		public void ReadMessageWrongContentType ()
		{
			var encoder = new BinaryMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			encoder.ReadMessage (new MemoryStream (new byte [0]), 100, "application/octet-stream");
		}

		[Test]
		public void ReadMessage ()
		{
			using (var ms = File.OpenRead (TestResourceHelper.GetFullPathOfResource ("Test/Resources/binary-message.raw"))) {

				var session = new XmlBinaryReaderSession ();
				byte [] rsbuf = new BinaryFrameSupportReader (ms).ReadSizedChunk ();
				int count = 0;
				using (var rms = new MemoryStream (rsbuf, 0, rsbuf.Length)) {
					var rbr = new BinaryReader (rms, Encoding.UTF8);
					while (rms.Position < rms.Length)
						session.Add (count++, rbr.ReadString ());
				}
				var xr = XmlDictionaryReader.CreateBinaryReader (ms, BinaryFrameSupportReader.soap_dictionary, new XmlDictionaryReaderQuotas (), session);

				string soapNS = "http://www.w3.org/2003/05/soap-envelope";
				string addrNS = "http://www.w3.org/2005/08/addressing";
				string xmlnsNS = "http://www.w3.org/2000/xmlns/";
				string peerNS = "http://schemas.microsoft.com/net/2006/05/peer";

				xr.MoveToContent ();
				AssertNode (xr, 0, XmlNodeType.Element, "s", "Envelope", soapNS, String.Empty, "#1");
				Assert.AreEqual (2, xr.AttributeCount, "#1-1-1");
				Assert.IsTrue (xr.MoveToAttribute ("s", xmlnsNS), "#1-2");
				AssertNode (xr, 1, XmlNodeType.Attribute, "xmlns", "s", xmlnsNS, soapNS, "#2");
				Assert.IsTrue (xr.MoveToAttribute ("a", xmlnsNS), "#2-2");
				AssertNode (xr, 1, XmlNodeType.Attribute, "xmlns", "a", xmlnsNS, addrNS, "#3");
				Assert.IsTrue (xr.Read (), "#3-2");
				AssertNode (xr, 1, XmlNodeType.Element, "s", "Header", soapNS, String.Empty, "#4");
				Assert.IsTrue (xr.Read (), "#4-2");
				AssertNode (xr, 2, XmlNodeType.Element, "a", "Action", addrNS, String.Empty, "#5");
				Assert.IsTrue (xr.MoveToAttribute ("mustUnderstand", soapNS), "#5-2");
				AssertNode (xr, 3, XmlNodeType.Attribute, "s", "mustUnderstand", soapNS, "1", "#6");
				Assert.IsTrue (xr.Read (), "#6-2");
				AssertNode (xr, 3, XmlNodeType.Text, "", "", "", "http://schemas.microsoft.com/net/2006/05/peer/resolver/Resolve", "#7");
				Assert.IsTrue (xr.Read (), "#7-2");
				AssertNode (xr, 2, XmlNodeType.EndElement, "a", "Action", addrNS, String.Empty, "#8");
				Assert.IsTrue (xr.Read (), "#8-2");
				AssertNode (xr, 2, XmlNodeType.Element, "a", "MessageID", addrNS, String.Empty, "#9");
				Assert.IsTrue (xr.Read (), "#9-2");
				Assert.AreEqual (XmlNodeType.Text, xr.NodeType, "#10");
				Assert.IsTrue (xr.Read (), "#10-2");
				AssertNode (xr, 2, XmlNodeType.EndElement, "a", "MessageID", addrNS, String.Empty, "#11");
				Assert.IsTrue (xr.Read (), "#11-2"); // -> a:ReplyTo
				AssertNode (xr, 2, XmlNodeType.Element, "a", "ReplyTo", addrNS, String.Empty, "#12");
				xr.Skip (); // -> a:To
				AssertNode (xr, 2, XmlNodeType.Element, "a", "To", addrNS, String.Empty, "#13");
				xr.Skip (); // -> /s:Header
				AssertNode (xr, 1, XmlNodeType.EndElement, "s", "Header", soapNS, String.Empty, "#14");

				Assert.IsTrue (xr.Read (), "#14-2");
				AssertNode (xr, 1, XmlNodeType.Element, "s", "Body", soapNS, String.Empty, "#15");
				Assert.IsTrue (xr.Read (), "#15-2");
				AssertNode (xr, 2, XmlNodeType.Element, "", "Resolve", peerNS, String.Empty, "#16");
				Assert.IsTrue (xr.MoveToAttribute ("xmlns"), "#16-2");
				AssertNode (xr, 3, XmlNodeType.Attribute, "", "xmlns", xmlnsNS, peerNS, "#17");
				Assert.IsTrue (xr.Read (), "#17-2");
				AssertNode (xr, 3, XmlNodeType.Element, "", "ClientId", peerNS, String.Empty, "#18");
/*
				while (!xr.EOF) {
					xr.Read ();
					Console.WriteLine ("{0}: {1}:{2} {3} {4}", xr.NodeType, xr.Prefix, xr.LocalName, xr.NamespaceURI, xr.Value);
					for (int i = 0; i < xr.AttributeCount; i++) {
						xr.MoveToAttribute (i);
						Console.WriteLine ("  Attribute: {0}:{1} {2} {3}", xr.Prefix, xr.LocalName, xr.NamespaceURI, xr.Value);
					}
				}
*/
			}
		}

		[Test]
		public void ConnectionTcpTransport ()
		{
			var host = new ServiceHost (typeof (Foo));
			var bindingsvc = new CustomBinding (new BindingElement [] {
				new BinaryMessageEncodingBindingElement (),
				new TcpTransportBindingElement () });
			int port = NetworkHelpers.FindFreePort ();
			host.AddServiceEndpoint (typeof (IFoo), bindingsvc, "net.tcp://localhost:" + port + "/");
			host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().IncludeExceptionDetailInFaults = true;
			host.Open (TimeSpan.FromSeconds (5));
			try {
				for (int i = 0; i < 2; i++) {
					var bindingcli = new NetTcpBinding ();
					bindingcli.Security.Mode = SecurityMode.None;
					var cli = new ChannelFactory<IFooClient> (bindingcli, new EndpointAddress ("net.tcp://localhost:" + port + "/")).CreateChannel ();
					Assert.AreEqual ("test for echo", cli.Echo ("TEST FOR ECHO"), "#1");
					var sid = cli.SessionId;
					Assert.AreEqual (3000, cli.Add (1000, 2000), "#2");
					Assert.AreEqual (sid, cli.SessionId, "#3");
					cli.Close ();
				}
			} finally {
				host.Close (TimeSpan.FromSeconds (5));
				var t = new TcpListener (port);
				t.Start ();
				t.Stop ();
			}
		}

		[Test]
		public void ConnectionHttpTransport ()
		{
			var host = new ServiceHost (typeof (Foo));
			var bindingsvc = new CustomBinding (new BindingElement [] {
				new BinaryMessageEncodingBindingElement (),
				new HttpTransportBindingElement () });
			int port = NetworkHelpers.FindFreePort ();
			host.AddServiceEndpoint (typeof (IFoo), bindingsvc, "http://localhost:" + port + "/");
			host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().IncludeExceptionDetailInFaults = true;
			host.Open (TimeSpan.FromSeconds (5));
			try {
				for (int i = 0; i < 2; i++) {
					var bindingcli = new CustomBinding (new BindingElement [] {
						new BinaryMessageEncodingBindingElement (),
						new HttpTransportBindingElement () });
					var cli = new ChannelFactory<IFooClient> (bindingcli, new EndpointAddress ("http://localhost:" + port + "/")).CreateChannel ();
					Assert.AreEqual ("test for echo", cli.Echo ("TEST FOR ECHO"), "#1");
					var sid = cli.SessionId;
					Assert.AreEqual (3000, cli.Add (1000, 2000), "#2");
					Assert.AreEqual (sid, cli.SessionId, "#3");
					cli.Close ();
				}
			} finally {
				host.Close (TimeSpan.FromSeconds (5));
				var t = new TcpListener (port);
				t.Start ();
				t.Stop ();
			}
		}

		public interface IFooClient : IFoo, IClientChannel
		{
		}

		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			string Echo (string msg);

			[OperationContract]
			uint Add (uint v1, uint v2);
		}

		class Foo : IFoo
		{
			public string Echo (string msg)
			{
				return msg.ToLower ();
			}

			public uint Add (uint v1, uint v2)
			{
				return v1 + v2;
			}
		}

		void AssertNode (XmlReader reader, int depth, XmlNodeType nodeType, string prefix, string localName, string ns, string value, string label)
		{
			Assert.AreEqual (nodeType, reader.NodeType, label + ".NodeType");
			Assert.AreEqual (localName, reader.LocalName, label + ".LocalName");
			Assert.AreEqual (prefix, reader.Prefix, label + ".Prefix");
			Assert.AreEqual (ns, reader.NamespaceURI, label + ".NS");
			Assert.AreEqual (value, reader.Value, label + ".Value");
			Assert.AreEqual (depth, reader.Depth, label + ".Depth");
		}
	}

	class BinaryFrameSupportReader : BinaryReader
	{
		public BinaryFrameSupportReader (Stream s)
			: base (s)
		{
		}
		
		public byte [] ReadSizedChunk ()
		{
			int length = Read7BitEncodedInt ();
			
			if (length > 65536)
				throw new InvalidOperationException ("The message is too large.");

			byte [] buffer = new byte [length];
			Read (buffer, 0, length);
			
			return buffer;
		}

		// Copied from BinaryMessageEncoder.cs.

		internal static XmlDictionary soap_dictionary;

		// See [MC-NBFS] in Microsoft OSP. The strings are copied from the PDF, so the actual values might be wrong.
		static readonly string [] dict_strings = {
			"mustUnderstand", "Envelope",
			"http://www.w3.org/2003/05/soap-envelope",
			"http://www.w3.org/2005/08/addressing", "Header", "Action", "To", "Body", "Algorithm", "RelatesTo",
			"http://www.w3.org/2005/08/addressing/anonymous", "URI", "Reference", "MessageID", "Id", "Identifier",
			"http://schemas.xmlsoap.org/ws/2005/02/rm", "Transforms", "Transform", "DigestMethod", "DigestValue", "Address", "ReplyTo", "SequenceAcknowledgement", "AcknowledgementRange", "Upper", "Lower", "BufferRemaining",
			"http://schemas.microsoft.com/ws/2006/05/rm",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/SequenceAcknowledgement", "SecurityTokenReference", "Sequence", "MessageNumber",
			"http://www.w3.org/2000/09/xmldsig#",
			"http://www.w3.org/2000/09/xmldsig#enveloped-signature", "KeyInfo",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
			"http://www.w3.org/2001/04/xmlenc#",
			"http://schemas.xmlsoap.org/ws/2005/02/sc", "DerivedKeyToken", "Nonce", "Signature", "SignedInfo", "CanonicalizationMethod", "SignatureMethod", "SignatureValue", "DataReference", "EncryptedData", "EncryptionMethod", "CipherData", "CipherValue",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", "Security", "Timestamp", "Created", "Expires", "Length", "ReferenceList", "ValueType", "Type", "EncryptedHeader",
			"http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd", "RequestSecurityTokenResponseCollection",
			"http://schemas.xmlsoap.org/ws/2005/02/trust",
			"http://schemas.xmlsoap.org/ws/2005/02/trust#BinarySecret",
			"http://schemas.microsoft.com/ws/2006/02/transactions", "s", "Fault", "MustUnderstand", "role", "relay", "Code", "Reason", "Text", "Node", "Role", "Detail", "Value", "Subcode", "NotUnderstood", "qname", "", "From", "FaultTo", "EndpointReference", "PortType", "ServiceName", "PortName", "ReferenceProperties", "RelationshipType", "Reply", "a",
			"http://schemas.xmlsoap.org/ws/2006/02/addressingidentity", "Identity", "Spn", "Upn", "Rsa", "Dns", "X509v3Certificate",
			"http://www.w3.org/2005/08/addressing/fault", "ReferenceParameters", "IsReferenceParameter",
			"http://www.w3.org/2005/08/addressing/reply",
			"http://www.w3.org/2005/08/addressing/none", "Metadata",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous",
			"http://schemas.xmlsoap.org/ws/2004/08/addressing/fault",
			"http://schemas.xmlsoap.org/ws/2004/06/addressingex", "RedirectTo", "Via",
			"http://www.w3.org/2001/10/xml-exc-c14n#", "PrefixList", "InclusiveNamespaces", "ec", "SecurityContextToken", "Generation", "Label", "Offset", "Properties", "Cookie", "wsc",
			"http://schemas.xmlsoap.org/ws/2004/04/sc",
			"http://schemas.xmlsoap.org/ws/2004/04/security/sc/dk",
			"http://schemas.xmlsoap.org/ws/2004/04/security/sc/sct",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT", "RenewNeeded", "BadContextToken", "c",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/dk",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/sct",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Cancel",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Cancel",
			"http://www.w3.org/2001/04/xmlenc#aes128-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes128",
			"http://www.w3.org/2001/04/xmlenc#aes192-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes192",
			"http://www.w3.org/2001/04/xmlenc#aes256-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-aes256",
			"http://www.w3.org/2001/04/xmlenc#des-cbc",
			"http://www.w3.org/2000/09/xmldsig#dsa-sha1",
			"http://www.w3.org/2001/10/xml-exc-c14n#WithComments",
			"http://www.w3.org/2000/09/xmldsig#hmac-sha1",
			"http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
			"http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1",
			"http://www.w3.org/2001/04/xmlenc#ripemd160",
			"http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p",
			"http://www.w3.org/2000/09/xmldsig#rsa-sha1",
			"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
			"http://www.w3.org/2001/04/xmlenc#rsa-1_5",
			"http://www.w3.org/2000/09/xmldsig#sha1",
			"http://www.w3.org/2001/04/xmlenc#sha256",
			"http://www.w3.org/2001/04/xmlenc#sha512",
			"http://www.w3.org/2001/04/xmlenc#tripledes-cbc",
			"http://www.w3.org/2001/04/xmlenc#kw-tripledes",
			"http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap",
			"http://schemas.xmlsoap.org/2005/02/trust/spnego#GSS_Wrap",
			"http://schemas.microsoft.com/ws/2006/05/security", "dnse", "o", "Password", "PasswordText", "Username", "UsernameToken", "BinarySecurityToken", "EncodingType", "KeyIdentifier",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text",
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ1510",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.0#SAMLAssertionID", "Assertion", "urn:oasis:names:tc:SAML:1.0:assertion",
			"http://docs.oasis-open.org/wss/oasis-wss-rel-token-profile-1.0.pdf#license", "FailedAuthentication", "InvalidSecurityToken", "InvalidSecurity", "k", "SignatureConfirmation", "TokenType",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey",
			"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0",
			"http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID", "AUTH-HASH", "RequestSecurityTokenResponse", "KeySize", "RequestedTokenReference", "AppliesTo", "Authenticator", "CombinedHash", "BinaryExchange", "Lifetime", "RequestedSecurityToken", "Entropy", "RequestedProofToken", "ComputedKey", "RequestSecurityToken", "RequestType", "Context", "BinarySecret",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/spnego",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego", "wst",
			"http://schemas.xmlsoap.org/ws/2004/04/trust",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/CK/PSHA1",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2004/04/security/trust/Nonce", "KeyType",
			"http://schemas.xmlsoap.org/ws/2004/04/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2004/04/trust/PublicKey", "Claims", "InvalidRequest", "RequestFailed", "SignWith", "EncryptWith", "EncryptionAlgorithm", "CanonicalizationAlgorithm", "ComputedKeyAlgorithm", "UseKey",
			"http://schemas.microsoft.com/net/2004/07/secext/WS-SPNego",
			"http://schemas.microsoft.com/net/2004/07/secext/TLSNego", "t",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Issue",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Nonce", "RenewTarget", "CancelTarget", "RequestedTokenCancelled", "RequestedAttachedReference", "RequestedUnattachedReference", "IssuedTokens",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Renew",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/Cancel",
			"http://schemas.xmlsoap.org/ws/2005/02/trust/PublicKey", "Access", "AccessDecision", "Advice", "AssertionID", "AssertionIDReference", "Attribute", "AttributeName", "AttributeNamespace", "AttributeStatement", "AttributeValue", "Audience", "AudienceRestrictionCondition", "AuthenticationInstant", "AuthenticationMethod", "AuthenticationStatement", "AuthorityBinding", "AuthorityKind", "AuthorizationDecisionStatement", "Binding", "Condition", "Conditions", "Decision", "DoNotCacheCondition", "Evidence", "IssueInstant", "Issuer", "Location", "MajorVersion", "MinorVersion", "NameIdentifier", "Format", "NameQualifier", "Namespace", "NotBefore", "NotOnOrAfter", "saml", "Statement", "Subject", "SubjectConfirmation", "SubjectConfirmationData", "ConfirmationMethod", "urn:oasis:names:tc:SAML:1.0:cm:holder-of-key", "urn:oasis:names:tc:SAML:1.0:cm:sender-vouches", "SubjectLocality", "DNSAddress", "IPAddress", "SubjectStatement", "urn:oasis:names:tc:SAML:1.0:am:unspecified", "xmlns", "Resource", "UserName", "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName", "EmailName", "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress", "u", "ChannelInstance",
			"http://schemas.microsoft.com/ws/2005/02/duplex", "Encoding", "MimeType", "CarriedKeyName", "Recipient", "EncryptedKey", "KeyReference", "e",
			"http://www.w3.org/2001/04/xmlenc#Element",
			"http://www.w3.org/2001/04/xmlenc#Content", "KeyName", "MgmtData", "KeyValue", "RSAKeyValue", "Modulus", "Exponent", "X509Data", "X509IssuerSerial", "X509IssuerName", "X509SerialNumber", "X509Certificate", "AckRequested",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/AckRequested", "AcksTo", "Accept", "CreateSequence",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequence", "CreateSequenceRefused", "CreateSequenceResponse",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/CreateSequenceResponse", "FaultCode", "InvalidAcknowledgement", "LastMessage",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/LastMessage", "LastMessageNumberExceeded", "MessageNumberRollover", "Nack", "netrm", "Offer", "r", "SequenceFault", "SequenceTerminated", "TerminateSequence",
			"http://schemas.xmlsoap.org/ws/2005/02/rm/TerminateSequence", "UnknownSequence",
			"http://schemas.microsoft.com/ws/2006/02/tx/oletx", "oletx", "OleTxTransaction", "PropagationToken",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor", "wscoor", "CreateCoordinationContext", "CreateCoordinationContextResponse", "CoordinationContext", "CurrentContext", "CoordinationType", "RegistrationService", "Register", "RegisterResponse", "ProtocolIdentifier", "CoordinatorProtocolService", "ParticipantProtocolService",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse",
			"http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", "ActivationCoordinatorPortType", "RegistrationCoordinatorPortType", "InvalidState", "InvalidProtocol", "InvalidParameters", "NoActivity", "ContextRefused", "AlreadyRegistered",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat", "wsat",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Completion",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Durable2PC",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Volatile2PC", "Prepare", "Prepared", "ReadOnly", "Commit", "Rollback", "Committed", "Aborted", "Replay",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Commit",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Rollback",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepare",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepared",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/ReadOnly",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/Replay",
			"http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", "CompletionCoordinatorPortType", "CompletionParticipantPortType", "CoordinatorPortType", "ParticipantPortType", "InconsistentInternalState", "mstx", "Enlistment", "protocol", "LocalTransactionId", "IsolationLevel", "IsolationFlags", "Description", "Loopback", "RegisterInfo", "ContextId", "TokenId", "AccessDenied", "InvalidPolicy", "CoordinatorRegistrationFailed", "TooManyEnlistments", "Disabled", "ActivityId",
			"http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics",
			"http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#Kerberosv5APREQSHA1",
			"http://schemas.xmlsoap.org/ws/2002/12/policy", "FloodMessage", "LinkUtility", "Hops",
			"http://schemas.microsoft.com/net/2006/05/peer/HopCount", "PeerVia",
			"http://schemas.microsoft.com/net/2006/05/peer", "PeerFlooder", "PeerTo",
			"http://schemas.microsoft.com/ws/2005/05/routing", "PacketRoutable",
			"http://schemas.microsoft.com/ws/2005/05/addressing/none",
			"http://schemas.microsoft.com/ws/2005/05/envelope/none",
			"http://www.w3.org/2001/XMLSchema-instance",
			"http://www.w3.org/2001/XMLSchema", "nil", "type", "char", "boolean", "byte", "unsignedByte", "short", "unsignedShort", "int", "unsignedInt", "long", "unsignedLong", "float", "double", "decimal", "dateTime", "string", "base64Binary", "anyType", "duration", "guid", "anyURI", "QName", "time", "date", "hexBinary", "gYearMonth", "gYear", "gMonthDay", "gDay", "gMonth", "integer", "positiveInteger", "negativeInteger", "nonPositiveInteger", "nonNegativeInteger", "normalizedString", "ConnectionLimitReached",
			"http://schemas.xmlsoap.org/soap/envelope/", "Actor", "Faultcode", "Faultstring", "Faultactor", "Detail"
		};

		static BinaryFrameSupportReader ()
		{
			var d = new XmlDictionary ();
			soap_dictionary = d;
			foreach (var s in dict_strings)
				d.Add (s);
		}
	}
}
#endif
