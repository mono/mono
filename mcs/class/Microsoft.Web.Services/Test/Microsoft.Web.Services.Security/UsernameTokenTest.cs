//
// UsernameTokenTest.cs - NUnit Test Cases for UsernameToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Web.Services.Protocols;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	[TestFixture]
	public class UsernameTokenTest : Assertion {

		[Test]
		public void Constructor_UP () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertEquals ("Username", "me", token.Username);
			AssertEquals ("Password", "mine", token.Password);
			AssertEquals ("PasswordOption", PasswordOption.SendNone, token.PasswordOption);
			AssertNull ("Nonce", token.Nonce);
		}

		[Test]
		public void Constructor_UPO () 
		{
			UsernameToken token = new UsernameToken ("me", "mine", PasswordOption.SendNone);
			AssertEquals ("SendNone", PasswordOption.SendNone, token.PasswordOption);
			AssertNull ("Nonce", token.Nonce);

			token = new UsernameToken ("me", "mine", PasswordOption.SendPlainText);
			AssertEquals ("SendPlainText", PasswordOption.SendPlainText, token.PasswordOption);
			AssertNull ("Nonce", token.Nonce);

			token = new UsernameToken ("me", "mine", PasswordOption.SendHashed);
			AssertEquals ("SendHashed", PasswordOption.SendHashed, token.PasswordOption);
			AssertNull ("Nonce", token.Nonce); // strange - must be generated later...
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullUsername () 
		{
			UsernameToken token = new UsernameToken (null, "mine");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_EmptyUsername () 
		{
			UsernameToken token = new UsernameToken ("", "mine");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullPassword () 
		{
			UsernameToken token = new UsernameToken ("me", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_EmptyPassword () 
		{
			UsernameToken token = new UsernameToken ("me", "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_NullElement () 
		{
			UsernameToken token = new UsernameToken (null);
		}

		[Test]
		public void Created () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertEquals ("Created before GetXml", DateTime.MinValue, token.Created);

			XmlDocument doc = new XmlDocument ();
			XmlElement xel = token.GetXml (doc);
			Assert ("Created after GetXml", DateTime.MinValue < token.Created);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AuthenticationKey_BeforeGetXml () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertNotNull ("AuthenticationKey", token.AuthenticationKey);
		}

		[Test]
		[Ignore("this works on MS only when stepping in debugger")]
		public void AuthenticationKey_AfterGetXml () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = token.GetXml (doc);
			AssertNotNull ("AuthenticationKey", token.AuthenticationKey);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DecryptionKey () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertNotNull ("DecryptionKey", token.DecryptionKey);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void EncryptionKey () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertNotNull ("EncryptionKey", token.EncryptionKey);
		}

		[Test]
		public void SignatureKey () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertNotNull ("SignatureKey", token.SignatureKey);
			// TODO use signature key
		}

		[Test]
		public void Password () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertEquals ("mine", token.Password);
			AssertEquals ("SendNone(implicit)", PasswordOption.SendNone, token.PasswordOption);

			token = new UsernameToken ("me", "none", PasswordOption.SendNone);
			AssertEquals ("Password", "none", token.Password);
			AssertEquals ("SendNone(explicit)", PasswordOption.SendNone, token.PasswordOption);
		}

		[Test]
		public void Nonce () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			AssertNull ("Nonce before GetXml", token.Nonce);

			XmlDocument doc = new XmlDocument ();
			XmlElement xel = token.GetXml (doc);
			AssertNotNull ("Nonce after GetXml", token.Nonce);
		}

		[Test]
		public void Supports () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			Assert ("SupportsDataEncryption", !token.SupportsDataEncryption);
			Assert ("SupportsDigitalSignature", token.SupportsDigitalSignature);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetXmlNull () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			XmlElement xel = token.GetXml (null);
		}

		[Test]
		public void GetXml () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			XmlDocument doc = new XmlDocument ();
			string xml = token.GetXml (doc).OuterXml;
			Assert ("Id", xml.IndexOf (" wsu:Id=\"SecurityToken-") > 0);
			Assert ("Username", xml.IndexOf ("<wsse:Username>me</wsse:Username>") > 0);
			Assert ("Password", xml.IndexOf ("<wsse:Password>mine</wsse:Password>") < 0);
			Assert ("Nonce", xml.IndexOf ("<wsse:Nonce>") > 0);
			Assert ("Created", xml.IndexOf ("<wsu:Created>") > 0);

			token = new UsernameToken ("me", "mine", PasswordOption.SendPlainText);
			xml = token.GetXml (doc).OuterXml;
			Assert ("Id", xml.IndexOf (" wsu:Id=\"SecurityToken-") > 0);
			Assert ("Username", xml.IndexOf ("<wsse:Username>me</wsse:Username>") > 0);
			Assert ("Password", xml.IndexOf ("<wsse:Password Type=\"wsse:PasswordText\">mine</wsse:Password>") > 0);
			Assert ("Nonce", xml.IndexOf ("<wsse:Nonce>") > 0);
			Assert ("Created", xml.IndexOf ("<wsu:Created>") > 0);

			token = new UsernameToken ("me", "mine", PasswordOption.SendHashed);
			xml = token.GetXml (doc).OuterXml;
			Assert ("Id", xml.IndexOf (" wsu:Id=\"SecurityToken-") > 0);
			Assert ("Username", xml.IndexOf ("<wsse:Username>me</wsse:Username>") > 0);
			Assert ("Password", xml.IndexOf ("<wsse:Password Type=\"wsse:PasswordDigest\">") > 0);
			Assert ("Nonce", xml.IndexOf ("<wsse:Nonce>") > 0);
			Assert ("Created", xml.IndexOf ("<wsu:Created>") > 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadXmlNull () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			token.LoadXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LoadXml_BadElement () 
		{
			UsernameToken token = new UsernameToken ("me", "mine");
			XmlDocument doc = new XmlDocument ();
			// bad element (Timestamp case is invalid)
			doc.LoadXml ("<wsu:timeStamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" />");
			token.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
		public void LoadXml_WithoutPasswordProvider () 
		{
			string xml = "<wsse:UsernameToken xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"SecurityToken-a567950c-ceb7-4fb6-b78f-10aeb7078985\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Username>me</wsse:Username><wsse:Nonce>U98BosqSRZFZAH9Izw4k7Q==</wsse:Nonce><wsu:Created>2003-09-10T01:33:13Z</wsu:Created></wsse:UsernameToken>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			UsernameToken token = new UsernameToken (doc.DocumentElement);
		}


		[Test]
		[ExpectedException (typeof (ConfigurationException))]
//		[Ignore("requires setting up a PasswordProvider - strange because no password is sent !?!")]
		// sample taken from http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnglobspec/html/ws-security.asp
		public void LoadXml_PasswordNone () 
		{
			string xml = "<wsse:UsernameToken xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"SecurityToken-a567950c-ceb7-4fb6-b78f-10aeb7078985\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Username>me</wsse:Username><wsse:Nonce>U98BosqSRZFZAH9Izw4k7Q==</wsse:Nonce><wsu:Created>2003-09-10T01:33:13Z</wsu:Created></wsse:UsernameToken>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			UsernameToken token = new UsernameToken (doc.DocumentElement);
			XmlElement xel = token.GetXml (doc);
			// TODO - with a PasswordProvider
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
//		[Ignore("requires setting up a PasswordProvider")]
		// sample taken from http://msdn.microsoft.com/webservices/building/wse/default.aspx?pull=/library/en-us/dnwebsrv/html/wssecdrill.asp
		public void LoadXml_PasswordText () 
		{
			string xml = "<wsse:UsernameToken xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"SecurityToken-c7ef4231-4397-4b4b-ab8d-0ea3fad1f79e\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Username>domain_name\\joeblow</wsse:Username><wsse:Password Type=\"wsse:PasswordText\">NoTelinNE1</wsse:Password><wsse:Nonce>QLSVRt9g3e19jJXJYhtBKA==</wsse:Nonce><wsu:Created>2003-07-05T22:37:52Z</wsu:Created></wsse:UsernameToken>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			UsernameToken token = new UsernameToken (doc.DocumentElement);
			XmlElement xel = token.GetXml (doc);
			// TODO - with a PasswordProvider
		}

		[Test]
		[ExpectedException (typeof (ConfigurationException))]
//		[Ignore("requires setting up a PasswordProvider")]
		public void LoadXml_PasswordHashed () 
		{
			string xml = "<wsse:UsernameToken xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"SecurityToken-536806c1-ef07-4a46-9876-4cae214a7db7\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\"><wsse:Username>me</wsse:Username><wsse:Password Type=\"wsse:PasswordDigest\">mvFakbZuqOwWZ+ULU0CYy1YAYtM=</wsse:Password><wsse:Nonce>IHMGSS18kXhPqBqRZezDNg==</wsse:Nonce><wsu:Created>2003-09-13T17:51:54Z</wsu:Created></wsse:UsernameToken>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			UsernameToken token = new UsernameToken (doc.DocumentElement);
			XmlElement xel = token.GetXml (doc);
			// TODO - with a PasswordProvider
		}
	}
}