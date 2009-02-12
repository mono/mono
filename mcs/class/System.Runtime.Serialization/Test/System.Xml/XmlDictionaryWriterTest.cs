//
// XmlDictionaryWriterTest.cs
//
// Author:
//   Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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

#define TRACE

using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	class DelegatingXmlDictionaryWriter : XmlDictionaryWriter
	{
		XmlWriter d;

		public DelegatingXmlDictionaryWriter (XmlWriter delegateTo)
		{
			d = delegateTo;
		}

		public override WriteState WriteState {
			get {return d.WriteState;}
		}

		public override void Close ()
		{
			d.Close ();
		}

		public override void Flush ()
		{
			d.Flush ();
		}

		public override string LookupPrefix (string ns)
		{
			return d.LookupPrefix (ns);
		}

		public override void WriteBase64 (byte[] buffer, int index, int count)
		{
			d.WriteBase64 (buffer, index, count);
		}

		public override void WriteCData (string text)
		{
			d.WriteCData (text);
		}

		public override void WriteCharEntity (char ch)
		{
			d.WriteCharEntity (ch);
		}

		public override void WriteChars (char[] buffer, int index, int count)
		{
			d.WriteChars (buffer, index, count);
		}

		public override void WriteComment (string text)
		{
			d.WriteComment (text);
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
			d.WriteDocType (name, pubid, sysid, subset);
		}

		public override void WriteEndAttribute ()
		{
			d.WriteEndAttribute ();
		}

		public override void WriteEndDocument ()
		{
			d.WriteEndDocument ();
		}

		public override void WriteEndElement ()
		{
			d.WriteEndElement ();
		}

		public override void WriteEntityRef (string name)
		{
			d.WriteEntityRef (name);
		}

		public override void WriteFullEndElement ()
		{
			d.WriteFullEndElement ();
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
			d.WriteProcessingInstruction (name, text);
		}

		public override void WriteRaw (char[] buffer, int index, int count)
		{
			d.WriteRaw (buffer, index, count);
		}

		public override void WriteRaw (string data)
		{
			d.WriteRaw (data);
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
			d.WriteStartAttribute (prefix, localName, ns);
		}

		public override void WriteStartDocument ()
		{
			d.WriteStartDocument ();
		}

		public override void WriteStartDocument (bool standalone)
		{
			d.WriteStartDocument (standalone);
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
			d.WriteStartElement (prefix, localName, ns);
		}

		public override void WriteString (string text)
		{
			d.WriteString (text);
		}

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
			d.WriteSurrogateCharEntity (lowChar, highChar);
		}

		public override void WriteWhitespace (string ws)
		{
			d.WriteWhitespace (ws);
		}
	}

	[TestFixture]
	public class XmlDictionaryWriterTest
	{
		XmlDictionaryWriter writer;
		StringBuilder       contents;

		[SetUp]
		public void Setup()
		{
			var s = new XmlWriterSettings ();
			s.ConformanceLevel   = ConformanceLevel.Fragment;
			s.OmitXmlDeclaration = true;
			writer = new DelegatingXmlDictionaryWriter (
					XmlWriter.Create (contents = new StringBuilder (), s));
		}

		[TearDown]
		public void TearDown()
		{
			contents = null;
			writer   = null;
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void WriteElementString_localNameNull ()
		{
			XmlDictionaryString localName = null, namespaceUri = null;
			string prefix = null, value = null;
			writer.WriteElementString (prefix, localName, namespaceUri, value);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void WriteElementString_PrefixWithEmptyNamespace ()
		{
			XmlDictionary d = new XmlDictionary ();
			XmlDictionaryString localName = d.Add ("foo"), namespaceUri = null;
			string prefix = "ns", value = null;
			writer.WriteElementString (prefix, localName, namespaceUri, value);
		}

		[Test]
		public void WriteElementString ()
		{
			XmlDictionary d = new XmlDictionary ();
			XmlDictionaryString foo     = d.Add ("foo");
			XmlDictionaryString fooUri  = d.Add ("urn:bar");
			string             ns      = "ns";

			// 
			// Skipping empty string values because Mono & .NET generate
			// different XML: <foo /> (Mono) vs. <foo></foo> (.NET).
			//
			writer.WriteElementString (null, foo, null, "data");
			// writer.WriteElementString (null, foo, null, "");
			writer.WriteElementString (null, foo, null, null);

			writer.WriteElementString (null, foo, fooUri, "data");
			// writer.WriteElementString (null, foo, fooUri, "");
			writer.WriteElementString (null, foo, fooUri, null);

			writer.WriteElementString (ns, foo, fooUri, "data");
			// writer.WriteElementString (ns, foo, fooUri, "");
			writer.WriteElementString (ns, foo, fooUri, null);
			writer.Flush ();

			Assert.AreEqual (
					"<foo>data</foo><foo />" +
					"<foo xmlns=\"urn:bar\">data</foo><foo xmlns=\"urn:bar\" />" +
					"<ns:foo xmlns:ns=\"urn:bar\">data</ns:foo><ns:foo xmlns:ns=\"urn:bar\" />", 
					contents.ToString ());
		}

		[Test]
		public void WriteValue_Guid ()
		{
			writer.WriteValue (new Guid ());
			writer.Flush ();

			Assert.AreEqual (new Guid().ToString (), contents.ToString ());
		}

		[Test]
		public void WriteValue_TimeSpan ()
		{
			writer.WriteValue (new TimeSpan ());
			writer.Flush ();

			Assert.AreEqual (XmlConvert.ToString (new TimeSpan()), contents.ToString ());
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void WriteValue_UniqueId_IdNull ()
		{
			UniqueId value = null;
			writer.WriteValue (value);
		}

		[Test]
		public void WriteValue_UniqueId ()
		{
			writer.WriteValue (new UniqueId (new Guid ()));
			writer.WriteValue (new UniqueId ("string"));
			writer.Flush ();

			Assert.AreEqual ("urn:uuid:" + new Guid ().ToString () + "string",
					contents.ToString ());
		}
	}
}
