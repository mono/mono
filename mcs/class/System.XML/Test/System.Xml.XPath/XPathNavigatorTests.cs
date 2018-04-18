//
// MonoTests.System.Xml.XPathNavigatorTests
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2002 Jason Diamond
// (C) 2003 Martin Willemoes Hansen
// (C) 2004-2006 Novell, Inc.
//

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XPathNavigatorTests
	{
		XmlDocument document;
		XPathNavigator navigator;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}
		
		[Test]
		public void CreateNavigator ()
		{
			document.LoadXml ("<foo />");
			navigator = document.CreateNavigator ();
			Assert.IsNotNull (navigator);
		}

		[Test]
		public void PropertiesOnDocument ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.CreateNavigator ();
			
			Assert.AreEqual (XPathNodeType.Root, navigator.NodeType, "#1");
			Assert.AreEqual (String.Empty, navigator.Name, "#2");
			Assert.AreEqual (String.Empty, navigator.LocalName, "#3");
			Assert.AreEqual (String.Empty, navigator.NamespaceURI, "#4");
			Assert.AreEqual (String.Empty, navigator.Prefix, "#5");
			Assert.IsTrue (!navigator.HasAttributes, "#6");
			Assert.IsTrue (navigator.HasChildren, "#7");
			Assert.IsTrue (!navigator.IsEmptyElement, "#8");
		}

		[Test]
		public void PropertiesOnElement ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.DocumentElement.CreateNavigator ();
			
			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("foo:bar", navigator.Name, "#2");
			Assert.AreEqual ("bar", navigator.LocalName, "#3");
			Assert.AreEqual ("#foo", navigator.NamespaceURI, "#4");
			Assert.AreEqual ("foo", navigator.Prefix, "#5");
			Assert.IsTrue (!navigator.HasAttributes, "#6");
			Assert.IsTrue (!navigator.HasChildren, "#7");
			Assert.IsTrue (navigator.IsEmptyElement, "#8");
		}

		[Test]
		public void PropertiesOnAttribute ()
		{
			document.LoadXml ("<foo bar:baz='quux' xmlns:bar='#bar' />");
			navigator = document.DocumentElement.GetAttributeNode("baz", "#bar").CreateNavigator ();
			
			Assert.AreEqual (XPathNodeType.Attribute, navigator.NodeType, "#1");
			Assert.AreEqual ("bar:baz", navigator.Name, "#2");
			Assert.AreEqual ("baz", navigator.LocalName, "#3");
			Assert.AreEqual ("#bar", navigator.NamespaceURI, "#4");
			Assert.AreEqual ("bar", navigator.Prefix, "#5");
			Assert.IsTrue (!navigator.HasAttributes, "#6");
			Assert.IsTrue (!navigator.HasChildren, "#7");
			Assert.IsTrue (!navigator.IsEmptyElement, "#8");
		}

		[Test]
		public void PropertiesOnNamespace ()
		{
			document.LoadXml ("<root xmlns='urn:foo' />");
			navigator = document.DocumentElement.Attributes [0].CreateNavigator ();
			Assert.AreEqual (XPathNodeType.Namespace, navigator.NodeType, "#1");
		}

		[Test]
		public void Navigation ()
		{
			document.LoadXml ("<foo><bar /><baz /></foo>");
			navigator = document.DocumentElement.CreateNavigator ();
			
			Assert.AreEqual ("foo", navigator.Name, "#1");
			Assert.IsTrue (navigator.MoveToFirstChild (), "#2");
			Assert.AreEqual ("bar", navigator.Name, "#3");
			Assert.IsTrue (navigator.MoveToNext (), "#4");
			Assert.AreEqual ("baz", navigator.Name, "#5");
			Assert.IsTrue (!navigator.MoveToNext (), "#6");
			Assert.AreEqual ("baz", navigator.Name, "#7");
			Assert.IsTrue (navigator.MoveToPrevious (), "#8");
			Assert.AreEqual ("bar", navigator.Name, "#9");
			Assert.IsTrue (!navigator.MoveToPrevious (), "#10");
			Assert.IsTrue (navigator.MoveToParent (), "#11");
			Assert.AreEqual ("foo", navigator.Name, "#12");
			navigator.MoveToRoot ();
			Assert.AreEqual (XPathNodeType.Root, navigator.NodeType, "#13");
			Assert.IsTrue (!navigator.MoveToParent (), "#14");
			Assert.AreEqual (XPathNodeType.Root, navigator.NodeType, "#15");
			Assert.IsTrue (navigator.MoveToFirstChild (), "#16");
			Assert.AreEqual ("foo", navigator.Name, "#17");
			Assert.IsTrue (navigator.MoveToFirst (), "#18");
			Assert.AreEqual ("foo", navigator.Name, "#19");
			Assert.IsTrue (navigator.MoveToFirstChild (), "#20");
			Assert.AreEqual ("bar", navigator.Name, "#21");
			Assert.IsTrue (navigator.MoveToNext (), "#22");
			Assert.AreEqual ("baz", navigator.Name, "#23");
			Assert.IsTrue (navigator.MoveToFirst (), "#24");
			Assert.AreEqual ("bar", navigator.Name, "#25");
		}

		[Test]
		public void MoveToAndIsSamePosition ()
		{
			XmlDocument document1 = new XmlDocument ();
			document1.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator1a = document1.DocumentElement.CreateNavigator ();
			XPathNavigator navigator1b = document1.DocumentElement.CreateNavigator ();

			XmlDocument document2 = new XmlDocument ();
			document2.LoadXml ("<foo><bar /></foo>");
			XPathNavigator navigator2 = document2.DocumentElement.CreateNavigator ();

			Assert.AreEqual ("foo", navigator1a.Name, "#1");
			Assert.IsTrue (navigator1a.MoveToFirstChild (), "#2");
			Assert.AreEqual ("bar", navigator1a.Name, "#3");

			Assert.IsTrue (!navigator1b.IsSamePosition (navigator1a), "#4");
			Assert.AreEqual ("foo", navigator1b.Name, "#5");
			Assert.IsTrue (navigator1b.MoveTo (navigator1a), "#6");
			Assert.IsTrue (navigator1b.IsSamePosition (navigator1a), "#7");
			Assert.AreEqual ("bar", navigator1b.Name, "#8");

			Assert.IsTrue (!navigator2.IsSamePosition (navigator1a), "#9");
			Assert.AreEqual ("foo", navigator2.Name, "#10");
			Assert.IsTrue (!navigator2.MoveTo (navigator1a), "#11");
			Assert.AreEqual ("foo", navigator2.Name, "#12");
		}

		[Test]
		public void AttributeNavigation ()
		{
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			navigator = document.DocumentElement.CreateNavigator ();

			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("foo", navigator.Name, "#2");
			Assert.IsTrue (navigator.MoveToFirstAttribute (), "#3");
			Assert.AreEqual (XPathNodeType.Attribute, navigator.NodeType, "#4");
			Assert.AreEqual ("bar", navigator.Name, "#5");
			Assert.AreEqual ("baz", navigator.Value, "#6");
			Assert.IsTrue (navigator.MoveToNextAttribute (), "#7");
			Assert.AreEqual (XPathNodeType.Attribute, navigator.NodeType, "#8");
			Assert.AreEqual ("quux", navigator.Name, "#9");
			Assert.AreEqual ("quuux", navigator.Value, "#10");
		}

		[Test]
		public void ElementAndRootValues()
		{
			document.LoadXml ("<foo><bar>baz</bar><quux>quuux</quux></foo>");
			navigator = document.DocumentElement.CreateNavigator ();

			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("foo", navigator.Name, "#2");
			//Assert.AreEqual ("bazquuux", navigator.Value, "#3");

			navigator.MoveToRoot ();
			//Assert.AreEqual ("bazquuux", navigator.Value, "#4");
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			document.LoadXml ("<?xml version=\"1.0\" standalone=\"yes\"?><Root><foo>bar</foo></Root>");
			navigator = document.CreateNavigator ();

			navigator.MoveToRoot ();
			navigator.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("Root", navigator.Name, "#2");
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			document.LoadXml ("<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />");
			navigator = document.CreateNavigator ();

			Assert.IsTrue (navigator.MoveToFirstChild ());
			Assert.AreEqual (XPathNodeType.ProcessingInstruction, navigator.NodeType, "#1");
			Assert.AreEqual ("xml-stylesheet", navigator.Name, "#2");

			XPathNodeIterator iter = navigator.SelectChildren (XPathNodeType.Element);
			Assert.AreEqual (0, iter.Count, "#3");
		}

		[Test]
		public void SelectFromOrphan ()
		{
			// SelectSingleNode () from node without parent.
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<foo><include id='original' /></foo>");

			XmlNode node = doc.CreateElement ("child");
			node.InnerXml = "<include id='new' />";

			XmlNode new_include = node.SelectSingleNode ("//include");
			Assert.AreEqual ("<include id=\"new\" />", new_include.OuterXml, "#1");

			// In this case 'node2' has parent 'node'
			doc = new XmlDocument ();
			doc.LoadXml ("<foo><include id='original' /></foo>");

			node = doc.CreateElement ("child");
			XmlNode node2 = doc.CreateElement ("grandchild");
			node.AppendChild (node2);
			node2.InnerXml = "<include id='new' />";

			new_include = node2.SelectSingleNode ("/");
			Assert.AreEqual ("<child><grandchild><include id=\"new\" /></grandchild></child>",
				new_include.OuterXml, "#2");
		}

		[Test]
		public void XPathDocumentMoveToId ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root id ID #REQUIRED>]>";
			string xml = dtd + "<root id='aaa'/>";
			StringReader sr = new StringReader (xml);
			XPathNavigator nav = new XPathDocument (sr).CreateNavigator ();
			Assert.IsTrue (nav.MoveToId ("aaa"), "ctor() from TextReader");

			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			nav = new XPathDocument (xvr).CreateNavigator ();
			Assert.IsTrue (nav.MoveToId ("aaa"), "ctor() from XmlValidatingReader");

			// FIXME: it seems to result in different in .NET 2.0.
		}

		[Test]
		public void SignificantWhitespaceConstruction ()
		{
			string xml = @"<root>
        <child xml:space='preserve'>    <!-- -->   </child>
        <child xml:space='preserve'>    </child>
</root>";
			XPathNavigator nav = new XPathDocument (
				new XmlTextReader (xml, XmlNodeType.Document, null),
				XmlSpace.Preserve).CreateNavigator ();
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Whitespace, nav.NodeType, "#1");
			nav.MoveToNext ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.SignificantWhitespace,
				nav.NodeType, "#2");
		}

		[Test]
		public void VariableReference ()
		{
			XPathDocument xpd = new XPathDocument (
				new StringReader ("<root>sample text</root>"));
			XPathNavigator nav = xpd.CreateNavigator ();

			XPathExpression expr = nav.Compile ("foo(string(.),$idx)");
			XsltArgumentList args = new XsltArgumentList ();
			args.AddParam ("idx", "", 5);
			MyContext ctx = new MyContext (nav.NameTable as NameTable, args);
			ctx.AddNamespace ("x", "urn:foo");

			expr.SetContext (ctx);

			XPathNodeIterator iter = nav.Select ("/root");
			iter.MoveNext ();
			Assert.AreEqual ("e", iter.Current.Evaluate (expr), "#1");
		}

		class MyContext : XsltContext
		{
			XsltArgumentList args;

			public MyContext (NameTable nt, XsltArgumentList args)
				: base (nt)
			{
				this.args = args;
			}

			public override IXsltContextFunction ResolveFunction (
				string prefix, string name, XPathResultType [] argtypes)
			{
				if (name == "foo")
					return new MyFunction (argtypes);
				return null;
			}

			public override IXsltContextVariable ResolveVariable (string prefix, string name)
			{
				return new MyVariable (name);
			}

			public override bool PreserveWhitespace (XPathNavigator nav)
			{
				return false;
			}

			public override int CompareDocument (string uri1, string uri2)
			{
				return String.CompareOrdinal (uri1, uri2);
			}

			public override bool Whitespace {
				get { return false; }
			}

			public object GetParam (string name, string ns)
			{
				return args.GetParam (name, ns);
			}
		}

		public class MyFunction : IXsltContextFunction
		{
			XPathResultType [] argtypes;

			public MyFunction (XPathResultType [] argtypes)
			{
				this.argtypes = argtypes;
			}

			public XPathResultType [] ArgTypes {
				get { return argtypes; }
			}

			public int Maxargs {
				get { return 2; }
			}

			public int Minargs {
				get { return 2; }
			}

			public XPathResultType ReturnType {
				get { return XPathResultType.String; }
			}

			public object Invoke (XsltContext xsltContext,
				object [] args, XPathNavigator instanceContext)
			{
				return ((string) args [0]) [(int) (double) args [1]].ToString ();
			}
		}

		public class MyVariable : IXsltContextVariable
		{
			string name;

			public MyVariable (string name)
			{
				this.name = name;
			}

			public object Evaluate (XsltContext ctx)
			{
				return ((MyContext) ctx).GetParam (name, String.Empty);
			}

			public bool IsLocal {
				get { return false; }
			}

			public bool IsParam {
				get { return false; }
			}

			public XPathResultType VariableType {
				get { return XPathResultType.Any; }
			}
		}

		[Test]
		public void TextMatchesWhitespace ()
		{
			string xml = "<root><ws>   </ws><sws xml:space='preserve'> </sws></root>";
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild (); // root
			nav.MoveToFirstChild (); // ws
			nav.MoveToFirstChild (); // '   '
			Assert.AreEqual (true, nav.Matches ("text()"), "#1");
			nav.MoveToParent ();
			nav.MoveToNext (); // sws
			nav.MoveToFirstChild (); // ' '
			Assert.AreEqual (true, nav.Matches ("text()"), "#2");
		}

		[Test]
		public void Bug456103 ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root><X/></root>");

			XPathNavigator nav = doc.DocumentElement.CreateNavigator ();
			// ".//*" does not reproduce the bug.
			var i = nav.Select ("descendant::*");

			// without this call to get_Count() the bug does not reproduce.
			Assert.AreEqual (1, i.Count, "#1");

			Assert.IsTrue (i.MoveNext (), "#2");
		}

		[Test]
		public void ValueAsBoolean ()
		{
			string xml = "<root>1</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (true, nav.ValueAsBoolean, "#1");
			nav.MoveToFirstChild ();
			Assert.AreEqual (true, nav.ValueAsBoolean, "#2");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ValueAsBooleanFail ()
		{
			string xml = "<root>1.0</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			bool i = nav.ValueAsBoolean;
		}

		[Test]
		public void ValueAsDateTime ()
		{
			DateTime time = new DateTime (2005, 12, 13);
			string xml = "<root>2005-12-13</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (time, nav.ValueAsDateTime, "#1");
			nav.MoveToFirstChild ();
			Assert.AreEqual (time, nav.ValueAsDateTime, "#2");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ValueAsDateTimeFail ()
		{
			string xml = "<root>dating time</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			DateTime time = nav.ValueAsDateTime;
		}

		[Test]
		public void ValueAsDouble ()
		{
			string xml = "<root>3.14159265359</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (3.14159265359, nav.ValueAsDouble, "#1");
			nav.MoveToFirstChild ();
			Assert.AreEqual (3.14159265359, nav.ValueAsDouble, "#2");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ValueAsDoubleFail ()
		{
			string xml = "<root>Double Dealer</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Double dealer = nav.ValueAsDouble;
		}

		[Test]
		public void ValueAsInt ()
		{
			string xml = "<root>1</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (1, nav.ValueAsInt, "#1");
			nav.MoveToFirstChild ();
			Assert.AreEqual (1, nav.ValueAsInt, "#2");
		}

		[Test]
		// Here, it seems to be using XQueryConvert (whatever was called)
		[ExpectedException (typeof (FormatException))]
		public void ValueAsIntFail ()
		{
			string xml = "<root>1.0</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			int i = nav.ValueAsInt;
		}

		[Test]
		public void ValueAsLong ()
		{
			string xml = "<root>10000000000000000</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (10000000000000000, nav.ValueAsLong, "#1");
			nav.MoveToFirstChild ();
			Assert.AreEqual (10000000000000000, nav.ValueAsLong, "#2");
		}

		[Test]
		// Here, it seems to be using XQueryConvert (whatever was called)
		[ExpectedException (typeof (FormatException))]
		public void ValueAsLongFail ()
		{
			string xml = "<root>0x10000000000000000</root>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToFirstChild ();
			long l = nav.ValueAsLong;
		}

		[Test] // bug #79874
		public void InnerXmlText ()
		{
			StringReader sr = new StringReader ("<Abc><Foo>Hello</Foo></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();
			XPathNodeIterator iter = nav.Select ("/Abc/Foo");
			iter.MoveNext ();
			Assert.AreEqual ("Hello", iter.Current.InnerXml, "#1");
			Assert.AreEqual ("<Foo>Hello</Foo>", iter.Current.OuterXml, "#2");
			iter = nav.Select ("/Abc/Foo/text()");
			iter.MoveNext ();
			Assert.AreEqual (String.Empty, iter.Current.InnerXml, "#3");
			Assert.AreEqual ("Hello", iter.Current.OuterXml, "#4");
		}

		[Test] // bug #79875
		public void InnerXmlAttribute ()
		{
			StringReader sr = new StringReader ("<Abc><Foo attr='val1'/></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();

			XPathNodeIterator iter = nav.Select ("/Abc/Foo/@attr");
			iter.MoveNext ();
			Assert.AreEqual ("val1", iter.Current.InnerXml, "#1");
		}

		string AlterNewLine (string s)
		{
			return s.Replace ("\r\n", Environment.NewLine);
		}

		[Test]
		public void InnerXmlTextEscape ()
		{
			StringReader sr = new StringReader ("<Abc><Foo>Hello&lt;\r\nInnerXml</Foo></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();
			XPathNodeIterator iter = nav.Select ("/Abc/Foo");
			iter.MoveNext ();
			Assert.AreEqual (AlterNewLine ("Hello&lt;\r\nInnerXml"), iter.Current.InnerXml, "#1");
			Assert.AreEqual (AlterNewLine ("<Foo>Hello&lt;\r\nInnerXml</Foo>"), iter.Current.OuterXml, "#2");
			iter = nav.Select ("/Abc/Foo/text()");
			iter.MoveNext ();
			Assert.AreEqual (String.Empty, iter.Current.InnerXml, "#3");
			Assert.AreEqual (AlterNewLine ("Hello&lt;\r\nInnerXml"), iter.Current.OuterXml, "#4");
		}

		[Test]
		[Category ("NotDotNet")] // .NET bug; it should escape value
		[Ignore ("Bug in Microsoft reference source")]
		public void InnerXmlAttributeEscape ()
		{
			StringReader sr = new StringReader ("<Abc><Foo attr='val&quot;1&#13;&#10;&gt;'/></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();

			XPathNodeIterator iter = nav.Select ("/Abc/Foo/@attr");
			iter.MoveNext ();
			Assert.AreEqual ("val&quot;1&#10;&gt;", iter.Current.InnerXml, "#1");
		}

		[Test]
		public void WriterAttributePrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlWriter w = doc.CreateNavigator ().AppendChild ();
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("xmlns", "x", "http://www.w3.org/2000/xmlns/", "urn:foo");
			Assert.AreEqual ("x", w.LookupPrefix ("urn:foo"), "#0");
			w.WriteStartElement (null, "bar", "urn:foo");
			w.WriteAttributeString (null, "ext", "urn:foo", "bah");
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			Assert.AreEqual ("x", doc.FirstChild.FirstChild.Prefix, "#1");
			Assert.AreEqual ("x", doc.FirstChild.FirstChild.Attributes [0].Prefix, "#2");
		}

		[Test]
		public void ValueAs ()
		{
			string xml = "<root>1</root>";
			XPathNavigator nav = new XPathDocument (XmlReader.Create (new StringReader (xml))).CreateNavigator ();
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			Assert.AreEqual ("1", nav.ValueAs (typeof (string), null), "#1");
			Assert.AreEqual (1, nav.ValueAs (typeof (int), null), "#2");
		}

		[Test]
		public void MoveToFollowingNodeTypeAll ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root><child/><child2/></root>");
			XPathNavigator nav = doc.CreateNavigator ();
			Assert.IsTrue (nav.MoveToFollowing (XPathNodeType.All), "#1");
			Assert.IsTrue (nav.MoveToFollowing (XPathNodeType.All), "#2");
			Assert.AreEqual ("child", nav.LocalName, "#3");
			Assert.IsTrue (nav.MoveToNext (XPathNodeType.All), "#4");
			Assert.AreEqual ("child2", nav.LocalName, "#5");
		}

		[Test] // bug #324606.
		public void XPathDocumentFromSubtreeNodes ()
		{
			string xml = "<root><child1><nest1><nest2>hello!</nest2></nest1></child1><child2/><child3/></root>";
			XmlReader r = new XmlTextReader (new StringReader (xml));
			while (r.Read ()) {
				if (r.Name == "child1")
					break;
			}
			XPathDocument d = new XPathDocument (r);
			XPathNavigator n = d.CreateNavigator ();
			string result = @"<child1>
  <nest1>
    <nest2>hello!</nest2>
  </nest1>
</child1>
<child2 />
<child3 />";
			Assert.AreEqual (result.Replace ("\r\n", "\n"), n.OuterXml.Replace ("\r\n", "\n"), "#1");
		}

		[Test] // bug #376191
		public void InnerXmlOnRoot ()
		{
			XmlDocument document = new XmlDocument ();
			document.LoadXml (@"<test>
			<node>z</node>
			<node>a</node>
			<node>b</node>
			<node>q</node>
			</test>");
			XPathNavigator navigator = document.CreateNavigator();
			Assert.AreEqual (navigator.OuterXml, navigator.InnerXml, "#1");
		}

		[Test] // bug #515136
		public void SelectChildrenEmpty ()
		{
			string s = "<root> <foo> </foo> </root>";
			XPathDocument doc = new XPathDocument (new StringReader (s));
			XPathNavigator nav = doc.CreateNavigator ();
			XPathNodeIterator it = nav.SelectChildren (String.Empty, String.Empty);
			foreach (XPathNavigator xpn in it)
				return;
			Assert.Fail ("no selection");
		}
	}
}
