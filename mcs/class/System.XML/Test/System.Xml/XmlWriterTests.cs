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

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlWriterTests : Assertion
	{
		StringWriter writer;
		XmlTextWriter xtw;

		[SetUp]
		public void SetUp ()
		{
			writer = new StringWriter ();
			xtw = new XmlTextWriter (writer);
		}

		private void setupWriter ()
		{
			writer.GetStringBuilder ().Length = 0;
		}

		[Test]
		public void WriteNodeFullDocument ()
		{
			setupWriter ();
			string xml = "<?xml version='1.0'?><root />";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtw.WriteNode (xtr, false);
			AssertEquals (xml, writer.ToString ());

			// With encoding
			setupWriter ();
			xml = "<?xml version='1.0' encoding='iso-2022-jp'?><root />";
			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtw.WriteNode (xtr, false);
			AssertEquals (xml, writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeXmlDecl ()
		{
			setupWriter ();
			string xml = "<?xml version='1.0'?><root />";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtr.Read ();
			xtw.WriteNode (xtr, false);
			AssertEquals ("<?xml version='1.0'?>",
				 writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeEmptyElement ()
		{
			setupWriter ();
			string xml = "<root attr='value' attr2='value' />";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtw.WriteNode (xtr, false);
			AssertEquals (xml.Replace ("'", "\""),
				writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeNonEmptyElement ()
		{
			setupWriter ();
			string xml = @"<foo><bar></bar></foo>";
			xtw.WriteNode (new XmlTextReader (xml, XmlNodeType.Document, null), false);
			AssertEquals (xml, writer.ToString ());
		}

		[Test]
		public void WriteNodeSingleContentElement ()
		{
			setupWriter ();
			string xml = "<root attr='value' attr2='value'><foo /></root>";
			StringReader sr = new StringReader (xml);
			XmlTextReader xtr = new XmlTextReader (sr);
			xtw.WriteNode (xtr, false);
			AssertEquals (xml.Replace ("'", "\""),
				writer.ToString ());
			xtr.Close ();
		}

		[Test]
		public void WriteNodeNone ()
		{
			setupWriter ();
			XmlTextReader xtr = new XmlTextReader ("", XmlNodeType.Element, null);
			xtr.Read ();
			xtw.WriteNode (xtr, false); // does not report any errors
			xtr.Close ();
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void WriteNodeError ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root>", XmlNodeType.Document, null);
			xtr.Read ();
			try {
				xtr.Read ();
			} catch (XmlException ex) {
			}
			XmlTextWriter xtw = new XmlTextWriter (new StringWriter ());
			xtw.WriteNode (xtr, false);
		}

		[Test]
		public void WriteSurrogateCharEntity ()
		{
			setupWriter ();
			xtw.WriteSurrogateCharEntity ('\udfff', '\udb00');
			AssertEquals ("&#xD03FF;", writer.ToString ());

			try {
				xtw.WriteSurrogateCharEntity ('\ud800', '\udc00');
				Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\udbff', '\ud800');
				Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\ue000', '\ud800');
				Fail ();
			} catch {
			}
			try {
				xtw.WriteSurrogateCharEntity ('\udfff', '\udc00');
				Fail ();
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
			AssertEquals ("StartElementOverride.NS", null, xw.NS);
			AssertEquals ("StartElementOverride.Prefix", null, xw.Prefix);
			xw.NS = String.Empty;
			xw.Prefix = String.Empty;
			xw.WriteStartElement ("test", "urn:hoge");
			AssertEquals ("StartElementOverride.NS", "urn:hoge", xw.NS);
			AssertEquals ("StartElementOverride.Prefix", null, xw.Prefix);
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
