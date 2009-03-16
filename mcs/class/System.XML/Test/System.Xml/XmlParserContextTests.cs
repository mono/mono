//
// Unit tests for System.Xml.XmlParserContext
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2008 Gert Driesen
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlParserContextTests
	{
		[Test]
		public void Constructor1 ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);

			XmlParserContext pc = new XmlParserContext (doc.NameTable,
				ns, "whatever", XmlSpace.None);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#A1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#A2");
			Assert.IsNull (pc.Encoding, "#A3");
			Assert.AreEqual (string.Empty, pc.InternalSubset, "#A4");
			Assert.AreSame (ns, pc.NamespaceManager, "#A5");
			Assert.AreSame (doc.NameTable, pc.NameTable, "#A6");
			Assert.AreEqual (string.Empty, pc.PublicId, "#A7");
			Assert.AreEqual (string.Empty, pc.SystemId, "#A8");
			Assert.AreEqual ("whatever", pc.XmlLang, "#A9");
			Assert.AreEqual (XmlSpace.None, pc.XmlSpace, "#A10");

			pc = new XmlParserContext ((NameTable) null, (XmlNamespaceManager) null,
				(string) null, XmlSpace.None);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#B1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#B2");
			Assert.IsNull (pc.Encoding, "#B3");
			Assert.AreEqual (string.Empty, pc.InternalSubset, "#B4");
			Assert.IsNull (pc.NamespaceManager, "#B5");
			Assert.IsNull (pc.NameTable, "#B6");
			Assert.AreEqual (string.Empty, pc.PublicId, "#B7");
			Assert.AreEqual (string.Empty, pc.SystemId, "#B8");
			Assert.AreEqual (string.Empty, pc.XmlLang, "#B9");
			Assert.AreEqual (XmlSpace.None, pc.XmlSpace, "#B10");
		}

		[Test]
		public void Constructor2 ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);

			XmlParserContext pc = new XmlParserContext (doc.NameTable,
				ns, "dunno", XmlSpace.None, Encoding.UTF8);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#A1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#A2");
			Assert.AreEqual (Encoding.UTF8, pc.Encoding, "#A3");
			Assert.AreEqual (string.Empty, pc.InternalSubset, "#A4");
			Assert.AreSame (ns, pc.NamespaceManager, "#A5");
			Assert.AreSame (doc.NameTable, pc.NameTable, "#A6");
			Assert.AreEqual (string.Empty, pc.PublicId, "#A7");
			Assert.AreEqual (string.Empty, pc.SystemId, "#A8");
			Assert.AreEqual ("dunno", pc.XmlLang, "#A9");
			Assert.AreEqual (XmlSpace.None, pc.XmlSpace, "#A10");

			pc = new XmlParserContext ((NameTable) null, (XmlNamespaceManager) null,
				(string) null, XmlSpace.None, (Encoding) null);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#B1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#B2");
			Assert.IsNull (pc.Encoding, "#B3");
			Assert.AreEqual (string.Empty, pc.InternalSubset, "#B4");
			Assert.IsNull (pc.NamespaceManager, "#B5");
			Assert.IsNull (pc.NameTable, "#B6");
			Assert.AreEqual (string.Empty, pc.PublicId, "#B7");
			Assert.AreEqual (string.Empty, pc.SystemId, "#B8");
			Assert.AreEqual (string.Empty, pc.XmlLang, "#B9");
			Assert.AreEqual (XmlSpace.None, pc.XmlSpace, "#B10");
		}

		[Test]
		public void Constructor3 ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);
			XmlParserContext pc;

			pc = new XmlParserContext (doc.NameTable, ns, "html",
				"-//W3C//DTD XHTML 1.0 Strict//EN",
				"xhtml1-strict.dtd", "<!-- comment -->", "file://.",
				"xa", XmlSpace.Preserve);
			Assert.AreEqual ("file://.", pc.BaseURI, "#1");
			Assert.AreEqual ("html", pc.DocTypeName, "#2");
			Assert.IsNull (pc.Encoding, "#3");
			Assert.AreEqual ("<!-- comment -->", pc.InternalSubset, "#4");
			Assert.AreSame (ns, pc.NamespaceManager, "#5");
			Assert.AreSame (doc.NameTable, pc.NameTable, "#6");
			Assert.AreEqual ("-//W3C//DTD XHTML 1.0 Strict//EN", pc.PublicId, "#7");
			Assert.AreEqual ("xhtml1-strict.dtd", pc.SystemId, "#8");
			Assert.AreEqual ("xa", pc.XmlLang, "#9");
			Assert.AreEqual (XmlSpace.Preserve, pc.XmlSpace, "#10");

			pc = new XmlParserContext (null, null, (string) null,
				(string) null, (string) null, (string) null,
				(string) null, (string) null, XmlSpace.Preserve);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#B1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#B2");
			Assert.IsNull (pc.Encoding, "#B3");
			Assert.AreEqual ("", pc.InternalSubset, "#B4");
			Assert.IsNull (pc.NamespaceManager, "#B5");
			Assert.IsNull (pc.NameTable, "#B6");
			Assert.AreEqual ("", pc.PublicId, "#B7");
			Assert.AreEqual ("", pc.SystemId, "#B8");
			Assert.AreEqual ("", pc.XmlLang, "#B9");
			Assert.AreEqual (XmlSpace.Preserve, pc.XmlSpace, "#B10");
		}

		[Test]
		public void Constructor4 ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlNamespaceManager ns = new XmlNamespaceManager (doc.NameTable);
			XmlParserContext pc;

			pc = new XmlParserContext (doc.NameTable, ns, "html",
				"-//W3C//DTD XHTML 1.0 Strict//EN",
				"xhtml1-strict.dtd", string.Empty, "file://.",
				string.Empty, XmlSpace.Preserve, Encoding.UTF7);
			Assert.AreEqual ("file://.", pc.BaseURI, "#A1");
			Assert.AreEqual ("html", pc.DocTypeName, "#A2");
			Assert.AreEqual (Encoding.UTF7, pc.Encoding, "#A3");
			Assert.AreEqual ("", pc.InternalSubset, "#A4");
			Assert.AreSame (ns, pc.NamespaceManager, "#A5");
			Assert.AreSame (doc.NameTable, pc.NameTable, "#A6");
			Assert.AreEqual ("-//W3C//DTD XHTML 1.0 Strict//EN", pc.PublicId, "#A7");
			Assert.AreEqual ("xhtml1-strict.dtd", pc.SystemId, "#A8");
			Assert.AreEqual ("", pc.XmlLang, "#A9");
			Assert.AreEqual (XmlSpace.Preserve, pc.XmlSpace, "#A10");

			pc = new XmlParserContext (null, null,
				(string) null, (string) null, (string) null,
				(string) null, (string) null, (string) null,
				XmlSpace.Preserve, (Encoding) null);
			Assert.AreEqual (string.Empty, pc.BaseURI, "#B1");
			Assert.AreEqual (string.Empty, pc.DocTypeName, "#B2");
			Assert.IsNull (pc.Encoding, "#B3");
			Assert.AreEqual ("", pc.InternalSubset, "#B4");
			Assert.IsNull (pc.NamespaceManager, "#B5");
			Assert.IsNull (pc.NameTable, "#B6");
			Assert.AreEqual ("", pc.PublicId, "#B7");
			Assert.AreEqual ("", pc.SystemId, "#B8");
			Assert.AreEqual ("", pc.XmlLang, "#B9");
			Assert.AreEqual (XmlSpace.Preserve, pc.XmlSpace, "#B10");
		}

		[Test]
		public void NameTableConstructor ()
		{
			NameTable nt = new NameTable ();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (nt);
			nsmgr.AddNamespace("Dynamic", "urn:Test");
			Assert.IsNotNull (new XmlParserContext (nt, nsmgr,
null, XmlSpace.Default).NameTable, "#1");
			Assert.IsNotNull (new XmlParserContext (null, nsmgr,
null, XmlSpace.Default).NameTable, "#2"); // bug #485419
		}
	}
}
