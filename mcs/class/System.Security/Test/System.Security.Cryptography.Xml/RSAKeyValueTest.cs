//
// RSAKeyValueTest.cs - NUnit Test Cases for RSAKeyValue
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

public class RSAKeyValueTest : TestCase {

	public RSAKeyValueTest () : base ("System.Security.Cryptography.Xml.RSAKeyValue testsuite") {}
	public RSAKeyValueTest (string name) : base (name) {}

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (RSAKeyValueTest)); 
		}
	}

	public void TestGeneratedKey () 
	{
		RSAKeyValue rsa1 = new RSAKeyValue ();
		AssertNotNull ("Key", rsa1.Key);
		XmlElement xmlkey = rsa1.GetXml ();

		RSAKeyValue rsa2 = new RSAKeyValue ();
		rsa2.LoadXml (xmlkey);

		Assert ("rsa1==rsa2", (rsa1.GetXml ().OuterXml) == (rsa2.GetXml ().OuterXml));

		RSA key = rsa1.Key;
		RSAKeyValue rsa3 = new RSAKeyValue (key);
		Assert ("rsa3==rsa1", (rsa3.GetXml ().OuterXml) == (rsa1.GetXml ().OuterXml));
		Assert ("rsa3==rsa2", (rsa3.GetXml ().OuterXml) == (rsa2.GetXml ().OuterXml));
	}

	public void TestImportKey () 
	{
		string rsaKey = "<KeyValue xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><RSAKeyValue><Modulus>ogZ1/O7iks9ncETqNxLDKoPvgrT4nFx1a3lOmpywEmgbc5+8vI5dSzReH4v0YrflY75rIJx13CYWMsaHfQ78GtXvaeshHlQ3lLTuSdYEJceKll/URlBoKQtOj5qYIVSFOIVGHv4Y/0lnLftOzIydem29KKH6lJQlJawBBssR12s=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (rsaKey);

		RSAKeyValue rsa1 = new RSAKeyValue ();
		rsa1.LoadXml (doc.DocumentElement);

		string s = (rsa1.GetXml ().OuterXml);
		AssertEquals ("RSA Key", rsaKey, s);
	}

	public void TestInvalidValue () 
	{
		string badKey = "<Test></Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (badKey);

		RSAKeyValue rsa1 = new RSAKeyValue ();

		try {
			rsa1.LoadXml (null);
			Fail ("Expected ArgumentNullException but got none");
		}
		catch (ArgumentNullException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected ArgumentNullException but got: " + e.ToString ());
		}

		try {
			rsa1.LoadXml (doc.DocumentElement);
			Fail ("Expected CryptographicException but got none");
		}
		catch (CryptographicException) {
			// this is what we expect
		}
		catch (Exception e) {
			Fail ("Expected CryptographicException but got: " + e.ToString ());
		}
	}
}

}