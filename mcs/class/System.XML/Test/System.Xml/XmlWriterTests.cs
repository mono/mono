//
// System.Xml.XmlTextWriterTests
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2003 Atsushi Enomoto
// (C) 2003 Martin Willemoes Hansen
//
//
//  This class mainly checks inheritance and behaviors of XmlWriter.
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

using AssertType = NUnit.Framework.Assert;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlWriterTests
	{
		StringWriter writer;
		XmlTextWriter xtw;

		[SetUp]
		public void SetUp ()
		{
			writer = new StringWriter ();
			xtw = new XmlTextWriter (writer);
		}

		[Test]
		public void WriteNodeFullDocument ()
		{
			string xml = "<?xml version='1.0'?><root />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtw.WriteNode (xtr, false);
			Assert.AreEqual (xml, writer.ToString ());

			writer.GetStringBuilder ().Length = 0;

			// With encoding
			xml = "<?xml version='1.0' encoding='iso-2022-jp'?><root />";
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtw.WriteNode (xtr, false);
			Assert.AreEqual (xml, writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeXmlDecl ()
		{
			string xml = "<?xml version='1.0'?><root />";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtr.Read ();
			xtw.WriteNode (xtr, false);
			Assert.AreEqual ("<?xml version='1.0'?>",
				 writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeEmptyElement ()
		{
			string xml = "<root attr='value' attr2='value' />";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtw.WriteNode (xtr, false);
			Assert.AreEqual (xml.Replace ("'", "\""),
				writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeNonEmptyElement ()
		{
			string xml = @"<foo><bar></bar></foo>";
			xtw.WriteNode (new XmlTextReader (xml, XmlNodeType.Document, null), false);
			Assert.AreEqual (xml, writer.ToString ());
		}

		[Test]
		public void WriteNodeSingleContentElement ()
		{
			string xml = "<root attr='value' attr2='value'><foo /></root>";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtw.WriteNode (xtr, false);
			Assert.AreEqual (xml.Replace ("'", "\""),
				writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeNone ()
		{
			XmlTextReader xtr = new XmlTextReader ("", XmlNodeType.Element, null);
			xtr.Read ();
			xtw.WriteNode (xtr, false); // does not report any errors
			xtr.Close ();
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // enbugged in 2.0
#endif
		[ExpectedException (typeof (XmlException))]
		public void WriteNodeError ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root>", XmlNodeType.Document, null);
			xtr.Read ();
			try {
				xtr.Read ();
			} catch {
			}
			XmlTextWriter xtw = new XmlTextWriter (new StringWriter ());
			xtw.WriteNode (xtr, false);
		}

		[Test]
		public void WriteSurrogateCharEntity ()
		{
			xtw.WriteSurrogateCharEntity ('\udfff', '\udb00');
			Assert.AreEqual ("&#xD03FF;", writer.ToString ());

			try {
				xtw.WriteSurrogateCharEntity ('\ud800', '\udc00');
				Assert.Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\udbff', '\ud800');
				Assert.Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\ue000', '\ud800');
				Assert.Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\udfff', '\udc00');
				Assert.Fail ();
			} catch {
			}
		}

		// MS.NET's not-overriden XmlWriter.WriteStartElement(name)
		// invokes WriteStartElement(null, name, null). 
		// WriteStartElement(name, ns) invokes (null, name, ns), too.
		[Test]
		public void StartElement ()
		{
			StartElementTestWriter xw = new StartElementTestWriter ();
			xw.WriteStartDocument ();
			xw.WriteStartElement ("test");
			Assert.IsNull (xw.NS, "StartElementOverride.NS");
			Assert.IsNull (xw.Prefix, "StartElementOverride.Prefix");
			xw.NS = String.Empty;
			xw.Prefix = String.Empty;
			xw.WriteStartElement ("test", "urn:hoge");
			Assert.AreEqual ("urn:hoge", xw.NS, "StartElementOverride.NS");
			Assert.IsNull (null, xw.Prefix, "StartElementOverride.Prefix");
		}
		
		class StartElementTestWriter : DefaultXmlWriter
		{
			public StartElementTestWriter () : base () {}
			public string NS = String.Empty;
			public string Prefix = String.Empty;

			public override void WriteStartElement (string prefix, string localName, string ns)
			{
				this.NS = ns;
				this.Prefix = prefix;
			}
		}

		[Test]
		public void WriteAttributes ()
		{
			string xml = "<root><test a='b' c='d' /><b /></root>";
			XmlTextReader xtr = new XmlTextReader (xml,
				XmlNodeType.Document, null);
			xtw.QuoteChar = '\'';
			xtr.Read ();
			xtw.WriteStartElement ("root"); // <root
			xtr.Read ();
			xtw.WriteStartElement ("test"); // ><test
			xtw.WriteAttributes (xtr, false); // a='b' c='d'
			Assert.AreEqual (XmlNodeType.Element, xtr.NodeType);
			xtw.WriteEndElement (); // />
			xtw.WriteStartElement ("b"); // <b
			xtw.WriteEndElement (); // />
			xtw.WriteEndElement (); // </root>
			xtw.Close ();
			Assert.AreEqual (xml, writer.ToString ());
		}

#if NET_2_0

		XPathNavigator GetNavigator (string xml)
		{
			return new XPathDocument (XmlReader.Create (
				new StringReader (xml))).CreateNavigator ();
		}

		string WriteNavigator (XPathNavigator nav, bool defattr)
		{
			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				w.WriteNode (nav, defattr);
			}
			return sw.ToString ();
		}

		[Test]
		public void WriteNodeNavigator1 ()
		{
			XPathNavigator nav = GetNavigator ("<root>test<!-- comment --></root>");
			// at Root
			AssertType.AreEqual ("<root>test<!-- comment --></root>", WriteNavigator (nav, false), "#1");
			// at document element
			nav.MoveToFirstChild ();
			AssertType.AreEqual ("<root>test<!-- comment --></root>", WriteNavigator (nav, false), "#2");
			// at text
			nav.MoveToFirstChild ();
			AssertType.AreEqual ("test", WriteNavigator (nav, false), "#3");

			// at comment
			nav.MoveToNext ();
			AssertType.AreEqual ("<!-- comment -->", WriteNavigator (nav, false), "#4");
		}

		string WriteSubtree (XPathNavigator nav)
		{
			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlWriter w = XmlWriter.Create (sw, settings)) {
				nav.WriteSubtree(w);
			}
			return sw.ToString ();
		}

		[Test]
		public void NavigatorWriteSubtree1 ()
		{
			XPathNavigator nav = GetNavigator ("<root>test<!-- comment --></root>");
			// at Root
			AssertType.AreEqual ("<root>test<!-- comment --></root>", WriteSubtree (nav), "#1");
			// at document element
			nav.MoveToFirstChild ();
			AssertType.AreEqual ("<root>test<!-- comment --></root>", WriteSubtree (nav), "#2");
			// at text
			nav.MoveToFirstChild ();
			AssertType.AreEqual ("test", WriteSubtree (nav), "#3");

			// at comment
			nav.MoveToNext ();
			AssertType.AreEqual ("<!-- comment -->", WriteSubtree (nav), "#4");
		}
#endif

	}

	internal class DefaultXmlWriter : XmlWriter
	{
		public DefaultXmlWriter () : base ()
		{
		}

		public override void Close ()
		{
		}

		public override void Flush ()
		{
		}

		public override string LookupPrefix (string ns)
		{
			return null;
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
		}

		public override void WriteCData (string text)
		{
		}

		public override void WriteCharEntity (char ch)
		{
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
		}

		public override void WriteComment (string text)
		{
		}

		public override void WriteDocType (string name, string pubid, string sysid, string subset)
		{
		}

		public override void WriteEndAttribute ()
		{
		}

		public override void WriteEndDocument ()
		{
		}

		public override void WriteEndElement ()
		{
		}

		public override void WriteEntityRef (string name)
		{
		}

		public override void WriteFullEndElement ()
		{
		}

		public override void WriteName (string name)
		{
		}

		public override void WriteNmToken (string name)
		{
		}

		public override void WriteNode (XmlReader reader, bool defattr)
		{
		}

		public override void WriteProcessingInstruction (string name, string text)
		{
		}

		public override void WriteQualifiedName (string localName, string ns)
		{
		}

		public override void WriteRaw (string data)
		{
		}

		public override void WriteRaw (char [] buffer, int index, int count)
		{
		}

		public override void WriteStartAttribute (string prefix, string localName, string ns)
		{
		}

		public override void WriteStartDocument (bool standalone)
		{
		}

		public override void WriteStartDocument ()
		{
		}

		public override void WriteStartElement (string prefix, string localName, string ns)
		{
		}

		public override void WriteString (string text)
		{
		}

		public override void WriteSurrogateCharEntity (char lowChar, char highChar)
		{
		}

		public override void WriteWhitespace (string ws)
		{
		}

		public override WriteState WriteState {
			get {
				return WriteState.Start;
			}
		}

		public override string XmlLang {
			get {
				return null;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				return XmlSpace.None;
			}
		}

	}
}
