//
// KeyInfoNameTest.cs - NUnit Test Cases for KeyInfoName
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
	public class KeyInfoNameTest : Assertion {

		[Test]
		public void NewKeyValue () 
		{
			string newKeyValue = "Mono::";
			KeyInfoName name1 = new KeyInfoName ();
			name1.Value = newKeyValue;
			XmlElement xel = name1.GetXml ();

			KeyInfoName name2 = new KeyInfoName ();
			name2.LoadXml (xel);

			AssertEquals ("newKeyValue==value", newKeyValue, name1.Value);
			AssertEquals ("name1==name2", (name1.GetXml ().OuterXml), (name2.GetXml ().OuterXml));
		}

		[Test]
		public void ImportKeyValue () 
		{
			string value = "<KeyName xmlns=\"http://www.w3.org/2000/09/xmldsig#\">Mono::</KeyName>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);

			KeyInfoName name = new KeyInfoName ();
			name.LoadXml (doc.DocumentElement);
			AssertEquals ("import.Name", "Mono::", name.Value);
			AssertEquals ("import.GetXml", value, name.GetXml ().OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InvalidValue1 () 
		{
			string bad = "<Test></Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (bad);

			KeyInfoName name = new KeyInfoName ();
			name.LoadXml (null);
		}

		[Test]
		public void InvalidValue2 () 
		{
			string bad = "<Test></Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (bad);

			KeyInfoName name = new KeyInfoName ();
			name.LoadXml (doc.DocumentElement);
			AssertEquals ("invalid.Name", "", name.Value);
			AssertEquals ("invalid.GetXml", "<KeyName xmlns=\"http://www.w3.org/2000/09/xmldsig#\"></KeyName>", (name.GetXml ().OuterXml));
		}
	}
}
