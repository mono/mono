//
// SignedInfoTest.cs - NUnit Test Cases for SignedInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class SignedInfoTest : TestCase {

	public SignedInfoTest () : base ("System.Security.Cryptography.Xml.SignedInfo testsuite") {}
	public SignedInfoTest (string name) : base (name) {}

	protected SignedInfo info;

	protected override void SetUp () 
	{
		info = new SignedInfo ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (SignedInfoTest)); 
		}
	}

	public void TestEmpty () 
	{
		AssertEquals ("CanonicalizationMethod", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315", info.CanonicalizationMethod);
		// NON-WORKING ??? AssertEquals ("Id", "", info.Id);
		AssertNotNull ("References", info.References);
		AssertEquals ("References.Count", 0, info.References.Count);
		// NON-WORKING ??? AssertEquals ("SignatureLength", "", info.SignatureLength);
		// NON-WORKING ??? AssertEquals ("SignatureMethod", "", info.SignatureMethod);
		AssertEquals ("ToString()", "System.Security.Cryptography.Xml.SignedInfo", info.ToString ());

		try {
			string xml = info.GetXml().OuterXml;
			Fail ("Empty Xml: Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Empty Xml: Expected CryptographicException but got: " + e.ToString ());
		}
	}

	public void TestProperties () 
	{
		info.CanonicalizationMethod = "http://www.go-mono.com/";
		AssertEquals ("CanonicalizationMethod", "http://www.go-mono.com/", info.CanonicalizationMethod);
		info.Id = "Mono::";
		AssertEquals ("Id", "Mono::", info.Id);
	}

	public void TestReferences () 
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

	public void TestLoad () 
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
	public void TestNotSupported () 
	{
		try {
			int n = info.Count;
			Fail ("Count: Expected NotSupportedException but got none");
		}
		catch (NotSupportedException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("Count: Expected NotSupportedException but got: " + e.ToString ());
		}

		try {
			bool b = info.IsReadOnly;
			Fail ("IsReadOnly: Expected NotSupportedException but got none");
		}
		catch (NotSupportedException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("IsReadOnly: Expected NotSupportedException but got: " + e.ToString ());
		}

		try {
			bool b = info.IsSynchronized;
			Fail ("IsSynchronized: Expected NotSupportedException but got none");
		}
		catch (NotSupportedException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("IsSynchronized: Expected NotSupportedException but got: " + e.ToString ());
		}

		try {
			object o = info.SyncRoot;
			Fail ("SyncRoot: Expected NotSupportedException but got none");
		}
		catch (NotSupportedException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("SyncRoot: Expected NotSupportedException but got: " + e.ToString ());
		}

		try {
			info.CopyTo (null, 0);
			Fail ("CopyTo: Expected NotSupportedException but got none");
		}
		catch (NotSupportedException) {
			// this is expected
		}
		catch (Exception e) {
			Fail ("CopyTo: Expected NotSupportedException but got: " + e.ToString ());
		}
	}
}

}
