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
	public class XPathNavigatorTests : Assertion
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
			AssertNotNull (navigator);
		}

		[Test]
		public void PropertiesOnDocument ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.CreateNavigator ();
			
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			AssertEquals (String.Empty, navigator.Name);
			AssertEquals (String.Empty, navigator.LocalName);
			AssertEquals (String.Empty, navigator.NamespaceURI);
			AssertEquals (String.Empty, navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (navigator.HasChildren);
			Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnElement ()
		{
			document.LoadXml ("<foo:bar xmlns:foo='#foo' />");
			navigator = document.DocumentElement.CreateNavigator ();
			
			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo:bar", navigator.Name);
			AssertEquals ("bar", navigator.LocalName);
			AssertEquals ("#foo", navigator.NamespaceURI);
			AssertEquals ("foo", navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (!navigator.HasChildren);
			Assert (navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnAttribute ()
		{
			document.LoadXml ("<foo bar:baz='quux' xmlns:bar='#bar' />");
			navigator = document.DocumentElement.GetAttributeNode("baz", "#bar").CreateNavigator ();
			
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("bar:baz", navigator.Name);
			AssertEquals ("baz", navigator.LocalName);
			AssertEquals ("#bar", navigator.NamespaceURI);
			AssertEquals ("bar", navigator.Prefix);
			Assert (!navigator.HasAttributes);
			Assert (!navigator.HasChildren);
			Assert (!navigator.IsEmptyElement);
		}

		[Test]
		public void PropertiesOnNamespace ()
		{
			document.LoadXml ("<root xmlns='urn:foo' />");
			navigator = document.DocumentElement.Attributes [0].CreateNavigator ();
			AssertEquals (XPathNodeType.Namespace, navigator.NodeType);
		}

		[Test]
		public void Navigation ()
		{
			document.LoadXml ("<foo><bar /><baz /></foo>");
			navigator = document.DocumentElement.CreateNavigator ();
			
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("bar", navigator.Name);
			Assert (navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (!navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (navigator.MoveToPrevious ());
			AssertEquals ("bar", navigator.Name);
			Assert (!navigator.MoveToPrevious ());
			Assert (navigator.MoveToParent ());
			AssertEquals ("foo", navigator.Name);
			navigator.MoveToRoot ();
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assert (!navigator.MoveToParent ());
			AssertEquals (XPathNodeType.Root, navigator.NodeType);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirst ());
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstChild ());
			AssertEquals ("bar", navigator.Name);
			Assert (navigator.MoveToNext ());
			AssertEquals ("baz", navigator.Name);
			Assert (navigator.MoveToFirst ());
			AssertEquals ("bar", navigator.Name);
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

			AssertEquals ("foo", navigator1a.Name);
			Assert (navigator1a.MoveToFirstChild ());
			AssertEquals ("bar", navigator1a.Name);

			Assert (!navigator1b.IsSamePosition (navigator1a));
			AssertEquals ("foo", navigator1b.Name);
			Assert (navigator1b.MoveTo (navigator1a));
			Assert (navigator1b.IsSamePosition (navigator1a));
			AssertEquals ("bar", navigator1b.Name);

			Assert (!navigator2.IsSamePosition (navigator1a));
			AssertEquals ("foo", navigator2.Name);
			Assert (!navigator2.MoveTo (navigator1a));
			AssertEquals ("foo", navigator2.Name);
		}

		[Test]
		public void AttributeNavigation ()
		{
			document.LoadXml ("<foo bar='baz' quux='quuux' />");
			navigator = document.DocumentElement.CreateNavigator ();

			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo", navigator.Name);
			Assert (navigator.MoveToFirstAttribute ());
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("bar", navigator.Name);
			AssertEquals ("baz", navigator.Value);
			Assert (navigator.MoveToNextAttribute ());
			AssertEquals (XPathNodeType.Attribute, navigator.NodeType);
			AssertEquals ("quux", navigator.Name);
			AssertEquals ("quuux", navigator.Value);
		}

		[Test]
		public void ElementAndRootValues()
		{
			document.LoadXml ("<foo><bar>baz</bar><quux>quuux</quux></foo>");
			navigator = document.DocumentElement.CreateNavigator ();

			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("foo", navigator.Name);
			//AssertEquals ("bazquuux", navigator.Value);

			navigator.MoveToRoot ();
			//AssertEquals ("bazquuux", navigator.Value);
		}

		[Test]
		public void DocumentWithXmlDeclaration ()
		{
			document.LoadXml ("<?xml version=\"1.0\" standalone=\"yes\"?><Root><foo>bar</foo></Root>");
			navigator = document.CreateNavigator ();

			navigator.MoveToRoot ();
			navigator.MoveToFirstChild ();
			AssertEquals (XPathNodeType.Element, navigator.NodeType);
			AssertEquals ("Root", navigator.Name);
		}

		[Test]
		public void DocumentWithProcessingInstruction ()
		{
			document.LoadXml ("<?xml-stylesheet href='foo.xsl' type='text/xsl' ?><foo />");
			navigator = document.CreateNavigator ();

			Assert (navigator.MoveToFirstChild ());
			AssertEquals (XPathNodeType.ProcessingInstruction, navigator.NodeType);
			AssertEquals ("xml-stylesheet", navigator.Name);

			XPathNodeIterator iter = navigator.SelectChildren (XPathNodeType.Element);
			AssertEquals (0, iter.Count);
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
			AssertEquals ("<include id=\"new\" />", new_include.OuterXml);

			// In this case 'node2' has parent 'node'
			doc = new XmlDocument ();
			doc.LoadXml ("<foo><include id='original' /></foo>");

			node = doc.CreateElement ("child");
			XmlNode node2 = doc.CreateElement ("grandchild");
			node.AppendChild (node2);
			node2.InnerXml = "<include id='new' />";

			new_include = node2.SelectSingleNode ("/");
			AssertEquals ("<child><grandchild><include id=\"new\" /></grandchild></child>",
				new_include.OuterXml);
		}

		[Test]
		public void XPathDocumentMoveToId ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root id ID #REQUIRED>]>";
			string xml = dtd + "<root id='aaa'/>";
			StringReader sr = new StringReader (xml);
			XPathNavigator nav = new XPathDocument (sr).CreateNavigator ();
			Assert ("ctor() from TextReader", nav.MoveToId ("aaa"));

			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			nav = new XPathDocument (xvr).CreateNavigator ();
			Assert ("ctor() from XmlValidatingReader", nav.MoveToId ("aaa"));

			// FIXME: it seems to result in different in .NET 2.0.
#if NET_2_0
#else
			// When it is XmlTextReader, XPathDocument fails.
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			nav = new XPathDocument (xtr).CreateNavigator ();
			Assert ("ctor() from XmlTextReader", !nav.MoveToId ("aaa"));
			xtr.Close ();
#endif
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
			AssertEquals ("#1", XPathNodeType.Whitespace, nav.NodeType);
			nav.MoveToNext ();
			nav.MoveToFirstChild ();
			AssertEquals ("#2", XPathNodeType.SignificantWhitespace,
				nav.NodeType);
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
			AssertEquals ("e", iter.Current.Evaluate (expr));
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
			AssertEquals ("#1", true, nav.Matches ("text()"));
			nav.MoveToParent ();
			nav.MoveToNext (); // sws
			nav.MoveToFirstChild (); // ' '
			AssertEquals ("#2", true, nav.Matches ("text()"));
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
			AssertEquals ("#1", 1, i.Count);

			Assert ("#2", i.MoveNext ());
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
			AssertEquals ("#1", true, nav.ValueAsBoolean);
			nav.MoveToFirstChild ();
			AssertEquals ("#2", true, nav.ValueAsBoolean);
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
			AssertEquals ("#1", time, nav.ValueAsDateTime);
			nav.MoveToFirstChild ();
			AssertEquals ("#2", time, nav.ValueAsDateTime);
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
			AssertEquals ("#1", 3.14159265359, nav.ValueAsDouble);
			nav.MoveToFirstChild ();
			AssertEquals ("#2", 3.14159265359, nav.ValueAsDouble);
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
			AssertEquals ("#1", 1, nav.ValueAsInt);
			nav.MoveToFirstChild ();
			AssertEquals ("#2", 1, nav.ValueAsInt);
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
			AssertEquals ("#1", 10000000000000000, nav.ValueAsLong);
			nav.MoveToFirstChild ();
			AssertEquals ("#2", 10000000000000000, nav.ValueAsLong);
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
			AssertEquals ("#1", "Hello", iter.Current.InnerXml);
			AssertEquals ("#2", "<Foo>Hello</Foo>", iter.Current.OuterXml);
			iter = nav.Select ("/Abc/Foo/text()");
			iter.MoveNext ();
			AssertEquals ("#3", String.Empty, iter.Current.InnerXml);
			AssertEquals ("#4", "Hello", iter.Current.OuterXml);
		}

		[Test] // bug #79875
		public void InnerXmlAttribute ()
		{
			StringReader sr = new StringReader ("<Abc><Foo attr='val1'/></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();

			XPathNodeIterator iter = nav.Select ("/Abc/Foo/@attr");
			iter.MoveNext ();
			AssertEquals ("val1", iter.Current.InnerXml);
		}

		[Test]
		public void InnerXmlTextEscape ()
		{
			StringReader sr = new StringReader ("<Abc><Foo>Hello&lt;\r\nInnerXml</Foo></Abc>");
			XPathDocument doc = new XPathDocument (sr);
			XPathNavigator nav = doc.CreateNavigator ();
			XPathNodeIterator iter = nav.Select ("/Abc/Foo");
			iter.MoveNext ();
			AssertEquals ("#1", "Hello&lt;\r\nInnerXml", iter.Current.InnerXml);
			AssertEquals ("#2", "<Foo>Hello&lt;\r\nInnerXml</Foo>", iter.Current.OuterXml);
			iter = nav.Select ("/Abc/Foo/text()");
			iter.MoveNext ();
			AssertEquals ("#3", String.Empty, iter.Current.InnerXml);
			AssertEquals ("#4", "Hello&lt;\r\nInnerXml", iter.Current.OuterXml);
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
			AssertEquals ("val&quot;1&#10;&gt;", iter.Current.InnerXml);
		}

		[Test]
		public void WriterAttributePrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlWriter w = doc.CreateNavigator ().AppendChild ();
			w.WriteStartElement ("foo");
			w.WriteAttributeString ("xmlns", "x", "http://www.w3.org/2000/xmlns/", "urn:foo");
			AssertEquals ("#0", "x", w.LookupPrefix ("urn:foo"));
			w.WriteStartElement (null, "bar", "urn:foo");
			w.WriteAttributeString (null, "ext", "urn:foo", "bah");
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			AssertEquals ("#1", "x", doc.FirstChild.FirstChild.Prefix);
			AssertEquals ("#2", "x", doc.FirstChild.FirstChild.Attributes [0].Prefix);
		}

		[Test]
		public void ValueAs ()
		{
			string xml = "<root>1</root>";
			XPathNavigator nav = new XPathDocument (XmlReader.Create (new StringReader (xml))).CreateNavigator ();
			nav.MoveToFirstChild ();
			nav.MoveToFirstChild ();
			AssertEquals ("1", nav.ValueAs (typeof (string), null));
			AssertEquals (1, nav.ValueAs (typeof (int), null));
		}

		[Test]
		public void MoveToFollowingNodeTypeAll ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root><child/><child2/></root>");
			XPathNavigator nav = doc.CreateNavigator ();
			Assert ("#1", nav.MoveToFollowing (XPathNodeType.All));
			Assert ("#2", nav.MoveToFollowing (XPathNodeType.All));
			AssertEquals ("#3", "child", nav.LocalName);
			Assert ("#4", nav.MoveToNext (XPathNodeType.All));
			AssertEquals ("#5", "child2", nav.LocalName);
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
			AssertEquals (result, n.OuterXml.Replace ("\r\n", "\n"));
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
			AssertEquals (navigator.OuterXml, navigator.InnerXml);
		}
#endif
	}
}
