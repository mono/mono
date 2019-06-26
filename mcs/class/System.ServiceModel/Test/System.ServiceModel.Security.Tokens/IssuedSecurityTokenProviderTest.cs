//
// IssuedSecurityTokenProviderTest.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.IdentityModel.Tokens;
using System.Text;
using System.Xml;
using NUnit.Framework;

using MonoTests.System.ServiceModel.Channels;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Security.Tokens
{
	[TestFixture]
	public class IssuedSecurityTokenProviderTest
	{
		[Test]
		public void DefaultValues ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			Assert.AreEqual (true, p.CacheIssuedTokens, "#1");
			Assert.AreEqual (TimeSpan.FromMinutes (1), p.DefaultOpenTimeout, "#2");
			Assert.AreEqual (TimeSpan.FromMinutes (1), p.DefaultCloseTimeout, "#3");
			Assert.IsNotNull (p.IdentityVerifier, "#4");
			Assert.AreEqual (60, p.IssuedTokenRenewalThresholdPercentage, "#5");
			Assert.IsNull (p.IssuerAddress, "#6");
			Assert.AreEqual (0, p.IssuerChannelBehaviors.Count, "#7");
			Assert.AreEqual (SecurityKeyEntropyMode.CombinedEntropy, p.KeyEntropyMode, "#8");
			Assert.AreEqual (TimeSpan.MaxValue, p.MaxIssuedTokenCachingTime, "#9");
			Assert.AreEqual (MessageSecurityVersion.Default,
				p.MessageSecurityVersion, "#10");
			Assert.IsNull (p.SecurityAlgorithmSuite, "#11");
			Assert.IsNull (p.SecurityTokenSerializer, "#12");
			Assert.IsNull (p.TargetAddress, "#13");
			Assert.AreEqual (true, p.SupportsTokenCancellation, "#14");
			Assert.AreEqual (0, p.TokenRequestParameters.Count, "#15");
			Assert.IsNull (p.IssuerBinding, "#16");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenWithoutSerializer ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenWithoutIssuerAddress ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.SecurityTokenSerializer = WSSecurityTokenSerializer.DefaultInstance;
			p.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenWithoutBinding ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.SecurityTokenSerializer = WSSecurityTokenSerializer.DefaultInstance;
			p.IssuerAddress = new EndpointAddress ("http://localhost:8080");
			p.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenWithoutTargetAddress ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.SecurityTokenSerializer = WSSecurityTokenSerializer.DefaultInstance;
			p.IssuerAddress = new EndpointAddress ("http://localhost:8080");
			p.IssuerBinding = new BasicHttpBinding ();

			// wiithout it indigo causes NRE
			p.SecurityAlgorithmSuite = SecurityAlgorithmSuite.Default;
			p.Open ();
		}

		[Test]
		[Category ("NotWorking")]
		public void Open ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (new BasicHttpBinding ());
			try {
				p.Open ();
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetTokenWithoutOpen ()
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.GetToken (TimeSpan.FromSeconds (10));
		}

		// From WinFX beta2:
		// System.ServiceModel.Security.SecurityNegotiationException : 
		// SOAP security negotiation with 'stream:dummy' for target
		// 'stream:dummy' failed. See inner exception for more details.
		// ----> System.InvalidOperationException : The request
		// message must be protected. This is required by an operation
		// of the contract ('IWsTrustFeb2005SecurityTokenService',
		// 'http://tempuri.org/'). The protection must be provided by
		// the binding ('BasicHttpBinding','http://tempuri.org/').
		[Test]
		[ExpectedException (typeof (SecurityNegotiationException))]
		[Category ("NotWorking")]
		public void GetTokenNoSecureBinding ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (new BasicHttpBinding ());
			try {
				p.Open ();
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		[Test]
		// SymmetricSecurityBindingElement requires protection
		// token parameters to build a channel or listener factory.
		[ExpectedException (typeof (SecurityNegotiationException))]
		[Category ("NotWorking")]
		public void GetTokenWithoutProtectionTokenParameters ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (CreateIssuerBinding (null, false));
			try {
				p.Open ();
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		// SecurityNegotiationException (InvalidOperationException (
		//   "The service certificate is not provided for target
		//   'stream:dummy'. Specify a service certificate in 
		//   ClientCredentials."))
		[Test]
		[ExpectedException (typeof (SecurityNegotiationException))]
		[Category ("NotWorking")]
		public void GetTokenWithoutServiceCertificate ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (CreateIssuerBinding (null, true));
			p.IssuerAddress = new EndpointAddress ("stream:dummy");
			try {
				p.Open (TimeSpan.FromSeconds (5));
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (MyException))]
		public void GetTokenWrongResponse ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (CreateIssuerBinding (new RequestSender (OnGetTokenWrongResponse), true));
			try {
				p.Open (TimeSpan.FromSeconds (5));
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (MessageSecurityException))]
		public void GetTokenUnsignedReply ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (CreateIssuerBinding (new RequestSender (OnGetTokenUnsignedReply), true));
			try {
				p.Open (TimeSpan.FromSeconds (5));
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		// InnerException: System.InvalidOperationException:
		// The issuer must provide a computed key in key entropy mode
		// 'CombinedEntropy'.
		[Test]
		[Ignore ("todo")]
		[ExpectedException (typeof (SecurityNegotiationException))]
		public void GetTokenNoEntropyInResponseInCombinedMode ()
		{
			// FIXME: implement it after we get working token issuer.
			// In the reply, do not include Nonce
		}

		// on the other hand, in Client entropy mode it must not
		// provide entropy.
		[Test]
		[Ignore ("todo")]
		[ExpectedException (typeof (SecurityNegotiationException))]
		public void GetTokenIncludesEntropyInResponseInClientMode ()
		{
			// FIXME: implement it after we get working token issuer.
			// specify SecurityKeyEntropyMode.ClientEntropy on 
			// client side. And in the reply, include Nonce.
		}

		[Test]
		[Ignore ("need to implement response")]
		[Category ("NotWorking")]
		public void GetToken ()
		{
			IssuedSecurityTokenProvider p = SetupProvider (CreateIssuerBinding (new RequestSender (OnGetToken), true));
			try {
				p.Open (TimeSpan.FromSeconds (5));
				p.GetToken (TimeSpan.FromSeconds (10));
			} finally {
				if (p.State == CommunicationState.Opened)
					p.Close ();
			}
		}

		class MyException : Exception
		{
		}

		Message OnGetTokenWrongResponse (Message input)
		{
			VerifyInput (input.CreateBufferedCopy (10000));

			throw new MyException ();
		}

		Message OnGetTokenUnsignedReply (Message input)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Response>RESPONSE</Response>");

			Message msg = Message.CreateMessage (input.Version, "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/IssueResponse", doc.DocumentElement);
			msg.Headers.Add (MessageHeader.CreateHeader (
				"Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", null, true));

			return msg;
		}

		Message OnGetToken (Message input)
		{
			MessageBuffer buf = input.CreateBufferedCopy (10000);
			VerifyInput2 (buf);

			// FIXME: create response message (when I understand what I should return.)
//			throw new MyException ();
//*
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Response>RESPONSE</Response>");
			X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			SignedXml sxml = new SignedXml (doc);
			MemoryStream ms = new MemoryStream (new byte [] {1, 2, 3});
			sxml.AddReference (new Reference (ms));
			sxml.SigningKey = cert.PrivateKey;
			sxml.ComputeSignature ();

			Message msg = Message.CreateMessage (input.Version, "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue", sxml.GetXml ());
			msg.Headers.Add (MessageHeader.CreateHeader (
				"Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", null, true));

			return msg;
//*/
		}

		void VerifyInput (MessageBuffer buf)
		{
			Message input = buf.CreateMessage ();
/*
XmlWriterSettings settings = new XmlWriterSettings ();
settings.Indent = true;
using (XmlWriter w = XmlWriter.Create (Console.Error, settings)) {
buf.CreateMessage ().WriteMessage (w);
}
Console.Error.WriteLine ("******************** DONE ********************");
Console.Error.Flush ();
*/

			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue", input.Headers.Action, "GetToken.Request.Action");
			Assert.IsNotNull (input.Headers.MessageId, "GetToken.Request.MessageID");
			// in the raw Message it is "http://www.w3.org/2005/08/addressing/anonymous", but it is replaced by MessageHeaders implementation.
			Assert.AreEqual (new EndpointAddress ("http://schemas.microsoft.com/2005/12/ServiceModel/Addressing/Anonymous"), input.Headers.ReplyTo, "GetToken.Request.ReplyTo");

			// o:Security
			// FIXME: test WSSecurity more
			// <o:Security>
			//  <u:Timestamp>
			//   <u:Created>...</u:Created>
			//   <u:Expires>...</u:Expires>
			//  </u:Timestamp>
			//  <o:BinarySecurityToken>...</o:BinarySecurityToken>
			//  <e:EncryptedKey>
			//   <e:EncryptionMethod><DigestMethod/></e:EncryptionMethod>
			//   <KeyInfo>
			//    <o:SecurityTokenReference><o:Reference/></o:SecurityTokenReference>
			//   </KeyInfo>
			//   <e:CipherData>
			//    <e:CipherValue>...</e:CipherValue>
			//   </e:CipherData>
			//  </e:EncryptedKey>
			//  [
			//  <c:DerivedKeyToken>
			//   <o:SecurityTokenReference><o:Reference/></o:SecurityTokenReference>
			//   <c:Offset>...</c:Offset>
			//   <c:Length>...</c:Length>
			//   <c:Nonce>...</c:Nonce>
			//  </c:DerivedKeyToken>
			//  ]
			//  <e:ReferenceList>
			//   [
			//   <e:DataReference>
			//   ]
			//  </e:ReferenceList>
			//  <e:EncryptedData>
			//   <e:EncryptionMethod/>
			//   <KeyInfo> {{....}} </KeyInfo>
			//   <e:CipherData> {{....}} </e:CipherData>
			//  </e:EncryptedData>
			// </o:Security>
			int i = input.Headers.FindHeader ("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			Assert.IsTrue (i >= 0, "Security header existence");
			MessageHeaderInfo info = input.Headers [i];
			Assert.IsNotNull (info, "Security header item");
			XmlReader r = input.Headers.GetReaderAtHeader (i);

			// FIXME: test WSSecurity more
			// <o:Security>
			r.MoveToContent ();
			r.ReadStartElement ("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			//  <u:Timestamp>
			r.MoveToContent ();
			r.ReadStartElement ("Timestamp", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			//   <u:Created>...</u:Created>
			r.MoveToContent ();
			r.ReadStartElement ("Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			r.ReadString ();
			r.MoveToContent ();
			r.ReadEndElement ();
			//   <u:Expires>...</u:Expires>
			r.MoveToContent ();
			r.ReadStartElement ("Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
			r.ReadString ();
			r.MoveToContent ();
			r.ReadEndElement ();
			//  </u:Timestamp>
			r.MoveToContent ();
			r.ReadEndElement ();
			//  <o:BinarySecurityToken>...</o:BinarySecurityToken>
			r.MoveToContent ();
			r.ReadStartElement ("BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			byte [] rawcert = Convert.FromBase64String (r.ReadString ());
			r.ReadEndElement ();
			X509Certificate2 cert = new X509Certificate2 (rawcert);

			// FIXME: test EncryptedKey
			r.MoveToContent ();
			r.Skip ();
			//  <e:EncryptedKey>
			//   <e:EncryptionMethod><DigestMethod/></e:EncryptionMethod>
			//   <KeyInfo>
			//    <o:SecurityTokenReference><o:Reference/></o:SecurityTokenReference>
			//   </KeyInfo>
			//   <e:CipherData>
			//    <e:CipherValue>...</e:CipherValue>
			//   </e:CipherData>
			//  </e:EncryptedKey>

			// FIXME: test DerivedKeyTokens
			r.MoveToContent ();
			while (r.LocalName == "DerivedKeyToken") {
				r.Skip ();
				r.MoveToContent ();
			}
			//  [
			//  <c:DerivedKeyToken>
			//   <o:SecurityTokenReference><o:Reference/></o:SecurityTokenReference>
			//   <c:Offset>...</c:Offset>
			//   <c:Length>...</c:Length>
			//   <c:Nonce>...</c:Nonce>
			//  </c:DerivedKeyToken>
			//  ]
			
			//  <e:ReferenceList>
			//   [
			//   <e:DataReference>
			//   ]
			//  </e:ReferenceList>
			//  <e:EncryptedData>
			//   <e:EncryptionMethod/>
			//   <KeyInfo> {{....}} </KeyInfo>
			//   <e:CipherData> {{....}} </e:CipherData>
			//  </e:EncryptedData>
			// </o:Security>

			// SOAP Body
			r = input.GetReaderAtBodyContents (); // just verifying itself ;)
		}

		XmlElement VerifyInput2 (MessageBuffer buf)
		{
			Message msg2 = buf.CreateMessage ();
			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw))) {
				msg2.WriteMessage (w);
			}
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.LoadXml (sw.ToString ());

			// decrypt the key with service certificate privkey
			PaddingMode mode = PaddingMode.PKCS7; // not sure which is correct ... ANSIX923, ISO10126, PKCS7, Zeros, None.
			EncryptedXml encXml = new EncryptedXml (doc);
			encXml.Padding = mode;
			X509Certificate2 cert2 = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
			nsmgr.AddNamespace ("c", "http://schemas.xmlsoap.org/ws/2005/02/sc");
			nsmgr.AddNamespace ("o", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			nsmgr.AddNamespace ("e", "http://www.w3.org/2001/04/xmlenc#");
			nsmgr.AddNamespace ("dsig", "http://www.w3.org/2000/09/xmldsig#");
			XmlNode n = doc.SelectSingleNode ("//o:Security/e:EncryptedKey/e:CipherData/e:CipherValue", nsmgr);
			Assert.IsNotNull (n, "premise: enckey does not exist");
			string raw = n.InnerText;
			byte [] rawbytes = Convert.FromBase64String (raw);
			RSACryptoServiceProvider rsa = (RSACryptoServiceProvider) cert2.PrivateKey;
			byte [] decryptedKey = EncryptedXml.DecryptKey (rawbytes, rsa, true);//rsa.Decrypt (rawbytes, true);

#if false
			// create derived keys
			Dictionary<string,byte[]> keys = new Dictionary<string,byte[]> ();
			InMemorySymmetricSecurityKey skey =
				new InMemorySymmetricSecurityKey (decryptedKey);
			foreach (XmlElement el in doc.SelectNodes ("//o:Security/c:DerivedKeyToken", nsmgr)) {
				n = el.SelectSingleNode ("c:Offset", nsmgr);
				int offset = (n == null) ? 0 :
					int.Parse (n.InnerText, CultureInfo.InvariantCulture);
				n = el.SelectSingleNode ("c:Length", nsmgr);
				int length = (n == null) ? 32 :
					int.Parse (n.InnerText, CultureInfo.InvariantCulture);
				n = el.SelectSingleNode ("c:Label", nsmgr);
				byte [] label = (n == null) ? decryptedKey :
					Convert.FromBase64String (n.InnerText);
				n = el.SelectSingleNode ("c:Nonce", nsmgr);
				byte [] nonce = (n == null) ? new byte [0] :
					Convert.FromBase64String (n.InnerText);
				byte [] derkey = skey.GenerateDerivedKey (
					//SecurityAlgorithms.Psha1KeyDerivation,
					"http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1",
// FIXME: maybe due to the label, this key resolution somehow does not seem to work.
					label,
					nonce,
					length * 8,
					offset);

				keys [el.GetAttribute ("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")] = derkey;
			}
#endif

			// decrypt the signature with the decrypted key
#if true
			n = doc.SelectSingleNode ("//o:Security/e:EncryptedData/e:CipherData/e:CipherValue", nsmgr);
			Assert.IsNotNull (n, "premise: encdata does not exist");
			raw = n.InnerText;
			rawbytes = Convert.FromBase64String (raw);
			Rijndael aes = RijndaelManaged.Create ();
//			aes.Key = keys [n.SelectSingleNode ("../../dsig:KeyInfo/o:SecurityTokenReference/o:Reference/@URI", nsmgr).InnerText.Substring (1)];
			aes.Key = decryptedKey;
			aes.Mode = CipherMode.CBC;
			aes.Padding = mode;
			MemoryStream ms = new MemoryStream ();
			CryptoStream cs = new CryptoStream (ms, aes.CreateDecryptor (), CryptoStreamMode.Write);
			cs.Write (rawbytes, 0, rawbytes.Length);
			cs.Close ();
			byte [] decryptedSignature = ms.ToArray ();
#else
			Rijndael aes = RijndaelManaged.Create ();
//			aes.Key = keys [n.SelectSingleNode ("../../dsig:KeyInfo/o:SecurityTokenReference/o:Reference/@URI", nsmgr).InnerText.Substring (1)];
			aes.Key = decryptedKey;
			aes.Mode = CipherMode.CBC;
			aes.Padding = mode;

			EncryptedData ed = new EncryptedData ();
			n = doc.SelectSingleNode ("//o:Security/e:EncryptedData", nsmgr);
			Assert.IsNotNull (n, "premise: encdata does not exist");
			ed.LoadXml (n as XmlElement);
			byte [] decryptedSignature = encXml.DecryptData (ed, aes);
#endif
//Console.Error.WriteLine (Encoding.UTF8.GetString (decryptedSignature));
//Console.Error.WriteLine ("============= Decrypted Signature End ===========");

			// decrypt the body with the decrypted key
#if true
			n = doc.SelectSingleNode ("//s:Body/e:EncryptedData/e:CipherData/e:CipherValue", nsmgr);
			Assert.IsNotNull (n, "premise: encdata does not exist");
			raw = n.InnerText;
			rawbytes = Convert.FromBase64String (raw);
//			aes.Key = keys [n.SelectSingleNode ("../../dsig:KeyInfo/o:SecurityTokenReference/o:Reference/@URI", nsmgr).InnerText.Substring (1)];
			aes.Key = decryptedKey;
			ms = new MemoryStream ();
			cs = new CryptoStream (ms, aes.CreateDecryptor (), CryptoStreamMode.Write);
			cs.Write (rawbytes, 0, rawbytes.Length);
			cs.Close ();
			byte [] decryptedBody = ms.ToArray ();
#else
			// decrypt the body with the decrypted key
			EncryptedData ed2 = new EncryptedData ();
			XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/e:EncryptedData", nsmgr) as XmlElement;
			ed2.LoadXml (el);
//			aes.Key = keys [n.SelectSingleNode ("../../dsig:KeyInfo/o:SecurityTokenReference/o:Reference/@URI", nsmgr).InnerText.Substring (1)];
			aes.Key = decryptedKey;
			byte [] decryptedBody = encXml.DecryptData (ed2, aes);
#endif
//foreach (byte b in decryptedBody) Console.Error.Write ("{0:X02} ", b);
Console.Error.WriteLine (Encoding.UTF8.GetString (decryptedBody));
Console.Error.WriteLine ("============= Decrypted Body End ===========");

			// FIXME: find out what first 16 bytes mean.
			for (int mmm = 0; mmm < 16; mmm++) decryptedBody [mmm] = 0x20;
			doc.LoadXml (Encoding.UTF8.GetString (decryptedBody));
			Assert.AreEqual ("RequestSecurityToken", doc.DocumentElement.LocalName, "#b-1");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/02/trust", doc.DocumentElement.NamespaceURI, "#b-2");

			return doc.DocumentElement;
		}

		Binding CreateIssuerBinding (RequestSender handler, bool tokenParams)
		{
			SymmetricSecurityBindingElement sbe =
				new SymmetricSecurityBindingElement ();
			if (tokenParams)
				sbe.ProtectionTokenParameters = new X509SecurityTokenParameters ();
			sbe.LocalServiceSettings.NegotiationTimeout = TimeSpan.FromSeconds (5);
			sbe.KeyEntropyMode = SecurityKeyEntropyMode.ClientEntropy;
			//sbe.IncludeTimestamp = false;
			//sbe.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;

			// for ease of decryption, let's remove DerivedKeyToken.
			sbe.SetKeyDerivation (false);

			return new CustomBinding (
//				new DebugBindingElement (),
				sbe,
				new TextMessageEncodingBindingElement (),
				new HandlerTransportBindingElement (handler));
		}

		EndpointAddress GetSecureEndpointAddress (string uri)
		{
			return new EndpointAddress (new Uri (uri),
				new X509CertificateEndpointIdentity (
					new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono")));
		}

		IssuedSecurityTokenProvider SetupProvider (Binding binding)
		{
			IssuedSecurityTokenProvider p =
				new IssuedSecurityTokenProvider ();
			p.SecurityTokenSerializer = WSSecurityTokenSerializer.DefaultInstance;
			p.IssuerAddress = GetSecureEndpointAddress ("stream:dummy");
			p.IssuerBinding = binding;

			// wiithout it indigo causes NRE
			p.SecurityAlgorithmSuite = SecurityAlgorithmSuite.Default;

			p.TargetAddress = new EndpointAddress ("http://localhost:9090");
			return p;
		}
	}
}
#endif

