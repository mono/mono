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

namespace Ximian.Mono.Tests
{
	public class XmlTextWriterTests : TestCase
	{
		public XmlTextWriterTests () : base ("Ximian.Mono.Tests.XmlTextWriterTests testsuite") {}
		public XmlTextWriterTests (string name) : base (name) {}

		StringWriter sw;
		XmlTextWriter xtw;

		protected override void SetUp ()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
		}

		// TODO : Seeing weird behavior from Microsoft on namespace declarations via WriteAttributeString
		// so saving until it can be figured out.
		public void saveTestAttributeNamespaces ()
		{
			xtw.QuoteChar = '\'';
			xtw.WriteStartElement ("foo");

			try {
				xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
				Fail ("Expected an ArgumentException to be thrown.");
			}
			catch (ArgumentException) {}

			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			AssertEquals ("<foo   xmlns:abc='http://abc.def'", sw.GetStringBuilder().ToString());

			xtw.WriteAttributeString ("xmlns", "def", "http://somenamespace.com", "http://def.ghi");
			AssertEquals ("<foo xmlns='http://netsack.com' xmlns:abc='http://abc.def'", sw.GetStringBuilder().ToString());

			xtw.WriteAttributeString ("xmlns", null, "http://ghi.jkl");
			AssertEquals ("<foo xmlns='http://netsack.com' xmlns:abc='http://abc.def' xmlns='http://ghi.jkl'", sw.GetStringBuilder().ToString());

			xtw.WriteAttributeString ("xmlns", null, "http://netsack.com");
			AssertEquals ("<foo xmlns='http://netsack.com'", sw.GetStringBuilder().ToString());

			xtw.WriteAttributeString ("xmlns", "foo", "http://netsack.com", "bar");
			AssertEquals ("<foo xmlns:foo='http://netsack.com'", sw.GetStringBuilder().ToString());
		}

		public void TestAttributeWriteAttributeString ()
		{
			xtw.WriteStartElement ("foo");
			xtw.QuoteChar = '\'';

			xtw.WriteAttributeString ("foo", "bar");
			AssertEquals ("<foo foo='bar'", sw.GetStringBuilder().ToString());

			xtw.WriteAttributeString ("bar", "");
			AssertEquals ("<foo foo='bar' bar=''", sw.GetStringBuilder ().ToString ());

			xtw.WriteAttributeString ("baz", null);
			AssertEquals ("<foo foo='bar' bar='' baz=''", sw.GetStringBuilder ().ToString ());

			// TODO: Why does this pass Microsoft?
			xtw.WriteAttributeString ("", "quux");
			AssertEquals ("<foo foo='bar' bar='' baz='' ='quux'", sw.GetStringBuilder ().ToString ());

			// TODO: Why does this pass Microsoft?
			xtw.WriteAttributeString (null, "quuux");
			AssertEquals ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", sw.GetStringBuilder ().ToString ());
		}

