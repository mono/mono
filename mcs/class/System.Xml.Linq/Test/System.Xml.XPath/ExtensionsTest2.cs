//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2002 Jason Diamond
// (C) 2003 Martin Willemoes Hansen
// (C) 2004-2006 Novell, Inc.
// (C) 2003 Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
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

//
// imported from XPathNavigatorTests
//

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class ExtensionsTest2
	{
		XPathNavigator navigator;
		
		[Test]
		public void CreateNavigator ()
		{
			navigator = XDocument.Parse ("<foo />").CreateNavigator ();
			Assert.IsNotNull (navigator);
		}

		[Test]
		public void PropertiesOnDocument ()
		{
			navigator = XDocument.Parse ("<foo:bar xmlns:foo='#foo' />").CreateNavigator ();
			
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
			navigator = XDocument.Parse ("<foo:bar xmlns:foo='#foo' />").FirstNode.CreateNavigator ();
			
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
		public void Navigation ()
		{
			navigator = XDocument.Parse ("<foo><bar /><baz /></foo>").FirstNode.CreateNavigator ();
			
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
		[Category ("NotDotNet")] // fails to differentiate document instances
		public void MoveToAndIsSamePosition ()
		{
			var doc1 = XDocument.Parse ("<foo><bar /></foo>");
			XPathNavigator navigator1a = doc1.FirstNode.CreateNavigator ();
			XPathNavigator navigator1b = doc1.FirstNode.CreateNavigator ();

			var doc2 = XDocument.Parse ("<foo><bar /></foo>");
			XPathNavigator navigator2 = doc2.FirstNode.CreateNavigator ();

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
			Assert.IsFalse (navigator2.MoveTo (navigator1a), "#11");
			Assert.AreEqual ("foo", navigator2.Name, "#12");
		}

		[Test]
		public void AttributeNavigation ()
		{
			navigator = XDocument.Parse ("<foo bar='baz' quux='quuux' />").FirstNode.CreateNavigator ();

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
			navigator = XDocument.Parse ("<foo><bar>baz</bar><quux>quuux</quux></foo>").FirstNode.CreateNavigator ();

			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("foo", navigator.Name, "#2");
			//Assert.AreEqual ("bazquuux", navigator.Value, "#3");

			navigator.MoveToRoot ();
			//Assert.AreEqual ("bazquuux", navigator.Value, "#4");
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			navigator = XDocument.Parse ("<?xml version=\"1.0\" standalone=\"yes\"?><Root><foo>bar</foo></Root>").CreateNavigator ();

			navigator.MoveToRoot ();
			navigator.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Element, navigator.NodeType, "#1");
			Assert.AreEqual ("Root", navigator.Name, "#2");
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			navigator = XDocument.Parse ("<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />").CreateNavigator ();

			Assert.IsTrue (navigator.MoveToFirstChild ());
			Assert.AreEqual (XPathNodeType.ProcessingInstruction, navigator.NodeType, "#1");
			Assert.AreEqual ("xml-stylesheet", navigator.Name, "#2");

			XPathNodeIterator iter = navigator.SelectChildren (XPathNodeType.Element);
			Assert.AreEqual (0, iter.Count, "#3");
		}

		/*
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
		*/

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void XPathDocumentMoveToId ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root id ID #REQUIRED>]>";
			string xml = dtd + "<root id='aaa'/>";
			XPathNavigator nav = navigator = XDocument.Parse (xml).CreateNavigator ();
			Assert.IsTrue (nav.MoveToId ("aaa"), "ctor() from TextReader");

			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			nav = new XPathDocument (xvr).CreateNavigator ();
			nav.MoveToId ("aaa"); // it does not support this method
		}

		[Test]
		public void SignificantWhitespaceConstruction ()
		{
			string xml = @"<root>
        <child xml:space='preserve'>    <!-- -->   </child>
        <child xml:space='preserve'>    </child>
</root>";
			XPathNavigator nav = XDocument.Parse (xml, LoadOptions.PreserveWhitespace).CreateNavigator ();
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Text, nav.NodeType, "#1"); // not Whitespace but Text
			nav.MoveToNext ();
			nav.MoveToFirstChild ();
			Assert.AreEqual (XPathNodeType.Text, nav.NodeType, "#2"); // not SignificantWhitespace but Text
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

#if NET_2_0
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

		[Test]
		public void InnerXmlTextEscape ()
		{
			StringReader sr = new StringReader ("<Abc><Foo>Hello&lt;\r\nInnerXml</Foo></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();
			XPathNodeIterator iter = nav.Select ("/Abc/Foo");
			iter.MoveNext ();
			Assert.AreEqual ("Hello&lt;\r\nInnerXml", iter.Current.InnerXml, "#1");
			Assert.AreEqual ("<Foo>Hello&lt;\r\nInnerXml</Foo>", iter.Current.OuterXml, "#2");
			iter = nav.Select ("/Abc/Foo/text()");
			iter.MoveNext ();
			Assert.AreEqual (String.Empty, iter.Current.InnerXml, "#3");
			Assert.AreEqual ("Hello&lt;\r\nInnerXml", iter.Current.OuterXml, "#4");
		}

		[Test]
		[Category ("NotDotNet")] // .NET bug; it should escape value
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
			Assert.AreEqual (result, n.OuterXml.Replace ("\r\n", "\n"), "#1");
		}

		[Test] // bug #376191
		public void InnerXmlOnRoot ()
		{
			string xml = @"<test>
			<node>z</node>
			<node>a</node>
			<node>b</node>
			<node>q</node>
			</test>";
			navigator = XDocument.Parse (xml).CreateNavigator ();
			Assert.AreEqual (navigator.OuterXml, navigator.InnerXml, "#1");
		}

		[Test] // bug #515136
		public void SelectChildrenEmpty ()
		{
			string s = "<root> <foo> </foo> </root>";
			XPathNavigator nav = XDocument.Parse (s).CreateNavigator ();
			XPathNodeIterator it = nav.SelectChildren (String.Empty, String.Empty);
			foreach (XPathNavigator xpn in it)
				return;
			Assert.Fail ("no selection");
		}
#endif
	}
}
