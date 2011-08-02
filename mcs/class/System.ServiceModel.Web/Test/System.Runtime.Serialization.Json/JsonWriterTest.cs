//
// JsonWriterTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization.Json;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Json
{
	[TestFixture]
	public class JsonWriterTest
	{
		MemoryStream ms;
		XmlDictionaryWriter w;

		string ResultString {
			get { return Encoding.UTF8.GetString (ms.ToArray ()); }
		}

		[SetUp]
		public void Setup ()
		{
			ms = new MemoryStream ();
			w = JsonReaderWriterFactory.CreateJsonWriter (ms);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullStream ()
		{
			JsonReaderWriterFactory.CreateJsonWriter (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullEncoding ()
		{
			JsonReaderWriterFactory.CreateJsonWriter (new MemoryStream (), null);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void SimpleElementNotRoot ()
		{
			w.WriteStartElement ("foo");
		}

		[Test]
		public void SimpleElement ()
		{
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Close ();
			// empty string literal ("")
			Assert.AreEqual ("\"\"", ResultString, "#1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void SimpleElement2 ()
		{
			w.WriteStartElement ("root");
			w.WriteStartElement ("foo");
			// type='array' or type='object' is required before writing immediate child of an element.
		}

		[Test]
		public void SimpleElement3 ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("e1");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("e1_1");
			w.WriteEndElement (); // treated as a string literal
			w.WriteEndElement ();
			w.WriteStartElement ("e2");
			w.WriteString ("value");
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			string json = "{\"e1\":{\"e1_1\":\"\"},\"e2\":\"value\"}";
			Assert.AreEqual (json, ResultString, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AttributeNonType ()
		{
			w.WriteStartElement ("root");
			// only "type" attribute is expected.
			w.WriteStartAttribute ("a1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void TypeAttributeNonStandard ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "foo");
		}

		[Test]
		public void SimpleTypeAttribute ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "number");
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual (String.Empty, ResultString, "#1");
		}

		[Test]
		public void SimpleTypeAttribute2 ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "number");
			w.WriteString ("1");
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual ("{\"foo\":1}", ResultString, "#1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStringForNull ()
		{
			w.WriteStartElement ("root");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "null");
			w.WriteString ("1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStringForArray ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "array");
			w.WriteString ("1");
		}

		[Test]
		// uh, no exception?
		public void WriteStringForBoolean ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "boolean");
			w.WriteString ("xyz");
			w.WriteEndElement ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStringForObject ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteString ("1");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteArrayNonItem ()
		{
			w.WriteStartElement ("root");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "array");
			w.WriteStartElement ("bar");
		}

		[Test]
		public void WriteArray ()
		{
			w.WriteStartElement ("root"); // name is ignored
			w.WriteAttributeString ("type", "array");
			w.WriteElementString ("item", "v1");
			w.WriteElementString ("item", "v2");
			w.Close ();
			Assert.AreEqual (@"[""v1"",""v2""]", ResultString, "#1");
		}

		[Test]
		public void WriteArrayInObject ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "array");
			w.WriteElementString ("item", "v1");
			w.WriteElementString ("item", "v2");
			w.Close ();
			Assert.AreEqual (@"{""foo"":[""v1"",""v2""]}", ResultString, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElementNonEmptyNS ()
		{
			// namespaces are not allowed
			w.WriteStartElement (String.Empty, "x", "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElementNonEmptyPrefix ()
		{
			// prefixes are not allowed
			w.WriteStartElement ("p", "x", "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartElementMultiTopLevel ()
		{
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			// hmm...
			Assert.AreEqual (WriteState.Content, w.WriteState, "#1");
			// writing of multiple root elements is not supported
			w.WriteStartElement ("root2");
			w.Close ();
			Assert.AreEqual (String.Empty, ResultString, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartAttributeNonEmptyNS ()
		{
			// namespaces are not allowed
			w.WriteStartElement ("root");
			// well, empty prefix for a global attribute would be
			// replaced anyways ...
			w.WriteStartAttribute (String.Empty, "x", "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartAttributeInXmlNamespace ()
		{
			// even "xml" namespace is not allowed (anyways only "type" is allowed ...)
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("xml", "lang", "http://www.w3.org/XML/1998/namespace");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LookupPrefixNull ()
		{
			w.LookupPrefix (null);
		}

		[Test]
		public void LookupPrefix ()
		{
			// since namespaces are not allowed, it mostly makes no sense...
			Assert.AreEqual (String.Empty, w.LookupPrefix (String.Empty), "#1");
			Assert.IsNull (w.LookupPrefix ("urn:nonexistent"), "#2");
			Assert.AreEqual ("xml", w.LookupPrefix ("http://www.w3.org/XML/1998/namespace"), "#3");
			Assert.AreEqual ("xmlns", w.LookupPrefix ("http://www.w3.org/2000/xmlns/"), "#4");
		}

		[Test]
		public void WriteStartDocument ()
		{
			Assert.AreEqual (WriteState.Start, w.WriteState, "#1");
			w.WriteStartDocument ();
			Assert.AreEqual (WriteState.Start, w.WriteState, "#2");
			w.WriteStartDocument (true);
			Assert.AreEqual (WriteState.Start, w.WriteState, "#3");
			// So, it does nothing
		}

		[Test]
		public void WriteEndDocument ()
		{
			w.WriteEndDocument (); // so, it is completely wrong, but ignored.
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WriteDocType ()
		{
			w.WriteDocType (null, null, null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WriteComment ()
		{
			w.WriteComment ("test");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void WriteEntityRef ()
		{
			w.WriteEntityRef ("ent");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteProcessingInstruction ()
		{
			// since this method accepts case-insensitive "XML",
			// it throws ArgumentException.
			w.WriteProcessingInstruction ("T", "D");
		}

		[Test]
		public void WriteProcessingInstructionXML ()
		{
			// You might not know, but in some cases, things like
			// XmlWriter.WriteNode() is implemented to invoke
			// this method for writing XML declaration. This
			// check is (seems) case-insensitive.
			w.WriteProcessingInstruction ("XML", "foobar");
			// In this case, the data is simply ignored (as
			// WriteStartDocument() is).
		}

		[Test]
		public void WriteRaw ()
		{
			w.WriteStartElement ("root");
			w.WriteRaw ("sample");
			w.WriteRaw (new char [] {'0', '1', '2', '3'}, 1, 2);
			w.Close ();
			Assert.AreEqual ("\"sample12\"", ResultString);
		}

		[Test]
		public void WriteCData ()
		{
			w.WriteStartElement ("root");
			w.WriteCData ("]]>"); // this behavior is incompatible with ordinal XmlWriters.
			w.Close ();
			Assert.AreEqual ("\"]]>\"", ResultString);
		}

		[Test]
		public void WriteCharEntity ()
		{
			w.WriteStartElement ("root");
			w.WriteCharEntity ('>');
			w.Close ();
			Assert.AreEqual ("\">\"", ResultString);
		}

		[Test]
		public void WriteWhitespace ()
		{
			w.WriteStartElement ("root");
			w.WriteWhitespace ("\t  \n\r");
			w.Close ();
			Assert.AreEqual (@"""\u0009  \u000a\u000d""", ResultString);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteWhitespaceNonWhitespace ()
		{
			w.WriteStartElement ("root");
			w.WriteWhitespace ("TEST");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteStringTopLevel ()
		{
			w.WriteString ("test");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartAttributeTopLevel ()
		{
			w.WriteStartAttribute ("test");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteStartDocumentAtClosed ()
		{
			w.Close ();
			w.WriteStartDocument ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteStartElementAtClosed ()
		{
			w.Close ();
			w.WriteStartElement ("foo");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WriteProcessingInstructionAtClosed ()
		{
			w.Close ();
			w.WriteProcessingInstruction ("xml", "version='1.0'");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteMixedContent ()
		{
			w.WriteStartElement ("root");
			w.WriteString ("TEST");
			w.WriteStartElement ("mixed"); // is not allowed.
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartElementInvalidTopLevelName ()
		{
			w.WriteStartElement ("anyname");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteStartElementNullName ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteStartElementEmptyName ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement (String.Empty);
			// It is regarded as invalid name in JSON. However,
			// I don't think there is such limitation in JSON specification.
		}

		[Test]
		public void WriteStartElementWithRuntimeTypeName ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteAttributeString ("__type", "FooType:#FooNamespace");
			w.Close ();
			Assert.AreEqual (@"{""__type"":""FooType:#FooNamespace""}", ResultString);
		}

		[Test]
		public void WriteStartElementWeirdName ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("!!!");
			w.Close ();
			Assert.AreEqual (@"{""!!!"":""""}", ResultString);
		}

		[Test]
		public void WriteRootAsObject ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteString ("object");
			w.WriteEndAttribute ();
			w.Close ();
			Assert.AreEqual ("{}", ResultString);
		}

		[Test]
		public void WriteRootAsArray ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteString ("array");
			w.WriteEndAttribute ();
			w.Close ();
			Assert.AreEqual ("[]", ResultString);
		}

		[Test]
		public void WriteRootAsLiteral ()
		{
			w.WriteStartElement ("root");
			w.Close ();
			Assert.AreEqual ("\"\"", ResultString);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteEndElementOnAttribute ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteString ("array");
			w.WriteEndElement ();
		}

		[Test]
		public void WriteAttributeAsSeparateStrings ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteString ("arr");
			w.WriteString ("ay");
			w.WriteEndAttribute ();
			w.Close ();
			Assert.AreEqual ("[]", ResultString);
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartAttributeInAttributeMode ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteStartAttribute ("type");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartAttributeInContentMode ()
		{
			w.WriteStartElement ("root");
			w.WriteString ("TEST");
			w.WriteStartAttribute ("type");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteStartElementInAttributeMode ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteStartElement ("child");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void CloseAtAtributeState ()
		{
			w.WriteStartElement ("root");
			w.WriteStartAttribute ("type");
			w.WriteString ("array");
			// It calls WriteEndElement() without calling
			// WriteEndAttribute().
			w.Close ();
		}

		[Test]
		public void WriteSlashEscaped ()
		{
			w.WriteStartElement ("root");
			w.WriteString ("/my date/");
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual ("\"\\/my date\\/\"", ResultString);
		}

		[Test]
		public void WriteNullType ()
		{
			w.WriteStartElement ("root");
			w.WriteAttributeString ("type", "object");
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("type", "null");
			w.Close ();
			Assert.AreEqual ("{\"foo\":null}", ResultString);
		}
	}
}