		public void TestCDataValid ()
		{
			xtw.WriteCData ("foo");
			AssertEquals ("WriteCData had incorrect output.", "<![CDATA[foo]]>", sw.GetStringBuilder().ToString());
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
			AssertEquals ("Close didn't write out end elements properly.", "<foo><bar><baz /></bar></foo>",
				sw.GetStringBuilder().ToString());
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
			AssertEquals ("WriteComment had incorrect output.", "<!--foo-->", sw.GetStringBuilder().ToString());
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
			sr = new StreamReader (ms);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\"?>", sr.ReadToEnd ());
			Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UnicodeEncoding ());
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UTF8Encoding ());
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-8\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms);
			AssertEquals ("<?xml version=\"1.0\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms);
			AssertEquals ("<?xml version=\"1.0\" standalone=\"yes\"?>", sr.ReadToEnd ());
			Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));
		}

		public void TestDocumentStart ()
		{
			xtw.WriteStartDocument ();
			AssertEquals ("XmlDeclaration is incorrect.", "<?xml version=\"1.0\" encoding=\"utf-16\"?>",
				sw.GetStringBuilder ().ToString ());

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
			xtw.WriteStartDocument (true);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>",
				sw.GetStringBuilder ().ToString ());

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.WriteStartDocument (false);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"no\"?>",
				sw.GetStringBuilder ().ToString ());
		}

		public void TestElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("Incorrect output.", "<foo />", sw.GetStringBuilder().ToString());
		}

		public void TestElementWriteElementString ()
		{
			xtw.WriteElementString ("foo", "bar");
			AssertEquals ("WriteElementString has incorrect output.", "<foo>bar</foo>", sw.GetStringBuilder().ToString());

			xtw.WriteElementString ("baz", "");
			AssertEquals ("<foo>bar</foo><baz />", sw.GetStringBuilder ().ToString ());

			xtw.WriteElementString ("quux", null);
			AssertEquals ("<foo>bar</foo><baz /><quux />", sw.GetStringBuilder ().ToString ());

			xtw.WriteElementString ("", "quuux");
			AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</>", sw.GetStringBuilder ().ToString ());

			xtw.WriteElementString (null, "quuuux");
			AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", sw.GetStringBuilder ().ToString ());
		}

		public void TestFormatting ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteElementString ("bar", "");
			xtw.Close ();
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<foo>\r\n  <bar />\r\n</foo>",
				sw.GetStringBuilder ().ToString ());
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
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<foo>\r\nxx<bar>\r\nxxxx<baz />\r\nxx</bar>\r\n</foo>",
				sw.GetStringBuilder ().ToString ());
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
			AssertEquals ("<ol>\r\n  <li>The big <b>E</b><i>lephant</i> walks slowly.</li>\r\n</ol>",
				sw.GetStringBuilder ().ToString ());
		}


		public void TestNamespacesAttributesPassingInNamespaces ()
		{
			xtw.QuoteChar = '\'';
			xtw.Namespaces = false;
			xtw.WriteStartElement ("foo");

			// These shouldn't throw any exceptions since they don't pass in
			// a namespace.
			xtw.WriteAttributeString ("bar", "baz");
			xtw.WriteAttributeString ("", "a", "", "b");
			xtw.WriteAttributeString (null, "c", "", "d");
			xtw.WriteAttributeString ("", "e", null, "f");
			xtw.WriteAttributeString (null, "g", null, "h");

			AssertEquals ("<foo bar='baz' a='b' c='d' e='f' g='h'", sw.GetStringBuilder ().ToString ());

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

			AssertEquals ("<foo>bar</foo><baz><quux><quuux><a><b><c><d", sw.GetStringBuilder ().ToString ());

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
				"<foo xmlns=\"http://netsack.com/\"><bar xmlns=\"\"><baz /></bar></foo>",
				sw.GetStringBuilder().ToString());
		}

		public void TestNamespacesPrefix ()
		{
			xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
			xtw.WriteStartElement ("foo", "baz", "http://netsack.com/");
			xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			AssertEquals ("XmlTextWriter is incorrectly outputting prefixes.",
				"<foo:bar xmlns:foo=\"http://netsack.com/\"><foo:baz><foo:qux /></foo:baz></foo:bar>",
				sw.GetStringBuilder ().ToString ());
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
			AssertEquals ("WriteProcessingInstruction had incorrect output.", "<?foo bar?>", sw.GetStringBuilder().ToString());
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

		public void TestQuoteCharSingleQuote ()
		{
			xtw.QuoteChar = '\'';

			// version, encoding, standalone
			xtw.WriteStartDocument (true);
			
			// namespace declaration
			xtw.WriteElementString ("foo", "http://netsack.com", "bar");

			AssertEquals ("<?xml version='1.0' encoding='utf-16' standalone='yes'?><foo xmlns='http://netsack.com'>bar</foo>",
				sw.GetStringBuilder ().ToString ());


		}

		public void TestQuoteCharInvalid ()
		{
			try {
				xtw.QuoteChar = 'x';
				Fail ("Should have thrown an ArgumentException.");
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

		public void TestWriteEndElement ()
		{
			try 
			{
				xtw.WriteEndElement ();
				Fail ("Should have thrown an InvalidOperationException.");
			}
			catch (InvalidOperationException e) {
				AssertEquals ("Exception message is incorrect.",
					"There was no XML start tag open.", e.Message);
			}
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
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\"?><foo bar=\"&amp;&lt;&gt;\"",
				sw.GetStringBuilder ().ToString ());

			// When QuoteChar is double quote then replaces double quotes within attributes
			// but not single quotes.
			sw.GetStringBuilder ().Remove (0, sw.GetStringBuilder ().Length);
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			AssertEquals ("><foo bar=\"&quot;baz&quot;\" quux=\"'baz'\"",
				sw.GetStringBuilder ().ToString ());

			// When QuoteChar is single quote then replaces single quotes within attributes
			// but not double quotes.
			xtw.QuoteChar = '\'';
			sw.GetStringBuilder ().Remove (0, sw.GetStringBuilder ().Length);
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			AssertEquals ("><foo bar='\"baz\"' quux='&apos;baz&apos;'",
				sw.GetStringBuilder ().ToString ());

			// Testing element values

			sw.GetStringBuilder ().Remove (0, sw.GetStringBuilder ().Length);
			xtw.WriteElementString ("foo", "&<>\"'");
			AssertEquals ("><foo>&amp;&lt;&gt;\"'</foo>",
				sw.GetStringBuilder ().ToString ());
		}

		public void TestXmlLang ()
		{
			xtw.QuoteChar = '\'';

			AssertNull (xtw.XmlLang);
			
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "lang", null, "langfoo");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo'", sw.GetStringBuilder ().ToString ());

			xtw.WriteAttributeString ("boo", "yah");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'", sw.GetStringBuilder ().ToString ());
			
			xtw.WriteElementString("bar", "baz");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteString("baz");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "lang", null);
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteString("langbar");
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteEndAttribute ();
			AssertEquals ("langbar", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteElementString ("quuux", "squonk");
			AssertEquals ("langbar", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><quuux>squonk</quuux>",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteEndElement ();
			AssertEquals ("langfoo", xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><quuux>squonk</quuux></quux>",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteEndElement ();
			AssertNull (xtw.XmlLang);
			AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><quuux>squonk</quuux></quux></foo>",
				sw.GetStringBuilder ().ToString ());
			
			xtw.Close ();
			AssertNull (xtw.XmlLang);
		}

		// TODO: test operational aspects
		public void TestXmlSpace ()
		{
			xtw.QuoteChar = '\'';

			xtw.WriteStartElement ("foo");
			AssertEquals (XmlSpace.None, xtw.XmlSpace);

			xtw.WriteString ("foo");
			xtw.WriteWhitespace (" ");
			xtw.WriteString ("bar");
			xtw.WriteString (" baz quux");
			AssertEquals ("<foo>foo bar baz quux",
				sw.GetStringBuilder ().ToString ());

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);

			xtw.WriteString ("foo");
			xtw.WriteWhitespace (" ");
			xtw.WriteString ("bar");
			xtw.WriteString (" baz quux");
			AssertEquals ("<foo>foo bar baz quux<bar xml:space='preserve'>foo bar baz quux",
				sw.GetStringBuilder ().ToString ());

			xtw.WriteStartElement ("baz");
			xtw.WriteStartAttribute ("xml", "space", null);
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo>foo bar baz quux<bar xml:space='preserve'>foo bar baz quux<baz xml:space='",
				sw.GetStringBuilder ().ToString ());

			xtw.WriteString ("default");
			AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			AssertEquals ("<foo>foo bar baz quux<bar xml:space='preserve'>foo bar baz quux<baz xml:space='",
				sw.GetStringBuilder ().ToString ());
			
			xtw.WriteEndAttribute ();
			AssertEquals (XmlSpace.Default, xtw.XmlSpace);
			AssertEquals ("<foo>foo bar baz quux<bar xml:space='preserve'>foo bar baz quux<baz xml:space='default'",
				sw.GetStringBuilder ().ToString ());

			xtw.WriteString ("foo");
			xtw.WriteWhitespace (" ");
			xtw.WriteString ("bar");
			xtw.WriteString (" baz quux");
			AssertEquals ("<foo>foo bar baz quux<bar xml:space='preserve'>foo bar baz quux<baz xml:space='default'>foo bar baz quux",
				sw.GetStringBuilder ().ToString ());

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
	}
}
