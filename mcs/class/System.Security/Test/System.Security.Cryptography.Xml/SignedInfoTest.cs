//
// SignedInfoTest.cs - NUnit Test Cases for SignedInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class SignedInfoTest : Assertion {

		protected SignedInfo info;

		[SetUp]
		protected void SetUp () 
		{
			info = new SignedInfo ();
		}

		[Test]
		public void Empty () 
		{
			AssertEquals ("CanonicalizationMethod", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", info.CanonicalizationMethod);
			AssertNull ("Id", info.Id);
			AssertNotNull ("References", info.References);
			AssertEquals ("References.Count", 0, info.References.Count);
			AssertNull ("SignatureLength", info.SignatureLength);
			AssertNull ("SignatureMethod", info.SignatureMethod);
			AssertEquals ("ToString()", "System.Security.Cryptography.Xml.SignedInfo", info.ToString ());
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void EmptyException () 
		{
			string xml = info.GetXml ().OuterXml;
		}

		[Test]
		public void Properties () 
		{
			info.CanonicalizationMethod = "http://www.go-mono.com/";
			AssertEquals ("CanonicalizationMethod", "http://www.go-mono.com/", info.CanonicalizationMethod);
			info.Id = "Mono::";
			AssertEquals ("Id", "Mono::", info.Id);
		}

		[Test]
		public void References () 
		{
			Reference r1 = new Reference ();
			r1.Uri = "http://www.go-mono.com/";
			r1.AddTransform (new XmlDsigBase64Transform ());
			info.AddReference (r1);
			AssertEquals ("References.Count 1", 1, info.References.Count);

			Reference r2 = new Reference ("http://www.motus.com/");
			r2.AddTransform (new XmlDsigBase64Transform ());
			info.AddReference (r2);
			AssertEquals ("References.Count 2", 2, info.References.Count);

			info.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
		}

		[Test]
		public void Load () 
		{
			string xml = "<SignedInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\" /><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\" /><Reference URI=\"#MyObjectId\"><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\" /><DigestValue>/Vvq6sXEVbtZC8GwNtLQnGOy/VI=</DigestValue></Reference></SignedInfo>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			info.LoadXml (doc.DocumentElement);
			AssertEquals ("LoadXml", xml, (info.GetXml ().OuterXml));
			AssertEquals ("LoadXml-C14N", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", info.CanonicalizationMethod);
			AssertEquals ("LoadXml-Algo", "http://www.w3.org/2000/09/xmldsig#rsa-sha1", info.SignatureMethod);
			AssertEquals ("LoadXml-Ref1", 1, info.References.Count);
		}

		// there are many (documented) not supported methods in SignedInfo

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void NotSupportedCount () 
		{
			int n = info.Count;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void NotSupportedIsReadOnly () 
		{
			bool b = info.IsReadOnly;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void NotSupportedIsSynchronized () 
		{
			bool b = info.IsSynchronized;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void NotSupportedSyncRoot () 
		{
			object o = info.SyncRoot;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void NotSupportedCopyTo () 
		{
			info.CopyTo (null, 0);
		}
	}
}
