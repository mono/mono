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
			Assertion.AssertEquals ("<foo bar='baz' xmlns:abc='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeAfter ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			xtw.WriteAttributeString("bar", "baz");
			Assertion.AssertEquals ("<foo xmlns:abc='http://abc.def' bar='baz'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", null, "http://abc.def");
			Assertion.AssertEquals ("xmlns='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithTextInNamespaceParam ()
		{
			try 
			{
				xtw.WriteAttributeString ("xmlns", "http://somenamespace.com", "http://abc.def");
			} 
			catch (ArgumentException) {}
		}

		[Test]
		public void AttributeNamespacesWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			Assertion.AssertEquals ("xmlns:abc='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesWithTextInNamespaceParam ()
		{
			try {
				xtw.WriteAttributeString ("xmlns", "abc", "http://somenamespace.com", "http://abc.def");
			} catch (ArgumentException) {}
		}

		[Test]
		public void AttributeNamespacesXmlnsXmlns ()
		{
			xtw.WriteStartElement ("foo");
			try 
			{
				xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
				Assertion.Fail ("any prefix which name starts from \"xml\" must not be allowed.");
			} 
 			catch (ArgumentException e) {}
		}

		[Test]
		public void AttributeWriteAttributeString ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("foo", "bar");
			Assertion.AssertEquals ("<foo foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("bar", "");
			Assertion.AssertEquals ("<foo foo='bar' bar=''", StringWriterText);

			xtw.WriteAttributeString ("baz", null);
			Assertion.AssertEquals ("<foo foo='bar' bar='' baz=''", StringWriterText);

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString ("", "quux");
//				Assertion.AssertEquals ("<foo foo='bar' bar='' baz='' ='quux'", StringWriterText);
				Assertion.Fail ("empty name not allowed.");
			} catch (Exception) {
			}

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString (null, "quuux");
//				Assertion.AssertEquals ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", StringWriterText);
				Assertion.Fail ("null name not allowed.");
			} catch (Exception) {
			}
		}

		[Test]
		public void AttributeWriteAttributeStringNotInsideOpenStartElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteString ("bar");
			
			try 
			{
				xtw.WriteAttributeString ("baz", "quux");
				Assertion.Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}
		}

		[Test]
		public void AttributeWriteAttributeStringWithoutParentElement ()
		{
			xtw.WriteAttributeString ("foo", "bar");
			Assertion.AssertEquals ("foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("baz", "quux");
			Assertion.AssertEquals ("foo='bar' baz='quux'", StringWriterText);
		}

		[Test]
		public void CDataValid ()
		{
			xtw.WriteCData ("foo");
			Assertion.AssertEquals ("WriteCData had incorrect output.", "<![CDATA[foo]]>", StringWriterText);
		}

		[Test]
		public void CDataInvalid ()
		{
			try {
				xtw.WriteCData("foo]]>bar");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		[Test]
		public void CloseOpenElements ()
		{
			xtw.WriteStartElement("foo");
			xtw.WriteStartElement("bar");
			xtw.WriteStartElement("baz");
			xtw.Close();
			Assertion.AssertEquals ("Close didn't write out end elements properly.", "<foo><bar><baz /></bar></foo>",	StringWriterText);
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
				Assertion.Fail ("WriteCData after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteComment ("foo");
				Assertion.Fail ("WriteComment after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteProcessingInstruction ("foo", "bar");
				Assertion.Fail ("WriteProcessingInstruction after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteStartElement ("foo", "bar", "baz");
				Assertion.Fail ("WriteStartElement after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try 
			{
				xtw.WriteAttributeString ("foo", "bar");
				Assertion.Fail ("WriteAttributeString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) 
			{
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteString ("foo");
				Assertion.Fail ("WriteString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}
		}

		[Test]
		public void CommentValid ()
		{
			xtw.WriteComment ("foo");
			Assertion.AssertEquals ("WriteComment had incorrect output.", "<!--foo-->", StringWriterText);
		}

		[Test]
		public void CommentInvalid ()
		{
			try {
				xtw.WriteComment("foo-");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try {
				xtw.WriteComment("foo-->bar");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		[Test]
		public void ConstructorsAndBaseStream ()
		{
			Assertion.Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (null, this.xtw.BaseStream));

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
			Assertion.AssertEquals (expectedXmlDeclaration, actualXmlDeclaration);
			Assertion.Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UnicodeEncoding ());
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.Unicode);
			Assertion.AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, new UTF8Encoding ());
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assertion.AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-8\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument ();
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assertion.AssertEquals ("<?xml version=\"1.0\"?>", sr.ReadToEnd ());

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, null);
			xtw.WriteStartDocument (true);
			xtw.Flush ();
			ms.Seek (0, SeekOrigin.Begin);
			sr = new StreamReader (ms, Encoding.UTF8);
			Assertion.AssertEquals ("<?xml version=\"1.0\" standalone=\"yes\"?>", sr.ReadToEnd ());
			Assertion.Assert ("BaseStream property returned wrong value.", Object.ReferenceEquals (ms, xtw.BaseStream));
		}

		[Test]
		public void DocumentStart ()
		{
			xtw.WriteStartDocument ();
			Assertion.AssertEquals ("XmlDeclaration is incorrect.", "<?xml version='1.0' encoding='utf-16'?>", StringWriterText);

			try 
			{
				xtw.WriteStartDocument ();
				Assertion.Fail("Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.",
					"WriteStartDocument should be the first call.", e.Message);
			}

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (true);
			Assertion.AssertEquals ("<?xml version='1.0' encoding='utf-16' standalone='yes'?>", StringWriterText);

			xtw = new XmlTextWriter (sw = new StringWriter ());
			xtw.QuoteChar = '\'';
			xtw.WriteStartDocument (false);
			Assertion.AssertEquals ("<?xml version='1.0' encoding='utf-16' standalone='no'?>", StringWriterText);
		}

		[Test]
		public void ElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("Incorrect output.", "<foo />", StringWriterText);
		}

		[Test]
		public void ElementWriteElementString ()
		{
			xtw.WriteElementString ("foo", "bar");
			Assertion.AssertEquals ("WriteElementString has incorrect output.", "<foo>bar</foo>", StringWriterText);

			xtw.WriteElementString ("baz", "");
			Assertion.AssertEquals ("<foo>bar</foo><baz />", StringWriterText);

			xtw.WriteElementString ("quux", null);
			Assertion.AssertEquals ("<foo>bar</foo><baz /><quux />", StringWriterText);

			xtw.WriteElementString ("", "quuux");
			Assertion.AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</>", StringWriterText);

			xtw.WriteElementString (null, "quuuux");
			Assertion.AssertEquals ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", StringWriterText);
		}

		[Test]
		public void FormattingTest ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteElementString ("bar", "");
			xtw.Close ();
 			Assertion.AssertEquals (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}  <bar />{0}</foo>", Environment.NewLine), StringWriterText);
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
 			Assertion.AssertEquals (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}xx<bar>{0}xxxx<baz />{0}xx</bar>{0}</foo>", Environment.NewLine), StringWriterText);
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
 			Assertion.AssertEquals (String.Format ("<ol>{0}  <li>The big <b>E</b><i>lephant</i> walks slowly.</li>{0}</ol>", Environment.NewLine), StringWriterText);
		}

		[Test]
		public void LookupPrefix ()
		{
			xtw.WriteStartElement ("root");

			xtw.WriteStartElement ("one");
			xtw.WriteAttributeString ("xmlns", "foo", null, "http://abc.def");
			xtw.WriteAttributeString ("xmlns", "bar", null, "http://ghi.jkl");
			Assertion.AssertEquals ("foo", xtw.LookupPrefix ("http://abc.def"));
			Assertion.AssertEquals ("bar", xtw.LookupPrefix ("http://ghi.jkl"));
			xtw.WriteEndElement ();

			xtw.WriteStartElement ("two");
			xtw.WriteAttributeString ("xmlns", "baz", null, "http://mno.pqr");
			xtw.WriteString("quux");
			Assertion.AssertEquals ("baz", xtw.LookupPrefix ("http://mno.pqr"));
			Assertion.AssertNull (xtw.LookupPrefix ("http://abc.def"));
			Assertion.AssertNull (xtw.LookupPrefix ("http://ghi.jkl"));

			Assertion.AssertNull (xtw.LookupPrefix ("http://bogus"));
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

			Assertion.AssertEquals ("<foo bar='baz' a='b' c='d' e='f' g='h'", StringWriterText);

			// These should throw ArgumentException because they pass in a
			// namespace when Namespaces = false.
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

			Assertion.AssertEquals ("<foo>bar</foo><baz><quux><quuux><a><b><c><d", StringWriterText);

			// These should throw ArgumentException because they pass in a
			// namespace when Namespaces = false.
			try {
				xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "http://netsack.com/");
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", null);
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "bar", "");
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteStartElement ("foo", "", "");
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}
		}

		[Test]
		public void NamespacesNoNamespaceClearsDefaultNamespace ()
		{
			xtw.WriteStartElement(String.Empty, "foo", "http://netsack.com/");
			xtw.WriteStartElement(String.Empty, "bar", String.Empty);
			xtw.WriteElementString("baz", String.Empty, String.Empty);
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			Assertion.AssertEquals ("XmlTextWriter is incorrectly outputting namespaces.",
				"<foo xmlns='http://netsack.com/'><bar xmlns=''><baz /></bar></foo>", StringWriterText);
		}

		[Test]
		public void NamespacesPrefix ()
		{
			xtw.WriteStartElement ("foo", "bar", "http://netsack.com/");
			xtw.WriteStartElement ("foo", "baz", "http://netsack.com/");
			xtw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("XmlTextWriter is incorrectly outputting prefixes.",
				"<foo:bar xmlns:foo='http://netsack.com/'><foo:baz><foo:qux /></foo:baz></foo:bar>", StringWriterText);
		}

		[Test]
		public void NamespacesPrefixWithEmptyAndNullNamespace ()
		{
			try {
				xtw.WriteStartElement ("foo", "bar", "");
				Assertion.Fail ("Should have thrown an ArgumentException.");
			} catch (ArgumentException) {}

			try 
			{
				xtw.WriteStartElement ("foo", "bar", null);
				Assertion.Fail ("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) {}
		}

		[Test]
		public void NamespacesSettingWhenWriteStateNotStart ()
		{
			xtw.WriteStartElement ("foo");
			try 
			{
				xtw.Namespaces = false;
				Assertion.Fail ("Expected an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {}
			Assertion.AssertEquals (true, xtw.Namespaces);
		}

		[Test]
		public void ProcessingInstructionValid ()
		{
			xtw.WriteProcessingInstruction("foo", "bar");
			Assertion.AssertEquals ("WriteProcessingInstruction had incorrect output.", "<?foo bar?>", StringWriterText);
		}

		[Test]
		public void ProcessingInstructionInvalid ()
		{
			try 
			{
				xtw.WriteProcessingInstruction("fo?>o", "bar");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction("foo", "ba?>r");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction("", "bar");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try 
			{
				xtw.WriteProcessingInstruction(null, "bar");
				Assertion.Fail("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		[Test]
		public void QuoteCharDoubleQuote ()
		{
			xtw.QuoteChar = '"';

			// version, encoding, standalone
			xtw.WriteStartDocument (true);
			
			// namespace declaration
			xtw.WriteElementString ("foo", "http://netsack.com", "bar");

			Assertion.AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><foo xmlns=\"http://netsack.com\">bar</foo>", StringWriterText);


		}

		[Test]
		public void QuoteCharInvalid ()
		{
			try {
				xtw.QuoteChar = 'x';
				Assertion.Fail ("Should have thrown an ArgumentException.");
			} catch (ArgumentException) {}
		}

		[Test]
		public void WriteBase64 ()
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] fooBar = encoding.GetBytes("foobar");
			xtw.WriteBase64 (fooBar, 0, 6);
			Assertion.AssertEquals("Zm9vYmFy", StringWriterText);

			try {
				xtw.WriteBase64 (fooBar, 3, 6);
				Assertion.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentException) {}

			try {
				xtw.WriteBase64 (fooBar, -1, 6);
				Assertion.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (fooBar, 3, -1);
				Assertion.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xtw.WriteBase64 (null, 0, 6);
				Assertion.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentNullException) {}
		}

		[Test]
		public void WriteCharEntity ()
		{
			xtw.WriteCharEntity ('a');
			Assertion.AssertEquals ("&#x61;", StringWriterText);

			xtw.WriteCharEntity ('A');
			Assertion.AssertEquals ("&#x61;&#x41;", StringWriterText);

			xtw.WriteCharEntity ('1');
			Assertion.AssertEquals ("&#x61;&#x41;&#x31;", StringWriterText);

			xtw.WriteCharEntity ('K');
			Assertion.AssertEquals ("&#x61;&#x41;&#x31;&#x4B;", StringWriterText);

			try {
				xtw.WriteCharEntity ((char)0xd800);
			} catch (ArgumentException) {}
		}

		[Test]
		public void WriteEndAttribute ()
		{
			try 
			{
				xtw.WriteEndAttribute ();
				Assertion.Fail ("Should have thrown an InvalidOperationException.");
			}
			catch (InvalidOperationException) {}
		}

		[Test]
		public void WriteEndDocument ()
		{
			try {
				xtw.WriteEndDocument ();
				Assertion.Fail ("Expected an ArgumentException.");
			} catch (ArgumentException) {}

			xtw.WriteStartDocument ();

			try 
			{
				xtw.WriteEndDocument ();
				Assertion.Fail ("Expected an ArgumentException.");
			} 
			catch (ArgumentException) {}

			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			Assertion.AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='", StringWriterText);

			xtw.WriteEndDocument ();
			Assertion.AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='' />", StringWriterText);
			Assertion.AssertEquals (WriteState.Start, xtw.WriteState);
		}

		[Test]
		public void WriteEndElement ()
		{
			try {
				xtw.WriteEndElement ();
				Assertion.Fail ("Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException e) {
				Assertion.AssertEquals ("Exception message is incorrect.", "There was no XML start tag open.", e.Message);
			}

			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("<foo />", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteStartAttribute ("baz", null);
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("<foo /><bar baz='' />", StringWriterText);
		}

		[Test]
		public void FullEndElement ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteFullEndElement ();
			Assertion.AssertEquals ("<foo></foo>", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("foo", "bar");
			xtw.WriteFullEndElement ();
			Assertion.AssertEquals ("<foo></foo><bar foo='bar'></bar>", StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteFullEndElement ();
			Assertion.AssertEquals ("<foo></foo><bar foo='bar'></bar><baz bar=''></baz>", StringWriterText);
		}

		[Test]
		public void WriteQualifiedName ()
		{
			xtw.WriteStartElement (null, "test", null);
			xtw.WriteAttributeString ("xmlns", "me", null, "http://localhost/");
			xtw.WriteQualifiedName ("bob", "http://localhost/");
			xtw.WriteEndElement ();

			Assertion.AssertEquals ("<test xmlns:me='http://localhost/'>me:bob</test>", StringWriterText);
		}

		[Test]
		public void WriteRaw ()
		{
			xtw.WriteRaw("&<>\"'");
			Assertion.AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw(null);
			Assertion.AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw("");
			Assertion.AssertEquals ("&<>\"'", StringWriterText);
		}

		[Test]
		public void WriteRawInvalidInAttribute ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteRaw ("&<>\"'");
			xtw.WriteEndAttribute ();
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("<foo bar='&<>\"'' />", StringWriterText);
		}

		[Test]
		public void WriteStateTest ()
		{
			Assertion.AssertEquals (WriteState.Start, xtw.WriteState);
			xtw.WriteStartDocument ();
			Assertion.AssertEquals (WriteState.Prolog, xtw.WriteState);
			xtw.WriteStartElement ("root");
			Assertion.AssertEquals (WriteState.Element, xtw.WriteState);
			xtw.WriteElementString ("foo", "bar");
			Assertion.AssertEquals (WriteState.Content, xtw.WriteState);
			xtw.Close ();
			Assertion.AssertEquals (WriteState.Closed, xtw.WriteState);
		}

		[Test]
		public void WriteString ()
		{
			xtw.WriteStartDocument ();
			try {
				xtw.WriteString("foo");
			} catch (InvalidOperationException) {}

			// Testing attribute values

			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "&<>");
			Assertion.AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='&amp;&lt;&gt;'", StringWriterText);
		}

		[Test]
		public void WriteAttributeStringSingleQuoteChar()
		{
			// When QuoteChar is single quote then replaces single quotes within attributes
			// but not double quotes.
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			Assertion.AssertEquals ("<foo bar='\"baz\"' quux='&apos;baz&apos;'", StringWriterText);
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
			Assertion.AssertEquals ("<foo bar=\"&quot;baz&quot;\" quux=\"'baz'\"", StringWriterText);
		}

		[Test]
		public void WriteStringWithEntities()
		{
			// Testing element values
			xtw.QuoteChar = '\'';
			xtw.WriteElementString ("foo", "&<>\"'");
			Assertion.AssertEquals ("<foo>&amp;&lt;&gt;\"'</foo>", StringWriterText);
		}

		[Test]
		public void XmlLang ()
		{
			Assertion.AssertNull (xtw.XmlLang);
			
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xml", "lang", null, "langfoo");
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo'", StringWriterText);

			xtw.WriteAttributeString ("boo", "yah");
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'", StringWriterText);
			
			xtw.WriteElementString("bar", "baz");
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>", StringWriterText);
			
			xtw.WriteString("baz");
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz", StringWriterText);
			
			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "lang", null);
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteString("langbar");
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			Assertion.AssertEquals ("langbar", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'", StringWriterText);

			// check if xml:lang repeats output even if same as current scope.
			xtw.WriteStartElement ("joe");
			xtw.WriteAttributeString ("xml", "lang", null, "langbar");
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'", StringWriterText);

			
			xtw.WriteElementString ("quuux", "squonk");
			Assertion.AssertEquals ("langbar", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux>", StringWriterText);

			xtw.WriteEndElement ();
			xtw.WriteEndElement ();
			Assertion.AssertEquals ("langfoo", xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux>", StringWriterText);
			
			xtw.WriteEndElement ();
			Assertion.AssertNull (xtw.XmlLang);
			Assertion.AssertEquals ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux></foo>", StringWriterText);
			
			xtw.Close ();
			Assertion.AssertNull (xtw.XmlLang);
		}

		// TODO: test operational aspects
		[Test]
		public void XmlSpaceTest ()
		{
			xtw.WriteStartElement ("foo");
			Assertion.AssertEquals (XmlSpace.None, xtw.XmlSpace);

			xtw.WriteStartElement ("bar");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo><bar xml:space='preserve'",	StringWriterText);

			xtw.WriteStartElement ("baz");
			xtw.WriteAttributeString ("xml", "space", null, "preserve");
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'", StringWriterText);

			xtw.WriteStartElement ("quux");
			xtw.WriteStartAttribute ("xml", "space", null);
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);

			xtw.WriteString ("default");
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", StringWriterText);
			
			xtw.WriteEndAttribute ();
			Assertion.AssertEquals (XmlSpace.Default, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='default'", StringWriterText);

			xtw.WriteEndElement ();
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			Assertion.AssertEquals (XmlSpace.Preserve, xtw.XmlSpace);
			xtw.WriteEndElement ();
			Assertion.AssertEquals (XmlSpace.None, xtw.XmlSpace);

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

		[Test]
		public void XmlSpaceRaw ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("xml", "space", null);
			Assertion.AssertEquals (XmlSpace.None, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo xml:space='", StringWriterText);

			xtw.WriteString ("default");
			Assertion.AssertEquals (XmlSpace.None, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo xml:space='", StringWriterText);

			xtw.WriteEndAttribute ();
			Assertion.AssertEquals (XmlSpace.Default, xtw.XmlSpace);
			Assertion.AssertEquals ("<foo xml:space='default'", StringWriterText);
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
			Assertion.AssertEquals("#WriteAttributes.XmlDecl.1", "version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"", sw.ToString().Trim());

			sb.Remove(0, sb.Length);	// init
			ctx = new XmlParserContext(doc.NameTable, new XmlNamespaceManager(doc.NameTable), "", XmlSpace.Default);
			xtr = new XmlTextReader("<?xml version='1.0'		 standalone='no'?><root a1='A' b2='B' c3='C'><foo><bar /></foo></root>", XmlNodeType.Document, ctx);
			xtr.Read();	// read XMLDecl
			wr.WriteAttributes(xtr, false);
			// This method don't always have to take this double-quoted style...
			Assertion.AssertEquals("#WriteAttributes.XmlDecl.2", "version=\"1.0\" standalone=\"no\"", sw.ToString().Trim());

			sb.Remove(0, sb.Length);	// init
			xtr.Read();	// read root
			wr.WriteStartElement(xtr.LocalName, xtr.Value);
			wr.WriteAttributes(xtr, false);
			wr.WriteEndElement();
			wr.Close();
			// This method don't always have to take this double-quoted style...
			Assertion.AssertEquals("#WriteAttributes.Element", "<root a1=\"A\" b2=\"B\" c3=\"C\" />", sw.ToString().Trim());
		}
	}
}
