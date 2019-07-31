//
// WSSecurityTokenSerializerTest.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Security
{
	[TestFixture]
	public class WSSecurityTokenSerializerTest
	{
		const string wssNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
		const string wsuNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

		static X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");

		const string derived_key_token1 = @"<c:DerivedKeyToken u:Id='_1' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd' xmlns:c='http://schemas.xmlsoap.org/ws/2005/02/sc'>
        <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
          <o:Reference ValueType='http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey' URI='#uuid:urn:abc' />
        </o:SecurityTokenReference>
        <c:Offset>0</c:Offset>
        <c:Length>24</c:Length>
        <c:Nonce>BIUeTKeOhR5HeE646ZyA+w==</c:Nonce>
      </c:DerivedKeyToken>";

		const string derived_key_token2 = @"<c:DerivedKeyToken u:Id='_1' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd' xmlns:c='http://schemas.xmlsoap.org/ws/2005/02/sc'>
        <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
          <o:Reference ValueType='urn:my-own-way' URI='#uuid:urn:abc' />
        </o:SecurityTokenReference>
        <c:Offset>0</c:Offset>
        <c:Length>24</c:Length>
        <c:Nonce>BIUeTKeOhR5HeE646ZyA+w==</c:Nonce>
      </c:DerivedKeyToken>";

		const string wrapped_key1 = @"<e:EncryptedKey Id='_0' xmlns:e='http://www.w3.org/2001/04/xmlenc#'>
  <e:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p'>
    <DigestMethod Algorithm='http://www.w3.org/2000/09/xmldsig#sha1' xmlns='http://www.w3.org/2000/09/xmldsig#'></DigestMethod>
  </e:EncryptionMethod>
  <KeyInfo xmlns='http://www.w3.org/2000/09/xmldsig#'>
    <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
<o:KeyIdentifier ValueType='http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1'>GQ3YHlGQhDF1bvMixHliX4uLjlY=</o:KeyIdentifier>
    </o:SecurityTokenReference>
  </KeyInfo>
  <e:CipherData>
    <e:CipherValue>RLRUq81oJNSKPZz4ToCmin7ymCdMpCJiiRx5c1RGZuILiLcU3zCZI2bN9UNgfTHnE4arcJzjwSOeuzFSn948Lr0w6kUaZQjJVzLozu2hBhhb8Kps4ekLWmrsca2c2VmjT9kKEihfCX4s1Pfv9aJyVpT3EGwH7vd9fr9k5G2RtKY=</e:CipherValue>
  </e:CipherData>
  <e:ReferenceList>
    <e:DataReference URI='#_2'></e:DataReference>
  </e:ReferenceList>
</e:EncryptedKey>";

		XmlWriterSettings GetWriterSettings ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return s;
		}

		[Test]
		public void DefaultValues ()
		{
			WSSecurityTokenSerializer ser = new WSSecurityTokenSerializer ();
			DefaultValues (ser);
			DefaultValues (WSSecurityTokenSerializer.DefaultInstance);
		}

		void DefaultValues (WSSecurityTokenSerializer ser)
		{
			Assert.AreEqual (false, ser.EmitBspRequiredAttributes, "#1");
			Assert.AreEqual (128, ser.MaximumKeyDerivationLabelLength, "#2");
			Assert.AreEqual (128, ser.MaximumKeyDerivationNonceLength, "#3");
			Assert.AreEqual (64, ser.MaximumKeyDerivationOffset, "#4");
			Assert.AreEqual (SecurityVersion.WSSecurity11, ser.SecurityVersion, "#5");
		}

		[Test]
		public void WriteX509SecurityToken1 ()
		{
			StringWriter sw = new StringWriter ();
			X509SecurityToken t = new X509SecurityToken (cert, "urn:x509:1");
			Assert.IsNotNull (cert.GetRawCertData (), "premise: X509Certificate2.RawData");
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			string rawdata = Convert.ToBase64String (cert.RawData);
			Assert.AreEqual ("<o:BinarySecurityToken u:Id=\"urn:x509:1\" ValueType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">" + rawdata + "</o:BinarySecurityToken>", sw.ToString ());
		}

		[Test]
		public void WriteUserNameSecurityToken1 ()
		{
			StringWriter sw = new StringWriter ();
			UserNameSecurityToken t = new UserNameSecurityToken ("mono", "poly", "urn:username:1");
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			// Hmm, no PasswordToken (and TokenType) ?
			Assert.AreEqual ("<o:UsernameToken u:Id=\"urn:username:1\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Username>mono</o:Username><o:Password>poly</o:Password></o:UsernameToken>", sw.ToString ());
		}

		[Test]
		public void WriteBinarySecretSecurityToken1 ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [] {0, 1, 2, 3, 4, 5, 6, 7};
			BinarySecretSecurityToken t = new BinarySecretSecurityToken ("urn:binary:1", bytes);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			// AAECAwQFBgc=
			string base64 = Convert.ToBase64String (bytes);
			Assert.AreEqual ("<t:BinarySecret u:Id=\"urn:binary:1\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">" + base64 + "</t:BinarySecret>", sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteRsaSecurityToken ()
		{
			StringWriter sw = new StringWriter ();
			RSA rsa = (RSA) cert.PublicKey.Key;
			RsaSecurityToken t = new RsaSecurityToken (rsa, "urn:rsa:1");
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
		}

		[Test]
		public void WriteGenericXmlSecurityToken1 ()
		{
			StringWriter sw = new StringWriter ();

			XmlElement xml = new XmlDocument ().CreateElement ("foo");
			SecurityToken token = new X509SecurityToken (new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono"));
			SecurityKeyIdentifierClause intref =
				token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause> ();
			SecurityKeyIdentifierClause extref =
			null; //	token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause> ();
			ReadOnlyCollection<IAuthorizationPolicy> policies =
				new ReadOnlyCollection<IAuthorizationPolicy> (
					new IAuthorizationPolicy [0]);

			GenericXmlSecurityToken t = new GenericXmlSecurityToken (xml, token, DateTime.Now, new DateTime (2112, 9, 3), intref, extref, policies);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			// Huh?
			Assert.AreEqual ("<foo />", sw.ToString ());
		}

		[Test]
		public void WriteWrappedKeySecurityToken ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [64];
			for (byte i = 1; i < 64; i++)
				bytes [i] = i;

			SecurityToken wt = new X509SecurityToken (cert);
			SecurityKeyIdentifier ski = new SecurityKeyIdentifier (
				wt.CreateKeyIdentifierClause< X509ThumbprintKeyIdentifierClause> ());
			WrappedKeySecurityToken t = new WrappedKeySecurityToken (
				"urn:wrapper-key:1", bytes, SecurityAlgorithms.RsaOaepKeyWrap, wt, ski);

			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			string actual = sw.ToString ();
			int idx = actual.IndexOf ("<e:CipherValue>", StringComparison.Ordinal);
			Assert.IsTrue (idx >= 0, "No <CipherValue>");
			actual = 
				actual.Substring (0, idx) +
				"<e:CipherValue>removed here" +
				actual.Substring (actual.IndexOf ("</e:CipherValue>", StringComparison.Ordinal));
			Assert.AreEqual ("GQ3YHlGQhDF1bvMixHliX4uLjlY=", Convert.ToBase64String (cert.GetCertHash ()), "premise#1");
			Assert.AreEqual (
				String.Format ("<e:EncryptedKey Id=\"urn:wrapper-key:1\" xmlns:e=\"{0}\"><e:EncryptionMethod Algorithm=\"{1}\"><DigestMethod Algorithm=\"{2}\" xmlns=\"{3}\" /></e:EncryptionMethod><KeyInfo xmlns=\"{3}\"><o:SecurityTokenReference xmlns:o=\"{4}\"><o:KeyIdentifier ValueType=\"{5}\">{6}</o:KeyIdentifier></o:SecurityTokenReference></KeyInfo><e:CipherData><e:CipherValue>removed here</e:CipherValue></e:CipherData></e:EncryptedKey>",
					EncryptedXml.XmlEncNamespaceUrl,
					SecurityAlgorithms.RsaOaepKeyWrap,
					SignedXml.XmlDsigSHA1Url,
					SignedXml.XmlDsigNamespaceUrl,
					"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
					"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1",
					Convert.ToBase64String (cert.GetCertHash ())),// "GQ3YHlGQhDF1bvMixHliX4uLjlY="
				actual);
		}

		[Test]
		public void WriteSecurityContextSecurityToken ()
		{
			StringWriter sw = new StringWriter ();
			SecurityContextSecurityToken t = new SecurityContextSecurityToken (
				new UniqueId ("urn:unique-id:securitycontext:1"),
				"urn:securitycontext:1",
				Convert.FromBase64String ("o/ilseZu+keLBBWGGPlUHweqxIPc4gzZEFWr2nBt640="),
				new DateTime (2006, 9, 26), new DateTime (2006, 9, 27));
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteToken (w, t);
			}
			Assert.AreEqual ("<c:SecurityContextToken u:Id=\"urn:securitycontext:1\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:c=\"http://schemas.xmlsoap.org/ws/2005/02/sc\"><c:Identifier>urn:unique-id:securitycontext:1</c:Identifier></c:SecurityContextToken>", sw.ToString ());
		}

		[Test]
		public void WriteX509IssuerSerialKeyIdentifierClause1 ()
		{
			StringWriter sw = new StringWriter ();
			X509IssuerSerialKeyIdentifierClause ic = new X509IssuerSerialKeyIdentifierClause  (cert);
			string expected = "<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><X509Data xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><X509IssuerSerial><X509IssuerName>CN=Mono Test Root Agency</X509IssuerName><X509SerialNumber>22491767666218099257720700881460366085</X509SerialNumber></X509IssuerSerial></X509Data></o:SecurityTokenReference>";

			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual (expected, sw.ToString (), "WSS1.1");

			sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				new WSSecurityTokenSerializer (SecurityVersion.WSSecurity10).WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual (expected, sw.ToString (), "WSS1.0");
		}

		[Test]
		public void WriteX509ThumbprintKeyIdentifierClause1 ()
		{
			StringWriter sw = new StringWriter ();
			X509ThumbprintKeyIdentifierClause ic = new X509ThumbprintKeyIdentifierClause (cert);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1\">GQ3YHlGQhDF1bvMixHliX4uLjlY=</o:KeyIdentifier></o:SecurityTokenReference>", sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteX509ThumbprintKeyIdentifierClause2 ()
		{
			// WS-Security1.0 x thumbprint = death
			using (XmlWriter w = XmlWriter.Create (TextWriter.Null)) {
				new WSSecurityTokenSerializer (SecurityVersion.WSSecurity10)
					.WriteKeyIdentifierClause (w, new X509ThumbprintKeyIdentifierClause (cert));
			}
		}

		[Test]
		public void WriteX509ThumbprintKeyIdentifierClause3 ()
		{
			// EmitBspRequiredAttributes
			StringWriter sw = new StringWriter ();
			X509ThumbprintKeyIdentifierClause ic = new X509ThumbprintKeyIdentifierClause (cert);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				new WSSecurityTokenSerializer (true).WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:KeyIdentifier ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1\" EncodingType=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\">GQ3YHlGQhDF1bvMixHliX4uLjlY=</o:KeyIdentifier></o:SecurityTokenReference>", sw.ToString ());
		}

		[Test]
		public void WriteEncryptedKeyIdentifierClause ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			SecurityKeyIdentifier cki = new SecurityKeyIdentifier ();
			cki.Add (new X509ThumbprintKeyIdentifierClause (cert));
			EncryptedKeyIdentifierClause ic =
				new EncryptedKeyIdentifierClause (bytes, SecurityAlgorithms.Aes256KeyWrap, cki);
			
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<e:EncryptedKey xmlns:e=\"{0}\"><e:EncryptionMethod Algorithm=\"{1}\" /><KeyInfo xmlns=\"{2}\"><o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:KeyIdentifier ValueType=\"{3}\">GQ3YHlGQhDF1bvMixHliX4uLjlY=</o:KeyIdentifier></o:SecurityTokenReference></KeyInfo><e:CipherData><e:CipherValue>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=</e:CipherValue></e:CipherData></e:EncryptedKey>",
				EncryptedXml.XmlEncNamespaceUrl,
				SecurityAlgorithms.Aes256KeyWrap,
				SignedXml.XmlDsigNamespaceUrl,
				"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1");
			Assert.AreEqual (expected, sw.ToString ());
		}

		[Test]
		public void WriteEncryptedKeyIdentifierClause2 () // derived key
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			SecurityKeyIdentifier cki = new SecurityKeyIdentifier ();
			cki.Add (new X509ThumbprintKeyIdentifierClause (cert));
			EncryptedKeyIdentifierClause ic =
				new EncryptedKeyIdentifierClause (bytes, SecurityAlgorithms.Aes256KeyWrap, cki, "carriedKeyNaaaaame", new byte [32], 32);
			
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<e:EncryptedKey xmlns:e=\"{0}\"><e:EncryptionMethod Algorithm=\"{1}\" /><KeyInfo xmlns=\"{2}\"><o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:KeyIdentifier ValueType=\"{3}\">GQ3YHlGQhDF1bvMixHliX4uLjlY=</o:KeyIdentifier></o:SecurityTokenReference></KeyInfo><e:CipherData><e:CipherValue>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=</e:CipherValue></e:CipherData><e:CarriedKeyName>carriedKeyNaaaaame</e:CarriedKeyName></e:EncryptedKey>",
				EncryptedXml.XmlEncNamespaceUrl,
				SecurityAlgorithms.Aes256KeyWrap,
				SignedXml.XmlDsigNamespaceUrl,
				"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1");
			Assert.AreEqual (expected, sw.ToString ());
		}

		[Test]
		public void WriteEncryptedKeyIdentifierClause3 ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			SecurityKeyIdentifier cki = new SecurityKeyIdentifier ();
			cki.Add (new X509ThumbprintKeyIdentifierClause (cert));
			EncryptedKeyIdentifierClause ic =
				new EncryptedKeyIdentifierClause (bytes, SecurityAlgorithms.Aes256Encryption);
			
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<e:EncryptedKey xmlns:e=\"{0}\"><e:EncryptionMethod Algorithm=\"{1}\" /><e:CipherData><e:CipherValue>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=</e:CipherValue></e:CipherData></e:EncryptedKey>",
				EncryptedXml.XmlEncNamespaceUrl,
				SecurityAlgorithms.Aes256Encryption,
				SignedXml.XmlDsigNamespaceUrl,
				"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1");
			Assert.AreEqual (expected, sw.ToString ());
		}

		[Test]
		public void WriteEncryptedKeyIdentifierClause4 ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			SecurityKeyIdentifier cki = new SecurityKeyIdentifier ();
			cki.Add (new BinarySecretKeyIdentifierClause (bytes));
			EncryptedKeyIdentifierClause ic =
				new EncryptedKeyIdentifierClause (bytes, SecurityAlgorithms.Aes256Encryption);
			
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<e:EncryptedKey xmlns:e=\"{0}\"><e:EncryptionMethod Algorithm=\"{1}\" /><e:CipherData><e:CipherValue>AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=</e:CipherValue></e:CipherData></e:EncryptedKey>",
				EncryptedXml.XmlEncNamespaceUrl,
				SecurityAlgorithms.Aes256Encryption,
				SignedXml.XmlDsigNamespaceUrl,
				"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1");
			Assert.AreEqual (expected, sw.ToString ());
		}

		[Test]
		public void WriteBinarySecretKeyIdentifierClause1 ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			BinarySecretKeyIdentifierClause ic = new BinarySecretKeyIdentifierClause (bytes);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<t:BinarySecret xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">{0}</t:BinarySecret>", Convert.ToBase64String (bytes));
			Assert.AreEqual (expected, sw.ToString (), "#1");
		}

		class MySecurityTokenParameters : SecurityTokenParameters
		{
			public SecurityKeyIdentifierClause CallCreateKeyIdentifierClause (SecurityToken token, SecurityTokenReferenceStyle style)
			{
				return CreateKeyIdentifierClause (token, style);
			}

			protected override SecurityTokenParameters CloneCore ()
			{
				return this;
			}

			protected override bool HasAsymmetricKey {
				get { return false; }
			}
			protected override bool SupportsServerAuthentication {
				get { return false; }
			}
			protected override bool SupportsClientAuthentication {
				get { return false; }
			}
			protected override bool SupportsClientWindowsIdentity {
				get { return false; }
			}
			protected override void InitializeSecurityTokenRequirement (SecurityTokenRequirement r)
			{
			}

			protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (SecurityToken token, SecurityTokenReferenceStyle style)
			{
				throw new Exception ();
			}
		}

