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
		public void AttributeNamespacesThreeParamWithTextInNamespaceParam ()
		{
			try  {
				xtw.WriteAttributeString ("xmlns", "http://somenamespace.com", "http://abc.def");
			} catch (ArgumentException) {}
		}

		[Test]
		public void AttributeNamespacesWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			Assert.AreEqual ("xmlns:abc='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesWithTextInNamespaceParam ()
		{
			try {
				xtw.WriteAttributeString ("xmlns", "abc", "http://somenamespace.com", "http://abc.def");
			} catch (ArgumentException) {}
		}

		[Test]
		[Category ("NotDotNet")]
		public void AttributeNamespacesXmlnsXmlns ()
		{
			xtw.WriteStartElement ("foo");
			try {
				xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
				// This should not be allowed, even though MS.NET doesn't treat as an error.
				// See http://www.w3.org/TR/REC-xml-names/ Namespace Constraint: Prefix Declared
				Assert.Fail ("any prefix which name starts from \"xml\" must not be allowed.");
			} catch (ArgumentException) {}
			xtw.WriteAttributeString ("", "xmlns", null, "http://abc.def");
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

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString ("", "quux");
				// Assert.AreEqual ("<foo foo='bar' bar='' baz='' ='quux'", StringWriterText);
				Assert.Fail ("empty name not allowed.");
			} catch (Exception) {
			}

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString (null, "quuux");
				// Assert.AreEqual ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", StringWriterText);
				Assert.Fail ("null name not allowed.");
			} catch (Exception) {
			}
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
			Assert.AreEqual ("<foo>bar</foo><baz />", StringWriterText);

			xtw.WriteElementString ("quux", null);
			Assert.AreEqual ("<foo>bar</foo><baz /><quux />", StringWriterText);

			xtw.WriteElementString ("", "quuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</>", StringWriterText);

			xtw.WriteElementString (null, "quuuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", StringWriterText);
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
			Assert.AreEqual ("foo", xtw.LookupPrefix ("http://abc.def"));
			Assert.AreEqual ("bar", xtw.LookupPrefix ("http://ghi.jkl"));
			xtw.WriteEndElement ();

			xtw.WriteStartElement ("two");
			xtw.WriteAttributeString ("xmlns", "baz", null, "http://mno.pqr");
			xtw.WriteString("quux");
			Assert.AreEqual ("baz", xtw.LookupPrefix ("http://mno.pqr"));
			Assert.IsNull (xtw.LookupPrefix ("http://abc.def"));
			Assert.IsNull (xtw.LookupPrefix ("http://ghi.jkl"));

			Assert.IsNull (xtw.LookupPrefix ("http://bogus"));
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
		[ExpectedException (typeof (ArgumentException))]
#if NET_2_0
		[Category ("NotDotNet")] // ... bug or design?
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
			Assert.AreEqual (XmlSpace.None, xtw.XmlSpace);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'", StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'", StringWriterText);

			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "space", null);
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);

			// Commented out there: it is implementation-dependent
			// and incompatible between .NET 1.0 and 1.1
			xtw.WriteString ("default");
			// Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			// Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			Assert.AreEqual (XmlSpace.Default, xtw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='default'", StringWriterText);

			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.None, xtw.XmlSpace);

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
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement ("b");
			Assert.AreEqual (String.Empty, xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 xmlns='' /></b></foo>", StringWriterText);
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration2 ()
		{
			xtw.WriteStartDocument ();
			// IMPORTANT DIFFERENCE!! ns = "", not null
			xtw.WriteStartElement ("foo", "");
			xtw.WriteAttributeString ("xmlns", "probe");
			Assert.IsNull (xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement ("b");
			Assert.IsNull (xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 /></b></foo>", StringWriterText);
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
			// Compare with the test AttributeWriteStartAttributeXmlnsNullNS().
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
			Assert.IsTrue (xml.IndexOf ("p:foo='xyz'") > 0, "p:foo");
			Assert.IsTrue (xml.IndexOf ("d1p1:bar='xyz'") > 0, "d1p1:bar");
			Assert.IsTrue (xml.IndexOf ("d1p2:baz='xyz'") > 0, "d1p1:baz");
			Assert.IsTrue (xml.IndexOf ("xmlns:d1p2='urn:baz'") > 0, "xmlns:d1p2");
			Assert.IsTrue (xml.IndexOf ("xmlns:d1p1='urn:bar'") > 0, "xmlns:d1p1");
			Assert.IsTrue (xml.IndexOf ("xmlns:p='urn:foo'") > 0, "xmlns:p");
			Assert.IsTrue (xml.IndexOf ("<out p:foo='xyz'><out p:foo='xyz' /></out></out>") > 0, "remaining");
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
