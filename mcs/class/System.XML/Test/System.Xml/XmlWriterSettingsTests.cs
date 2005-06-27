//
// System.Xml.XmlWriterSettingsTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

#if NET_2_0
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlWriterSettingsTests : Assertion
	{
		[Test]
		public void DefaultValue ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			DefaultValue (s);
			s.Reset ();
			DefaultValue (s);
		}

		private void DefaultValue (XmlWriterSettings s)
		{
			AssertEquals (true, s.CheckCharacters);
			AssertEquals (false, s.CloseOutput);
			AssertEquals (ConformanceLevel.Document, s.ConformanceLevel);
			AssertEquals (Encoding.UTF8, s.Encoding);
			AssertEquals (false, s.Indent);
			AssertEquals ("  ", s.IndentChars);
			AssertEquals (Environment.NewLine, s.NewLineChars);
			AssertEquals (false, s.NewLineOnAttributes);
			AssertEquals (true, s.NormalizeNewLines);
			AssertEquals (false, s.OmitXmlDeclaration);
		}

		[Test]
		public void EncodingTest ()
		{
			// For Stream it makes sense
			XmlWriterSettings s = new XmlWriterSettings ();
			s.Encoding = Encoding.GetEncoding ("shift_jis");
			MemoryStream ms = new MemoryStream ();
			XmlWriter w = XmlWriter.Create (ms, s);
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Close ();
			byte [] data = ms.ToArray ();
			Assert (data.Length != 0);
			string str = s.Encoding.GetString (data);
			AssertEquals ("<?xml version=\"1.0\" encoding=\"shift_jis\"?><root />", str);

			// For TextWriter it does not make sense
			StringWriter sw = new StringWriter ();
			w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Close ();
			AssertEquals ("<?xml version=\"1.0\" encoding=\"utf-16\"?><root />", sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CheckCharactersTest ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteString ("\0"); // invalid
			w.WriteEndElement ();
			w.Close ();
		}

		[Test]
		public void CheckCharactersFalseTest ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.CheckCharacters = false;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteString ("\0"); // invalid
			w.WriteEndElement ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void CloseOutputTest ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.CloseOutput = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Close ();
			sw.Write ("more"); // not allowed
		}

		[Test]
		[Ignore ("Write Test!")]
		public void ConformanceLevelTest ()
		{
			throw new NotImplementedException ();
		}

		[Test]
		public void IndentationAndFormatting ()
		{
			// Test for Indent, IndentChars, NewLineOnAttributes,
			// NewLineChars and OmitXmlDeclaration.
			string output = "<root\n    attr=\"value\"\n    attr2=\"value\">\n    <child>test</child>\n</root>";
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			s.Indent = true;
			s.IndentChars = "    ";
			s.NewLineChars = "\n";
			s.NewLineOnAttributes = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteAttributeString ("attr", "value");
			w.WriteAttributeString ("attr2", "value");
			w.WriteStartElement ("child");
			w.WriteString ("test");
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			AssertEquals (output, sw.ToString ());
		}

		[Test]
		[Ignore ("Write Test!")]
		public void NormalizeNewLinesTest ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