/* FIXME: something should output key identifier clause xml like this ...
		[Test]
		public void WriteInternalWrappedKeyIdentifierClause ()
		{
			StringWriter sw = new StringWriter ();
			byte [] bytes = new byte [32];
			EncryptedKeyIdentifierClause eic =
				new EncryptedKeyIdentifierClause (bytes, SecurityAlgorithms.Sha1Digest);
			SecurityKeyIdentifier ski = new SecurityKeyIdentifier (eic);
			WrappedKeySecurityToken token = new WrappedKeySecurityToken ("urn:foo", bytes, SecurityAlgorithms.RsaOaepKeyWrap, new X509SecurityToken (cert), ski);

			MySecurityTokenParameters p = new MySecurityTokenParameters ();
			SecurityKeyIdentifierClause ic =
				p.CallCreateKeyIdentifierClause (token, SecurityTokenReferenceStyle.External);

			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			string expected = String.Format ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:KeyIdentifier ValueType=\"{0}\" Value=\"{1}\" /></o:SecurityTokenReference>",
				"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1", Convert.ToBase64String (bytes));
			Assert.AreEqual (expected, sw.ToString (), "#1");
		}
*/

		[Test]
		public void WriteLocalIdKeyIdentifierClause1 ()
		{
			StringWriter sw = new StringWriter ();
			LocalIdKeyIdentifierClause ic = new LocalIdKeyIdentifierClause ("urn:myIDValue");
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Reference URI=\"#urn:myIDValue\" /></o:SecurityTokenReference>", sw.ToString (), "#1");
		}

		[Test]
		public void WriteLocalIdKeyIdentifierClause2 ()
		{
			StringWriter sw = new StringWriter ();
			LocalIdKeyIdentifierClause ic = new LocalIdKeyIdentifierClause ("#urn:myIDValue");
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			// ... so, specifying an URI including '#' does not make sense
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Reference URI=\"##urn:myIDValue\" /></o:SecurityTokenReference>", sw.ToString (), "#2");
		}

		[Test]
		public void WriteLocalIdKeyIdentifierClause3 ()
		{
			StringWriter sw = new StringWriter ();
			LocalIdKeyIdentifierClause ic = new LocalIdKeyIdentifierClause ("urn:myIDValue", typeof (WrappedKeySecurityToken));
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Reference URI=\"#urn:myIDValue\" /></o:SecurityTokenReference>", sw.ToString (), "#1");
		}

		[Test]
		public void WriteLocalIdKeyIdentifierClause4 () // EmitBsp
		{
			StringWriter sw = new StringWriter ();
			LocalIdKeyIdentifierClause ic = new LocalIdKeyIdentifierClause ("urn:myIDValue", typeof (WrappedKeySecurityToken));
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				new WSSecurityTokenSerializer (true).WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Reference ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey\" URI=\"#urn:myIDValue\" /></o:SecurityTokenReference>", sw.ToString (), "#1");
		}

		[Test]
		[Ignore ("fails on .net; no further verification")]
		public void WriteLocalIdKeyIdentifierClause5 () // derivedKey
		{
			StringWriter sw = new StringWriter ();
			LocalIdKeyIdentifierClause ic = new LocalIdKeyIdentifierClause ("urn:myIDValue", new byte [32], 16, typeof (WrappedKeySecurityToken));
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				new WSSecurityTokenSerializer (true).WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual ("<o:SecurityTokenReference xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><o:Reference ValueType=\"http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey\" URI=\"#urn:myIDValue\" /></o:SecurityTokenReference>", sw.ToString (), "#1");
		}

		[Test]
		public void WriteSecurityContextKeyIdentifierClause ()
		{
			StringWriter sw = new StringWriter ();
			SecurityContextKeyIdentifierClause ic = new SecurityContextKeyIdentifierClause (new UniqueId ("urn:foo:1"), null);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual (@"<o:SecurityTokenReference xmlns:o=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><o:Reference URI=""urn:foo:1"" ValueType=""http://schemas.xmlsoap.org/ws/2005/02/sc/sct"" /></o:SecurityTokenReference>", sw.ToString (), "#1");
			XmlReader reader = XmlReader.Create (new StringReader (sw.ToString ()));
			ic = WSSecurityTokenSerializer.DefaultInstance.ReadKeyIdentifierClause (reader) as SecurityContextKeyIdentifierClause;
			Assert.IsNotNull (ic, "#2");
		}

		class MySslParameters : SslSecurityTokenParameters
		{
			public SecurityKeyIdentifierClause CallCreateKeyIdentifierClause (
				SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
			{
				return CreateKeyIdentifierClause (token, referenceStyle);
			}
		}

		[Test]
		public void WriteSecurityContextKeyIdentifierClause2 ()
		{
			StringWriter sw = new StringWriter ();
			MySslParameters tp = new MySslParameters ();
			SecurityContextSecurityToken sct =
				new SecurityContextSecurityToken (new UniqueId ("urn:foo:1"), "urn:foo:2", new byte [32], DateTime.MinValue, DateTime.MaxValue);
			SecurityKeyIdentifierClause ic =
				tp.CallCreateKeyIdentifierClause (sct, SecurityTokenReferenceStyle.Internal);
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				WSSecurityTokenSerializer.DefaultInstance.WriteKeyIdentifierClause (w, ic);
			}
			Assert.AreEqual (@"<o:SecurityTokenReference xmlns:o=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd""><o:Reference URI=""#urn:foo:2"" /></o:SecurityTokenReference>", sw.ToString (), "#1");
			XmlReader reader = XmlReader.Create (new StringReader (sw.ToString ()));
			ic = WSSecurityTokenSerializer.DefaultInstance.ReadKeyIdentifierClause (reader) as LocalIdKeyIdentifierClause;
			Assert.IsNotNull (ic, "#2");
		}

		[Test]
		public void ReadKeyIdentifierClause ()
		{
			string xml = @"<o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:Reference URI='#uuid-9c90d2c7-c82f-4c63-9b28-fc24479ee3a7-2' />
          </o:SecurityTokenReference>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				SecurityKeyIdentifierClause kic = serializer.ReadKeyIdentifierClause (xr);
				Assert.IsTrue (kic is LocalIdKeyIdentifierClause, "#1");
			}
		}

		[Test]
		public void ReadEncryptedKeyIdentifierClause ()
		{
			string xml = @"<e:EncryptedKey xmlns:ds='http://www.w3.org/2000/09/xmldsig#' xmlns:e='http://www.w3.org/2001/04/xmlenc#' Id='ID_EncryptedKeyClause'>  <e:EncryptionMethod Algorithm='http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p'>  <ds:DigestMethod Algorithm='http://www.w3.org/2000/09/xmldsig#sha1' />  </e:EncryptionMethod>  <ds:KeyInfo>  <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>  <o:Reference URI='#uuid-9c90d2c7-c82f-4c63-9b28-fc24479ee3a7-2' />  </o:SecurityTokenReference>  </ds:KeyInfo>  <e:CipherData>  <e:CipherValue>Iwg585s5eQP5If4/bY/PPBmHVFt23z6MaHDaD9/u1Ua7hveRfoER3d6sJTk7PL4LoLHjwaAa6EGHZyrgq7He+efvsiAhJYTeh/C/RYO7jKdSr8Gp1IIY7wA+/CBhV7SUhRZs4YJ1GE+rIQ/No6FPk/MbpIALEZ6RpqiLYVVCvUI=</e:CipherValue>  </e:CipherData></e:EncryptedKey>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			EncryptedKeyIdentifierClause kic;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				kic = serializer.ReadKeyIdentifierClause (xr) as EncryptedKeyIdentifierClause;
			}
			Assert.IsNotNull (kic, "#1");
			Assert.IsNull (kic.CarriedKeyName, "#2");
			Assert.AreEqual (EncryptedXml.XmlEncRSAOAEPUrl, kic.EncryptionMethod, "#3");
			Assert.AreEqual (1, kic.EncryptingKeyIdentifier.Count, "#4");
			LocalIdKeyIdentifierClause ekic = kic.EncryptingKeyIdentifier [0] as LocalIdKeyIdentifierClause;
			Assert.IsNotNull (ekic, "#5");
			Assert.AreEqual ("uuid-9c90d2c7-c82f-4c63-9b28-fc24479ee3a7-2", ekic.LocalId, "#6");
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // .NET says that KeyIdentifier is not the expected element, but actually it is because of ValueType.
		public void ReadKeyIdentifierReferenceWrongVallueType ()
		{
			string xml = @"<o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:KeyIdentifier ValueType='hogehoge'>xr42fAKDBNItO0aRPKFqc0kaiiU=</o:KeyIdentifier>
          </o:SecurityTokenReference>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				SecurityKeyIdentifierClause kic = serializer.ReadKeyIdentifierClause (xr);
				Assert.IsTrue (kic is X509ThumbprintKeyIdentifierClause, "#1");
			}
		}

		[Test]
		public void ReadKeyIdentifierThumbprint ()
		{
			string xml = @"<o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:KeyIdentifier ValueType='http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1' EncodingType='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary'>xr42fAKDBNItO0aRPKFqc0kaiiU=</o:KeyIdentifier>
          </o:SecurityTokenReference>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				SecurityKeyIdentifierClause kic = serializer.ReadKeyIdentifierClause (xr);
				Assert.IsTrue (kic is X509ThumbprintKeyIdentifierClause, "#1");
			}
		}

		[Test]
		public void ReadKeyIdentifierEncryptedKey ()
		{
			string xml = @"<o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:KeyIdentifier ValueType='http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKeySHA1'>xr42fAKDBNItO0aRPKFqc0kaiiU=</o:KeyIdentifier>
          </o:SecurityTokenReference>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				SecurityKeyIdentifierClause kic = serializer.ReadKeyIdentifierClause (xr);
				Assert.IsTrue (kic is BinaryKeyIdentifierClause, "#1");
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // not sure how this exception type makes sense...
		public void ReadEmptyUsernameToken ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (String.Format ("<o:UsernameToken u:Id='urn:foo' xmlns:o='{0}' xmlns:u='{1}' />", wssNS, wsuNS)))) {
				serializer.ReadToken (xr, null);
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // tokenResolver is null
		public void ReadTokenDerivedKeyTokenNullResolver ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (derived_key_token1))) {
				serializer.ReadToken (xr, null);
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // DerivedKeyToken requires a reference to an existent token.
		public void ReadTokenDerivedKeyTokenRefToNonExistent ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (derived_key_token1))) {
				serializer.ReadToken (xr, GetResolver ());
			}
		}

		[Test]
		public void ReadWriteTokenDerivedKeyTokenRefToExistent ()
		{
			WSSecurityTokenSerializer serializer =
				new WSSecurityTokenSerializer (true); // emitBSP
			SecurityToken token;
			using (XmlReader xr = XmlReader.Create (new StringReader (derived_key_token1))) {
				token = serializer.ReadToken (xr,
					GetResolver (
						new WrappedKeySecurityToken ("uuid:urn:abc", new byte [32], SecurityAlgorithms.RsaOaepKeyWrap, new X509SecurityToken (cert), null)
					));
			}
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw, GetWriterSettings ())) {
				serializer.WriteToken (w, token);
			}
			Assert.AreEqual (derived_key_token1.Replace ('\'', '"').Replace ("  ", "").Replace ("\n", "").Replace ("\r", ""), sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // not sure how this exception type makes sense.
		public void ReadTokenDerivedKeyTokenRefToExistent2 ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (derived_key_token1))) {
				// different token value type to be resolved 
				// than what is explicitly specified in
				// <o:Reference>.
				serializer.ReadToken (xr,
					GetResolver (new X509SecurityToken (cert, "uuid:urn:abc")));
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))] // not sure how this exception type makes sense.
		public void ReadTokenDerivedKeyTokenRefUnsupported ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (derived_key_token2))) {
				// different token value type to be resolved 
				// than what is explicitly specified in
				// <o:Reference>.
				serializer.ReadToken (xr,
					GetResolver (new X509SecurityToken (cert, "uuid:urn:abc")));
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadSecurityContextSecurityTokenNoRegisteredToken ()
		{
			try {
				ReadSecurityContextSecurityTokenNoRegisteredTokenCore ();
				Assert.Fail ("Exception expected.");
			} catch (SecurityTokenException) {
			}
		}

		void ReadSecurityContextSecurityTokenNoRegisteredTokenCore ()
		{
			string xml = "<c:SecurityContextToken u:Id=\"urn:securitycontext:1\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:c=\"http://schemas.xmlsoap.org/ws/2005/02/sc\"><c:Identifier>urn:unique-id:securitycontext:1</c:Identifier></c:SecurityContextToken>";

			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				serializer.ReadToken (xr, GetResolver (new X509SecurityToken (cert, "urn:unique-id:securitycontext:1")));
			}
		}

		[Test]
		[Category ("NotWorking")] // SslNegoCookieResolver needs updates and/or fixes.
		public void ReadSslnegoSCTNoStateEncoder ()
		{
			string cookie = "QgBCAoNCBpkrdXVpZC03MDlhYjYwOC0yMDA0LTQ0ZDUtYjM5Mi1mM2M1YmY3YzY3ZmItMUIErZ3da7enifVFg+e0dObwRLNCCJ4egLowfrwP4Hgn0lOSqlA2fr0k4NAKgRZX+0BVs2EOnwJ6xkIOjzCAEnLHQMkIQhCPMJC+QxtByQhCFI8wgBJyx0DJCEIWjzCQvkMbQckIAQ==";
			string xml = String.Format (@"<c:SecurityContextToken u:Id='uuid-709ab608-2004-44d5-b392-f3c5bf7c67fb-1' xmlns:c='http://schemas.xmlsoap.org/ws/2005/02/sc' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
  <c:Identifier>urn:uuid:b76bdd9d-89a7-45f5-83e7-b474e6f044b3</c:Identifier>
  <dnse:Cookie xmlns:dnse='http://schemas.microsoft.com/ws/2006/05/security'>{0}</dnse:Cookie>
</c:SecurityContextToken>", cookie);
			string expectedKey = "gLowfrwP4Hgn0lOSqlA2fr0k4NAKgRZX+0BVs2EOesY=";

			WSSecurityTokenSerializer serializer =
				new WSSecurityTokenSerializer (MessageSecurityVersion.Default.SecurityVersion,
					false,
					new SamlSerializer (),
					new MyStateEncoder (),
					null);
			SecurityContextSecurityToken sct;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				// Token is not registered, but is restored from the cookie
				sct = serializer.ReadToken (xr, null) as SecurityContextSecurityToken;
			}
			Assert.IsNotNull (sct, "#1");
			Assert.AreEqual (new UniqueId ("urn:uuid:b76bdd9d-89a7-45f5-83e7-b474e6f044b3"), sct.ContextId, "#2");
			Assert.IsNotNull (sct.AuthorizationPolicies.Count, "#3");
			Assert.AreEqual (0, sct.AuthorizationPolicies.Count, "#4");
			Assert.AreEqual (1, sct.SecurityKeys.Count, "#5");
			Assert.AreEqual (expectedKey, Convert.ToBase64String (((SymmetricSecurityKey) sct.SecurityKeys [0]).GetSymmetricKey ()), "#6");

			byte [] xmlbin = Convert.FromBase64String (cookie);
			XmlDictionary dic = new XmlDictionary ();
			for (int i = 0; i < 12; i++)
				dic.Add ("n" + i);
			XmlDictionaryReader br = XmlDictionaryReader.CreateBinaryReader (xmlbin, 0, xmlbin.Length, dic, new XmlDictionaryReaderQuotas ());
			while (br.LocalName != "n4")
				if (!br.Read ())
					Assert.Fail ("Unxpected binary xmlreader failure.");
			byte [] key = br.ReadElementContentAsBase64 ();
			// Hmm, so, looks like the Cookie binary depends not
			// on SSL protection but on the state encoder ...
			// does it make sense, or is a different key resolved
			// as a result of TLS negotiation?
			Assert.AreEqual (expectedKey, Convert.ToBase64String (key), "#7");
		}

		class MyStateEncoder : SecurityStateEncoder
		{
			protected override byte [] DecodeSecurityState (byte [] src)
			{
				return src;
			}
			protected override byte [] EncodeSecurityState (byte [] src)
			{
				return src;
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadSecurityContextSecurityTokenSslnego3 ()
		{
			// full RSTR ... fails
			string xml = @"<t:RequestSecurityTokenResponse Context='uuid-d88a7f14-97b7-4663-a548-c59a2a1c652f' xmlns:t='http://schemas.xmlsoap.org/ws/2005/02/trust' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
        <t:TokenType>http://schemas.xmlsoap.org/ws/2005/02/sc/sct</t:TokenType>
        <t:RequestedSecurityToken>
          <c:SecurityContextToken u:Id='uuid-8921c433-1f44-4ff1-99c7-c70ba90c56c3-1' xmlns:c='http://schemas.xmlsoap.org/ws/2005/02/sc'>
            <c:Identifier>urn:uuid:6ee2d642-484a-4e08-a9f4-a2bfe4f2d540</c:Identifier>
            <dnse:Cookie xmlns:dnse='http://schemas.microsoft.com/ws/2006/05/security'>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAbwpVqF25WkyFYDXHavE7SwAAAAACAAAAAAADZgAAqAAAABAAAAD+ZtWd8MLaBeaMn+xLvyAhAAAAAASAAACgAAAAEAAAAKxhFZ5l669I+hLsZTunH12gAAAAQKjqxZo4eRtLLRO0kA0qHCNaazWddGbVdVzeMY8uIjBgl6UAMroZ6N5MAsACNbKLcYfdtEvZa1P1MTT+8dpsnWRCy5/UcQkg6mlrBAkYzEMYT8yNxRF/xEIXMpRAB5e2De4tUTFwIBIRBBKoay+oWP1M4Hcq7C8HDAqOjNyMOAUILIcz5hMFjtBDwJ4EfogiUVr02xGiXoHqEodxT75wKxQAAABQ3v/KgM1WGIDVcDypm1sNE6SASQ==</dnse:Cookie>
          </c:SecurityContextToken>
        </t:RequestedSecurityToken>
        <t:RequestedAttachedReference>
          <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:Reference URI='#uuid-8921c433-1f44-4ff1-99c7-c70ba90c56c3-1'>
            </o:Reference>
          </o:SecurityTokenReference>
        </t:RequestedAttachedReference>
        <t:RequestedUnattachedReference>
          <o:SecurityTokenReference xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'>
            <o:Reference URI='urn:uuid:6ee2d642-484a-4e08-a9f4-a2bfe4f2d540' ValueType='http://schemas.xmlsoap.org/ws/2005/02/sc/sct'>
            </o:Reference>
          </o:SecurityTokenReference>
        </t:RequestedUnattachedReference>
        <t:RequestedProofToken>
          <e:EncryptedKey xmlns:e='http://www.w3.org/2001/04/xmlenc#'>
            <e:EncryptionMethod Algorithm='http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap'>
            </e:EncryptionMethod>
            <e:CipherData>
              <e:CipherValue>FwMBADB9aB76Af+8UmE6nuo5bSh1OwbBjlImD1BY2NUbcByLmCgIARvC+KutimPRXwnMio8=</e:CipherValue>
            </e:CipherData>
          </e:EncryptedKey>
        </t:RequestedProofToken>
        <t:Lifetime>
          <u:Created>2007-03-09T18:51:37.109Z</u:Created>
          <u:Expires>2007-03-10T04:51:37.109Z</u:Expires>
        </t:Lifetime>
        <t:KeySize>256</t:KeySize>
        <t:BinaryExchange ValueType=' http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego' EncodingType='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary'>FAMBAAEBFgMBACALQXwZMThkzZ0m4ICTSg/tcKk2eB+IKLRIhwFKHm+G6w==</t:BinaryExchange>
      </t:RequestSecurityTokenResponse>
      <t:RequestSecurityTokenResponse Context='uuid-d88a7f14-97b7-4663-a548-c59a2a1c652f' xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
        <t:Authenticator>
          <t:CombinedHash>O6A+tpZvcUnI/+HMW2qWREreuFHDV3SVfMCJ2haq27A=</t:CombinedHash>
        </t:Authenticator>
      </t:RequestSecurityTokenResponse>";
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				serializer.ReadToken (xr, null);
			}
		}


		[Test]
		public void ReadWrappedKeySecurityToken ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (wrapped_key1))) {
				WrappedKeySecurityToken token = serializer.ReadToken (xr, GetResolver (new X509SecurityToken (cert))) as WrappedKeySecurityToken;
				Assert.IsNotNull (token, "#1");
				Assert.AreEqual (1, token.SecurityKeys.Count, "#2");
				SymmetricSecurityKey sk = token.SecurityKeys [0] as SymmetricSecurityKey;
				Assert.IsNotNull (sk, "#3");
				byte [] wk = Convert.FromBase64String ("RLRUq81oJNSKPZz4ToCmin7ymCdMpCJiiRx5c1RGZuILiLcU3zCZI2bN9UNgfTHnE4arcJzjwSOeuzFSn948Lr0w6kUaZQjJVzLozu2hBhhb8Kps4ekLWmrsca2c2VmjT9kKEihfCX4s1Pfv9aJyVpT3EGwH7vd9fr9k5G2RtKY=");
				Assert.AreEqual (wk, token.GetWrappedKey (), "#4");
			}
		}

		[Test]
		public void ReadWrappedKeySecurityTokenImplCheck ()
		{
			SecurityTokenResolver tokenResolver = GetResolver (new X509SecurityToken (cert));
			XmlReader reader = XmlReader.Create (new StringReader (wrapped_key1));
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;

			EncryptedKey ek = new EncryptedKey ();
			ek.LoadXml (new XmlDocument ().ReadNode (reader) as XmlElement);
			SecurityKeyIdentifier ki = new SecurityKeyIdentifier ();
			foreach (KeyInfoClause kic in ek.KeyInfo)
				ki.Add (serializer.ReadKeyIdentifierClause (new XmlNodeReader (kic.GetXml ())));
			SecurityToken token = tokenResolver.ResolveToken (ki);
			string alg = ek.EncryptionMethod.KeyAlgorithm;

			SecurityKey skey = token.SecurityKeys [0];
			Assert.IsTrue (skey is X509AsymmetricSecurityKey, "#1");
			Assert.IsTrue (skey.IsSupportedAlgorithm (alg), "#2");
			Assert.AreEqual (
				EncryptedXml.DecryptKey (ek.CipherData.CipherValue, cert.PrivateKey as RSA, true),
				skey.DecryptKey (alg, ek.CipherData.CipherValue),
				"#3");

			byte [] key = skey.DecryptKey (alg, ek.CipherData.CipherValue);
			WrappedKeySecurityToken wk =
				new WrappedKeySecurityToken (ek.Id, key, alg, token, ki);
			Assert.AreEqual (
				EncryptedXml.DecryptKey (ek.CipherData.CipherValue, cert.PrivateKey as RSA, true),
				skey.DecryptKey (alg, wk.GetWrappedKey ()),
				"#4");
		}

		[Test]
		// It raises strange XmlException that wraps ArgumentNullException. Too silly to follow.
		public void ReadWrappedKeySecurityTokenNullResolver ()
		{
			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (wrapped_key1))) {
				try {
					serializer.ReadToken (xr, null);
					Assert.Fail ("Should fail due to the lack of resolver");
				} catch {
				}
			}
		}

		[Test]
		[Ignore ("not sure how we can consume this RequestedProofToken yet.")]
		public void ReadTlsnegoRequestedProofToken ()
		{
			string xml = @"<e:EncryptedKey xmlns:e=""http://www.w3.org/2001/04/xmlenc#""><e:EncryptionMethod Algorithm=""http://schemas.xmlsoap.org/2005/02/trust/tlsnego#TLS_Wrap""></e:EncryptionMethod><e:CipherData><e:CipherValue>FwMBADD/I64jS8yQM4+yn1FPr1+enSjRwoyw1c/hdEDWqfkW/parE9yq5zNKwO0g7zQaFXg=</e:CipherValue></e:CipherData></e:EncryptedKey>";

			WSSecurityTokenSerializer serializer =
				WSSecurityTokenSerializer.DefaultInstance;
			using (XmlReader xr = XmlReader.Create (new StringReader (xml))) {
				serializer.ReadToken (xr, GetResolver (new X509SecurityToken (cert, "urn:unique-id:foo")));
			}
		}

		class MyResolver : SecurityTokenResolver
		{
			protected override bool TryResolveTokenCore (SecurityKeyIdentifier ident, out SecurityToken token)
			{
throw new Exception ("1");
//				token = null;
//				return false;
			}

			protected override bool TryResolveTokenCore (SecurityKeyIdentifierClause clause, out SecurityToken token)
			{
throw new Exception ("2");
//				token = null;
//				return false;
			}

			protected override bool TryResolveSecurityKeyCore (SecurityKeyIdentifierClause clause, out SecurityKey key)
			{
throw new Exception ("3");
//				key = null;
//				return false;
			}
		}

		SecurityTokenResolver GetResolver (params SecurityToken [] tokens)
		{
			return SecurityTokenResolver.CreateDefaultSecurityTokenResolver (
				new ReadOnlyCollection<SecurityToken> (tokens), true);
		}

		[Test]
		public void GetTokenTypeUri ()
		{
			new MyWSSecurityTokenSerializer ().TestGetTokenTypeUri ();
		}
	}

	class MyWSSecurityTokenSerializer : WSSecurityTokenSerializer
	{
		public void TestGetTokenTypeUri ()
		{
			Assert.IsNull (GetTokenTypeUri (GetType ()), "#1");
			Assert.AreEqual ("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3",
				GetTokenTypeUri (typeof (X509SecurityToken)), "#2");
			Assert.IsNull (GetTokenTypeUri (typeof (RsaSecurityToken)), "#3");
			Assert.AreEqual ("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1",
				GetTokenTypeUri (typeof (SamlSecurityToken)), "#4");
			Assert.AreEqual ("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#UsernameToken",
				GetTokenTypeUri (typeof (UserNameSecurityToken)), "#5");
			Assert.IsNull (GetTokenTypeUri (typeof (SspiSecurityToken)), "#6");
			Assert.AreEqual ("http://schemas.xmlsoap.org/ws/2005/02/sc/sct",
				GetTokenTypeUri (typeof (SecurityContextSecurityToken)), "#7");
			Assert.IsNull (GetTokenTypeUri (typeof (GenericXmlSecurityToken)), "#8");
			Assert.AreEqual ("http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ",
				GetTokenTypeUri (typeof (KerberosRequestorSecurityToken)), "#9");
			Assert.AreEqual ("http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#EncryptedKey",
				GetTokenTypeUri (typeof (WrappedKeySecurityToken)), "#10");
		}
	}
}
#endif

