//
// BinarySecurityTokenTest.cs 
//	- NUnit Test Cases for BinarySecurityToken
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Security;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace MonoTests.MS.Web.Services.Security {

	// non-abstract BinarySecurityToken for test uses only
	public class BinarySecurityToken : Microsoft.Web.Services.Security.BinarySecurityToken {

		public BinarySecurityToken (XmlElement element) : base (element) {}

		public BinarySecurityToken (XmlQualifiedName valueType) : base (valueType) {}

		public override AuthenticationKey AuthenticationKey {
			get { return null; }
		}

		public override DecryptionKey DecryptionKey {
			get { return null; }
		}

		public override EncryptionKey EncryptionKey {
			get { return null; }
		}

		public override SignatureKey SignatureKey {
			get { return null; }
		}

		public override bool SupportsDataEncryption {
			get { return false; }
		}

		public override bool SupportsDigitalSignature {
			get { return false; }
		}

		public override void Verify() {}
	}

	[TestFixture]
	public class BinarySecurityTokenTest : Assertion {

		private static string name = "mono";
		private static string ns = "http://www.go-mono.com/";

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullXmlElement () 
		{
			// we do not want to confuse the compiler about null ;-)
			XmlElement xel = null;
			BinarySecurityToken bst = new BinarySecurityToken (xel);
		}

		[Test]
		public void ConstructorXmlElement () 
		{
			string xml = "<wsse:BinarySecurityToken xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" EncodingType=\"wsse:Base64Binary\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"SecurityToken-eb24c89d-012a-431e-af2d-db6a41f1b88e\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			BinarySecurityToken bst = new BinarySecurityToken (doc.DocumentElement);
			AssertNotNull ("BinarySecurityToken(XmlQualifiedName)", bst);
			AssertEquals ("EncodingType.Name", "Base64Binary", bst.EncodingType.Name);
			AssertEquals ("EncodingType.Namespace", "http://schemas.xmlsoap.org/ws/2002/07/secext", bst.EncodingType.Namespace);
			AssertEquals ("ValueType.Name", name, bst.ValueType.Name);
			AssertEquals ("ValueType.Namespace", ns, bst.ValueType.Namespace);
			AssertNull ("RawData", bst.RawData);
			Assert ("Id", bst.Id.StartsWith ("SecurityToken-"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullXmlQualifiedName () 
		{
			// we do not want to confuse the compiler about null ;-)
			XmlQualifiedName xqn = null;
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
		}

		[Test]
		public void ConstructorXmlQualifiedName () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			AssertNotNull ("BinarySecurityToken(XmlQualifiedName)", bst);
			AssertEquals ("EncodingType.Name", "Base64Binary", bst.EncodingType.Name);
			AssertEquals ("EncodingType.Namespace", "http://schemas.xmlsoap.org/ws/2002/07/secext", bst.EncodingType.Namespace);
			AssertEquals ("ValueType.Name", name, bst.ValueType.Name);
			AssertEquals ("ValueType.Namespace", ns, bst.ValueType.Namespace);
			AssertNull ("RawData", bst.RawData);
			Assert ("Id", bst.Id.StartsWith ("SecurityToken-"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullEncodingType () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			bst.EncodingType = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullValueType () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			bst.ValueType = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetXmlNull () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			XmlElement xel = bst.GetXml (null);
		}

		[Test]
		public void GetXml () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			bst.Id = "staticIdUsedForNUnit";
			XmlDocument doc = new XmlDocument ();
			AssertEquals ("GetXml(doc)", "<wsse:BinarySecurityToken xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" EncodingType=\"wsse:Base64Binary\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"staticIdUsedForNUnit\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />", bst.GetXml (doc).OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadXmlNull () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			bst.LoadXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadXml () 
		{
			XmlQualifiedName xqn = new XmlQualifiedName (name, ns);
			BinarySecurityToken bst = new BinarySecurityToken (xqn);
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<wsse:BinarySecurityToken xmlns:vt=\"http://www.go-mono.com/\" ValueType=\"vt:mono\" EncodingType=\"wsse:Base64Binary\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" wsu:Id=\"staticIdUsedForNUnit\" xmlns:wsse=\"http://schemas.xmlsoap.org/ws/2002/07/secext\" />");
			bst.LoadXml (doc.DocumentElement);
			AssertEquals ("EncodingType.Name", "Base64Binary", bst.EncodingType.Name);
			AssertEquals ("EncodingType.Namespace", "http://schemas.xmlsoap.org/ws/2002/07/secext", bst.EncodingType.Namespace);
			AssertEquals ("ValueType.Name", name, bst.ValueType.Name);
			AssertEquals ("ValueType.Namespace", ns, bst.ValueType.Namespace);
			AssertNull ("RawData", bst.RawData);
			AssertEquals ("Id", "staticIdUsedForNUnit", bst.Id);
		}
	}
}
