//
// System.Xml.XmlTextWriterTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlTextWriterTests
	{
		StringWriter sw;
		XmlTextWriter xtw;

		[SetUp]
		public void GetReady ()
		{
			sw = new StringWriter ();
			CreateXmlTextWriter ();
		}

		private void CreateXmlTextWriter ()
		{
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
		}

		private string StringWriterText 
		{
			get { return sw.GetStringBuilder ().ToString (); }
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeBefore ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString("bar", "baz");
			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			Assert.AreEqual ("<foo bar='baz' xmlns:abc='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeAfter ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			xtw.WriteAttributeString("bar", "baz");
			Assert.AreEqual ("<foo xmlns:abc='http://abc.def' bar='baz'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", null, "http://abc.def");
			Assert.AreEqual ("xmlns='http://abc.def'", StringWriterText);
		}

		[Test]
		public void WriteAttributeString_XmlNs_Valid ()
		{
			xtw.WriteAttributeString ("xmlns", null, "http://abc.def");
			Assert.AreEqual ("xmlns='http://abc.def'", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("xmlns", "http://www.w3.org/2000/xmlns/", "http://abc.def");
			Assert.AreEqual ("xmlns='http://abc.def'", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString (null, "test", "http://www.w3.org/2000/xmlns/", "http://abc.def");
			Assert.AreEqual ("xmlns:test='http://abc.def'", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("", "test", "http://www.w3.org/2000/xmlns/", "http://abc.def");
			Assert.AreEqual ("xmlns:test='http://abc.def'", StringWriterText, "#4");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("person");
			xtw.WriteAttributeString ("", "test", "http://www.w3.org/2000/xmlns/", "http://abc.def");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<person xmlns:test='http://abc.def' />", StringWriterText, "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteAttributeString_XmlNs_Invalid1 ()
		{
			// The 'xmlns' attribute is bound to the reserved namespace 'http://www.w3.org/2000/xmlns/'
			xtw.WriteAttributeString ("xmlns", "http://somenamespace.com", "http://abc.def");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteAttributeString_XmlNs_Invalid2 ()
		{
			// The 'xmlns' attribute is bound to the reserved namespace 'http://www.w3.org/2000/xmlns/'
			xtw.WriteAttributeString (null, "xmlns", "http://somenamespace.com", "http://abc.def");
		}

		[Test]
		public void XmlSpace_Valid () // bug #77084
		{
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual ("xml:space='preserve'", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("xml", "space", "whatever", "default");
			Assert.AreEqual ("xml:space='default'", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("person");
			xtw.WriteAttributeString ("xml", "space", "whatever", "default");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<person xml:space='default' />", StringWriterText, "#3");
		}

		[Test]
		public void WriteAttributeString_XmlPrefix_Valid ()
		{
			xtw.WriteAttributeString ("xml", "something", "whatever", "default");
			Assert.AreEqual ("xml:something='default'", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("xml", "else", null, "whatever");
			Assert.AreEqual ("xml:else='whatever'", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("person");
			xtw.WriteAttributeString ("xml", "something", "whatever", "default");
			xtw.WriteAttributeString ("xml", "else", null, "whatever");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<person xml:something='default' xml:else='whatever' />", 
				StringWriterText, "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteAttributeString_XmlSpace_Invalid ()
		{
			// only preserve and default are valid values for xml:space
			xtw.WriteAttributeString ("xml", "space", null, "something");
		}

		[Test]
		public void AttributeNamespacesThreeParamWithTextInNamespaceParam ()
		{
			xtw.WriteAttributeString ("a", "http://somenamespace.com", "http://abc.def");
			Assert.AreEqual ("d0p1:a='http://abc.def'", StringWriterText, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeNamespacesWithNullInNamespaceParam ()
		{
			// you cannot use prefix with an empty namespace
			xtw.WriteAttributeString ("a", "abc", null, "http://abc.def");
		}

		[Test]
		public void AttributeNamespacesWithTextInNamespaceParam ()
		{
			xtw.WriteAttributeString ("a", "abc", "http://somenamespace.com", "http://abc.def");
			Assert.AreEqual ("a:abc='http://abc.def'", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("", "abc", "http://somenamespace.com", "http://abc.def");
			Assert.AreEqual ("d0p1:abc='http://abc.def'", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString (null, "abc", "http://somenamespace.com", "http://abc.def");
			Assert.AreEqual ("d0p1:abc='http://abc.def'", StringWriterText, "#3");
		}

		[Test]
		[Ignore ("Due to the (silly) dependency on bug #77088, this test will not be fixed. The test could be rewritten but it depends on the original test author.")]
		public void AutoCreatePrefixes ()
		{
			xtw.WriteStartElement ("root");
			xtw.WriteAttributeString (null, "abc", "http://somenamespace.com", "http://abc.def");
			xtw.WriteAttributeString (null, "def", "http://somenamespace.com", "http://def.ghi");
			xtw.WriteAttributeString (null, "ghi", "http://othernamespace.com", "http://ghi.jkl");
			xtw.WriteEndElement ();

			Assert.AreEqual ("<root d1p1:abc='http://abc.def' d1p1:def='http://def.ghi' d1p2:ghi='http://ghi.jkl' xmlns:d1p2='http://othernamespace.com' xmlns:d1p1='http://somenamespace.com' />", StringWriterText, "#1");
		}

		[Test]
		[Ignore ("Due to the (silly) dependency on bug #77088, this test will not be fixed. The test could be rewritten but it depends on the original test author.")]
		public void AutoCreatePrefixes2 ()
		{
			xtw.WriteStartElement ("person");
			xtw.WriteAttributeString (null, "name", "http://somenamespace.com", "Driesen");
			xtw.WriteAttributeString (null, "initials", "http://othernamespace.com", "GD");
			xtw.WriteAttributeString (null, "firstName", "http://somenamespace.com", "Gert");
			xtw.WriteStartElement ("address");
			xtw.WriteAttributeString (null, "street", "http://somenamespace.com", "Campus");
			xtw.WriteAttributeString (null, "number", "http://othernamespace.com", "1");
			xtw.WriteAttributeString (null, "zip", "http://newnamespace.com", "3000");
			xtw.WriteAttributeString (null, "box", "http://othernamespace.com", "a");
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();

			Assert.AreEqual (
				"<person" +
					" d1p1:name='Driesen'" +
					" d1p2:initials='GD'" +
					" d1p1:firstName='Gert'" +
					" xmlns:d1p2='http://othernamespace.com'" +
					" xmlns:d1p1='http://somenamespace.com'>" +
					"<address" +
						" d1p1:street='Campus'" +
						" d1p2:number='1'" +
						" d2p1:zip='3000'" +
						" d1p2:box='a'" +
						" xmlns:d2p1='http://newnamespace.com' />" +
				"</person>", StringWriterText, "#2");
		}

		[Test]
		public void AttributeNamespacesXmlnsXmlns ()
		{
			xtw.WriteStartElement ("foo");
			// If XmlTextWriter conforms to "Namespaces in XML"
			// when namespaceURI argument is null, then this
			// is not allowed (http://www.w3.org/TR/REC-xml-names/
			// Namespace Constraint: Prefix Declared), but seems
			// like XmlTextWriter just ignores XML namespace
			// constraints when namespaceURI argument is null.
			xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
			//Assert.Fail ("A prefix must not start with \"xml\".");
		}

		[Test]
		public void AttributeNamespacesXmlnsXmlns2 ()
		{
			// It is split from AttributeNamespacesXmlnsXmlns()
			// because depending on XmlWriter it is likely to cause
			// duplicate attribute error (XmlTextWriter is pretty
			// hacky, so it does not raise such errors).
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("", "xmlns", null, "http://abc.def");
		}

		[Test]
		public void WriteAttributeString_EmptyLocalName ()
		{
			xtw.WriteAttributeString ("", "something");
			Assert.AreEqual ("='something'", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("", "", "something");
			Assert.AreEqual ("='something'", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("", "http://somenamespace.com", "something");
			Assert.AreEqual ("d0p1:='something'", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("x", "", "http://somenamespace.com", "something");
			Assert.AreEqual ("x:='something'", StringWriterText, "#4");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString (null, "something");
			Assert.AreEqual ("='something'", StringWriterText, "#5");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString (null, "", "something");
			Assert.AreEqual ("='something'", StringWriterText, "#6");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString (null, "http://somenamespace.com", "something");
			Assert.AreEqual ("d0p1:='something'", StringWriterText, "#7");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteAttributeString ("x", null, "http://somenamespace.com", "something");
			Assert.AreEqual ("x:='something'", StringWriterText, "#8");
		}

		[Test]
		public void WriteStartAttribute_EmptyLocalName ()
		{
			xtw.WriteStartAttribute ("", "");
			Assert.AreEqual ("='", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("", "", "");
			Assert.AreEqual ("='", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("", "", "http://somenamespace.com");
			Assert.AreEqual ("d0p1:='", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("x", "", "http://somenamespace.com");
			Assert.AreEqual ("x:='", StringWriterText, "#4");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("", null);
			Assert.AreEqual ("='", StringWriterText, "#5");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("", null, "");
			Assert.AreEqual ("='", StringWriterText, "#6");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("", null, "http://somenamespace.com");
			Assert.AreEqual ("d0p1:='", StringWriterText, "#7");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartAttribute ("x", null, "http://somenamespace.com");
			Assert.AreEqual ("x:='", StringWriterText, "#8");
		}

		[Test]
		public void AttributeWriteAttributeString ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("foo", "bar");
			Assert.AreEqual ("<foo foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("bar", "");
			Assert.AreEqual ("<foo foo='bar' bar=''", StringWriterText);

			xtw.WriteAttributeString ("baz", null);
			Assert.AreEqual ("<foo foo='bar' bar='' baz=''", StringWriterText);

			xtw.WriteAttributeString ("hoge", "a\nb");
			Assert.AreEqual ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b'", StringWriterText);

			xtw.WriteAttributeString ("fuga", " a\t\r\nb\t");
			Assert.AreEqual ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b' fuga=' a\t&#xD;&#xA;b\t'", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AttributeWriteAttributeStringNotInsideOpenStartElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteString ("bar");
			
			xtw.WriteAttributeString ("baz", "quux");
		}

		[Test]
		public void AttributeWriteAttributeStringWithoutParentElement ()
		{
			xtw.WriteAttributeString ("foo", "bar");
			Assert.AreEqual ("foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("baz", "quux");
			Assert.AreEqual ("foo='bar' baz='quux'", StringWriterText);
		}

		[Test]
		public void WriteStartElement_EmptyLocalName ()
		{
			xtw.WriteStartElement ("", "");
			Assert.AreEqual ("<", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("", "", "");
			Assert.AreEqual ("<", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("", "", "http://somenamespace.com");
			Assert.AreEqual ("<", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("x", "", "http://somenamespace.com");
			Assert.AreEqual ("<x:", StringWriterText, "#4");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("", null);
			Assert.AreEqual ("<", StringWriterText, "#5");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("", null, "");
			Assert.AreEqual ("<", StringWriterText, "#6");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("", null, "http://somenamespace.com");
			Assert.AreEqual ("<", StringWriterText, "#7");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("x", null, "http://somenamespace.com");
			Assert.AreEqual ("<x:", StringWriterText, "#8");
		}

		[Test]
		public void WriteElementString_EmptyLocalName ()
		{
			xtw.WriteElementString ("", "");
			Assert.AreEqual ("< />", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString ("", "", "");
			Assert.AreEqual ("< />", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString ("", "http://somenamespace.com", "whatever");
			Assert.AreEqual ("< xmlns='http://somenamespace.com'>whatever</>", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString ("", "http://somenamespace.com", "");
			Assert.AreEqual ("< xmlns='http://somenamespace.com' />", StringWriterText, "#4");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString (null, null);
			Assert.AreEqual ("< />", StringWriterText, "#5");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString (null, null, null);
			Assert.AreEqual ("< />", StringWriterText, "#6");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString (null, "http://somenamespace.com", "whatever");
			Assert.AreEqual ("< xmlns='http://somenamespace.com'>whatever</>", StringWriterText, "#7");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteElementString (null, "http://somenamespace.com", null);
			Assert.AreEqual ("< xmlns='http://somenamespace.com' />", StringWriterText, "#8");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS.NET 1.1 does not allow zero-length namespace URI
#endif
		public void WriteStartElement_Prefix_EmptyNamespace ()
		{
			xtw.WriteStartElement ("x", "whatever", "");
			Assert.AreEqual ("<whatever", StringWriterText, "#1");

			xtw.WriteEndElement ();

			Assert.AreEqual ("<whatever />", StringWriterText, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElement_Prefix_NullNamespace ()
		{
			xtw.WriteStartElement ("x", "whatever", null);
		}

		[Test]
		public void WriteStartElement_XmlPrefix ()
		{
			xtw.WriteStartElement ("xml", "something", "http://www.w3.org/XML/1998/namespace");
			Assert.AreEqual ("<xml:something", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("XmL", null, "http://www.w3.org/XML/1998/namespace");
			Assert.AreEqual ("<XmL:", StringWriterText, "#2");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("xmlsomething", "name", "http://www.w3.org/XML/1998/namespace");
			Assert.AreEqual ("<xmlsomething:name", StringWriterText, "#3");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElement_XmlPrefix_Invalid1 ()
		{
			xtw.WriteStartElement ("xml", null, "http://somenamespace.com");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElement_XmlPrefix_Invalid2 ()
		{
			xtw.WriteStartElement ("XmL", null, "http://somenamespace.com");
		}

		[Test]
		public void WriteStartElement_XmlPrefix_Invalid3 ()
		{
			// from XML 1.0 (third edition) specification:
			//
			// [Definition: A Name is a token beginning with a letter or one of a
			// few punctuation characters, and continuing with letters, digits, 
			// hyphens, underscores, colons, or full stops, together known as name 
			// characters.] Names beginning with the string "xml", or with any string
			// which would match (('X'|'x') ('M'|'m') ('L'|'l')), are reserved for 
			// standardization in this or future versions of this specification.
			//
			// from the Namespaces in XML 1.0 specification:
			//
			// Prefixes beginning with the three-letter sequence x, m, l, in any case 
			// combination, are reserved for use by XML and XML-related specifications. 
			//
			// should this prefix then not be considered invalid ?
			//
			// both Mono and MS.NET 1.x/2.0 accept it though

			xtw.WriteStartElement ("xmlsomething", null, "http://somenamespace.com");
			Assert.AreEqual ("<xmlsomething:", StringWriterText, "#1");

			sw.GetStringBuilder ().Length = 0;
			CreateXmlTextWriter ();

			xtw.WriteStartElement ("XmLsomething", null, "http://somenamespace.com");
			Assert.AreEqual ("<XmLsomething:", StringWriterText, "#2");
		}

		[Test]
		public void CDataValid ()
		{
			xtw.WriteCData ("foo");
			Assert.AreEqual ("<![CDATA[foo]]>", StringWriterText, "WriteCData had incorrect output.");
		}

		[Test]
		public void CDataNull ()
		{
			xtw.WriteCData (null);
			Assert.AreEqual ("<![CDATA[]]>", StringWriterText, "WriteCData had incorrect output.");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CDataInvalid ()
		{
			xtw.WriteCData("foo]]>bar");
		}
		
		[Test]
		public void CloseOpenElements ()
		{
			xtw.WriteStartElement("foo");
			xtw.WriteStartElement("bar");
			xtw.WriteStartElement("baz");
			xtw.Close();
			Assert.AreEqual ("<foo><bar><baz /></bar></foo>", StringWriterText,
				"Close didn't write out end elements properly.");
		}

		[Test]
		public void CloseWriteAfter ()
		{
			xtw.WriteElementString ("foo", "bar");
			xtw.Close ();

			// WriteEndElement and WriteStartDocument aren't tested here because
			// they will always throw different exceptions besides 'The Writer is closed.'
			// and there are already tests for those exceptions.

			try {
				xtw.WriteCData ("foo");
				Assert.Fail ("WriteCData after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
				// Assert.AreEqual ("The Writer is closed.", e.Message, "Exception message is incorrect.");
			}

			try {
				xtw.WriteComment ("foo");
				Assert.Fail ("WriteComment after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Assert.AreEqual ("The Writer is closed.", e.Message, "Exception message is incorrect.");
			}

			try {
				xtw.WriteProcessingInstruction ("foo", "bar");
				Assert.Fail ("WriteProcessingInstruction after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Assert.AreEqual ("The Writer is closed.", e.Message, "Exception message is incorrect.");
			}

			try {
				xtw.WriteStartElement ("foo", "bar", "baz");
				Assert.Fail ("WriteStartElement after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Assert.AreEqual ("The Writer is closed.", e.Message, "Exception message is incorrect.");
			}

			try {
				xtw.WriteAttributeString ("foo", "bar");
				Assert.Fail ("WriteAttributeString after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteString ("foo");
				Assert.Fail ("WriteString after Close Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Assert.AreEqual ("The Writer is closed.", e.Message, "Exception message is incorrect.");
			}
		}

		[Test]
		public void CommentValid ()
		{
			xtw.WriteComment ("foo");
			Assert.AreEqual ("<!--foo-->", StringWriterText, "WriteComment had incorrect output.");
		}

		[Test]
		public void CommentInvalid ()
		{
			try {
				xtw.WriteComment("foo-");
				Assert.Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }

			try {
				xtw.WriteComment("foo-->bar");
				Assert.Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }
		}

		[Test]
		public void ConstructorsAndBaseStream ()
		{
			Assert.IsTrue (Object.ReferenceEquals (null, this.xtw.BaseStream), "BaseStream property returned wrong value.");

			MemoryStream ms;
			StreamReader sr;
			XmlTextWriter xtw;

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UnicodeEncoding ());
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.Unicode);
			string expectedXmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
			string actualXmlDeclaration = sr.ReadToEnd();
			Assert.AreEqual (expectedXmlDeclaration, actualXmlDeclaration);
			Assert.IsTrue (Object.ReferenceEquals (ms, xtw.BaseStream), "BaseStream property returned wrong value.");

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UnicodeEncoding ());
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.Unicode);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UTF8Encoding ());
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-8\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assert.AreEqual ("<?xml version=\"1.0\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assert.AreEqual ("<?xml version=\"1.0\" standalone=\"yes\"?>", sr.ReadToEnd ());
			Assert.IsTrue (Object.ReferenceEquals (ms, xtw.BaseStream), "BaseStream property returned wrong value.");
		}

		[Test]
		public void DocumentStart ()
		{
			xtw.WriteStartDocument ();
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?>", StringWriterText,
				"XmlDeclaration is incorrect.");

			try {
				xtw.WriteStartDocument ();
				Assert.Fail("Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
				// Assert.AreEqual ("WriteStartDocument should be the first call.", e.Message, "Exception message is incorrect.");
			}

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (true);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16' standalone='yes'?>", StringWriterText);

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (false);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16' standalone='no'?>", StringWriterText);
		}

		[Test]
		public void ElementAndAttributeSameXmlns ()
		{
			xtw.WriteStartElement ("ped", "foo", "urn:foo");
			xtw.WriteStartAttribute ("ped", "foo", "urn:foo");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<ped:foo ped:foo='' xmlns:ped='urn:foo' />", StringWriterText);
		}

		[Test]
		[Category ("NotDotNet")]
		public void ElementXmlnsNeedEscape ()
		{
			xtw.WriteStartElement ("test", "foo", "'");
			xtw.WriteEndElement ();
			// MS.NET output is : xmlns:test='''
			Assert.AreEqual ("<test:foo xmlns:test='&apos;' />", StringWriterText);
		}

		[Test]
		public void ElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<foo />", StringWriterText, "Incorrect output.");
		}

		[Test]
		public void ElementWriteElementString ()
		{
			xtw.WriteElementString ("foo", "bar");
			Assert.AreEqual ("<foo>bar</foo>", StringWriterText, "WriteElementString has incorrect output.");

			xtw.WriteElementString ("baz", "");
			Assert.AreEqual ("<foo>bar</foo><baz />", StringWriterText, "#2");

			xtw.WriteElementString ("quux", null);
			Assert.AreEqual ("<foo>bar</foo><baz /><quux />", StringWriterText, "#3");

			xtw.WriteElementString ("", "quuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</>", StringWriterText, "#4");

			xtw.WriteElementString (null, "quuuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", StringWriterText, "#5");
		}

		[Test]
		public void FormattingTest ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteElementString ("bar", "");
			xtw.Close ();
			Assert.AreEqual (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}  <bar />{0}</foo>", Environment.NewLine), StringWriterText);
		}

		[Test]
		public void FormattingInvalidXmlForFun ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.IndentChar = 'x';
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteStartElement ("bar");
			xtw.WriteElementString ("baz", "");
			xtw.Close ();
			Assert.AreEqual (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}xx<bar>{0}xxxx<baz />{0}xx</bar>{0}</foo>", Environment.NewLine), StringWriterText);
		}

		[Test]
		public void FormattingFromRemarks ()
		{
			// Remarks section of on-line help for XmlTextWriter.Formatting suggests this test.
			xtw.Formatting = Formatting.Indented; 
			xtw.WriteStartElement ("ol"); 
			xtw.WriteStartElement ("li"); 
			xtw.WriteString ("The big "); // This means "li" now has a mixed content model. 
			xtw.WriteElementString ("b", "E"); 
			xtw.WriteElementString ("i", "lephant"); 
			xtw.WriteString (" walks slowly."); 
			xtw.WriteEndElement (); 
			xtw.WriteEndElement ();
			Assert.AreEqual (String.Format ("<ol>{0}  <li>The big <b>E</b><i>lephant</i> walks slowly.</li>{0}</ol>", Environment.NewLine), StringWriterText);
		}

		[Test]
		public void LookupPrefix ()
		{
			xtw.WriteStartElement ("root");

			xtw.WriteStartElement ("one");
			xtw.WriteAttributeString ("xmlns", "foo", null, "http://abc.def");
			xtw.WriteAttributeString ("xmlns", "bar", null, "http://ghi.jkl");
			Assert.AreEqual ("foo", xtw.LookupPrefix ("http://abc.def"), "#1");
			Assert.AreEqual ("bar", xtw.LookupPrefix ("http://ghi.jkl"), "#2");
			xtw.WriteEndElement ();

			xtw.WriteStartElement ("two");
			xtw.WriteAttributeString ("xmlns", "baz", null, "http://mno.pqr");
			xtw.WriteString("quux");
			Assert.AreEqual ("baz", xtw.LookupPrefix ("http://mno.pqr"), "#3");
			Assert.IsNull (xtw.LookupPrefix ("http://abc.def"), "#4");
			Assert.IsNull (xtw.LookupPrefix ("http://ghi.jkl"), "#5");

			Assert.IsNull (xtw.LookupPrefix ("http://bogus"), "#6");
		}

		[Test]
		public void NamespacesAttributesPassingInNamespaces ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo");

			// These shouldn't throw any exceptions since they don't pass in
			// a namespace.
			xtw.WriteAttributeString ("bar", "baz");
			xtw.WriteAttributeString ("", "a", "", "b");
			xtw.WriteAttributeString (null, "c", "", "d");
			xtw.WriteAttributeString ("", "e", null, "f");
			xtw.WriteAttributeString (null, "g", null, "h");

			Assert.AreEqual ("<foo bar='baz' a='b' c='d' e='f' g='h'", StringWriterText);
		}

		[Test]
		public void NamespacesElementsPassingInNamespaces ()
		{
			xtw.Namespaces = false;

			// These shouldn't throw any exceptions since they don't pass in
			// a namespace.
			xtw.WriteElementString ("foo", "bar");
			xtw.WriteStartElement ("baz");
			xtw.WriteStartElement ("quux", "");
			xtw.WriteStartElement ("quuux", null);
			xtw.WriteStartElement (null, "a", null);
			xtw.WriteStartElement (null, "b", "");
			xtw.WriteStartElement ("", "c", null);
			xtw.WriteStartElement ("", "d", "");

			Assert.AreEqual ("<foo>bar</foo><baz><quux><quuux><a><b><c><d", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid1 ()
		{
			// These should throw ArgumentException because they pass in a
			// namespace when Namespaces = false.
			xtw.Namespaces = false;
			xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid2 ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo", "http://netsack.com/");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid3 ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid4 ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo", "bar", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid5 ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo", "bar", "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesElementsPassingInNamespacesInvalid6 ()
		{
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo", "", "");
		}

		[Test]
		public void NamespacesNoNamespaceClearsDefaultNamespace ()
		{
			xtw.WriteStartElement(String.Empty, "foo", "http://netsack.com/");
			xtw.WriteStartElement(String.Empty, "bar", String.Empty);
			xtw.WriteElementString("baz", String.Empty, String.Empty);
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			Assert.AreEqual ("<foo xmlns='http://netsack.com/'><bar xmlns=''><baz /></bar></foo>",
				StringWriterText, "XmlTextWriter is incorrectly outputting namespaces.");
		}

		[Test]
		public void NamespacesPrefix ()
		{
			xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
			xtw.WriteStartElement ("foo", "baz", "http://netsack.com/");
			xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			Assert.AreEqual ("<foo:bar xmlns:foo='http://netsack.com/'><foo:baz><foo:qux /></foo:baz></foo:bar>",
				StringWriterText, "XmlTextWriter is incorrectly outputting prefixes.");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // MS.NET 1.1 does not allow zero-length namespace URI
#endif
		public void NamespacesPrefixWithEmptyAndNullNamespaceEmpty ()
		{
			xtw.WriteStartElement ("foo", "bar", "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesPrefixWithEmptyAndNullNamespaceNull ()
		{
			xtw.WriteStartElement ("foo", "bar", null);
		}

		[Test]
		public void NamespacesSettingWhenWriteStateNotStart ()
		{
			xtw.WriteStartElement ("foo");
			try {
				xtw.Namespaces = false;
				Assert.Fail ("Expected an InvalidOperationException.");
			} catch (InvalidOperationException) {}
			Assert.IsTrue (xtw.Namespaces);
		}

		[Test]
		public void ProcessingInstructionValid ()
		{
			xtw.WriteProcessingInstruction("foo", "bar");
			Assert.AreEqual ("<?foo bar?>", StringWriterText, "WriteProcessingInstruction had incorrect output.");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid1 ()
		{
			xtw.WriteProcessingInstruction("fo?>o", "bar");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid2 ()
		{
			xtw.WriteProcessingInstruction("foo", "ba?>r");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid3 ()
		{
			xtw.WriteProcessingInstruction("", "bar");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid4 ()
		{
			xtw.WriteProcessingInstruction(null, "bar");
		}

		[Test]
		public void QuoteCharDoubleQuote ()
		{
			xtw.QuoteChar = '"';

			// version, encoding, standalone
			xtw.WriteStartDocument (true);
			
			// namespace declaration
			xtw.WriteElementString ("foo", "http://netsack.com", "bar");

			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><foo xmlns=\"http://netsack.com\">bar</foo>", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void QuoteCharInvalid ()
		{
			xtw.QuoteChar = 'x';
		}

		[Test]
		public void WriteBase64 ()
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] fooBar = encoding.GetBytes("foobar");
			xtw.WriteBase64 (fooBar, 0, 6);
			Assert.AreEqual ("Zm9vYmFy", StringWriterText);

			try {
				xtw.WriteBase64 (fooBar, 3, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteBase64 (fooBar, -1, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (fooBar, 3, -1);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (null, 0, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentNullException) {}
		}

		[Test]
		public void WriteBinHex ()
		{
			byte [] bytes = new byte [] {4,14,34, 54,94,114, 134,194,255, 0,5};
			xtw.WriteBinHex (bytes, 0, 11);
			Assert.AreEqual ("040E22365E7286C2FF0005", StringWriterText);
		}

		[Test]
		public void WriteCharEntity ()
		{
			xtw.WriteCharEntity ('a');
			Assert.AreEqual ("&#x61;", StringWriterText);

			xtw.WriteCharEntity ('A');
			Assert.AreEqual ("&#x61;&#x41;", StringWriterText);

			xtw.WriteCharEntity ('1');
			Assert.AreEqual ("&#x61;&#x41;&#x31;", StringWriterText);

			xtw.WriteCharEntity ('K');
			Assert.AreEqual ("&#x61;&#x41;&#x31;&#x4B;", StringWriterText);

			try {
				xtw.WriteCharEntity ((char)0xd800);
			} catch (ArgumentException) {}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteEndAttribute ()
		{
			xtw.WriteEndAttribute ();
		}

		[Test]
		public void WriteEndDocument ()
		{
			try {
				xtw.WriteEndDocument ();
				Assert.Fail ("Expected an Exception.");
			// in .NET 2.0 it is InvalidOperationException.
			// in .NET 1,1 it is ArgumentException.
			} catch (Exception) {}
		}

		[Test]
		public void WriteEndDocument2 ()
		{
			xtw.WriteStartDocument ();
			try 
			{
				xtw.WriteEndDocument ();
				Assert.Fail ("Expected an Exception.");
			// in .NET 2.0 it is InvalidOperationException.
			// in .NET 1,1 it is ArgumentException.
			} catch (Exception) {}
		}

		[Test]
		public void WriteEndDocument3 ()
		{
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='", StringWriterText);

			xtw.WriteEndDocument ();
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='' />", StringWriterText);
			Assert.AreEqual (WriteState.Start, xtw.WriteState);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteEndElement ()
		{
			// no matching StartElement
			xtw.WriteEndElement ();
		}

		[Test]
		public void WriteEndElement2 ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<foo />", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteStartAttribute ("baz", null);
			xtw.WriteEndElement ();
			Assert.AreEqual ("<foo /><bar baz='' />", StringWriterText);
		}

		[Test]
		public void FullEndElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo>", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("foo", "bar");
			xtw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo><bar foo='bar'></bar>", StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo><bar foo='bar'></bar><baz bar=''></baz>", StringWriterText);
		}

		[Test]
		public void WriteQualifiedName ()
		{
			xtw.WriteStartElement (null, "test", null);
			xtw.WriteAttributeString ("xmlns", "me", null, "http://localhost/");
			xtw.WriteQualifiedName ("bob", "http://localhost/");
			xtw.WriteEndElement ();

			Assert.AreEqual ("<test xmlns:me='http://localhost/'>me:bob</test>", StringWriterText);
		}

		[Test]
		public void WriteQualifiedNameNonDeclaredAttribute ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("a", "");
			xtw.WriteQualifiedName ("attr", "urn:a");
			xtw.WriteWhitespace (" ");
			xtw.WriteQualifiedName ("attr", "urn:b");
			xtw.WriteEndAttribute ();
			xtw.WriteEndElement ();
			string xml = sw.ToString ();
			Assert.IsTrue (xml.IndexOf ("<foo ") >= 0, "foo");
			Assert.IsTrue (xml.IndexOf ("a='d1p1:attr d1p2:attr'") > 0, "qnames");
			Assert.IsTrue (xml.IndexOf (" xmlns:d1p1='urn:a'") > 0, "xmlns:a");
			Assert.IsTrue (xml.IndexOf (" xmlns:d1p2='urn:b'") > 0, "xmlns:b");
		}

		[Test]
		public void WriteQualifiedNameNonNamespacedName ()
		{
			xtw.WriteStartElement ("root");
			xtw.WriteQualifiedName ("foo", "");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<root>foo</root>", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteQualifiedNameNonDeclaredContent ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteQualifiedName ("abc", "urn:abc");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteQualifiedNameNonNCName ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xmlns", "urn:default");
			xtw.WriteStartElement ("child");
			xtw.WriteStartAttribute ("a", "");
			xtw.WriteQualifiedName ("x:def", "urn:def");
		}

		[Test]
		public void WriteRaw ()
		{
			xtw.WriteRaw("&<>\"'");
			Assert.AreEqual ("&<>\"'", StringWriterText);

			xtw.WriteRaw(null);
			Assert.AreEqual ("&<>\"'", StringWriterText);

			xtw.WriteRaw("");
			Assert.AreEqual ("&<>\"'", StringWriterText);

			// bug #77623
			xtw.WriteRaw ("{0}{1}");
		}

		[Test]
		public void WriteRawInvalidInAttribute ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteRaw ("&<>\"'");
			xtw.WriteEndAttribute ();
			xtw.WriteEndElement ();
			Assert.AreEqual ("<foo bar='&<>\"'' />", StringWriterText);
		}

		[Test]
		public void WriteStateTest ()
		{
			Assert.AreEqual (WriteState.Start, xtw.WriteState);
			xtw.WriteStartDocument ();
			Assert.AreEqual (WriteState.Prolog, xtw.WriteState);
			xtw.WriteStartElement ("root");
			Assert.AreEqual (WriteState.Element, xtw.WriteState);
			xtw.WriteElementString ("foo", "bar");
			Assert.AreEqual (WriteState.Content, xtw.WriteState);
			xtw.Close ();
			Assert.AreEqual (WriteState.Closed, xtw.WriteState);
		}

		[Test]
		public void WriteString ()
		{
			xtw.WriteStartDocument ();
			try {
				xtw.WriteString("foo");
				Assert.Fail ("should raise an error.");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void WriteString2 ()
		{
			xtw.WriteStartDocument ();
			// Testing attribute values

			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "&<>");
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='&amp;&lt;&gt;'", StringWriterText);
		}

		[Test]
		public void WriteAttributeStringSingleQuoteChar()
		{
			// When QuoteChar is single quote then replaces single quotes within attributes
			// but not double quotes.
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			Assert.AreEqual ("<foo bar='\"baz\"' quux='&apos;baz&apos;'", StringWriterText);
		}

		[Test]
		public void WriteAttributeStringDoubleQuoteChar()
		{
			// When QuoteChar is double quote then replaces double quotes within attributes
			// but not single quotes.
			xtw.QuoteChar = '"';
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			Assert.AreEqual ("<foo bar=\"&quot;baz&quot;\" quux=\"'baz'\"", StringWriterText);
		}

		[Test]
		public void WriteStringWithEntities()
		{
			// Testing element values
			xtw.QuoteChar = '\'';
			xtw.WriteElementString ("foo", "&<>\"'");
			Assert.AreEqual ("<foo>&amp;&lt;&gt;\"'</foo>", StringWriterText);
		}

		[Test]
		public void XmlLang ()
		{
			Assert.IsNull (xtw.XmlLang);
			
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "lang", null, "langfoo");
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo'", StringWriterText);

			xtw.WriteAttributeString ("boo", "yah");
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'", StringWriterText);
			
			xtw.WriteElementString("bar", "baz");
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>", StringWriterText);
			
			xtw.WriteString("baz");
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz", StringWriterText);
			
			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "lang", null);
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteString("langbar");
			// Commented out there: it is implementation-dependent.
			// and incompatible between .NET 1.0 and 1.1
			// Assert.AreEqual ("langfoo", xtw.XmlLang);
			// Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			// Commented out there: it is implementation-dependent.
			// and incompatible between .NET 1.0 and 1.1
			// Assert.AreEqual ("langbar", xtw.XmlLang);
			// Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'", StringWriterText);

			// check if xml:lang repeats output even if same as current scope.
			xtw.WriteStartElement ("joe");
			xtw.WriteAttributeString ("xml", "lang", null, "langbar");
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'", StringWriterText);

			
			xtw.WriteElementString ("quuux", "squonk");
			Assert.AreEqual ("langbar", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux>", StringWriterText);

			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			Assert.AreEqual ("langfoo", xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux>", StringWriterText);
			
			xtw.WriteEndElement ();
			Assert.IsNull (xtw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux></foo>", StringWriterText);
			
			xtw.Close ();
			Assert.IsNull (xtw.XmlLang);
		}

		// TODO: test operational aspects
		[Test]
		public void XmlSpaceTest ()
		{
			xtw.WriteStartElement ("foo");
			Assert.AreEqual (XmlSpace.None, xtw.XmlSpace, "#1");

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace, "#2");
			Assert.AreEqual ("<foo><bar xml:space='preserve'", StringWriterText, "#3");

			xtw.WriteStartElement ("baz");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace, "#4");
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'", StringWriterText, "#5");

			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "space", null);
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace, "#6");
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText, "#7");

			// Commented out there: it is implementation-dependent
			// and incompatible between .NET 1.0 and 1.1
			xtw.WriteString ("default");
			// Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			// Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			Assert.AreEqual (XmlSpace.Default, xtw.XmlSpace, "#8");
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='default'", StringWriterText, "#9");

			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace, "#10");
			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace, "#11");
			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.None, xtw.XmlSpace, "#12");

			xtw.WriteStartElement ("quux");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue1 ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "space", null, "bubba");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue2 ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "space", null, "PRESERVE");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue3 ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "space", null, "Default");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue4 ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "space", null, "bubba");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteWhitespaceNonWhitespace ()
		{
			xtw.WriteWhitespace ("x");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteWhitespace_Null ()
		{
			xtw.WriteWhitespace ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteWhitespace_Empty ()
		{
			xtw.WriteWhitespace (string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteNmToken_Null ()
		{
			xtw.WriteNmToken ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteNmToken_Empty ()
		{
			xtw.WriteNmToken (string.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteNmToken_InvalidChars ()
		{
			xtw.WriteNmToken ("\uFFFF");
 		}

		[Test]
		public void WriteNmToken ()
		{
			xtw.WriteNmToken ("some:name");
			Assert.AreEqual ("some:name", StringWriterText);
		}

		[Test]
		public void XmlSpaceRaw ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("xml", "space", null);
			Assert.AreEqual (XmlSpace.None, xtw.XmlSpace);
			Assert.AreEqual ("<foo xml:space='", StringWriterText);

			xtw.WriteString ("default");
			// Commented out there: it is implementation-dependent
			// and incompatible between .NET 1.0 and 1.1
			// Assert.AreEqual (XmlSpace.None, xtw.XmlSpace);
			// Assert.AreEqual ("<foo xml:space='", StringWriterText);

			xtw.WriteEndAttribute ();
			Assert.AreEqual (XmlSpace.Default, xtw.XmlSpace);
			Assert.AreEqual ("<foo xml:space='default'", StringWriterText);
		}

		[Test]
		public void WriteAttributes ()
		{
			XmlDocument doc = new XmlDocument();
			StringWriter sw = new StringWriter();
			XmlWriter wr = new XmlTextWriter(sw);
			StringBuilder sb = sw.GetStringBuilder();
			XmlParserContext ctx = new XmlParserContext(doc.NameTable, new XmlNamespaceManager(doc.NameTable), "", XmlSpace.Default);
			XmlTextReader xtr = new XmlTextReader("<?xml version='1.0' encoding='utf-8' standalone='no'?><root a1='A' b2='B' c3='C'><foo><bar /></foo></root>", XmlNodeType.Document, ctx);

			xtr.Read();	// read XMLDecl
			wr.WriteAttributes(xtr, false);
			// This method don't always have to take this double-quoted style...
			Assert.AreEqual ("version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"", sw.ToString ().Trim (),
				"#WriteAttributes.XmlDecl.1");

			sb.Remove(0, sb.Length);	// init
			ctx = new XmlParserContext(doc.NameTable, new XmlNamespaceManager(doc.NameTable), "", XmlSpace.Default);
			xtr = new XmlTextReader("<?xml version='1.0'		 standalone='no'?><root a1='A' b2='B' c3='C'><foo><bar /></foo></root>", XmlNodeType.Document, ctx);
			xtr.Read();	// read XMLDecl
			Assert.AreEqual (XmlNodeType.XmlDeclaration, xtr.NodeType);
			sw = new StringWriter ();
			wr = new XmlTextWriter (sw);

			// This block raises an error on MS.NET 1.0.
			wr.WriteAttributes(xtr, false);
			// This method don't always have to take this double-quoted style...
			Assert.AreEqual ("version=\"1.0\" standalone=\"no\"", sw.ToString ().Trim (),
				"#WriteAttributes.XmlDecl.2");

			sw = new StringWriter ();
			wr = new XmlTextWriter (sw);
			sb.Remove(0, sb.Length);	// init

			xtr.Read();	// read root
			Assert.AreEqual (XmlNodeType.Element, xtr.NodeType);
			wr.WriteStartElement(xtr.LocalName, xtr.NamespaceURI);
			wr.WriteAttributes(xtr, false);
			wr.WriteEndElement();
			wr.Close();
			// This method don't always have to take this double-quoted style...
			Assert.AreEqual ("<root a1=\"A\" b2=\"B\" c3=\"C\" />", sw.ToString ().Trim (),
				"#WriteAttributes.Element");
			xtr.Close ();
		}

		[Test]
		public void WriteWhitespace ()
		{
			xtw.WriteStartElement ("a");
			xtw.WriteWhitespace ("\n\t");
			xtw.WriteStartElement ("b");
			xtw.WriteWhitespace ("\n\t");
			xtw.WriteEndElement ();
			xtw.WriteWhitespace ("\n");
			xtw.WriteEndElement ();
			xtw.WriteWhitespace ("\n");
			xtw.Flush ();
			Assert.AreEqual ("<a>\n\t<b>\n\t</b>\n</a>\n", StringWriterText);
		}

		[Test]
		public void FlushDoesntCloseTag ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "baz");
			xtw.Flush ();
			Assert.AreEqual ("<foo bar='baz'", StringWriterText);
		}

		[Test]
		public void WriteWhitespaceClosesTag ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "baz");
			xtw.WriteWhitespace (" ");
			Assert.AreEqual ("<foo bar='baz'> ", StringWriterText);
		}

		[Test]
		public void DontOutputMultipleXmlns ()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<a xmlns:dt=\"b\" dt:dt=\"c\"/>");
			XmlDocument doc2 = new XmlDocument();
			doc2.LoadXml(doc.InnerXml);
			Assert.AreEqual ("<a xmlns:dt=\"b\" dt:dt=\"c\" />",
				doc2.OuterXml);
		}

		[Test]
		public void DontOutputNonDeclaredXmlns ()
		{
			string xml = "<x:a foo='foo' xmlns:x='urn:foo'><b /></x:a>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XmlDocument doc2 = new XmlDocument();
			doc2.LoadXml(doc.InnerXml);
			Assert.AreEqual (xml.Replace ('\'', '"'), doc2.OuterXml);
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration ()
		{
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xmlns", "probe");
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("probe"), "#1");
			xtw.WriteStartElement ("b");
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("probe"), "#2");
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 xmlns='' /></b></foo>", StringWriterText, "#3");
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration2 ()
		{
			xtw.WriteStartDocument ();
			// IMPORTANT DIFFERENCE!! ns = "", not null
			xtw.WriteStartElement ("foo", "");
			xtw.WriteAttributeString ("xmlns", "probe");
			Assert.IsNull (xtw.LookupPrefix ("probe"), "#1");
			xtw.WriteStartElement ("b");
			Assert.IsNull (xtw.LookupPrefix ("probe"), "#2");
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 /></b></foo>", StringWriterText, "#3");
		}

		[Test]
		public void DoOutputRemovalDefaultNSDeclaration ()
		{
			xtw.WriteStartElement ("docelem", "a-namespace");
			
			XmlDocument doc = new XmlDocument ();
			doc.CreateElement ("hola").WriteTo (xtw);
			// This means, WriteTo never passes null NamespaceURI argument to XmlWriter.
			xtw.WriteEndElement ();
			xtw.Close ();

			Assert.AreEqual ("<docelem xmlns='a-namespace'><hola xmlns='' /></docelem>", StringWriterText);
		}

		[Test]
		public void WriteAttributeTakePrecedenceOnXmlns ()
		{
			xtw.WriteStartElement ("root", "urn:foo");
			xtw.WriteAttributeString ("xmlns", "urn:bar");
			xtw.WriteEndElement ();
			xtw.Close ();
			Assert.AreEqual ("<root xmlns='urn:bar' />", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LookupPrefixNull ()
		{
			xtw.LookupPrefix (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LookupPrefixEmpty ()
		{
			xtw.LookupPrefix (String.Empty);
		}

		[Test]
		public void LookupPrefixIgnoresXmlnsAttribute ()
		{
			Assert.IsNull (xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("root");
			Assert.IsNull (xtw.LookupPrefix ("urn:foo"));
			xtw.WriteAttributeString ("xmlns", "urn:foo");
			// Surprisingly to say, it is ignored!!
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("hoge");
			// (still after flushing previous start element.)
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("fuga", "urn:foo");
			// Is this testing on the correct way? Yes, here it is.
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("urn:foo"));
		}

		[Test]
		public void WriteInvalidNames ()
		{
			xtw.WriteStartElement ("foo<>");
			xtw.WriteAttributeString ("ho<>ge", "value");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributePrefixWithoutNS ()
		{
			xtw.WriteStartAttribute ("some", "foo", null);
		}

		[Test]
		public void AttributeWriteStartAttributeXmlnsNullNS ()
		{
			xtw.WriteStartAttribute ("xmlns", "foo", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteEndAttributeXmlnsNullNs ()
		{
			// This test checks if the specified namespace URI is
			// incorrectly empty or not. Compare it with
			// AttributeWriteStartAttributeXmlnsNullNS().
			xtw.WriteStartAttribute ("xmlns", "foo", null);
			xtw.WriteEndAttribute ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributePrefixXmlnsNonW3CNS ()
		{
			xtw.WriteStartAttribute ("xmlns", "foo", "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributeLocalXmlnsNonW3CNS ()
		{
			xtw.WriteStartAttribute ("", "xmlns", "urn:foo");
		}

		[Test]
		public void WriteRawProceedToProlog ()
		{
			XmlTextWriter xtw = new XmlTextWriter (new StringWriter ());
			xtw.WriteRaw ("");
			Assert.AreEqual (WriteState.Prolog, xtw.WriteState);
		}

		[Test]
		public void Indent ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root><test>test<foo></foo>string</test><test>string</test></root>");
			StringWriter sw = new StringWriter ();
			sw.NewLine = "_";
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			doc.WriteContentTo (xtw);
			Assert.AreEqual (@"<root>_  <test>test<foo></foo>string</test>_  <test>string</test>_</root>", sw.ToString ());
		}

		[Test]
		public void Indent2 ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.Formatting = Formatting.Indented;
			// sadly, this silly usage of this method is actually
			// used in WriteNode() in MS.NET.
			xtw.WriteProcessingInstruction ("xml",
				"version=\"1.0\"");
			xtw.WriteComment ("sample XML fragment");
			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				@"<?xml version=""1.0""?>{0}" +
				"<!--sample XML fragment-->", Environment.NewLine),
				sw.ToString ());
		}

		[Test]
		public void Indent3 ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			string s;

			doc.LoadXml ("<root><element></element><!-- comment indented --><element>sample <!-- comment non-indented --></element></root>");
			s = GetIndentedOutput (doc.DocumentElement);
			Assert.AreEqual (s, String.Format ("<root>{0}  <element>{0}  </element>{0}  <!-- comment indented -->{0}  <element>sample <!-- comment non-indented --></element>{0}</root>", "\n"), "#1");

			doc.LoadXml ("<root> \n<mid> \n<mid>   \n<child attr='value'>sample <nested attr='value' /> string</child>     <child2 attr='value'>sample string</child2>  <empty attr='value'/>\n<a>test</a> \n</mid> <returnValue>  <returnType>System.String</returnType>  </returnValue>  </mid>   </root>");
			s = GetIndentedOutput (doc.DocumentElement);
			Assert.AreEqual (s, String.Format ("<root> {0}<mid> {0}<mid>   {0}<child attr='value'>sample <nested attr='value' /> string</child>     <child2 attr='value'>sample string</child2>  <empty attr='value' />{0}<a>test</a> {0}</mid> <returnValue>  <returnType>System.String</returnType>  </returnValue>  </mid>   </root>", "\n"), "#2");

			doc.LoadXml ("<!-- after /MemberType and after /returnValue --><root><MemberType>blah</MemberType>\n  <returnValue><returnType>System.String</returnType></returnValue>\n  <Docs><summary>text</summary><value>text<see cref='ttt' /></value><remarks/></Docs></root>");
			s = GetIndentedOutput (doc.DocumentElement);
			Assert.AreEqual (s, String.Format ("<root>{0}  <MemberType>blah</MemberType>{0}  <returnValue><returnType>System.String</returnType></returnValue>{0}  <Docs><summary>text</summary><value>text<see cref='ttt' /></value><remarks /></Docs></root>", "\n"), "#3");
		}

		string GetIndentedOutput (XmlNode n)
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.Indented;
			n.WriteTo (xtw);
			return sw.ToString ();
		}

		[Test]
		public void CloseTwice ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter writer = new XmlTextWriter (sw);
			writer.Close ();
			// should not result in an exception
			writer.Close ();
		}

		[Test]
		public void WriteRawWriteString ()
		{
			// WriteRaw () -> WriteString ().
			xtw.WriteRaw ("");
			xtw.WriteString ("foo");
			Assert.AreEqual (WriteState.Content, xtw.WriteState);
		}

		[Test]
		public void LookupOverridenPrefix ()
		{
			xtw.WriteStartElement ("out");
			xtw.WriteAttributeString ("xmlns", "baz", "http://www.w3.org/2000/xmlns/", "xyz");
			xtw.WriteStartElement ("baz", "foo", "abc");
			Assert.IsNull (xtw.LookupPrefix ("xyz"));
		}

		[Test]
		public void DuplicatingNamespaceMappingInAttributes ()
		{
			xtw.WriteStartElement ("out");
			xtw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xtw.WriteAttributeString ("p", "bar", "urn:bar", "xyz");
			xtw.WriteAttributeString ("p", "baz", "urn:baz", "xyz");
			xtw.WriteStartElement ("out");
			xtw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xtw.WriteStartElement ("out");
			xtw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			string xml = sw.ToString ();
			Assert.IsTrue (xml.IndexOf ("p:foo='xyz'") > 0, "p:foo" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("d1p1:bar='xyz'") > 0, "d1p1:bar" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("d1p2:baz='xyz'") > 0, "d1p1:baz" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("xmlns:d1p2='urn:baz'") > 0, "xmlns:d1p2" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("xmlns:d1p1='urn:bar'") > 0, "xmlns:d1p1" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("xmlns:p='urn:foo'") > 0, "xmlns:p" + ". output is " + xml);
			Assert.IsTrue (xml.IndexOf ("<out p:foo='xyz'><out p:foo='xyz' /></out></out>") > 0, "remaining" + ". output is " + xml);
		}

		[Test]
		public void WriteXmlSpaceIgnoresNS ()
		{
			xtw.WriteStartElement ("root");
			xtw.WriteAttributeString ("xml", "space", "abc", "preserve");
			xtw.WriteEndElement ();
			Assert.AreEqual ("<root xml:space='preserve' />", sw.ToString ());
		}

		[Test] // bug #75546
		public void WriteEmptyNSQNameInAttribute ()
		{
			XmlTextWriter xtw = new XmlTextWriter (TextWriter.Null);
			xtw.WriteStartElement ("foo", "urn:goo");
			xtw.WriteAttributeString ("xmlns:bar", "urn:bar");
			xtw.WriteStartAttribute ("foo", "");
			xtw.WriteQualifiedName ("n1", "urn:bar");
			xtw.WriteEndAttribute ();
			xtw.WriteStartAttribute ("foo", "");
			xtw.WriteQualifiedName ("n2", "");
			xtw.WriteEndAttribute ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		// cannot bind any prefix to "http://www.w3.org/2000/xmlns/".
		public void WriteQualifiedNameXmlnsError ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteQualifiedName ("", "http://www.w3.org/2000/xmlns/");
		}

		[Test]
		public void WriteDocType ()
		{
			// we have the following test matrix:
			//
			// | name | publicid | systemid | subset|
			// |------------------------------------|
			// |  X   |    X     |    X     |   X   | #01
			// |  X   |    E     |    X     |   X   | #02
			// |  X   |    X     |    E     |   X   | #03
			// |  X   |    X     |    X     |   E   | #04
			// |  X   |    E     |    E     |   X   | #05
			// |  X   |    X     |    E     |   E   | #06
			// |  X   |    E     |    X     |   E   | #07
			// |  X   |    E     |    E     |   E   | #08
			// |  X   |    N     |    X     |   X   | #09
			// |  X   |    X     |    N     |   X   | #10
			// |  X   |    X     |    X     |   N   | #11
			// |  X   |    N     |    N     |   X   | #12
			// |  X   |    X     |    N     |   N   | #13
			// |  X   |    N     |    X     |   N   | #14
			// |  X   |    N     |    N     |   N   | #15
			//
			// Legend:
			// -------
			// X = Has value
			// E = Zero-length string
			// N = Null

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "sub");
			Assert.AreEqual ("<!DOCTYPE test PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN'" +
				" 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'[sub]>",
				sw.ToString (), "#01");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", string.Empty,
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "sub");
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"\"" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"[sub]>",
				sw.ToString (), "#02");

			sw.GetStringBuilder ().Length = 0; 
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				string.Empty, "sub");
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"\"[sub]>",
				sw.ToString (), "#03");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", string.Empty);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"[]>",
				sw.ToString (), "#04");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", string.Empty, string.Empty, "sub");
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"\" \"\"[sub]>",
				sw.ToString (), "#05");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				string.Empty, string.Empty);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"\"[]>",
				sw.ToString (), "#06");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", string.Empty,
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", string.Empty);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"\"" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"[]>",
				sw.ToString (), "#07");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", string.Empty, string.Empty, string.Empty);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"\" \"\"[]>",
				sw.ToString (), "#08");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", (string) null,
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "sub");
			Assert.AreEqual ("<!DOCTYPE test SYSTEM" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"[sub]>",
				sw.ToString (), "#09");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				(string) null, "sub");
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"\"[sub]>",
				sw.ToString (), "#10");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", (string) null);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">",
				sw.ToString (), "#11");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", (string) null, (string) null, "sub");
			Assert.AreEqual ("<!DOCTYPE test[sub]>",
				sw.ToString (), "#12");

			sw.GetStringBuilder ().Length = 0; 
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", "-//W3C//DTD XHTML 1.0 Strict//EN",
				(string) null, (string) null);
			Assert.AreEqual ("<!DOCTYPE test PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"" +
				" \"\">",
				sw.ToString (), "#13");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", (string) null,
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", (string) null);
			Assert.AreEqual ("<!DOCTYPE test SYSTEM" +
				" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">",
				sw.ToString (), "#14");

			sw.GetStringBuilder ().Length = 0;
			xtw = new XmlTextWriter (sw);

			xtw.WriteDocType ("test", (string) null, (string) null, (string) null);
			Assert.AreEqual ("<!DOCTYPE test>",
				sw.ToString (), "#15");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteDocType_EmptyName ()
		{
			xtw.WriteDocType (string.Empty, "-//W3C//DTD XHTML 1.0 Strict//EN",
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "sub");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteDocType_NullName ()
		{
			xtw.WriteDocType ((string) null, "-//W3C//DTD XHTML 1.0 Strict//EN",
				"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "sub");
		}

		[Test] // bug #76095
		public void SurrogatePairsInWriteString ()
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriter writer = new XmlTextWriter(ms, null);
			writer.WriteElementString("a", "\ud800\udf39");
			writer.Close();
			byte [] referent = new byte [] {0x3c, 0x61, 0x3e, 0xf0,
				0x90, 0x8c, 0xb9, 0x3c, 0x2f, 0x61, 0x3e};
			NUnit.Framework.Assert.AreEqual (referent, ms.ToArray ());
		}

		[Test]
		public void InvalidCharIsWrittenAsSillyReferences ()
		{
			// I can't say how MS XmlTextWriter is silly. 
			// The expected output is *not* well-formed XML.
			// Everyone have to make sure that he or she does 
			// not write invalid characters directly so that
			// the output XML string can be fed by other XML
			// processors.

			// The funny thing is that XmlTextWriter spends
			// significant performance on checking invalid
			// characters, but results in nothing.
			xtw.WriteElementString ("a", "\x0");
			NUnit.Framework.Assert.AreEqual ("<a>&#x0;</a>",
				StringWriterText);
		}

		[Test] // see also bug #77082
		public void WriteDocTypeIndent ()
		{
			string expected = String.Format (@"<?xml version='1.0'?>{0}<!DOCTYPE root PUBLIC '' 'urn:foo'[]>{0}<root />", Environment.NewLine);
			xtw.Formatting = Formatting.Indented;
			xtw.WriteProcessingInstruction ("xml", "version='1.0'");
			xtw.WriteDocType ("root", "", "urn:foo", "");
			xtw.WriteStartElement ("root");
			xtw.WriteEndElement ();
			xtw.Close ();
			Assert.AreEqual (expected, StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteDocTypeTwice ()
		{
			xtw.WriteDocType ("root", "", "urn:foo", "");
			xtw.WriteDocType ("root", "", "urn:foo", "");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlDeclAfterDocType ()
		{
			xtw.WriteDocType ("root", "", "urn:foo", "");
			xtw.WriteStartDocument ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlDeclAfterWhitespace ()
		{
			xtw.WriteWhitespace ("   ");
			xtw.WriteStartDocument ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void XmlDeclAfterPI ()
		{
			xtw.WriteProcessingInstruction ("pi", "");
			xtw.WriteStartDocument ();
		}

		[Test]
		public void WriteRawEmptyCloseStartTag ()
		{
			xtw.WriteStartElement ("stream", "stream","http://etherx.jabber.org/streams");
			xtw.WriteAttributeString ("version", "1.0");
			xtw.WriteAttributeString ("to", "me@test.com");
			xtw.WriteAttributeString ("from", "server");
			xtw.WriteAttributeString ("xmlns", "jabber:client");
			xtw.WriteRaw ("");// Ensure that the tag is closed
			xtw.Flush ();

			Assert.AreEqual ("<stream:stream version='1.0' to='me@test.com' from='server' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>", StringWriterText);
		}

		[Test] // bug #78148
		public void UpdateFormattingOnTheFly ()
		{
			XmlTextWriter w = new XmlTextWriter (TextWriter.Null);
			w.WriteStartElement ("test");
			w.Formatting = Formatting.Indented;
		}

		[Test] // bug #78598
		public void WriteGlobalAttributeInDefaultNS ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			string ns = "http://schemas.xmlsoap.org/soap/envelope/";
			tw.WriteStartElement ("Envelope");
			tw.WriteAttributeString ("xmlns", ns);
			int start = sw.ToString ().Length;
			tw.WriteStartElement ("UserInfo");
			tw.WriteStartAttribute ("actor", ns);
			tw.WriteEndAttribute ();
			tw.WriteEndElement ();
			tw.WriteEndElement ();
			Assert.IsTrue (sw.ToString ().IndexOf (ns, start) > 0);
		}

		[Test]
		public void WriteCommentPIAndIndent ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter w = new XmlTextWriter (sw);
			w.Formatting = Formatting.Indented;
			w.WriteStartElement ("foo");
			w.WriteComment ("test");
			w.WriteProcessingInstruction ("PI", "");
			w.WriteStartElement ("child");
			w.WriteEndElement ();
			w.WriteComment ("test");
			w.WriteString ("STRING");
			w.WriteEndElement ();
			Assert.AreEqual (String.Format (@"<foo>{0}  <!--test-->{0}  <?PI ?>{0}  <child />{0}  <!--test-->STRING</foo>", Environment.NewLine), sw.ToString ());
		}

		[Test]
		public void WriteBinHexAttribute () // for bug #79019
		{
			XmlWriter writer = new XmlTextWriter (TextWriter.Null);
			writer.WriteStartElement ("test");
			byte [] buffer1 = new byte [] {200, 155};
			writer.WriteStartAttribute ("key", "");
			writer.WriteBinHex (buffer1, 0, buffer1.Length);
			writer.WriteEndAttribute ();
			writer.WriteEndElement ();
		}

		[Test]
		public void LookupNamespace ()
		{
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.Formatting = Formatting.Indented;
			string q1 = "urn:test";

			string q1prefix_first= "q1";
			// Ensure we get a different reference for the string "q1"
			string q1prefix_second = ("q1" + "a").Substring(0,2);

			xw.WriteStartElement("document");
			xw.WriteStartElement("item");
			xw.WriteStartElement (q1prefix_first, "addMedia", q1);
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.WriteStartElement("item");
			xw.WriteStartElement (q1prefix_second, "addMedia", q1);
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.WriteEndElement();
			string xml = sw.ToString ();
			int first = xml.IndexOf ("xmlns");
			Assert.IsTrue (xml.IndexOf ("xmlns", first + 5) > 0);
		}

		[Test]
		public void WriteAttributePrefixedNullNamespace ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter xw = new XmlTextWriter (sw);
			xw.WriteStartElement ("root");
			xw.WriteAttributeString ("xmlns", "abc", null, "uri:abcnamespace");
			xw.WriteAttributeString ("abc", "def", null, "value");
			xw.WriteEndElement ();
			Assert.AreEqual ("<root xmlns:abc=\"uri:abcnamespace\" abc:def=\"value\" />", sw.ToString ());
		}

		[Test]
		public void WriteElementPrefixedNullNamespace ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter xw = new XmlTextWriter (sw);
			xw.WriteStartElement ("root");
			xw.WriteAttributeString ("xmlns", "abc", null, "uri:abcnamespace");
			xw.WriteStartElement ("abc", "def", null);
			xw.WriteEndElement ();
			xw.WriteEndElement ();
			Assert.AreEqual ("<root xmlns:abc=\"uri:abcnamespace\"><abc:def /></root>", sw.ToString ());
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RejectWritingAtErrorState ()
		{
			try {
				xtw.WriteEndElement ();
			} catch (Exception) {
			}

			xtw.WriteStartElement ("foo");
		}
#endif
	}
}
