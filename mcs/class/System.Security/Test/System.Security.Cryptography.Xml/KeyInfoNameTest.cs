//
// KeyInfoNameTest.cs - NUnit Test Cases for KeyInfoName
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

public class KeyInfoNameTest : TestCase {

	public KeyInfoNameTest () : base ("System.Security.Cryptography.Xml.KeyInfoName testsuite") {}
	public KeyInfoNameTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (KeyInfoNameTest)); 
		}
	}

	public void TestNewKeyValue () 
	{
		string newKeyValue = "Mono::";
		KeyInfoName name1 = new KeyInfoName ();
		name1.Value = newKeyValue;
		XmlElement xel = name1.GetXml ();

		KeyInfoName name2 = new KeyInfoName ();
		name2.LoadXml (xel);

		AssertEquals ("name1==name2", (name1.GetXml ().OuterXml), (name2.GetXml ().OuterXml));
		AssertEquals ("newKeyValue==value", newKeyValue, name1.Value);
	}

	public void TestImportKeyValue () 
	{
		string value = "<KeyName xmlns=\"http://www.w3.org/2000/09/xmldsig#\">Mono::</KeyName>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		KeyInfoName name1 = new KeyInfoName ();
		name1.LoadXml (doc.DocumentElement);

		string s = (name1.GetXml ().OuterXml);
		AssertEquals ("Name", value, s);
	}

	public void TestInvalidValue () 
	{
		string bad = "<Test></Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (bad);

		KeyInfoName name1 = new KeyInfoName ();
		try {
			name1.LoadXml (null);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}
		name1.LoadXml (doc.DocumentElement);
		AssertEquals("invalid", "<KeyName xmlns=\"http://www.w3.org/2000/09/xmldsig#\"></KeyName>", (name1.GetXml ().OuterXml));
	}
}

}