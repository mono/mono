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
		[Test]
		public void Create_File ()
		{
			string file = Path.GetTempFileName ();
			XmlWriter writer = XmlWriter.Create (file);
			Assert.IsNotNull (writer.Settings, "#A1");
			//Assert.IsTrue (writer.Settings.CloseOutput, "#A2");
			writer.Close ();
			File.Delete (file);

			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			writer = XmlWriter.Create (file, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#B1");
			Assert.IsTrue (settings.CloseOutput, "#B2");
			writer.Close ();
			File.Delete (file);

			writer = XmlWriter.Create (file, (XmlWriterSettings) null);
			Assert.IsNotNull (writer.Settings, "#C1");
			//Assert.IsTrue (writer.Settings.CloseOutput, "#C2");
			writer.Close ();
			File.Delete (file);

			settings = new XmlWriterSettings ();
			writer = XmlWriter.Create (file, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#D1");
			//Assert.IsTrue (writer.Settings.CloseOutput, "#D2");
			writer.Close ();
			File.Delete (file);

			writer = XmlWriter.Create (file);
			Assert.IsNotNull (writer.Settings, "#E1");
			//Assert.IsTrue (writer.Settings.CloseOutput, "#E2");
			writer.Close ();
			File.Delete (file);
		}

		[Test]
		public void Create_Stream ()
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriter writer = XmlWriter.Create (ms);
			Assert.IsNotNull (writer.Settings, "#A1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#A2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#A3");

			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			writer = XmlWriter.Create (ms, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#B1");
			Assert.IsTrue (settings.CloseOutput, "#B2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B3");

			ms = new MemoryStream ();
			settings = new XmlWriterSettings ();
			writer = XmlWriter.Create (ms, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#C1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#C2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#C3");

			ms = new MemoryStream ();
			writer = XmlWriter.Create (ms, (XmlWriterSettings) null);
			Assert.IsNotNull (writer.Settings, "#D1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#D2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#D3");
		}

		[Test]
		public void Create_TextWriter ()
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriter writer = XmlWriter.Create (new StreamWriter (ms));
			Assert.IsNotNull (writer.Settings, "#A1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#A2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#A3");

			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			writer = XmlWriter.Create (new StreamWriter (ms), settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#B1");
			Assert.IsTrue (settings.CloseOutput, "#B2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B3");

			ms = new MemoryStream ();
			settings = new XmlWriterSettings ();
			writer = XmlWriter.Create (new StreamWriter (ms), settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#C1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#C2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#C3");

			ms = new MemoryStream ();
			writer = XmlWriter.Create (new StreamWriter (ms), (XmlWriterSettings) null);
			Assert.IsNotNull (writer.Settings, "#D1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#D2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#D3");
		}

		[Test]
		public void Create_XmlWriter ()
		{
			MemoryStream ms = new MemoryStream ();
			XmlTextWriter xtw = new XmlTextWriter (ms, Encoding.UTF8);
			XmlWriter writer = XmlWriter.Create (xtw);
			Assert.IsNotNull (writer.Settings, "#A1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#A2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#A3");

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, Encoding.UTF8);
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			writer = XmlWriter.Create (xtw, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#B1");
			//Assert.IsFalse (writer.Settings.CloseOutput, "#B2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B3");

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, Encoding.UTF8);
			settings = new XmlWriterSettings ();
			writer = XmlWriter.Create (xtw, settings);
			//Assert.IsFalse (object.ReferenceEquals (settings, writer.Settings), "#C1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#C2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#C3");

			ms = new MemoryStream ();
			xtw = new XmlTextWriter (ms, Encoding.UTF8);
			writer = XmlWriter.Create (xtw, (XmlWriterSettings) null);
			Assert.IsNotNull (writer.Settings, "#D1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#D2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#D3");
		}

		[Test]
		public void Create_XmlWriter2 ()
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			XmlWriter xw = XmlWriter.Create (ms, settings);
			XmlWriter writer = XmlWriter.Create (xw, new XmlWriterSettings ());
			Assert.IsNotNull (writer.Settings, "#A1");
			Assert.IsFalse (writer.Settings.CloseOutput, "#A2");
			writer.Close ();
			Assert.IsTrue (ms.CanWrite, "#A3");

			ms = new MemoryStream ();
			settings = new XmlWriterSettings ();
			settings.CloseOutput = true;
			settings.OmitXmlDeclaration = true;
			xw = XmlWriter.Create (ms, settings);
			writer = XmlWriter.Create (xw, new XmlWriterSettings ());
			Assert.IsNotNull (writer.Settings, "#B1");
			Assert.IsTrue (writer.Settings.CloseOutput, "#B2");
			Assert.IsTrue (writer.Settings.OmitXmlDeclaration, "#B3");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B3");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void CreateWriter_AttributeNamespacesXmlnsXmlns ()
		{
			// Unlike XmlTextWriter, null namespace is not ignored.
			XmlWriter w = XmlWriter.Create (new StringWriter ());
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("xmlns", "xmlns", null, "http://abc.def");
		}

		XmlWriter CreateWriter (TextWriter tw)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			XmlWriter w = XmlWriter.Create (tw, s);
			w.WriteStartElement ("root");
			return w;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteValueNull ()
		{
			XmlWriter w = CreateWriter (TextWriter.Null);
			w.WriteValue ((object) null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))] // it throws somewhat funny exception
		public void WriteValueNonExistentQName ()
		{
			XmlWriter w = CreateWriter (TextWriter.Null);
			w.WriteValue (new XmlQualifiedName ("foo", "urn:foo"));
		}

		[Test]
		public void WriteValueEmptyQName ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter w = CreateWriter (sw);
			w.WriteValue (XmlQualifiedName.Empty);
			w.Close ();
		}

		[Test]
		public void WriteValueQName ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter w = CreateWriter (sw);
			w.WriteAttributeString ("xmlns", "x", "http://www.w3.org/2000/xmlns/", "urn:foo");
			w.WriteValue (new XmlQualifiedName ("foo", "urn:foo"));
			w.Close ();
			Assert.AreEqual ("<root xmlns:x=\"urn:foo\">x:foo</root>", sw.ToString ());
		}

		[Test]
		public void WriteValueTimeSpan ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter w = CreateWriter (sw);
			w.WriteValue (TimeSpan.FromSeconds (5));
			w.Close ();
			Assert.AreEqual ("<root>PT5S</root>", sw.ToString ());
		}

		[Test]
		public void WriteValueArray ()
		{
			StringWriter sw = new StringWriter ();
			XmlWriter w = CreateWriter (sw);
			w.WriteValue (new int [] {1, 2, 3});
			w.WriteValue (new int [] {4, 5, 6});
			w.Close ();
			Assert.AreEqual ("<root>1 2 34 5 6</root>", sw.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))] // it throws somewhat funny exception
		public void WriteValueTextReader ()
		{
			// it is documented as supported, but actually isn't.
			XmlWriter w = CreateWriter (TextWriter.Null);
			w.WriteValue (new StringReader ("foobar"));
		}

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

		[Test]
		public void WriteNodeXPathNavigator ()
		{
			string xml = "<A xmlns='urn:x'><B xmlns='urn:y' /></A>";
			XPathNavigator nav = new XPathDocument (new StringReader(xml)).CreateNavigator ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteNode (nav, false);
			w.Close ();
			AssertType.AreEqual ("<A xmlns=\"urn:x\"><B xmlns=\"urn:y\" /></A>", sw.ToString ());
		}

		[Test]
		public void WriteNodeXPathNavigatorAttribute ()
		{
			string xml = "<!DOCTYPE root [<!ELEMENT root EMPTY> <!ATTLIST root implicit NMTOKEN 'nam'>]><root attr='val' />";
			XPathNavigator nav = new XPathDocument (new StringReader (xml)).CreateNavigator ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			StringWriter sw = new StringWriter ();
			XmlWriter w = XmlWriter.Create (sw, s);
			w.WriteStartElement ("hoge");
			nav.MoveToFirstChild ();
			nav.MoveToFirstAttribute ();
			w.WriteNode (nav, false);
			nav.MoveToNextAttribute ();
			w.WriteNode (nav, false);
			w.Close ();
			AssertType.AreEqual ("<hoge />", sw.ToString ());
		}

		[Test]
		public void WriteStringDifferentBehavior ()
		{
			// Messy implementation difference.
			// from XmlTextWriter -> <foo />
			// from XmlWriter.XCreate() -> <foo></foo>
			var sw = new StringWriter ();
			var xw = XmlWriter.Create (sw);
			xw.WriteStartElement ("foo");
			xw.WriteString ("");
			xw.WriteEndElement ();
			xw.Close ();
			AssertType.AreEqual ("<?xml version='1.0' encoding='utf-16'?><foo></foo>".Replace ('\'', '"'), sw.ToString ());
		}

		//          |       |      |returns the same reader
		// source   |wrapper|result| |checks state (and err)
		// overrides|setting|      | | |test name
		// ---------+----------------------------
		//      -   | -(Doc)| Doc  | |x|0:CreateNOCL
		//          | Auto  | Doc  | |x|1:CreateNOCLSettingsCLAuto
		//          | Doc   | Doc  | |x|2:CreateNOCLSettingsCLDoc
		//          | Frag  | Doc  | |x|3:CreateNOCLSettingsCLFrag
		// Auto     | -(Doc)| Doc  | | |4:CreateCLAuto
		//          | Auto  | Auto |=|x|5:CreateCLAutoSettingsCLAuto
		//          | Doc   | Doc  | | |6:CreateCLAutoSettingsCLDoc
		//          | Frag  | Frag | | |7:CreateCLAutoSettingsCLFrag
		// Document | -(Doc)| Doc  |=|x|8:CreateCLDoc
		//          | Auto  | Doc  |=|x|9:CreateCLDocSettingsCLAuto
		//          | Doc   | Doc  |=|x|A:CreateCLDocSettingsCLDoc
		//          | Frag  | Frag | | |B:CreateCLDocSettingsCLFrag
		// Fragment | -(Doc)| Doc  | | |C:CreateCLFrag
		//          | Auto  | Frag |=|x|D:CreateCLFragSettingsCLAuto
		//          | Doc   | Doc  | | |E:CreateCLFragSettingsCLDoc
		//          | Frag  | Frag |=|x|F:CreateCLFragSettingsCLFrag
		//
		// What we can see from above:
		// - default ConformanceLevel is Document. (0 4 8 C)
		// - If wrapper is Auto, it delegates to source. (1 5 9 D)
		//   - Auto can happen only if both inputs are Auto (5)
		// - If wrapper is Frag, all but default becomes Frag (7 B F)
		//   - with default it becomes Document (3)
		//     So a default value is likely stronger(!)
		// - Basically, when there is no difference between the wrapper
		//   and the source settings, it does not create wrapper.

		[Test]
		public void CreateCLAuto ()
		{
			ConformanceLevelAuto cl = new ConformanceLevelAuto ();
			XmlWriter xw = XmlWriter.Create (cl);
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLDoc ()
		{
			ConformanceLevelDocument cl = new ConformanceLevelDocument ();
			XmlWriter xw = XmlWriter.Create (cl);
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
		}

		[Test]
		public void CreateCLFrag ()
		{
			ConformanceLevelFragment cl = new ConformanceLevelFragment ();
			XmlWriter xw = XmlWriter.Create (cl);
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateNOCL ()
		{
			InvalidWriteState cl = new InvalidWriteState ();
			Assert.IsNull (cl.Settings, "#0");
			XmlWriter xw = XmlWriter.Create (cl);
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateNOCLSettingsCLAuto ()
		{
			InvalidWriteState cl = new InvalidWriteState ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Auto });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateNOCLSettingsCLDoc ()
		{
			InvalidWriteState cl = new InvalidWriteState ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Document });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateNOCLSettingsCLFrag ()
		{
			InvalidWriteState cl = new InvalidWriteState ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Fragment });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLAutoSettingsCLAuto ()
		{
			ConformanceLevelAuto cl = new ConformanceLevelAuto ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Auto });
			Assert.AreEqual (ConformanceLevel.Auto, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
		}

		[Test]
		public void CreateCLAutoSettingsCLDoc ()
		{
			ConformanceLevelAuto cl = new ConformanceLevelAuto ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Document });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		public void CreateCLAutoSettingsCLFrag ()
		{
			ConformanceLevelAuto cl = new ConformanceLevelAuto ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Fragment });
			Assert.AreEqual (ConformanceLevel.Fragment, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLDocSettingsCLAuto ()
		{
			ConformanceLevelDocument cl = new ConformanceLevelDocument ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Auto });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLDocSettingsCLDoc ()
		{
			ConformanceLevelDocument cl = new ConformanceLevelDocument ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Document });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
		}

		[Test]
		public void CreateCLDocSettingsCLFrag ()
		{
			ConformanceLevelDocument cl = new ConformanceLevelDocument ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Fragment });
			Assert.AreEqual (ConformanceLevel.Fragment, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLFragSettingsCLAuto ()
		{
			ConformanceLevelFragment cl = new ConformanceLevelFragment ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Auto });
			Assert.AreEqual (ConformanceLevel.Fragment, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
		}

		[Test]
		public void CreateCLFragSettingsCLDoc ()
		{
			ConformanceLevelFragment cl = new ConformanceLevelFragment ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Document });
			Assert.AreEqual (ConformanceLevel.Document, xw.Settings.ConformanceLevel, "#1");
			Assert.AreNotEqual (xw, cl, "#2");
			WriteState state = xw.WriteState;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CreateCLFragSettingsCLFrag ()
		{
			ConformanceLevelFragment cl = new ConformanceLevelFragment ();
			XmlWriter xw = XmlWriter.Create (cl, new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Fragment });
			Assert.AreEqual (ConformanceLevel.Fragment, xw.Settings.ConformanceLevel, "#1");
			Assert.AreEqual (xw, cl, "#2"); // equal
			WriteState state = xw.WriteState;
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


	class InvalidWriteState : XmlWriter {
		public override void Close () { }
		public override void Flush () { }
		public override string LookupPrefix (string ns) { return null; }
		public override void WriteBase64 (byte [] buffer, int index, int count) { }
		public override void WriteCData (string text) { }
		public override void WriteCharEntity (char ch) { }
		public override void WriteChars (char [] buffer, int index, int count) { }
		public override void WriteComment (string text) { }
		public override void WriteDocType (string name, string pubid, string sysid, string subset) { }
		public override void WriteEndAttribute () { }
		public override void WriteEndDocument () { }
		public override void WriteEndElement () { }
		public override void WriteEntityRef (string name) { }
		public override void WriteFullEndElement () { }
		public override void WriteProcessingInstruction (string name, string text) {}
		public override void WriteRaw (string data) {}
		public override void WriteRaw (char [] buffer, int index, int count) {}
		public override void WriteStartAttribute (string prefix, string localName, string ns) {}
		public override void WriteStartDocument (bool standalone) {}
		public override void WriteStartDocument () {}
		public override void WriteStartElement (string prefix, string localName, string ns) {}
		public override void WriteString (string text) {}
		public override void WriteSurrogateCharEntity (char lowChar, char highChar) {}
		public override void WriteWhitespace (string ws) {}

		public override WriteState WriteState {
			get { throw new InvalidOperationException (); }
		}
	}

	class ConformanceLevelAuto : InvalidWriteState {
		public override XmlWriterSettings Settings {
			get {
				return new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Auto };
			}
		}
	}

	class ConformanceLevelDocument : InvalidWriteState {
		public override XmlWriterSettings Settings {
			get {
				return new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Document };
			}
		}
	}

	class ConformanceLevelFragment : InvalidWriteState {
		public override XmlWriterSettings Settings {
			get {
				return new XmlWriterSettings () { ConformanceLevel = ConformanceLevel.Fragment };
			}
		}
	}

}

