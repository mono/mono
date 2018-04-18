//
// System.Xml.XmlWriterSettingsTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlWriterSettingsTests
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
			Assert.AreEqual (true, s.CheckCharacters);
			Assert.AreEqual (false, s.CloseOutput);
			Assert.AreEqual (ConformanceLevel.Document, s.ConformanceLevel);
			Assert.AreEqual (Encoding.UTF8, s.Encoding);
			Assert.AreEqual (false, s.Indent);
			Assert.AreEqual ("  ", s.IndentChars);
			Assert.AreEqual (Environment.NewLine, s.NewLineChars);
			Assert.AreEqual (false, s.NewLineOnAttributes);
			Assert.AreEqual (false, s.OmitXmlDeclaration);
			Assert.AreEqual (NewLineHandling.Replace, s.NewLineHandling);
			Assert.IsFalse (s.Async);
		}

		[Test]
		[Category ("MobileNotWorking")]
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
			Assert.IsTrue (data.Length != 0);
			string str = s.Encoding.GetString (data);
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"shift_jis\"?><root />", str);

			// For TextWriter it does not make sense
			StringWriter sw = new StringWriter ();
			w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><root />", sw.ToString ());
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
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConformanceLevelFragmentAndWriteStartDocument ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;
			s.OmitXmlDeclaration = true;
			XmlWriter w = XmlWriter.Create (Console.Out, s);
			w.WriteStartDocument ();
		}

		[Test]
		public void ConformanceLevelAuto ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Auto;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteElementString ("foo", "");
			w.Close ();
			Assert.AreEqual ("<foo />", sw.ToString ());
		}

		[Test]
		public void ConformanceLevelAuto_WriteStartDocument ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Auto;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartDocument ();
			w.WriteElementString ("foo", "");
			w.Close ();
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><foo />", sw.ToString ());
		}

		[Test]
		public void ConformanceLevelAuto_OmitXmlDecl_WriteStartDocument ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Auto;
			s.OmitXmlDeclaration = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartDocument ();
			w.WriteElementString ("foo", "");
			w.Close ();
			Assert.AreEqual ("<foo />", sw.ToString ());
		}

		[Test]
		public void ConformanceLevelDocument_OmitXmlDeclDeclaration ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Document;
			// LAMESPEC:
			// On MSDN, XmlWriterSettings.OmitXmlDeclaration is documented as:
			// "The XML declaration is always written if
			//  ConformanceLevel is set to Document, even 
			//  if OmitXmlDeclaration is set to true. "
			// but it is incorrect. It does consider 
			// OmitXmlDeclaration property.
			s.OmitXmlDeclaration = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteElementString ("foo", "");
			w.Close ();
			Assert.AreEqual ("<foo />", sw.ToString ());
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
			Assert.AreEqual (output, sw.ToString ());
		}

		[Test]
		public void SetEncodingNull ()
		{
			// null is allowed.
			new XmlWriterSettings ().Encoding = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NewLineCharsNull ()
		{
			new XmlWriterSettings ().NewLineChars = null;
		}

		[Test]
		public void CreateOmitXmlDeclaration ()
		{
			StringBuilder sb = new StringBuilder ();
			// Even if XmlWriter is allowed to write fragment,
			// DataContractSerializer never allows it to write
			// content in contentOnly mode.
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			//settings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter w = XmlWriter.Create (sb, settings);
			w.WriteStartElement ("root");
			w.WriteEndElement ();
			w.Flush ();
			Assert.AreEqual ("<root />", sb.ToString ());
		}

		[Test]
		public void NewLineOnAttributesMixedContent ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.NewLineOnAttributes = true;
			s.OmitXmlDeclaration = true;
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteAttributeString ("a", "v");
			w.WriteAttributeString ("b", "x");
			w.WriteString ("some string");
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual ("<root a=\"v\" b=\"x\">some string</root>", sw.ToString (), "#1");

			// mixed content: bug #81770
			string expected = "<root>some string<mixed\n    a=\"v\"\n    b=\"x\">some string</mixed></root>";
			s.Indent = true;
			sw = new StringWriter ();
			w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("root");
			w.WriteString ("some string");
			w.WriteStartElement ("mixed");
			w.WriteAttributeString ("a", "v");
			w.WriteAttributeString ("b", "x");
			w.WriteString ("some string");
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual (expected, sw.ToString ().Replace ("\r\n", "\n"), "#2");
		}

		[Test]
		public void OmitXmlDeclarationAndNewLine ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root/>");
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.NewLineChars = "\r\n";
			settings.NewLineHandling = NewLineHandling.Replace;
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;

			StringWriter sw = new StringWriter ();
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				doc.Save (xw);
			}
			// no heading newline.
			Assert.AreEqual ("<root />", sw.ToString ());
		}
		
		[Test] // surprisingly niche behavior yet caused bug #3231.
		public void IndentAndTopLevelWhitespaces ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw, new XmlWriterSettings () { Indent = true });
			xw.WriteProcessingInstruction ("xml", "version='1.0'");
			xw.WriteWhitespace ("\n");
			xw.WriteComment ("AAA");
			xw.WriteWhitespace ("\n");
			xw.WriteWhitespace ("\n");
			xw.WriteStartElement ("root");
			xw.Close ();
			string xml = @"<?xml version='1.0'?>
