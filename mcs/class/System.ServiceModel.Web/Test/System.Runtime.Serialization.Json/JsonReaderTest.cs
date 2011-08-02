//
// JsonReaderTest.cs
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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization.Json
{
	[TestFixture]
	public class JsonReaderTest
	{
		XmlDictionaryReader reader;

		Stream GetInput (string s)
		{
			return new MemoryStream (Encoding.ASCII.GetBytes (s));
		}

		XmlDictionaryReader CreateReader (string s)
		{
			return JsonReaderWriterFactory.CreateJsonReader (GetInput (s), new XmlDictionaryReaderQuotas ());
		}

		void AssertNode (int depth, string localName, XmlNodeType nodeType, string value, string type, XmlDictionaryReader reader, string label)
		{
			Assert.AreEqual (localName, reader.LocalName, label + ".LocalName");
			Assert.AreEqual (nodeType, reader.NodeType, label + ".NodeType");
			Assert.AreEqual (value, reader.Value, label + ".Value");
			Assert.AreEqual (type, reader.GetAttribute ("type"), label + ".GetAttribute('type')");
		}

		// Constructors

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullBytes ()
		{
			JsonReaderWriterFactory.CreateJsonReader ((byte []) null, new XmlDictionaryReaderQuotas ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullStream ()
		{
			JsonReaderWriterFactory.CreateJsonReader ((Stream) null, new XmlDictionaryReaderQuotas ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullReaderQuotas ()
		{
			JsonReaderWriterFactory.CreateJsonReader (GetInput ("{}"), null);
		}

		[Test]
		public void ConstructorNullEncodingAndReaderClose ()
		{
			JsonReaderWriterFactory.CreateJsonReader (GetInput ("{}"), null, new XmlDictionaryReaderQuotas (), null);
		}

		// Close()

		[Test]
		public void CloseTwice ()
		{
			reader = CreateReader ("{}");
			reader.Close ();
			reader.Close ();
		}

		[Test]
		// hmm... [ExpectedException (typeof (InvalidOperationException))]
		public void CloseAndRead ()
		{
			reader = CreateReader ("{}");
			reader.Close ();
			reader.Read ();
		}

		[Test]
		// hmm... [ExpectedException (typeof (InvalidOperationException))]
		public void CloseAndMoveToFirstAttribute ()
		{
			reader = CreateReader ("{}");
			reader.Close ();
			reader.MoveToFirstAttribute ();
		}

		// Read() several top-level types

		[Test]
		public void ReadStateEmpty ()
		{
			reader = CreateReader ("");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read ();
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#2");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#3");
		}

		[Test]
		public void ReadStateEmpty2 ()
		{
			reader = CreateReader ("  ");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read ();
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#2");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#3");
		}

		[Test]
		public void ReadStateObject ()
		{
			reader = CreateReader ("{}");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			reader.Read (); // endelement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#4");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#5");
		}

		[Test]
		public void ReadStateArray ()
		{
			reader = CreateReader ("[]");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			reader.Read (); // endelement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#4");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#5");
		}

		[Test]
		public void ReadNumber ()
		{
			reader = CreateReader ("1234");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // dummy root element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#2-1");
			Assert.AreEqual ("root", reader.LocalName, "#2-2");
			reader.Read (); // content (number)
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#3-1");
			Assert.AreEqual ("1234", reader.Value, "#3-2");
			reader.Read (); // endelement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#4");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4-1");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#5");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#6");
		}

		[Test]
		public void ReadBool ()
		{
			reader = CreateReader ("true");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // dummy root element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#2-1");
			Assert.AreEqual ("root", reader.LocalName, "#2-2");
			Assert.AreEqual ("boolean", reader.GetAttribute ("type"), "#2-3");
			reader.Read (); // content (boolean)
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#3-1");
			Assert.AreEqual ("true", reader.Value, "#3-2");
			reader.Read (); // endelement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#4");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4-1");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#5");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#6");
		}

		[Test]
		public void ReadNull ()
		{
			reader = CreateReader ("null");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // dummy root element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#2-1");
			Assert.AreEqual ("root", reader.LocalName, "#2-2");
			// When it is null, the value is never given and the reader is skipped to the end element.
			reader.Read (); // endlement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#3-1");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#4");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#4");
		}

		[Test]
		public void ReadString ()
		{
			reader = CreateReader ("\"true\"");
			Assert.AreEqual (ReadState.Initial, reader.ReadState, "#1");
			reader.Read (); // dummy root element
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#2");
			Assert.AreEqual (XmlNodeType.Element, reader.NodeType, "#2-1");
			Assert.AreEqual ("root", reader.LocalName, "#2-2");
			Assert.AreEqual ("string", reader.GetAttribute ("type"), "#2-3");
			reader.Read (); // content (number)
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#3");
			Assert.AreEqual (XmlNodeType.Text, reader.NodeType, "#3-1");
			Assert.AreEqual ("true", reader.Value, "#3-2");
			reader.Read (); // endelement
			Assert.AreEqual (ReadState.Interactive, reader.ReadState, "#4");
			Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#4-1");
			reader.Read (); // endoffile
			Assert.AreEqual (ReadState.EndOfFile, reader.ReadState, "#5");
			reader.Close ();
			Assert.AreEqual (ReadState.Closed, reader.ReadState, "#6");
		}

		// MoveToAttribute() / MoveToElement()

		[Test]
		public void MoveToAttributeObject ()
		{
			reader = CreateReader ("{}");
			Assert.IsFalse (reader.MoveToFirstAttribute (), "#1");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.AreEqual ("type", reader.LocalName, "#3");
			Assert.AreEqual ("object", reader.Value, "#4");
			Assert.IsTrue (reader.ReadAttributeValue (), "#5");
			Assert.AreEqual ("object", reader.Value, "#6");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#7");
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#8");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#9");
		}

		[Test]
		public void MoveToElementObject ()
		{
			reader = CreateReader ("{}");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#1");
			Assert.IsTrue (reader.MoveToElement (), "#1-1");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.IsTrue (reader.ReadAttributeValue (), "#2-1");
			Assert.IsTrue (reader.MoveToElement (), "#2-2");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#3");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#3-1");
			Assert.IsTrue (reader.MoveToElement (), "#3-2");
		}

		[Test]
		public void MoveToAttributeArray ()
		{
			reader = CreateReader ("[]");
			Assert.IsFalse (reader.MoveToFirstAttribute (), "#1");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.AreEqual ("type", reader.LocalName, "#3");
			Assert.AreEqual ("array", reader.Value, "#4");
			Assert.IsTrue (reader.ReadAttributeValue (), "#5");
			Assert.AreEqual ("array", reader.Value, "#6");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#7");
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#8");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#9");
		}

		[Test]
		public void MoveToElementArray ()
		{
			reader = CreateReader ("[]");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#1");
			Assert.IsTrue (reader.MoveToElement (), "#1-1");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.IsTrue (reader.ReadAttributeValue (), "#2-1");
			Assert.IsTrue (reader.MoveToElement (), "#2-2");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#3");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#3-1");
			Assert.IsTrue (reader.MoveToElement (), "#3-2");
		}

		[Test]
		public void MoveToAttributeSimpleDummyRoot ()
		{
			reader = CreateReader ("1234");
			Assert.IsFalse (reader.MoveToFirstAttribute (), "#1");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.AreEqual ("type", reader.LocalName, "#3");
			Assert.AreEqual ("number", reader.Value, "#4");
			Assert.IsTrue (reader.ReadAttributeValue (), "#5");
			Assert.AreEqual ("number", reader.Value, "#6");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#7");
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#8");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#9");
		}

		[Test]
		public void MoveToElementSimpleDummyRoot ()
		{
			reader = CreateReader ("1234");
			reader.Read (); // element
			Assert.IsTrue (reader.MoveToFirstAttribute (), "#1");
			Assert.IsTrue (reader.MoveToElement (), "#1-1");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#2");
			Assert.IsTrue (reader.ReadAttributeValue (), "#2-1");
			Assert.IsTrue (reader.MoveToElement (), "#2-2");

			Assert.IsTrue (reader.MoveToFirstAttribute (), "#3");
			Assert.IsFalse (reader.MoveToNextAttribute (), "#3-1");
			Assert.IsTrue (reader.MoveToElement (), "#3-2");
		}

		// Read() arrays and objects

		[Test]
		public void ReadArrayContent ()
		{
			reader = CreateReader ("[123, \"123\", true, \"true\"]");

			// number value
			reader.Read (); // element
			AssertNode (0, "root", XmlNodeType.Element, String.Empty, "array", reader, "#1");

			reader.Read (); // 123 - element
			Assert.AreEqual ("number", reader.GetAttribute ("type"), "#2-0");
			AssertNode (1, "item", XmlNodeType.Element, String.Empty, "number", reader, "#2");
			reader.Read (); // 123 - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "123", null, reader, "#3");
			reader.Read (); // 123 - endelement
			AssertNode (1, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#4");

			// string value #1
			reader.Read (); // "123" - element
			Assert.AreEqual ("string", reader.GetAttribute ("type"), "#5-0");
			AssertNode (1, "item", XmlNodeType.Element, String.Empty, "string", reader, "#5");
			reader.Read (); // "123" - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "123", null, reader, "#6");
			reader.Read (); // "123" - endelement
			AssertNode (1, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#7");

			reader.Read (); // true - element
			Assert.AreEqual ("boolean", reader.GetAttribute ("type"), "#8-0");
			AssertNode (1, "item", XmlNodeType.Element, String.Empty, "boolean", reader, "#8");
			reader.Read (); // true - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "true", null, reader, "#9");
			reader.Read (); // true - endelement
			AssertNode (1, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#10");

			// string value #2
			reader.Read (); // "true" - element
			Assert.AreEqual ("string", reader.GetAttribute ("type"), "#11-0");
			AssertNode (1, "item", XmlNodeType.Element, String.Empty, "string", reader, "#11");
			reader.Read (); // "true" - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "true", null, reader, "#12");
			reader.Read (); // "true" - endelement
			AssertNode (1, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#13");
			Assert.IsTrue (reader.Read (), "#14"); // ]
			AssertNode (0, "root", XmlNodeType.EndElement, String.Empty, null, reader, "#15");
			Assert.IsFalse (reader.Read (), "#16"); // EOF
		}

		[Test]
		public void ReadObjectContent ()
		{
			reader = CreateReader ("{\"A\":123, \"B\": \"123\", \"C\" :true, \"D\" : \"true\"}");

			// number value
			reader.Read (); // element
			AssertNode (0, "root", XmlNodeType.Element, String.Empty, "object", reader, "#1");

			reader.Read (); // 123 - element
			AssertNode (1, "A", XmlNodeType.Element, String.Empty, "number", reader, "#2");
			reader.Read (); // 123 - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "123", null, reader, "#3");
			reader.Read (); // 123 - endelement
			AssertNode (1, "A", XmlNodeType.EndElement, String.Empty, null, reader, "#4");

			// string value #1
			reader.Read (); // "123" - element
			AssertNode (1, "B", XmlNodeType.Element, String.Empty, "string", reader, "#5");
			reader.Read (); // "123" - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "123", null, reader, "#6");
			reader.Read (); // "123" - endelement
			AssertNode (1, "B", XmlNodeType.EndElement, String.Empty, null, reader, "#7");

			reader.Read (); // true - element
			AssertNode (1, "C", XmlNodeType.Element, String.Empty, "boolean", reader, "#8");
			reader.Read (); // true - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "true", null, reader, "#9");
			reader.Read (); // true - endelement
			AssertNode (1, "C", XmlNodeType.EndElement, String.Empty, null, reader, "#10");

			// string value #2
			reader.Read (); // "true" - element
			AssertNode (1, "D", XmlNodeType.Element, String.Empty, "string", reader, "#11");
			reader.Read (); // "true" - text
			AssertNode (2, String.Empty, XmlNodeType.Text, "true", null, reader, "#12");
			reader.Read (); // "true" - endelement
			AssertNode (1, "D", XmlNodeType.EndElement, String.Empty, null, reader, "#13");
			Assert.IsTrue (reader.Read (), "#14"); // }
			AssertNode (0, "root", XmlNodeType.EndElement, String.Empty, null, reader, "#15");
			Assert.IsFalse (reader.Read (), "#16"); // EOF
		}

		[Test]
		public void ReadNestedObjects ()
		{
			reader = CreateReader ("{\"A\": [123, {\"B\": \"456\", \"C\" :true}], \"D\" : {\"E\" : \"false\"}}");
			Assert.IsTrue (reader.Read (), "#1"); // {
			AssertNode (0, "root", XmlNodeType.Element, String.Empty, "object", reader, "#2");
			Assert.IsTrue (reader.Read (), "#3"); // A
			AssertNode (1, "A", XmlNodeType.Element, String.Empty, "array", reader, "#4");
			Assert.IsTrue (reader.Read (), "#5"); // (<123>)
			AssertNode (2, "item", XmlNodeType.Element, String.Empty, "number", reader, "#6");
			Assert.IsTrue (reader.Read (), "#7"); // (123)
			AssertNode (3, String.Empty, XmlNodeType.Text, "123", null, reader, "#8");
			Assert.IsTrue (reader.Read (), "#9"); // (</123>)
			AssertNode (2, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#10");
			Assert.IsTrue (reader.Read (), "#11"); // {
			AssertNode (2, "item", XmlNodeType.Element, String.Empty, "object", reader, "#12");
			Assert.IsTrue (reader.Read (), "#13"); // B
			AssertNode (3, "B", XmlNodeType.Element, String.Empty, "string", reader, "#14");
			Assert.IsTrue (reader.Read (), "#15"); // "456"
			AssertNode (4, String.Empty, XmlNodeType.Text, "456", null, reader, "#16");
			Assert.IsTrue (reader.Read (), "#17"); // /B
			AssertNode (3, "B", XmlNodeType.EndElement, String.Empty, null, reader, "#18");

			Assert.IsTrue (reader.Read (), "#19"); // C
			AssertNode (3, "C", XmlNodeType.Element, String.Empty, "boolean", reader, "#20");
			Assert.IsTrue (reader.Read (), "#21"); // true
			AssertNode (4, String.Empty, XmlNodeType.Text, "true", null, reader, "#22");
			Assert.IsTrue (reader.Read (), "#23"); // /C
			AssertNode (3, "C", XmlNodeType.EndElement, String.Empty, null, reader, "#24");
			Assert.IsTrue (reader.Read (), "#25"); // }
			AssertNode (2, "item", XmlNodeType.EndElement, String.Empty, null, reader, "#26");
			Assert.IsTrue (reader.Read (), "#27"); // ]
			AssertNode (1, "A", XmlNodeType.EndElement, String.Empty, null, reader, "#28");
			Assert.IsTrue (reader.Read (), "#29"); // {
			AssertNode (1, "D", XmlNodeType.Element, String.Empty, "object", reader, "#30");
			Assert.IsTrue (reader.Read (), "#31"); // D
			AssertNode (2, "E", XmlNodeType.Element, String.Empty, "string", reader, "#32");
			Assert.IsTrue (reader.Read (), "#33"); // "false"
			AssertNode (3, String.Empty, XmlNodeType.Text, "false", null, reader, "#34");
			Assert.IsTrue (reader.Read (), "#35"); // /D
			AssertNode (2, "E", XmlNodeType.EndElement, String.Empty, null, reader, "#36");
			Assert.IsTrue (reader.Read (), "#37"); // }
			AssertNode (1, "D", XmlNodeType.EndElement, String.Empty, null, reader, "#38");
			Assert.IsTrue (reader.Read (), "#39"); // }
			AssertNode (0, "root", XmlNodeType.EndElement, String.Empty, null, reader, "#40");
			Assert.IsFalse (reader.Read (), "#41"); // EOF
		}

		void ReadToEnd (XmlDictionaryReader reader)
		{
			while (!reader.EOF)
				reader.Read ();
		}

		// Read() valid and invalid contents

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadTwoTopLevelContents ()
		{
			ReadToEnd (CreateReader ("{}{}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadMissingCloseCurly ()
		{
			ReadToEnd (CreateReader ("{"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadMissingCloseCurly2 ()
		{
			ReadToEnd (CreateReader ("{{}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadExtraCloseCurly ()
		{
			ReadToEnd (CreateReader ("}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadExtraCloseCurly2 ()
		{
			ReadToEnd (CreateReader ("{}}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadMissingCloseBrace ()
		{
			ReadToEnd (CreateReader ("["));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadMissingCloseBrace2 ()
		{
			ReadToEnd (CreateReader ("[[]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadExtraCloseBrace ()
		{
			ReadToEnd (CreateReader ("]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // hmm, why does it pass?
		public void ReadExtraCloseBrace2 ()
		{
			ReadToEnd (CreateReader ("[]]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadOpenCurlyCloseBrace ()
		{
			ReadToEnd (CreateReader ("{]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadOpenBraceCloseCurly ()
		{
			ReadToEnd (CreateReader ("[}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadParens ()
		{
			ReadToEnd (CreateReader ("()"));
		}

		[Test]
		public void ReadValidNumber ()
		{
			ReadToEnd (CreateReader ("0"));
		}

		[Test]
		public void ReadValidNumber2 ()
		{
			ReadToEnd (CreateReader ("-0"));
		}

		[Test]
		public void ReadValidNumber3 ()
		{
			ReadToEnd (CreateReader ("0e5"));
		}

		[Test]
		public void ReadValidNumber4 ()
		{
			ReadToEnd (CreateReader ("0.5"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidNumber ()
		{
			CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
			try {
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("de-DE");
				// if we read a number just by current culture, it will be regarded as correct JSON.
				ReadToEnd (CreateReader ("123,45"));
			} finally {
				Thread.CurrentThread.CurrentCulture = originalCulture;
			}
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidNumber2 ()
		{
			ReadToEnd (CreateReader ("+5"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidNumber3 ()
		{
			ReadToEnd (CreateReader ("01"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidNumber4 ()
		{
			ReadToEnd (CreateReader (".1"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidNumber5 ()
		{
			ReadToEnd (CreateReader ("10."));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidNumber7 ()
		{
			ReadToEnd (CreateReader ("e5"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidNumber8 ()
		{
			ReadToEnd (CreateReader ("-e5"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidNumber9 ()
		{
			ReadToEnd (CreateReader ("-e5.5"));
		}

		[Test]
		public void ReadInvalidNumber10 () // bug #531904
		{
			ReadToEnd (CreateReader ("4.29153442382814E-05"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidObjectContent ()
		{
			ReadToEnd (CreateReader ("{\"foo\"}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidObjectContent2 ()
		{
			ReadToEnd (CreateReader ("{\"A\": 123 456}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidObjectContent3 ()
		{
			ReadToEnd (CreateReader ("{, \"A\":123, \"B\":456}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidObjectContent4 ()
		{
			ReadToEnd (CreateReader ("{\"A\":123, \"B\":456,}"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidArrayContent ()
		{
			ReadToEnd (CreateReader ("[\"foo\":\"bar\"]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidArrayContent2 ()
		{
			ReadToEnd (CreateReader ("[123 456]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidArrayContent3 ()
		{
			ReadToEnd (CreateReader ("[,123,456]"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		[Category ("NotDotNet")] // likely .NET bug
		public void ReadInvalidArrayContent4 ()
		{
			ReadToEnd (CreateReader ("[123,456,]"));
		}

		[Test]
		public void ReadObjectRuntimeTypeAsAttribute ()
		{
			XmlDictionaryReader r = CreateReader ("{\"__type\":\"System.Int32\"}");
			r.Read ();
			AssertNode (0, "root", XmlNodeType.Element, String.Empty, "object", r, "#1");
			Assert.IsTrue (r.MoveToAttribute ("type"), "#2");
			AssertNode (0, "type", XmlNodeType.Attribute, "object", "object", r, "#3");
			Assert.IsTrue (r.MoveToAttribute ("__type"), "#4");
			AssertNode (0, "__type", XmlNodeType.Attribute, "System.Int32", "object", r, "#5");
			r.Read ();
			Assert.AreEqual (XmlNodeType.EndElement, r.NodeType, "#6");
		}

		[Test]
		public void ReadObjectRuntimeType ()
		{
			XmlDictionaryReader r = CreateReader ("{\"__type\":\"System.Int32\", \"foo\":true}");
			r.Read ();
			AssertNode (0, "root", XmlNodeType.Element, String.Empty, "object", r, "#1");
			Assert.IsTrue (r.MoveToAttribute ("type"), "#2");
			AssertNode (0, "type", XmlNodeType.Attribute, "object", "object", r, "#3");
			Assert.IsTrue (r.MoveToAttribute ("__type"), "#4");
			AssertNode (0, "__type", XmlNodeType.Attribute, "System.Int32", "object", r, "#5");
			r.Read ();
			Assert.AreEqual (XmlNodeType.Element, r.NodeType, "#6");
			Assert.AreEqual ("foo", r.LocalName, "#7");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadInvalidObjectRuntimeTypeValue ()
		{
			ReadToEnd (CreateReader ("{\"__type\":true}"));
		}

		[Test]
		public void ReadObjectRuntimeTypeIncorrectPosition ()
		{
			XmlReader r = CreateReader ("{\"foo\" : false, \"__type\" : \"System.Int32\"}");
			r.Read ();
			// When __type is not at the first content, it is not regarded as an attribute. Note that it is not treated as an error.
			Assert.IsFalse (r.MoveToAttribute ("__type"));
			r.Skip ();
		}

		[Test]
		public void ReadObjectRuntimeTypeInArray ()
		{
			XmlReader r = CreateReader (@"[{""__type"":""DCWithEnum:#MonoTests.System.Runtime.Serialization.Json"",""_colors"":0}]");
			r.Read ();
			Assert.AreEqual ("root", r.LocalName, "#1-1");
			Assert.AreEqual ("array", r.GetAttribute ("type"), "#1-2");
			r.Read ();
			Assert.AreEqual ("item", r.LocalName, "#2-1");
			Assert.AreEqual ("object", r.GetAttribute ("type"), "#2-2");
			Assert.IsNotNull (r.GetAttribute ("__type"), "#2-3");
			r.Read ();
		}

		[Test]
		public void Skip ()
		{
			XmlReader r = CreateReader ("{\"type\" : \"\", \"valid\" : \"0\", \"other\" : \"\"}");
			r.ReadStartElement ();
			r.MoveToContent ();
			Assert.AreEqual ("type", r.Name, "Skip-1");
			r.ReadElementContentAsString ();
			r.MoveToContent ();
			Assert.AreEqual ("valid", r.Name, "Skip-2");
			r.Skip ();
			Assert.AreEqual ("other", r.Name, "Skip-3");
		}

		[Test]
		public void Depth ()
		{
			XmlReader r = CreateReader ("{\"type\" : \"\", \"valid\" : \"0\"}");
			r.ReadStartElement ();
			r.Read ();
			Assert.AreEqual (2, r.Depth, "Depth-1");
		}

		[Test]
		public void UnicodeEncodingAutoDetect ()
		{
			var ms = new MemoryStream (Encoding.Unicode.GetBytes ("{\"type\" : \"\", \"valid\" : \"0\"}"));
			XmlReader r = JsonReaderWriterFactory.CreateJsonReader (ms, new XmlDictionaryReaderQuotas ());
			r.ReadStartElement ();
			r.Read ();
		}
	}
}
