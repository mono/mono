//
// System.Xml.XmlDeclarationTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc.
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDeclarationTests
	{
		XmlDocument document;
		XmlDeclaration declaration;
		
		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<foo><bar></bar></foo>");
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
			Assert.AreEqual (String.Empty, declaration.InnerXml);
			Assert.AreEqual ("<?xml version=\"1.0\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "doesn't check", null);
			Assert.AreEqual (String.Empty, declaration.InnerXml);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"doesn't check\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", null, "yes");
			Assert.AreEqual (String.Empty, declaration.InnerXml);
			Assert.AreEqual ("<?xml version=\"1.0\" standalone=\"yes\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "foo", "no");
			Assert.AreEqual (String.Empty, declaration.InnerXml);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"foo\" standalone=\"no\"?>", declaration.OuterXml);
		}

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			Assert.IsNull (cloned.ParentNode);

			Assert.AreEqual (original.Value, cloned.Value, "Value incorrectly cloned");
			
                        Assert.IsTrue (!Object.ReferenceEquals (original, cloned), "Copies, not pointers");
		}

		[Test]
		public void Constructor ()
		{
			try {
				XmlDeclaration broken = document.CreateXmlDeclaration ("2.0", null, null);
			} catch (ArgumentException) {
				return;

			} catch (Exception e) {
				Assert.Fail ("first arg null, wrong exception: " + e.ToString());
			}
		}

		[Test]
		public void NodeType ()
		{
			Assert.AreEqual (XmlNodeType.XmlDeclaration, declaration.NodeType, "incorrect NodeType returned");
		}

		[Test]
		public void Names ()
		{
			Assert.AreEqual ("xml", declaration.Name, "Name is incorrect");
			Assert.AreEqual ("xml", declaration.LocalName, "LocalName is incorrect");
		}

		[Test]
		public void EncodingProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", "foo", null);
			Assert.AreEqual ("foo", d1.Encoding, "Encoding property");

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, null);
			Assert.AreEqual (String.Empty, d2.Encoding, "null Encoding property");
		}

		[Test]
		public void ValidInnerText ()
		{
			declaration.InnerText = "version='1.0'";

			declaration.InnerText = "version='1.0' encoding='euc-jp'";

			declaration.InnerText = "version='1.0'   standalone='no'";

			declaration.InnerText = "version='1.0' encoding='iso-8859-1' standalone=\"yes\"";

			declaration.InnerText = @"version = '1.0' encoding	=
				'euc-jp'  standalone	 =	'yes'    ";

			declaration.InnerText = "  version = '1.0'";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText ()
		{
			declaration.InnerText = "version='1.0a'";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText2 ()
		{
			declaration.InnerText = "version='1.0'  encoding='euc-kr\"";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText3 ()
		{
			declaration.InnerText = "version='2.0'";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText4 ()
		{
			declaration.InnerText = "version='1.0' standalone='Yeah!!!!!'";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText5 ()
		{
			declaration.InnerText = "version='1.0'standalone='yes'";
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void InvalidInnerText6 ()
		{
			declaration.InnerText = "version='1.0'standalone='yes' encoding='utf-8'";
		}

		[Test]
		public void StandaloneProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", null, "yes");
			Assert.AreEqual ("yes", d1.Standalone, "Yes standalone property");

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, "no");
			Assert.AreEqual ("no", d2.Standalone, "No standalone property");

			XmlDeclaration d3 = document.CreateXmlDeclaration ("1.0", null, null);
			Assert.AreEqual (String.Empty, d3.Standalone, "null Standalone property");
		}

		[Test]
		public void ValueProperty ()
		{
			string expected = "version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"" ;

			XmlDeclaration d = document.CreateXmlDeclaration ("1.0", "ISO-8859-1", "yes");
			Assert.AreEqual (expected, d.Value, "Value property");

			d.Value = expected;
			Assert.AreEqual (expected, d.Value, "Value round-trip");

			d.Value = "   " + expected;
			Assert.AreEqual (expected, d.Value, "Value round-trip (padded)");

			d.Value = "version=\"1.0\"     encoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assert.AreEqual (expected, d.Value, "Value round-trip (padded 2)");

			d.Value = "version=\"1.0\"\tencoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assert.AreEqual (expected, d.Value, "Value round-trip (\\t)");

			d.Value = "version=\"1.0\"\n    encoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assert.AreEqual (expected, d.Value, "Value round-trip (\\n)");

			d.Value = "version=\"1.0\"    encoding	=   \"ISO-8859-1\" standalone = \"yes\"" ;
			Assert.AreEqual (expected, d.Value, "Value round-trip (spaces)");

			d.Value = "version='1.0' encoding='ISO-8859-1' standalone='yes'" ;
			Assert.AreEqual (expected, d.Value, "Value round-trip ('s)");
		}

		[Test]
		public void Bug79496 ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.WriteStartDocument (true);
			xtw.WriteStartElement ("person");
			xtw.WriteEndElement ();
			xtw.Flush ();

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><person />", doc.OuterXml);
		}

		[Test]
		public void XmlCommentCloneNode ()
		{
			XmlNode original = declaration;

			XmlNode shallow = declaration.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
			
			XmlNode deep = declaration.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);

                        Assert.AreEqual (deep.OuterXml, shallow.OuterXml, "deep cloning differs from shallow cloning");
		}
	}
}
