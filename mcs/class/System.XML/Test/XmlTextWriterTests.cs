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


		// TODO: Need some tests for attributes with namespaces here...
		public void saveTestNamespacesAttributesPassingInNamespaces ()
		{
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

		public void TestQuoteCharInvalid ()
		{
			try {
				xtw.QuoteChar = 'x';
				Fail ("Should have thrown an ArgumentException.");
			} catch (ArgumentException) {}
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
	}
}
