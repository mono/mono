//
// XmlSimpleDictionaryWriterTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com

//
// Copied from System.XML/Test/System.Xml/XmlTextWriterTests.cs
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSimpleDictionaryWriterTest
	{
		[Test]
		public void WriteXmlnsAttribute ()
		{
			xw.WriteStartElement ("l1");
			xw.WriteXmlAttribute ("lang", "ja");
			xw.WriteXmlnsAttribute ("f", "urn:foo");
			xw.WriteStartElement ("l2");
			xw.WriteXmlnsAttribute ("", "");
			xw.WriteEndElement ();
			xw.WriteStartElement ("l2");
			xw.WriteXmlnsAttribute ("", "urn:bar");
			xw.WriteEndElement ();
			xw.WriteEndElement ();
			xw.Flush ();
			Assert.AreEqual ("<l1 xml:lang='ja' xmlns:f='urn:foo'><l2 xmlns='' /><l2 xmlns='urn:bar' /></l1>", Output);
		}

		[Test]
		public void WriteXmlnsAttributeNullPrefix ()
		{
			xw.WriteStartElement ("root", "urn:x");
			xw.WriteXmlnsAttribute (null, "urn:foo");
			xw.WriteEndElement ();
			xw.Close ();
			Assert.AreEqual ("<root xmlns:d1p1='urn:foo' xmlns='urn:x' />", Output);
		}

		#region Copied from XmlTextWriterTests.cs

		StringWriter sw;
		XmlDictionaryWriter xw;

		[SetUp]
		public void GetReady ()
		{
			sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xw = XmlDictionaryWriter.CreateDictionaryWriter (xtw);
		}

		private string Output
		{
			get { return sw.ToString (); }
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeBefore ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString("bar", "baz");
			xw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			Assert.AreEqual ("<foo bar='baz' xmlns:abc='http://abc.def'", Output);
		}

		[Test]
		public void AttributeNamespacesNonNamespaceAttributeAfter ()
		{
			xw.WriteStartElement ("foo");

			xw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			xw.WriteAttributeString("bar", "baz");
			Assert.AreEqual ("<foo xmlns:abc='http://abc.def' bar='baz'", Output);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithNullInNamespaceParam ()
		{
			xw.WriteAttributeString ("xmlns", null, "http://abc.def");
			Assert.AreEqual ("xmlns='http://abc.def'", Output);
		}

		[Test]
		public void AttributeNamespacesThreeParamWithTextInNamespaceParam ()
		{
			try 
			{
				xw.WriteAttributeString ("xmlns", "http://somenamespace.com", "http://abc.def");
			} 
			catch (ArgumentException) {}
		}

		[Test]
		public void AttributeNamespacesWithNullInNamespaceParam ()
		{
			xw.WriteAttributeString ("xmlns", "abc", null, "http://abc.def");
			Assert.AreEqual ("xmlns:abc='http://abc.def'", Output);
		}

		[Test]
		public void AttributeNamespacesWithTextInNamespaceParam ()
		{
			try {
				xw.WriteAttributeString ("xmlns", "abc", "http://somenamespace.com", "http://abc.def");
			} catch (ArgumentException) {}
		}

		[Test]
		public void AttributeNamespacesXmlnsXmlns ()
		{
			xw.WriteStartElement ("foo");
			// When namespaceURI argument is null, constraints by
			// namespaces in XML are ignored.
			xw.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
			xw.WriteAttributeString ("", "xmlns", null, "http://abc.def");
		}

		[Test]
		public void AttributeWriteAttributeString ()
		{
			xw.WriteStartElement ("foo");

			xw.WriteAttributeString ("foo", "bar");
			Assert.AreEqual ("<foo foo='bar'", Output);

			xw.WriteAttributeString ("bar", "");
			Assert.AreEqual ("<foo foo='bar' bar=''", Output);

			xw.WriteAttributeString ("baz", null);
			Assert.AreEqual ("<foo foo='bar' bar='' baz=''", Output);

			xw.WriteAttributeString ("hoge", "a\nb");
			Assert.AreEqual ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b'", Output);

			xw.WriteAttributeString ("fuga", " a\t\r\nb\t");
			Assert.AreEqual ("<foo foo='bar' bar='' baz='' hoge='a&#xA;b' fuga=' a\t&#xD;&#xA;b\t'", Output);

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xw.WriteAttributeString ("", "quux");
//				Assert.AreEqual ("<foo foo='bar' bar='' baz='' ='quux'", Output);
				Assert.Fail ("empty name not allowed.");
			} catch (Exception) {
			}

			try {
				// Why does this pass Microsoft?
				// Anyway, Mono should not allow such code.
				xw.WriteAttributeString (null, "quuux");
//				Assert.AreEqual ("<foo foo='bar' bar='' baz='' ='quux' ='quuux'", Output);
				Assert.Fail ("null name not allowed.");
			} catch (Exception) {
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AttributeWriteAttributeStringNotInsideOpenStartElement ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteString ("bar");
			
			xw.WriteAttributeString ("baz", "quux");
		}

		[Test]
		public void AttributeWriteAttributeStringWithoutParentElement ()
		{
			xw.WriteAttributeString ("foo", "bar");
			Assert.AreEqual ("foo='bar'", Output);

			xw.WriteAttributeString ("baz", "quux");
			Assert.AreEqual ("foo='bar' baz='quux'", Output);
		}

		[Test]
		public void CDataValid ()
		{
			xw.WriteCData ("foo");
			Assert.AreEqual ("<![CDATA[foo]]>", Output,
				"WriteCData had incorrect output.");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CDataInvalid ()
		{
			xw.WriteCData("foo]]>bar");
		}
		
		[Test]
		public void CloseOpenElements ()
		{
			xw.WriteStartElement("foo");
			xw.WriteStartElement("bar");
			xw.WriteStartElement("baz");
			xw.Close();
			Assert.AreEqual ("<foo><bar><baz /></bar></foo>",	Output,
				"Close didn't write out end elements properly.");
		}

		[Test]
		public void CloseWriteAfter ()
		{
			xw.WriteElementString ("foo", "bar");
			xw.Close ();

			// WriteEndElement and WriteStartDocument aren't tested here because
			// they will always throw different exceptions besides 'The Writer is closed.'
			// and there are already tests for those exceptions.

			try {
				xw.WriteCData ("foo");
				Assert.Fail ("WriteCData after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xw.WriteComment ("foo");
				Assert.Fail ("WriteComment after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xw.WriteProcessingInstruction ("foo", "bar");
				Assert.Fail ("WriteProcessingInstruction after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xw.WriteStartElement ("foo", "bar", "baz");
				Assert.Fail ("WriteStartElement after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try 
			{
				xw.WriteAttributeString ("foo", "bar");
				Assert.Fail ("WriteAttributeString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) 
			{
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}

			try {
				xw.WriteString ("foo");
				Assert.Fail ("WriteString after Close Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
//				Assert.AreEqual ("Exception message is incorrect.", "The Writer is closed.", e.Message);
			}
		}

		[Test]
		public void CommentValid ()
		{
			xw.WriteComment ("foo");
			Assert.AreEqual ("<!--foo-->", Output,
				"WriteComment had incorrect output.");
		}

		[Test]
		public void CommentInvalid ()
		{
			try {
				xw.WriteComment("foo-");
				Assert.Fail ("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }

			try {
				xw.WriteComment("foo-->bar");
				Assert.Fail ("Should have thrown an ArgumentException.");
			} 
			catch (ArgumentException) { }
		}

		[Test]
		public void DocumentStart ()
		{
			xw.WriteStartDocument ();
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?>", Output,
				"XmlDeclaration is incorrect.");

			try 
			{
				xw.WriteStartDocument ();
				Assert.Fail ("Should have thrown an InvalidOperationException.");
			} 
			catch (InvalidOperationException) {
				// Don't rely on English message assertion.
				// It is enough to check an exception occurs.
//				Assert.AreEqual ("Exception message is incorrect.",
//					"WriteStartDocument should be the first call.", e.Message);
			}

			GetReady ();
			xw.WriteStartDocument (true);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16' standalone='yes'?>", Output);

			GetReady ();
			xw.WriteStartDocument (false);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16' standalone='no'?>", Output);
		}

		[Test]
		public void ElementAndAttributeSameXmlns ()
		{
			xw.WriteStartElement ("ped", "foo", "urn:foo");
			xw.WriteStartAttribute ("ped", "foo", "urn:foo");
			xw.WriteEndElement ();
			Assert.AreEqual ("<ped:foo ped:foo='' xmlns:ped='urn:foo' />", Output);
		}

		[Test]
		[Category ("NotDotNet")]
		public void ElementXmlnsNeedEscape ()
		{
			xw.WriteStartElement ("test", "foo", "'");
			xw.WriteEndElement ();
			// MS.NET output is : xmlns:test='''
			Assert.AreEqual ("<test:foo xmlns:test='&apos;' />", Output);
		}

		[Test]
		public void ElementEmpty ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteEndElement ();
			Assert.AreEqual ("<foo />", Output);
		}

		[Test]
		public void ElementWriteElementString ()
		{
			xw.WriteElementString ("foo", "bar");
			Assert.AreEqual ("<foo>bar</foo>", Output,
				"WriteElementString has incorrect output.");

			xw.WriteElementString ("baz", "");
			Assert.AreEqual ("<foo>bar</foo><baz />", Output);

			xw.WriteElementString ("quux", null);
			Assert.AreEqual ("<foo>bar</foo><baz /><quux />", Output);

			xw.WriteElementString ("", "quuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</>", Output);

			xw.WriteElementString (null, "quuuux");
			Assert.AreEqual ("<foo>bar</foo><baz /><quux /><>quuux</><>quuuux</>", Output);
		}

		[Test]
		public void LookupPrefix ()
		{
			xw.WriteStartElement ("root");

			xw.WriteStartElement ("one");
			xw.WriteAttributeString ("xmlns", "foo", null, "http://abc.def");
			xw.WriteAttributeString ("xmlns", "bar", null, "http://ghi.jkl");
			Assert.AreEqual ("foo", xw.LookupPrefix ("http://abc.def"));
			Assert.AreEqual ("bar", xw.LookupPrefix ("http://ghi.jkl"));
			xw.WriteEndElement ();

			xw.WriteStartElement ("two");
			xw.WriteAttributeString ("xmlns", "baz", null, "http://mno.pqr");
			xw.WriteString("quux");
			Assert.AreEqual ("baz", xw.LookupPrefix ("http://mno.pqr"));
			Assert.IsNull (xw.LookupPrefix ("http://abc.def"));
			Assert.IsNull (xw.LookupPrefix ("http://ghi.jkl"));

			Assert.IsNull (xw.LookupPrefix ("http://bogus"));
		}

		[Test]
		public void NamespacesNoNamespaceClearsDefaultNamespace ()
		{
			xw.WriteStartElement(String.Empty, "foo", "http://netsack.com/");
			xw.WriteStartElement(String.Empty, "bar", String.Empty);
			xw.WriteElementString("baz", String.Empty, String.Empty);
			xw.WriteEndElement();
			xw.WriteEndElement();
			Assert.AreEqual (
				"<foo xmlns='http://netsack.com/'><bar xmlns=''><baz /></bar></foo>",
				Output,
				"XmlTextWriter is incorrectly outputting namespaces.");
		}

		[Test]
		public void NamespacesPrefix ()
		{
			xw.WriteStartElement ("foo", "bar", "http://netsack.com/");
			xw.WriteStartElement ("foo", "baz", "http://netsack.com/");
			xw.WriteElementString ("qux", "http://netsack.com/", String.Empty);
			xw.WriteEndElement ();
			xw.WriteEndElement ();
			Assert.AreEqual ("<foo:bar xmlns:foo='http://netsack.com/'><foo:baz><foo:qux /></foo:baz></foo:bar>",
				Output,
				"XmlTextWriter is incorrectly outputting prefixes.");
		}

		[Test]
		// [ExpectedException (typeof (ArgumentException))]
		public void NamespacesPrefixWithEmptyAndNullNamespaceEmpty ()
		{
			xw.WriteStartElement ("foo", "bar", "");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NamespacesPrefixWithEmptyAndNullNamespaceNull ()
		{
			xw.WriteStartElement ("foo", "bar", null);
		}

		[Test]
		public void ProcessingInstructionValid ()
		{
			xw.WriteProcessingInstruction("foo", "bar");
			Assert.AreEqual ("<?foo bar?>", Output, "WriteProcessingInstruction had incorrect output.");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid1 ()
		{
			xw.WriteProcessingInstruction("fo?>o", "bar");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid2 ()
		{
			xw.WriteProcessingInstruction("foo", "ba?>r");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid3 ()
		{
			xw.WriteProcessingInstruction("", "bar");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ProcessingInstructionInvalid4 ()
		{
			xw.WriteProcessingInstruction(null, "bar");
		}

		[Test]
		public void WriteBase64 ()
		{
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] fooBar = encoding.GetBytes("foobar");
			xw.WriteBase64 (fooBar, 0, 6);
			Assert.AreEqual("Zm9vYmFy", Output);

			try {
				xw.WriteBase64 (fooBar, 3, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentException) {}

			try {
				xw.WriteBase64 (fooBar, -1, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xw.WriteBase64 (fooBar, 3, -1);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentOutOfRangeException) {}

			try {
				xw.WriteBase64 (null, 0, 6);
				Assert.Fail ("Expected an Argument Exception to be thrown.");
			} catch (ArgumentNullException) {}
		}

		[Test]
		public void WriteBinHex ()
		{
			byte [] bytes = new byte [] {4,14,34, 54,94,114, 134,194,255, 0,5};
			xw.WriteBinHex (bytes, 0, 11);
			Assert.AreEqual ("040E22365E7286C2FF0005", Output);
		}

		[Test]
		public void WriteCharEntity ()
		{
			xw.WriteCharEntity ('a');
			Assert.AreEqual ("&#x61;", Output);

			xw.WriteCharEntity ('A');
			Assert.AreEqual ("&#x61;&#x41;", Output);

			xw.WriteCharEntity ('1');
			Assert.AreEqual ("&#x61;&#x41;&#x31;", Output);

			xw.WriteCharEntity ('K');
			Assert.AreEqual ("&#x61;&#x41;&#x31;&#x4B;", Output);

			try {
				xw.WriteCharEntity ((char)0xd800);
			} catch (ArgumentException) {}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteEndAttribute ()
		{
			xw.WriteEndAttribute ();
		}

		[Test]
		public void WriteEndDocument ()
		{
			try {
				xw.WriteEndDocument ();
				Assert.Fail ("Expected an Exception.");
			// in .NET 2.0 it is InvalidOperationException.
			// in .NET 1,1 it is ArgumentException.
			} catch (Exception) {}
		}

		[Test]
		public void WriteEndDocument2 ()
		{
			xw.WriteStartDocument ();
			try 
			{
				xw.WriteEndDocument ();
				Assert.Fail ("Expected an Exception.");
			// in .NET 2.0 it is InvalidOperationException.
			// in .NET 1,1 it is ArgumentException.
			} catch (Exception) {}
		}

		[Test]
		public void WriteEndDocument3 ()
		{
			xw.WriteStartDocument ();
			xw.WriteStartElement ("foo");
			xw.WriteStartAttribute ("bar", null);
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='", Output);

			xw.WriteEndDocument ();
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='' />", Output);
			Assert.AreEqual (WriteState.Start, xw.WriteState);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteEndElement ()
		{
			// no matching StartElement
			xw.WriteEndElement ();
		}

		[Test]
		public void WriteEndElement2 ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteEndElement ();
			Assert.AreEqual ("<foo />", Output);

			xw.WriteStartElement ("bar");
			xw.WriteStartAttribute ("baz", null);
			xw.WriteEndElement ();
			Assert.AreEqual ("<foo /><bar baz='' />", Output);
		}

		[Test]
		public void FullEndElement ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo>", Output);

			xw.WriteStartElement ("bar");
			xw.WriteAttributeString ("foo", "bar");
			xw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo><bar foo='bar'></bar>", Output);

			xw.WriteStartElement ("baz");
			xw.WriteStartAttribute ("bar", null);
			xw.WriteFullEndElement ();
			Assert.AreEqual ("<foo></foo><bar foo='bar'></bar><baz bar=''></baz>", Output);
		}

		[Test]
		public void WriteQualifiedName ()
		{
			xw.WriteStartElement (null, "test", null);
			xw.WriteAttributeString ("xmlns", "me", null, "http://localhost/");
			xw.WriteQualifiedName ("bob", "http://localhost/");
			xw.WriteEndElement ();

			Assert.AreEqual ("<test xmlns:me='http://localhost/'>me:bob</test>", Output);
		}

		[Test]
		public void WriteQualifiedNameNonDeclaredAttribute ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteStartAttribute ("a", "");
			xw.WriteQualifiedName ("attr", "urn:a");
			xw.WriteWhitespace (" ");
			xw.WriteQualifiedName ("attr", "urn:b");
			xw.WriteEndAttribute ();
			xw.WriteEndElement ();
			string xml = sw.ToString ();
			Assert.IsTrue (
				xml.IndexOf ("<foo ") >= 0,
				"foo");
			Assert.IsTrue (
				xml.IndexOf ("a='d1p1:attr d1p2:attr'") > 0,
				"qnames");
			Assert.IsTrue (
				xml.IndexOf (" xmlns:d1p1='urn:a'") > 0,
				"xmlns:a");
			Assert.IsTrue (
				xml.IndexOf (" xmlns:d1p2='urn:b'") > 0,
				"xmlns:b");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteQualifiedNameNonDeclaredContent ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteQualifiedName ("abc", "urn:abc");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteQualifiedNameNonNCName ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xmlns", "urn:default");
			xw.WriteStartElement ("child");
			xw.WriteStartAttribute ("a", "");
			xw.WriteQualifiedName ("x:def", "urn:def");
		}

		[Test]
		public void WriteRaw ()
		{
			xw.WriteRaw("&<>\"'");
			Assert.AreEqual ("&<>\"'", Output);

			xw.WriteRaw(null);
			Assert.AreEqual ("&<>\"'", Output);

			xw.WriteRaw("");
			Assert.AreEqual ("&<>\"'", Output);
		}

		[Test]
		public void WriteRawInvalidInAttribute ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteStartAttribute ("bar", null);
			xw.WriteRaw ("&<>\"'");
			xw.WriteEndAttribute ();
			xw.WriteEndElement ();
			Assert.AreEqual ("<foo bar='&<>\"'' />", Output);
		}

		[Test]
		public void WriteStateTest ()
		{
			Assert.AreEqual (WriteState.Start, xw.WriteState);
			xw.WriteStartDocument ();
			Assert.AreEqual (WriteState.Prolog, xw.WriteState);
			xw.WriteStartElement ("root");
			Assert.AreEqual (WriteState.Element, xw.WriteState);
			xw.WriteElementString ("foo", "bar");
			Assert.AreEqual (WriteState.Content, xw.WriteState);
			xw.Close ();
			Assert.AreEqual (WriteState.Closed, xw.WriteState);
		}

		[Test]
		public void WriteString ()
		{
			xw.WriteStartDocument ();
			try {
				xw.WriteString("foo");
			} catch (InvalidOperationException) {}
		}

		[Test]
		public void WriteString2 ()
		{
			xw.WriteStartDocument ();
			// Testing attribute values

			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("bar", "&<>");
			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo bar='&amp;&lt;&gt;'", Output);
		}

		[Test]
		public void WriteStringWithEntities()
		{
			// Testing element values
			xw.WriteElementString ("foo", "&<>\"'");
			Assert.AreEqual ("<foo>&amp;&lt;&gt;\"'</foo>", Output);
		}

		[Test]
		public void XmlLang ()
		{
			Assert.IsNull (xw.XmlLang);
			
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xml", "lang", null, "langfoo");
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo'", Output);

			xw.WriteAttributeString ("boo", "yah");
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'", Output);
			
			xw.WriteElementString("bar", "baz");
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>", Output);
			
			xw.WriteString("baz");
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz", Output);
			
			xw.WriteStartElement ("quux");
			xw.WriteStartAttribute ("xml", "lang", null);
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", Output);
			
			xw.WriteString("langbar");
			// Commented out there: it is implementation-dependent.
			// and incompatible between .NET 1.0 and 1.1
//			Assert.AreEqual ("langfoo", xw.XmlLang);
//			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='", Output);
			
			xw.WriteEndAttribute ();
			// Commented out there: it is implementation-dependent.
			// and incompatible between .NET 1.0 and 1.1
//			Assert.AreEqual ("langbar", xw.XmlLang);
//			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'", Output);

			// check if xml:lang repeats output even if same as current scope.
			xw.WriteStartElement ("joe");
			xw.WriteAttributeString ("xml", "lang", null, "langbar");
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'", Output);

			
			xw.WriteElementString ("quuux", "squonk");
			Assert.AreEqual ("langbar", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux>", Output);

			xw.WriteEndElement ();
			xw.WriteEndElement ();
			Assert.AreEqual ("langfoo", xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux>", Output);
			
			xw.WriteEndElement ();
			Assert.IsNull (xw.XmlLang);
			Assert.AreEqual ("<foo xml:lang='langfoo' boo='yah'><bar>baz</bar>baz<quux xml:lang='langbar'><joe xml:lang='langbar'><quuux>squonk</quuux></joe></quux></foo>", Output);
			
			xw.Close ();
			Assert.IsNull (xw.XmlLang);
		}

		// TODO: test operational aspects
		[Test]
		public void XmlSpaceTest ()
		{
			xw.WriteStartElement ("foo");
			Assert.AreEqual (XmlSpace.None, xw.XmlSpace);

			xw.WriteStartElement ("bar");
			xw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'",	Output);

			xw.WriteStartElement ("baz");
			xw.WriteAttributeString ("xml", "space", null, "preserve");
			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'", Output);

			xw.WriteStartElement ("quux");
			xw.WriteStartAttribute ("xml", "space", null);
			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", Output);

			// Commented out there: it is implementation-dependent
			// and incompatible between .NET 1.0 and 1.1
			xw.WriteString ("default");
//			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
//			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='", Output);
			
			xw.WriteEndAttribute ();
			Assert.AreEqual (XmlSpace.Default, xw.XmlSpace);
			Assert.AreEqual ("<foo><bar xml:space='preserve'><baz xml:space='preserve'><quux xml:space='default'", Output);

			xw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
			xw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.Preserve, xw.XmlSpace);
			xw.WriteEndElement ();
			Assert.AreEqual (XmlSpace.None, xw.XmlSpace);

			xw.WriteStartElement ("quux");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue1 ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xml", "space", null, "bubba");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue2 ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xml", "space", null, "PRESERVE");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue3 ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xml", "space", null, "Default");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void XmlSpaceTestInvalidValue4 ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xml", "space", null, "bubba");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteWhitespaceNonWhitespace ()
		{
			xw.WriteWhitespace ("x");
		}

		[Test]
		public void XmlSpaceRaw ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteStartAttribute ("xml", "space", null);
			Assert.AreEqual (XmlSpace.None, xw.XmlSpace);
			Assert.AreEqual ("<foo xml:space='", Output);

			xw.WriteString ("default");
			// Commented out there: it is implementation-dependent
			// and incompatible between .NET 1.0 and 1.1
//			Assert.AreEqual (XmlSpace.None, xw.XmlSpace);
//			Assert.AreEqual ("<foo xml:space='", Output);

			xw.WriteEndAttribute ();
			Assert.AreEqual (XmlSpace.Default, xw.XmlSpace);
			Assert.AreEqual ("<foo xml:space='default'", Output);
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
			Assert.AreEqual("version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"", sw.ToString().Trim(), "#WriteAttributes.XmlDecl.1");

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
			Assert.AreEqual("version=\"1.0\" standalone=\"no\"", sw.ToString().Trim(), "#WriteAttributes.XmlDecl.2");

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
			Assert.AreEqual("<root a1=\"A\" b2=\"B\" c3=\"C\" />", sw.ToString().Trim(), "#WriteAttributes.Element");
			xtr.Close ();
		}

		[Test]
		public void WriteWhitespace ()
		{
			xw.WriteStartElement ("a");
			xw.WriteWhitespace ("\n\t");
			xw.WriteStartElement ("b");
			xw.WriteWhitespace ("\n\t");
			xw.WriteEndElement ();
			xw.WriteWhitespace ("\n");
			xw.WriteEndElement ();
			xw.WriteWhitespace ("\n");
			xw.Flush ();
			Assert.AreEqual ("<a>\n\t<b>\n\t</b>\n</a>\n", Output);
		}

		[Test]
		public void FlushDoesntCloseTag ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("bar", "baz");
			xw.Flush ();
			Assert.AreEqual ("<foo bar='baz'", Output);
		}

		[Test]
		public void WriteWhitespaceClosesTag ()
		{
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("bar", "baz");
			xw.WriteWhitespace (" ");
			Assert.AreEqual ("<foo bar='baz'> ", Output);
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
			xw.WriteStartDocument ();
			xw.WriteStartElement ("foo");
			xw.WriteAttributeString ("xmlns", "probe");
			Assert.AreEqual (String.Empty, xw.LookupPrefix ("probe"));
			xw.WriteStartElement ("b");
			Assert.AreEqual (String.Empty, xw.LookupPrefix ("probe"));
			xw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xw.WriteEndElement (); // b2
			xw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xw.WriteEndElement (); // b2
			xw.WriteEndElement (); // b
			xw.WriteEndElement (); // foo
			xw.WriteEndDocument ();
			xw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 xmlns='' /></b></foo>", Output);
		}

		[Test]
		public void DontOutputRemovalDefaultNSDeclaration2 ()
		{
			xw.WriteStartDocument ();
			// IMPORTANT DIFFERENCE!! ns = "", not null
			xw.WriteStartElement ("foo", "");
			xw.WriteAttributeString ("xmlns", "probe");
			Assert.IsNull (xw.LookupPrefix ("probe"));
			xw.WriteStartElement ("b");
			Assert.IsNull (xw.LookupPrefix ("probe"));
			xw.WriteStartElement (null, "b2", null); // *Don't* output xmlns=""
			xw.WriteEndElement (); // b2
			xw.WriteStartElement (null, "b2", ""); // *Do* output xmlns=""
			xw.WriteEndElement (); // b2
			xw.WriteEndElement (); // b
			xw.WriteEndElement (); // foo
			xw.WriteEndDocument ();
			xw.Close ();

			Assert.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo xmlns='probe'><b><b2 /><b2 /></b></foo>", Output);
		}

		[Test]
		public void DoOutputRemovalDefaultNSDeclaration ()
		{
			xw.WriteStartElement ("docelem", "a-namespace");
			
			XmlDocument doc = new XmlDocument ();
			doc.CreateElement ("hola").WriteTo (xw);
			// This means, WriteTo never passes null NamespaceURI argument to XmlWriter.
			xw.WriteEndElement ();
			xw.Close ();

			Assert.AreEqual ("<docelem xmlns='a-namespace'><hola xmlns='' /></docelem>", Output);
		}

		[Test]
		public void WriteAttributeTakePrecedenceOnXmlns ()
		{
			xw.WriteStartElement ("root", "urn:foo");
			xw.WriteAttributeString ("xmlns", "urn:bar");
			xw.WriteEndElement ();
			xw.Close ();
			Assert.AreEqual ("<root xmlns='urn:bar' />", Output);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LookupPrefixNull ()
		{
			xw.LookupPrefix (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LookupPrefixEmpty ()
		{
			xw.LookupPrefix (String.Empty);
		}

		[Test]
		public void LookupPrefixIgnoresXmlnsAttribute ()
		{
			Assert.IsNull (xw.LookupPrefix ("urn:foo"));
			xw.WriteStartElement ("root");
			Assert.IsNull (xw.LookupPrefix ("urn:foo"));
			xw.WriteAttributeString ("xmlns", "urn:foo");
			// Surprisingly to say, it is ignored!!
			Assert.AreEqual (String.Empty, xw.LookupPrefix ("urn:foo"));
			xw.WriteStartElement ("hoge");
			// (still after flushing previous start element.)
			Assert.AreEqual (String.Empty, xw.LookupPrefix ("urn:foo"));
			xw.WriteStartElement ("fuga", "urn:foo");
			// Is this testing on the correct way? Yes, here it is.
			Assert.AreEqual (String.Empty, xw.LookupPrefix ("urn:foo"));
		}

		[Test]
		public void WriteInvalidNames ()
		{
			xw.WriteStartElement ("foo<>");
			xw.WriteAttributeString ("ho<>ge", "value");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributePrefixWithoutNS ()
		{
			xw.WriteStartAttribute ("some", "foo", null);
		}

		[Test]
		public void AttributeWriteStartAttributeXmlnsNullNS ()
		{
			xw.WriteStartAttribute ("xmlns", "foo", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteEndAttributeXmlnsNullNs ()
		{
			// Compare with the test AttributeWriteStartAttributeXmlnsNullNS().
			xw.WriteStartAttribute ("xmlns", "foo", null);
			xw.WriteEndAttribute ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributePrefixXmlnsNonW3CNS ()
		{
			xw.WriteStartAttribute ("xmlns", "foo", "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeWriteStartAttributeLocalXmlnsNonW3CNS ()
		{
			xw.WriteStartAttribute ("", "xmlns", "urn:foo");
		}

		[Test]
		public void WriteRawProceedToProlog ()
		{
			XmlTextWriter xw = new XmlTextWriter (new StringWriter ());
			xw.WriteRaw ("");
			Assert.AreEqual (WriteState.Prolog, xw.WriteState);
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
			xw.WriteRaw ("");
			xw.WriteString ("foo");
			Assert.AreEqual (WriteState.Content, xw.WriteState);
		}

		[Test]
		public void LookupOverridenPrefix ()
		{
			xw.WriteStartElement ("out");
			xw.WriteAttributeString ("xmlns", "baz", "http://www.w3.org/2000/xmlns/", "xyz");
			xw.WriteStartElement ("baz", "foo", "abc");
			Assert.IsNull (xw.LookupPrefix ("xyz"));
		}

		[Test]
		public void DuplicatingNamespaceMappingInAttributes ()
		{
			xw.WriteStartElement ("out");
			xw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xw.WriteAttributeString ("p", "bar", "urn:bar", "xyz");
			xw.WriteAttributeString ("p", "baz", "urn:baz", "xyz");
			xw.WriteStartElement ("out");
			xw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xw.WriteStartElement ("out");
			xw.WriteAttributeString ("p", "foo", "urn:foo", "xyz");
			xw.WriteEndElement ();
			xw.WriteEndElement ();
			xw.WriteEndElement ();
			string xml = sw.ToString ();
			Assert.IsTrue (
				xml.IndexOf ("p:foo='xyz'") > 0,
				"p:foo");
			Assert.IsTrue (
				xml.IndexOf ("d1p1:bar='xyz'") > 0,
				"d1p1:bar");
			Assert.IsTrue (
				xml.IndexOf ("d1p2:baz='xyz'") > 0,
				"d1p1:baz");
			Assert.IsTrue (
				xml.IndexOf ("xmlns:d1p2='urn:baz'") > 0,
				"xmlns:d1p2");
			Assert.IsTrue (
				xml.IndexOf ("xmlns:d1p1='urn:bar'") > 0,
				"xmlns:d1p1");
			Assert.IsTrue (
				xml.IndexOf ("xmlns:p='urn:foo'") > 0,
				"xmlns:p");
			Assert.IsTrue (
				xml.IndexOf ("<out p:foo='xyz'><out p:foo='xyz' /></out></out>") > 0,
				"remaining");
		}

		[Test]
		public void WriteXmlSpaceIgnoresNS ()
		{
			xw.WriteStartElement ("root");
			xw.WriteAttributeString ("xml", "space", "abc", "preserve");
			xw.WriteEndElement ();
			Assert.AreEqual ("<root xml:space='preserve' />", sw.ToString ());
		}

		[Test] // bug #75546
		public void WriteEmptyNSQNameInAttribute ()
		{
			XmlTextWriter xw = new XmlTextWriter (TextWriter.Null);
			xw.WriteStartElement ("foo", "urn:goo");
			xw.WriteAttributeString ("xmlns:bar", "urn:bar");
			xw.WriteStartAttribute ("foo", "");
			xw.WriteQualifiedName ("n1", "urn:bar");
			xw.WriteEndAttribute ();
			xw.WriteStartAttribute ("foo", "");
			xw.WriteQualifiedName ("n2", "");
			xw.WriteEndAttribute ();
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
		[ExpectedException (typeof (InvalidOperationException))]
		public void RejectWritingAtErrorState ()
		{
			try {
				xw.WriteEndElement ();
			} catch (Exception) {
			}

			xw.WriteStartElement ("foo");
		}

		#endregion

		[Test]
		public void WriteBooleanArray ()
		{
			bool [] array = new bool [] {true, false, true, true, false};
			xw.WriteArray ("", "root", "", array, 1, 3);
			Assert.AreEqual ("<root>false</root><root>true</root><root>true</root>", Output, "#1");
		}

		[Test]
		public void WriteNode ()
		{
			string s = @"<Resolve xmlns='http://schemas.microsoft.com/net/2006/05/peer' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'><ClientId>79310c9f-18d4-4337-a95a-1865ca54a66e</ClientId><MaxAddresses>3</MaxAddresses><MeshId>amesh</MeshId></Resolve>".Replace ('\'', '"');
			var sw = new StringWriter ();
			var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw, new XmlWriterSettings () { OmitXmlDeclaration = true }));
			var xr = XmlDictionaryReader.CreateDictionaryReader (XmlReader.Create (new StringReader (s)));
			xr.MoveToContent ();
			while (!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
				xw.WriteNode (xr, false);
			xw.Flush ();
			Assert.AreEqual (s, sw.ToString ());
		}
	}
}