<!--AAA-->

<root />";
			Assert.AreEqual (xml.Replace ("\r\n", "\n"), sw.ToString ().Replace ("\r\n", "\n"), "#1");
		}

		[Test]
		public void NewlineHandlingNone ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw, new XmlWriterSettings () { NewLineHandling = NewLineHandling.None });
			xw.WriteStartElement("root");
			xw.WriteElementString("element", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteStartElement("element");
			xw.WriteAttributeString("attr", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.Close();
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element>lf\ncr\rcrlf\r\nht\t</element><element attr=\"lf\ncr\rcrlf\r\nht\t\" /></root>";
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void NewlineHandlingReplace ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw, new XmlWriterSettings () { 
					NewLineHandling = NewLineHandling.Replace,
					NewLineChars = "\n"
			});
			xw.WriteStartElement("root");
			xw.WriteElementString("element", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteStartElement("element");
			xw.WriteAttributeString("attr", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.Close();
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element>lf\ncr\ncrlf\nht\t</element><element attr=\"lf&#xA;cr&#xD;crlf&#xD;&#xA;ht&#x9;\" /></root>";
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void NewlineHandlingReplaceCRLF ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw, new XmlWriterSettings () { 
					NewLineHandling = NewLineHandling.Replace,
					NewLineChars = "\r\n"
			});
			xw.WriteStartElement("root");
			xw.WriteElementString("element", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteStartElement("element");
			xw.WriteAttributeString("attr", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.Close();
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element>lf\r\ncr\r\ncrlf\r\nht\t</element><element attr=\"lf&#xA;cr&#xD;crlf&#xD;&#xA;ht&#x9;\" /></root>";
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void NewlineHandlingEntitize ()
		{
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw, new XmlWriterSettings () { 
					NewLineHandling = NewLineHandling.Entitize,
					NewLineChars = "\n"
			});
			xw.WriteStartElement("root");
			xw.WriteElementString("element", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteStartElement("element");
			xw.WriteAttributeString("attr", "lf\ncr\rcrlf\r\nht\t");
			xw.WriteEndElement();
			xw.WriteEndElement();
			xw.Close();
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><root><element>lf\ncr&#xD;crlf&#xD;\nht\t</element><element attr=\"lf&#xA;cr&#xD;crlf&#xD;&#xA;ht&#x9;\" /></root>";
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void ReadonlyAsync ()
		{
			var sw = new StringWriter ();
			var s = new XmlWriterSettings ();
			var w = XmlWriter.Create (sw, s);
			w.Settings.Async = true;
		}

		[Test]
		public void AsyncPropagation ()
		{
			var sw = new StringWriter ();
			var s = new XmlWriterSettings ();
			s.Async = true;
			var w = XmlWriter.Create (sw, s);

			var c = s.Clone ();
			Assert.IsTrue (c.Async);
			c.Reset ();
			Assert.IsFalse (c.Async);

			var w2 = XmlWriter.Create (w, c);
			Assert.IsTrue (w2.Settings.Async);
		}

	}
}
