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
	public class XmlTextWriterTests : Assertion
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
			AssertEquals ("<foo bar='baz' xmlns:abc='http://abc.def'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeAfter ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			xtw.WriteAttributeString("bar", "baz");
			AssertEquals ("<foo xmlns:abc='http://abc.def' bar='baz'", StringWriterText);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithNullInNamespaceParam ()
		{
			xtw.WriteAttributeString ("xmlns", null, "http://abc.def");
			AssertEquals ("xmlns='http://abc.def'", StringWriterText);
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
			AssertEquals ("xmlns:abc='http://abc.def'", StringWriterText);
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
			try {
				xtw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
				// This should not be allowed, even though MS.NET doesn't treat as an error.
				// See http://www.w3.org/TR/REC-xml-names/ Namespace Constraint: Prefix Declared
				Fail ("any prefix which name starts from \"xml\" must not be allowed.");
			}
 			catch (ArgumentException) {}
			xtw.WriteAttributeString ("", "xmlns", null, "http://abc.def");
		}

		[Test]
		public void AttributeWriteAttributeString ()
		{
			xtw.WriteStartElement ("foo");

			xtw.WriteAttributeString ("foo", "bar");
			AssertEquals ("<foo foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("bar", "");
			AssertEquals ("<foo foo='bar' bar=''", StringWriterText);

			xtw.WriteAttributeString ("baz", null);
			AssertEquals ("<foo foo='bar' bar='' baz=''", StringWriterText);

			xtw.WriteAttributeString ("hoge", "a\nb");
			AssertEquals ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b'", StringWriterText);

			xtw.WriteAttributeString ("fuga", " a\t\r\nb\t");
			AssertEquals ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b' fuga=' a\t&#xD;&#xA;b\t'", StringWriterText);

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString ("", "quux");
//				AssertEquals ("<foo foo='bar' bar='' baz='' ='quux'", StringWriterText);
				Fail ("empty name not allowed.");
			} catch (Exception) {
			}

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xtw.WriteAttributeString (null, "quuux");
//				AssertEquals ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", StringWriterText);
				Fail ("null name not allowed.");
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
			AssertEquals ("foo='bar'", StringWriterText);

			xtw.WriteAttributeString ("baz", "quux");
			AssertEquals ("foo='bar' baz='quux'", StringWriterText);
		}

		[Test]
		public void CDataValid ()
		{
			xtw.WriteCData ("foo");
			AssertEquals ("WriteCData had incorrect output.", "<![CDATA[foo]]>", StringWriterText);
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
			AssertEquals ("Close didn't write out end elements properly.", "<foo><bar><baz /></bar></foo>",	StringWriterText);
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
				Fail ("WriteCData after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteComment ("foo");
				Fail ("WriteComment after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteProcessingInstruction ("foo", "bar");
				Fail ("WriteProcessingInstruction after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteStartElement ("foo", "bar", "baz");
				Fail ("WriteStartElement after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try 
			{
				xtw.WriteAttributeString ("foo", "bar");
				Fail ("WriteAttributeString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) 
			{
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xtw.WriteString ("foo");
				Fail ("WriteString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				AssertEquals ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}
		}

		[Test]
		public void CommentValid ()
		{
			xtw.WriteComment ("foo");
			AssertEquals ("WriteComment had incorrect output.", "<!--foo-->", StringWriterText);
		}

		[Test]
		public void CommentInvalid ()
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

		[Test]
		public void ConstructorsAndBaseStream ()
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

		[Test]
		public void DocumentStart ()
		{
			xtw.WriteStartDocument ();
			AssertEquals ("XmlDeclaration is incorrect.", "<?xml version='1.0' encoding='utf-16'?>", StringWriterText);

			try 
			{
				xtw.WriteStartDocument ();
				Fail("Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
//				AssertEquals ("Exception message is incorrect.",
//					"WriteStartDocument should be the first call.", e.Message);
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

		[Test]
		public void ElementAndAttributeSameXmlns ()
		{
			xtw.WriteStartElement ("ped", "foo", "urn:foo");
			xtw.WriteStartAttribute ("ped", "foo", "urn:foo");
			xtw.WriteEndElement ();
			AssertEquals ("<ped:foo ped:foo='' xmlns:ped='urn:foo' />", StringWriterText);
		}

		[Test]
		public void ElementXmlnsNeedEscape ()
		{
			xtw.WriteStartElement ("test", "foo", "'");
			xtw.WriteEndElement ();
			// MS.NET fails this case.
			AssertEquals ("<test:foo xmlns:test='&apos;' />", StringWriterText);
		}

		[Test]
		public void ElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("Incorrect output.", "<foo />", StringWriterText);
		}

		[Test]
		public void ElementWriteElementString ()
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

		[Test]
		public void FormattingTest ()
		{
			xtw.Formatting = Formatting.Indented;
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteElementString ("bar", "");
			xtw.Close ();
 			AssertEquals (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}  <bar />{0}</foo>", Environment.NewLine), StringWriterText);
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
 			AssertEquals (String.Format ("<?xml version='1.0' encoding='utf-16'?>{0}<foo>{0}xx<bar>{0}xxxx<baz />{0}xx</bar>{0}</foo>", Environment.NewLine), StringWriterText);
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
 			AssertEquals (String.Format ("<ol>{0}  <li>The big <b>E</b><i>lephant</i> walks slowly.</li>{0}</ol>", Environment.NewLine), StringWriterText);
		}

		[Test]
		public void LookupPrefix ()
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

			AssertEquals ("<foo bar='baz' a='b' c='d' e='f' g='h'", StringWriterText);

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

		[Test]
		public void NamespacesNoNamespaceClearsDefaultNamespace ()
		{
			xtw.WriteStartElement(String.Empty, "foo", "http://netsack.com/");
			xtw.WriteStartElement(String.Empty, "bar", String.Empty);
			xtw.WriteElementString("baz", String.Empty, String.Empty);
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			AssertEquals ("XmlTextWriter is incorrectly outputting namespaces.",
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
			AssertEquals ("XmlTextWriter is incorrectly outputting prefixes.",
				"<foo:bar xmlns:foo='http://netsack.com/'><foo:baz><foo:qux /></foo:baz></foo:bar>", StringWriterText);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
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
			try 
			{
				xtw.Namespaces = false;
				Fail ("Expected an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {}
			AssertEquals (true, xtw.Namespaces);
		}

		[Test]
		public void ProcessingInstructionValid ()
		{
			xtw.WriteProcessingInstruction("foo", "bar");
			AssertEquals ("WriteProcessingInstruction had incorrect output.", "<?foo bar?>", StringWriterText);
		}

		[Test]
		public void ProcessingInstructionInvalid ()
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

		[Test]
		public void QuoteCharDoubleQuote ()
		{
			xtw.QuoteChar = '"';

			// version, encoding, standalone
			xtw.WriteStartDocument (true);
			
			// namespace declaration
			xtw.WriteElementString ("foo", "http://netsack.com", "bar");

			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><foo xmlns=\"http://netsack.com\">bar</foo>", StringWriterText);


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

		[Test]
		public void WriteBinHex ()
		{
			byte [] bytes = new byte [] {4,14,34, 54,94,114, 134,194,255, 0,5};
			xtw.WriteBinHex (bytes, 0, 11);
			AssertEquals ("040E22365E7286C2FF0005", StringWriterText);
		}

		[Test]
		public void WriteCharEntity ()
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

		[Test]
		public void WriteEndElement ()
		{
			try {
				xtw.WriteEndElement ();
				Fail ("Should have thrown an InvalidOperationException.");
			} catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
//				AssertEquals ("Exception message is incorrect.", "There was no XML start tag open.", e.Message);
			}

			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("<foo />", StringWriterText);

			xtw.WriteStartElement ("bar");
			xtw.WriteStartAttribute ("baz", null);
			xtw.WriteEndElement ();
			AssertEquals ("<foo /><bar baz='' />", StringWriterText);
		}

		[Test]
		public void FullEndElement ()
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

		[Test]
		public void WriteQualifiedName ()
		{
			xtw.WriteStartElement (null, "test", null);
			xtw.WriteAttributeString ("xmlns", "me", null, "http://localhost/");
			xtw.WriteQualifiedName ("bob", "http://localhost/");
			xtw.WriteEndElement ();

			AssertEquals ("<test xmlns:me='http://localhost/'>me:bob</test>", StringWriterText);
		}

		[Test]
		public void WriteRaw ()
		{
			xtw.WriteRaw("&<>\"'");
			AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw(null);
			AssertEquals ("&<>\"'", StringWriterText);

			xtw.WriteRaw("");
			AssertEquals ("&<>\"'", StringWriterText);
		}

		[Test]
		public void WriteRawInvalidInAttribute ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteStartAttribute ("bar", null);
			xtw.WriteRaw ("&<>\"'");
			xtw.WriteEndAttribute ();
			xtw.WriteEndElement ();
			AssertEquals ("<foo bar='&<>\"'' />", StringWriterText);
		}

		[Test]
		public void WriteStateTest ()
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
			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo bar='&amp;&lt;&gt;'", StringWriterText);
		}

		[Test]
		public void WriteAttributeStringSingleQuoteChar()
		{
			// When QuoteChar is single quote then replaces single quotes within attributes
			// but not double quotes.
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "\"baz\"");
			xtw.WriteAttributeString ("quux", "'baz'");
			AssertEquals ("<foo bar='\"baz\"' quux='&apos;baz&apos;'", StringWriterText);
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
			AssertEquals ("<foo bar=\"&quot;baz&quot;\" quux=\"'baz'\"", StringWriterText);
		}

		[Test]
		public void WriteStringWithEntities()
		{
			// Testing element values
			xtw.QuoteChar = '\'';
			xtw.WriteElementString ("foo", "&<>\"'");
			AssertEquals ("<foo>&amp;&lt;&gt;\"'</foo>", StringWriterText);
		}

		[Test]
		public void XmlLang ()
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
		[Test]
		public void XmlSpaceTest ()
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

		[Test]
		public void XmlSpaceRaw ()
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
			AssertEquals("#WriteAttributes.XmlDecl.1", "version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"", sw.ToString().Trim());

			sb.Remove(0, sb.Length);	// init
			ctx = new XmlParserContext(doc.NameTable, new XmlNamespaceManager(doc.NameTable), "", XmlSpace.Default);
			xtr = new XmlTextReader("<?xml version='1.0'		 standalone='no'?><root a1='A' b2='B' c3='C'><foo><bar /></foo></root>", XmlNodeType.Document, ctx);
			xtr.Read();	// read XMLDecl
			AssertEquals (XmlNodeType.XmlDeclaration, xtr.NodeType);
			sw = new StringWriter ();
			wr = new XmlTextWriter (sw);

			// This block raises an error on MS.NET 1.0.
			wr.WriteAttributes(xtr, false);
			// This method don't always have to take this double-quoted style...
			AssertEquals("#WriteAttributes.XmlDecl.2", "version=\"1.0\" standalone=\"no\"", sw.ToString().Trim());

			sw = new StringWriter ();
			wr = new XmlTextWriter (sw);
			sb.Remove(0, sb.Length);	// init

			xtr.Read();	// read root
			AssertEquals (XmlNodeType.Element, xtr.NodeType);
			wr.WriteStartElement(xtr.LocalName, xtr.NamespaceURI);
			wr.WriteAttributes(xtr, false);
			wr.WriteEndElement();
			wr.Close();
			// This method don't always have to take this double-quoted style...
			AssertEquals("#WriteAttributes.Element", "<root a1=\"A\" b2=\"B\" c3=\"C\" />", sw.ToString().Trim());
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
			AssertEquals ("<a>\n\t<b>\n\t</b>\n</a>\n", StringWriterText);
		}

		[Test]
		public void FlushDoesntCloseTag ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "baz");
			xtw.Flush ();
			AssertEquals ("<foo bar='baz'", StringWriterText);
		}

		[Test]
		public void WriteWhitespaceClosesTag ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("bar", "baz");
			xtw.WriteWhitespace (" ");
			AssertEquals ("<foo bar='baz'> ", StringWriterText);
		}

		[Test]
		public void DontOutputMultipleXmlns ()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<a xmlns:dt=\"b\" dt:dt=\"c\"/>");
			XmlDocument doc2 = new XmlDocument();
			doc2.LoadXml(doc.InnerXml);
			AssertEquals ("<a xmlns:dt=\"b\" dt:dt=\"c\" />",
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
			AssertEquals (xml.Replace ('\'', '"'), doc2.OuterXml);
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration ()
		{
			xtw.WriteStartDocument ();
			xtw.WriteStartElement ("foo");
			xtw.WriteAttributeString ("xmlns", "probe");
			AssertEquals (String.Empty, xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement ("b");
			AssertEquals (String.Empty, xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 xmlns='' /></b></foo>", StringWriterText);
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration2 ()
		{
			xtw.WriteStartDocument ();
			// IMPORTANT DIFFERENCE!! ns = "", not null
			xtw.WriteStartElement ("foo", "");
			xtw.WriteAttributeString ("xmlns", "probe");
			AssertNull (xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement ("b");
			AssertNull (xtw.LookupPrefix ("probe"));
			xtw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xtw.WriteEndElement (); // b2
			xtw.WriteEndElement (); // b
			xtw.WriteEndElement (); // foo
			xtw.WriteEndDocument ();
			xtw.Close ();

			AssertEquals ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 /></b></foo>", StringWriterText);
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

			AssertEquals ("<docelem xmlns='a-namespace'><hola xmlns='' /></docelem>", StringWriterText);
		}

		[Test]
		public void WriteAttributeTakePrecedenceOnXmlns ()
		{
			xtw.WriteStartElement ("root", "urn:foo");
			xtw.WriteAttributeString ("xmlns", "urn:bar");
			xtw.WriteEndElement ();
			xtw.Close ();
			AssertEquals ("<root xmlns='urn:bar' />", StringWriterText);
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
			AssertNull (xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("root");
			AssertNull (xtw.LookupPrefix ("urn:foo"));
			xtw.WriteAttributeString ("xmlns", "urn:foo");
			// Surprisingly to say, it is ignored!!
			AssertEquals (String.Empty, xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("hoge");
			// (still after flushing previous start element.)
			AssertEquals (String.Empty, xtw.LookupPrefix ("urn:foo"));
			xtw.WriteStartElement ("fuga", "urn:foo");
			// Is this testing on the correct way? Yes, here it is.
			AssertEquals (String.Empty, xtw.LookupPrefix ("urn:foo"));
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
			AssertEquals (WriteState.Prolog, xtw.WriteState);
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
			AssertEquals (@"<root>_  <test>test<foo></foo>string</test>_  <test>string</test>_</root>", sw.ToString ());
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
	}
}
