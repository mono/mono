//
// System.Xml.XmlTextWriterTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlTextWriterTests : TestCase
	{
		public XmlTextWriterTests () : base ("MonoTests.System.Xml.XmlTextWriterTests testsuite") {}
		public XmlTextWriterTests (string name) : base (name) {}

		StringWriter sw;
		XmlTextWriter xtw;

		protected override void SetUp ()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
		}

		private string StringWriterText 
		{
			get { return sw.GetStringBuilder ().ToString (); }
		}

		public void TestAttributeNamespacesNonNamespaceAttributeBefore ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString("bar", "baz");
			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			AssertEquals ("<foo bar='baz' xmlns:abc='http://abc.def'", StringWriterText);
		}

		public void TestAttributeNamespacesNonNamespaceAttributeAfter ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			xtw.WriteAttributeString("bar", "baz");
			AssertEquals ("<foo xmlns:abc='http://abc.def' bar='baz'", StringWriterText);
		}

		public void TestAttributeNamespacesThreeParamWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", null, "http://abc.def");
			AssertEquals ("xmlns='http://abc.def'", StringWriterText);
		}

		public void TestAttributeNamespacesThreeParamWithTextInNamespaceParam ()
		{
			try 
			{
				xtw.WriteAttributeString ("xmlns", "http://somenamespace.com", "http://abc.def");
			} 
			catch (ArgumentException) {}
		}

		public void TestAttributeNamespacesWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			AssertEquals ("xmlns:abc='http://abc.def'", StringWriterText);
		}

		public void TestAttributeNamespacesWithTextInNamespaceParam ()
		{
			try {
				xtw.WriteAttributeString ("xmlns", "abc", "http://somenamespace.com", "http://abc.def");
			} catch (ArgumentException) {}
		}

		public void TestAttributeNamespacesXmlnsXmlns ()
		{
			xtw.WriteStartElement ("foo");
			try 
			{
				xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
			} 
			catch (Exception e) {
				Fail ("Unexpected Exception thrown" + e);
			}
		}

		public void TestAttributeWriteAttributeString ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("foo", "bar");
			AssertEquals ("<foo foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("bar", "");
			AssertEquals ("<foo foo='bar' bar=''", StringWriterText);

			xtw.WriteAttributeString ("baz", null);
			AssertEquals ("<foo foo='bar' bar='' baz=''", StringWriterText);

			// TODO: Why does this pass Microsoft?
			xtw.WriteAttributeString ("", "quux");
			AssertEquals ("<foo foo='bar' bar='' baz='' ='quux'", StringWriterText);

			// TODO: Why does this pass Microsoft?
			xtw.WriteAttributeString (null, "quuux");
			AssertEquals ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", StringWriterText);
		}

		public void TestAttributeWriteAttributeStringNotInsideOpenStartElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteString ("bar");
			
			try 
			{
				xtw.WriteAttributeString ("baz", "quux");
				Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}
		}

		public void TestAttributeWriteAttributeStringWithoutParentElement ()
		{
			xtw.WriteAttributeString ("foo", "bar");
			AssertEquals ("foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("baz", "quux");
			AssertEquals ("foo='bar' baz='quux'", StringWriterText);
		}

		public void TestCDataValid ()
		{
			xtw.WriteCData ("foo");
			AssertEquals ("WriteCData had incorrect output.", "<![CDATA[foo]]>", StringWriterText);
		}

		public void TestCDataInvalid ()
		{
			try {
				xtw.WriteCData("foo]]>bar");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		public void TestCloseOpenElements ()
		{
			xtw.WriteStartElement("foo");
			xtw.WriteStartElement("bar");
			xtw.WriteStartElement("baz");
			xtw.Close();
			AssertEquals ("Close didn't write out end elements properly.", "<foo><bar><baz /></bar></foo>",	StringWriterText);
		}

		public void TestCloseWriteAfter ()
		{
			xtw.WriteElementString ("foo", "bar");
			xtw.Close ();

			// WriteEndElement and WriteStartDocument aren't tested here because
			// they will always throw different exceptions besides 'The Writer is closed.'
			// and there are already tests for those exceptions.

			try {
				xtw.WriteCData ("foo");
				Fail ("WriteCData after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteComment ("foo");
				Fail ("WriteComment after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteProcessingInstruction ("foo", "bar");
				Fail ("WriteProcessingInstruction after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteStartElement ("foo", "bar", "baz");
				Fail ("WriteStartElement after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try 
			{
				xtw.WriteAttributeString ("foo", "bar");
				Fail ("WriteAttributeString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) 
			{
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteString ("foo");
				Fail ("WriteString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}
		}

		public void TestCommentValid ()
		{
			xtw.WriteComment ("foo");
			AssertEquals ("WriteComment had incorrect output.", "<!--foo-->", StringWriterText);
		}

		public void TestCommentInvalid ()
		{
			try {
				xtw.WriteComment("foo-");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try {
				xtw.WriteComment("foo-->bar");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		public void TestConstructorsAndBaseStream ()
		{
			Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (null, this.xtw.BaseStream));

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
			AssertEquals (expectedXmlDeclaration, actualXmlDeclaration);
			Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UnicodeEncoding ());
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.Unicode);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UTF8Encoding ());
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-8\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			AssertEquals ("<?xml version=\"1.0\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			AssertEquals ("<?xml version=\"1.0\" standalone=\"yes\"?>", sr.ReadToEnd ());
			Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));
		}

		public void TestDocumentStart ()
		{
			xtw.WriteStartDocument ();
			AssertEquals ("XmlDeclaration is incorrect.", "<?xml version='1.0' encoding='utf-16'?>", StringWriterText);

			try 
			{
				xtw.WriteStartDocument ();
				Fail("Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.",
					"WriteStartDocument should be the first call.", e.Message);
			}

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (true);
			AssertEquals ("<?xml version='1.0' encoding='utf-16' standalone='yes'?>", StringWriterText);

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (false);
			AssertEquals ("<?xml version='1.0' encoding='utf-16' standalone='no'?>", StringWriterText);
		}

		public void TestElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("Incorrect output.", "<foo />", StringWriterText);
		}

		public void TestElementWriteElementString ()
		{
			xtw.WriteElementString ("foo", "bar");
			AssertEquals ("WriteElementString has incorrect output.", "<foo>bar</foo>", StringWriterText);

			xtw.WriteElementString ("baz", "");
			AssertEquals ("<foo>bar</foo><baz />", StringWriterText);

			xtw.WriteElementString ("quux", null);
			AssertEquals ("<foo>bar</foo><baz /><quux />", StringWriterText);

			xtw.WriteElementString ("", "quuux");
			AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</>", StringWriterText);

			xtw.WriteElementString (null, "quuuux");
			AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", StringWriterText);
		}

		public void TestFormatting ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteElementString ("bar", "");
			xtw.Close ();
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?>\r\n<foo>\r\n  <bar />\r\n</foo>", StringWriterText);
		}

		public void TestFormattingInvalidXmlForFun ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.IndentChar = 'x';
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteStartElement ("bar");
			xtw.WriteElementString ("baz", "");
			xtw.Close ();
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?>\r\n<foo>\r\nxx<bar>\r\nxxxx<baz />\r\nxx</bar>\r\n</foo>", StringWriterText);
		}

		public void TestFormattingFromRemarks ()
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
			AssertEquals ("<ol>\r\n  <li>The big <b>E</b><i>lephant</i> walks slowly.</li>\r\n</ol>", StringWriterText);
		}

		public void TestLookupPrefix ()
		{
			xtw.WriteStartElement ("root");

			xtw.WriteStartElement ("one");
			xtw.WriteAttributeString ("xmlns", "foo", null, "http://abc.def");
			xtw.WriteAttributeString ("xmlns", "bar", null, "http://ghi.jkl");
			AssertEquals ("foo", xtw.LookupPrefix ("http://abc.def"));
			AssertEquals ("bar", xtw.LookupPrefix ("http://ghi.jkl"));
			xtw.WriteEndElement ();

			xtw.WriteStartElement ("two");
			xtw.WriteAttributeString ("xmlns", "baz", null, "http://mno.pqr");
			xtw.WriteString("quux");
			AssertEquals ("baz", xtw.LookupPrefix ("http://mno.pqr"));
			AssertNull (xtw.LookupPrefix ("http://abc.def"));
			AssertNull (xtw.LookupPrefix ("http://ghi.jkl"));

			AssertNull (xtw.LookupPrefix ("http://bogus"));
		}

		public void TestNamespacesAttributesPassingInNamespaces ()
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

			AssertEquals ("<foo bar='baz' a='b' c='d' e='f' g='h'", StringWriterText);

			// These should throw ArgumentException because they pass in a
			// namespace when Namespaces = false.
		}

		public void TestNamespacesElementsPassingInNamespaces ()
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

			AssertEquals ("<foo>bar</foo><baz><quux><quuux><a><b><c><d", StringWriterText);

			// These should throw ArgumentException because they pass in a
			// namespace when Namespaces = false.
			try {
				xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "http://netsack.com/");
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", null);
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", "");
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "", "");
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}
		}

		public void TestNamespacesNoNamespaceClearsDefaultNamespace ()
		{
			xtw.WriteStartElement(String.Empty, "foo", "http://netsack.com/");
			xtw.WriteStartElement(String.Empty, "bar", String.Empty);
			xtw.WriteElementString("baz", String.Empty, String.Empty);
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			AssertEquals ("XmlTextWriter is incorrectly outputting namespaces.",
				"<foo xmlns='http://netsack.com/'><bar xmlns=''><baz /></bar></foo>", StringWriterText);
		}

		public void TestNamespacesPrefix ()
		{
			xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
			xtw.WriteStartElement ("foo", "baz", "http://netsack.com/");
			xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			AssertEquals ("XmlTextWriter is incorrectly outputting prefixes.",
				"<foo:bar xmlns:foo='http://netsack.com/'><foo:baz><foo:qux /></foo:baz></foo:bar>", StringWriterText);
		}

		public void TestNamespacesPrefixWithEmptyAndNullNamespace ()
		{
			try {
				xtw.WriteStartElement ("foo", "bar", "");
				Fail ("Should have thrown an ArgumentException.");
			} catch (ArgumentException) {}

			try 
			{
				xtw.WriteStartElement ("foo", "bar", null);
				Fail ("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) {}
		}

		public void TestNamespacesSettingWhenWriteStateNotStart ()
		{
			xtw.WriteStartElement ("foo");
			try 
			{
				xtw.Namespaces = false;
				Fail ("Expected an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {}
			AssertEquals (true, xtw.Namespaces);
		}

		public void TestProcessingInstructionValid ()
		{
			xtw.WriteProcessingInstruction("foo", "bar");
			AssertEquals ("WriteProcessingInstruction had incorrect output.", "<?foo bar?>", StringWriterText);
		}

		public void TestProcessingInstructionInvalid ()
		{
			try 
			{
				xtw.WriteProcessingInstruction("fo?>o", "bar");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction("foo", "ba?>r");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction("", "bar");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction(null, "bar");
				Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		public void TestQuoteCharDoubleQuote ()
		{
			xtw.QuoteChar = '"';

			// version, encoding, standalone
			xtw.WriteStartDocument (true);
			
			// namespace declaration
			xtw.WriteElementString ("foo", "http://netsack.com", "bar");

			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><foo xmlns=\"http://netsack.com\">bar</foo>", StringWriterText);


		}

		public void TestQuoteCharInvalid ()
		{
			try {
				xtw.QuoteChar = 'x';
				Fail ("Should have thrown an ArgumentException.");
			} catch (ArgumentException) {}
		}

		public void TestWriteBase64 ()
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] fooBar = encoding.GetBytes("foobar");
			xtw.WriteBase64 (fooBar, 0, 6);
			AssertEquals("Zm9vYmFy", StringWriterText);

			try {
				xtw.WriteBase64 (fooBar, 3, 6);
				Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteBase64 (fooBar, -1, 6);
				Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (fooBar, 3, -1);
				Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (null, 0, 6);
				Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentNullException) {}
		}

		public void TestWriteCharEntity ()
		{
			xtw.WriteCharEntity ('a');
			AssertEquals ("&#x61;", StringWriterText);

			xtw.WriteCharEntity ('A');
			AssertEquals ("&#x61;&#x41;", StringWriterText);

			xtw.WriteCharEntity ('1');
			AssertEquals ("&#x61;&#x41;&#x31;", StringWriterText);

			xtw.WriteCharEntity ('K');
			AssertEquals ("&#x61;&#x41;&#x31;&#x4B;", StringWriterText);

			try {
				xtw.WriteCharEntity ((char)0xd800);
			} catch (ArgumentException) {}
		}

		public void TestWriteEndAttribute ()
		{
			try 
			{
				xtw.WriteEndAttribute ();
				Fail ("Should have thrown an InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
		}

		public void TestWriteEndDocument ()
		{
			try {
				xtw.WriteEndDocument ();
				Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			xtw.WriteStartDocument ();

			try 
			{
				xtw.WriteEndDocument ();
				Fail ("Expected an ArgumentException.");
			} 
			catch (ArgumentException) {}

			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='", StringWriterText);

			xtw.WriteEndDocument ();
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='' />", StringWriterText);
			AssertEquals (WriteState.Start, xtw.WriteState);
		}

		public void TestWriteEndElement ()
		{
			try {
				xtw.WriteEndElement ();
				Fail ("Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.", "There was no XML start tag open.", e.Message);
			}

			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("<foo />", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteStartAttribute ("baz", null);
			xtw.WriteEndElement ();
			AssertEquals ("<foo /><bar baz='' />", StringWriterText);
		}

		public void TestFullEndElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteFullEndElement ();
			AssertEquals ("<foo></foo>", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("foo", "bar");
			xtw.WriteFullEndElement ();
			AssertEquals ("<foo></foo><bar foo='bar'></bar>", StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteFullEndElement ();
			AssertEquals ("<foo></foo><bar foo='bar'></bar><baz bar=''></baz>", StringWriterText);
		}

		public void TestWriteRaw ()
		{
			xtw.WriteRaw("&<>\"'");
			AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw(null);
			AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw("");
			AssertEquals ("&<>\"'", StringWriterText);
		}

		public void TestWriteRawInvalidInAttribute ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteRaw ("&<>\"'");
			xtw.WriteEndAttribute ();
			xtw.WriteEndElement ();
			AssertEquals ("<foo bar='&<>\"'' />", StringWriterText);
		}

		public void TestWriteState ()
		{
			AssertEquals (WriteState.Start, xtw.WriteState);
			xtw.WriteStartDocument ();
			AssertEquals (WriteState.Prolog, xtw.WriteState);
			xtw.WriteStartElement ("root");
			AssertEquals (WriteState.Element, xtw.WriteState);
			xtw.WriteElementString ("foo", "bar");
			AssertEquals (WriteState.Content, xtw.WriteState);
			xtw.Close ();
			AssertEquals (WriteState.Closed, xtw.WriteState);
		}

		public void TestWriteString ()
		{
			xtw.WriteStartDocument ();
			try {
				xtw.WriteString("foo");
			} catch (InvalidOperationException) {}

			// Testing attribute values

			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "&<>");
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='&amp;&lt;&gt;'", StringWriterText);
		}

		public void TestWriteAttributeStringSingleQuoteChar()
		{
			// When QuoteChar is single quote then replaces single quotes within attributes
			// but not double quotes.
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			AssertEquals ("<foo bar='\"baz\"' quux='&apos;baz&apos;'", StringWriterText);
		}

		public void TestWriteAttributeStringDoubleQuoteChar()
		{
			// When QuoteChar is double quote then replaces double quotes within attributes
			// but not single quotes.
			xtw.QuoteChar = '"';
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			AssertEquals ("<foo bar=\"&quot;baz&quot;\" quux=\"'baz'\"", StringWriterText);
		}

		public void TestWriteStringWithEntities()
		{
			// Testing element values
			xtw.QuoteChar = '\'';
			xtw.WriteElementString ("foo", "&<>\"'");
			AssertEquals ("<foo>&amp;&lt;&gt;\"'</foo>", StringWriterText);
		}

		public void TestXmlLang ()
		{
			AssertNull (xtw.XmlLang);
			
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "lang", null, "langfoo");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo'", StringWriterText);

			xtw.WriteAttributeString ("boo", "yah");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'", StringWriterText);
			
			xtw.WriteElementString("bar", "baz");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>", StringWriterText);
			
			xtw.WriteString("baz");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz", StringWriterText);
			
			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "lang", null);
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteString("langbar");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			AssertEquals ("langbar", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'", StringWriterText);

			// check if xml:lang repeats output even if same as current scope.
			xtw.WriteStartElement ("joe");
			xtw.WriteAttributeString ("xml", "lang", null, "langbar");
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'", StringWriterText);

			
			xtw.WriteElementString ("quuux", "squonk");
			AssertEquals ("langbar", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux>", StringWriterText);

			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux>", StringWriterText);
			
			xtw.WriteEndElement ();
			AssertNull (xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux></foo>", StringWriterText);
			
			xtw.Close ();
			AssertNull (xtw.XmlLang);
		}

		// TODO: test operational aspects
		public void TestXmlSpace ()
		{
			xtw.WriteStartElement ("foo");
			AssertEquals (XmlSpace.None, xtw.XmlSpace);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo><bar xml:space='preserve'",	StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'", StringWriterText);

			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "space", null);
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);

			xtw.WriteString ("default");
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			AssertEquals (XmlSpace.Default, xtw.XmlSpace);
			AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='default'", StringWriterText);

			xtw.WriteEndElement ();
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			AssertEquals (XmlSpace.None, xtw.XmlSpace);

			xtw.WriteStartElement ("quux");
			try {
				xtw.WriteAttributeString ("xml", "space", null, "bubba");
			} catch (ArgumentException) {}

			try {
				xtw.WriteAttributeString ("xml", "space", null, "PRESERVE");
			} catch (ArgumentException) {}

			try {
				xtw.WriteAttributeString ("xml", "space", null, "Preserve");
			} catch (ArgumentException) {}

			try {
				xtw.WriteAttributeString ("xml", "space", null, "Default");
			} catch (ArgumentException) {}

			try {
				xtw.WriteWhitespace ("x");
			} catch (ArgumentException) { }
		}

		public void TestXmlSpaceRaw ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("xml", "space", null);
			AssertEquals (XmlSpace.None, xtw.XmlSpace);
			AssertEquals ("<foo xml:space='", StringWriterText);

			xtw.WriteString ("default");
			AssertEquals (XmlSpace.None, xtw.XmlSpace);
			AssertEquals ("<foo xml:space='", StringWriterText);

			xtw.WriteEndAttribute ();
			AssertEquals (XmlSpace.Default, xtw.XmlSpace);
			AssertEquals ("<foo xml:space='default'", StringWriterText);
		}

		public void TestWriteAttributes ()
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
			AssertEquals("#WriteAttributes.XmlDecl.1", "version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"", sw.ToString().Trim());

			sb.Remove(0, sb.Length);	// init
			xtr = new XmlTextReader("<?xml version='1.0'		 standalone='no'?><root a1='A' b2='B' c3='C'><foo><bar /></foo></root>", XmlNodeType.Document, ctx);
			xtr.Read();	// read XMLDecl
			wr.WriteAttributes(xtr, false);
			// This method don't always have to take this double-quoted style...
			AssertEquals("#WriteAttributes.XmlDecl.2", "version=\"1.0\" standalone=\"no\"", sw.ToString().Trim());

			sb.Remove(0, sb.Length);	// init
			xtr.Read();	// read root
			wr.WriteStartElement(xtr.LocalName, xtr.Value);
			wr.WriteAttributes(xtr, false);
			wr.WriteEndElement();
			wr.Close();
			// This method don't always have to take this double-quoted style...
			AssertEquals("#WriteAttributes.Element", "<root a1=\"A\" b2=\"B\" c3=\"C\" />", sw.ToString().Trim());
		}
	}
}
