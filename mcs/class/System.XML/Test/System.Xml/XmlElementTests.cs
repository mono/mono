//
// XmlElementTests
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2003 Martin Willemoes Hansen 
//

using System;
using System.Xml;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlElementTests  : Assertion
	{
		private XmlDocument document;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		private void AssertElement (XmlElement element, string prefix,
					    string localName, string namespaceURI,
					    int attributesCount)
		{
			AssertEquals (prefix != String.Empty ? prefix + ":" + localName : localName, element.Name);
			AssertEquals (prefix, element.Prefix);
			AssertEquals (localName, element.LocalName);
			AssertEquals (namespaceURI, element.NamespaceURI);
			//AssertEquals (attributesCount, element.Attributes.Count);
		}

		// for NodeInserted Event
		private bool Inserted = false;
		private void OnNodeInserted (object o, XmlNodeChangedEventArgs e)
		{
			Inserted = true;
		}

		// for NodeChanged Event
		private bool Changed = false;
		private void OnNodeChanged (object o, XmlNodeChangedEventArgs e)
		{
			Changed = true;
		}

		// for NodeRemoved Event
		private bool Removed = false;
		private void OnNodeRemoved (object o, XmlNodeChangedEventArgs e)
		{
			Removed = true;
		}

		[Test]
		public void CloneNode ()
		{
			XmlElement element = document.CreateElement ("foo");
			XmlElement child = document.CreateElement ("bar");
			XmlElement grandson = document.CreateElement ("baz");

			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			element.AppendChild (child);
			child.SetAttribute ("attr3", "val3");
			child.AppendChild (grandson);
                        
			document.AppendChild (element);
			XmlNode deep = element.CloneNode (true);
			// AssertEquals ("These should be the same", deep.OuterXml, element.OuterXml); 
			AssertNull ("This is not null", deep.ParentNode);
			Assert ("Copies, not pointers", !Object.ReferenceEquals (element,deep));

			XmlNode shallow = element.CloneNode (false);
			AssertNull ("This is not null", shallow.ParentNode);
			Assert ("Copies, not pointers", !Object.ReferenceEquals (element,shallow));
			AssertEquals ("Shallow clones shalt have no children!", false, shallow.HasChildNodes);
		}

		[Test]
		public void ConstructionAndDefaultAttributes ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root foo CDATA 'def'>]>";
			string xml = dtd + "<root />";
//			XmlValidatingReader xvr = new XmlValidatingReader (new XmlTextReader (xml, XmlNodeType.Document, null));
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			Console.WriteLine (doc.DocumentElement.Attributes.Count);
			Console.WriteLine (doc.CreateElement ("root").Attributes.Count);
			Console.WriteLine (doc.CreateElement ("root2").Attributes.Count);
		}

		[Test]
		public void CreateElement1 ()
		{
			XmlElement element = document.CreateElement ("name");
			AssertElement (element, String.Empty, "name", String.Empty, 0);
		}

		[Test]
		public void CreateElement1WithPrefix ()
		{
			XmlElement element = document.CreateElement ("prefix:localName");
			AssertElement (element, "prefix", "localName", String.Empty, 0);
		}

		[Test]
		public void CreateElement2 ()
		{
			XmlElement element = document.CreateElement ("qualifiedName", "namespaceURI");
			AssertElement (element, String.Empty, "qualifiedName",
				       "namespaceURI", 0);
		}

		[Test]
		public void CreateElement2WithPrefix ()
		{
			XmlElement element = document.CreateElement ("prefix:localName", "namespaceURI");
			AssertElement (element, "prefix", "localName", "namespaceURI", 0);
		}

		[Test]
		public void CreateElement3 ()
		{
			XmlElement element = document.CreateElement ("prefix", "localName", "namespaceURI");
			AssertElement (element, "prefix", "localName", "namespaceURI", 0);
		}

		[Test]
		public void CreateElement3WithNullNamespace ()
		{
			// bug #26855, NamespaceURI should NEVER be null.
			XmlElement element = document.CreateElement (null, "localName", null);
			AssertElement (element, String.Empty, "localName", String.Empty, 0);
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			XmlElement element;
			XmlText text;
			XmlComment comment;
			
			element = document.CreateElement ("foo");
			AssertEquals (String.Empty, element.InnerXml);
			AssertEquals ("<foo />", element.OuterXml);

			text = document.CreateTextNode ("bar");
			element.AppendChild (text);
			AssertEquals ("bar", element.InnerXml);
			AssertEquals ("<foo>bar</foo>", element.OuterXml);

			element.SetAttribute ("baz", "quux");
			AssertEquals ("bar", element.InnerXml);
			AssertEquals ("<foo baz=\"quux\">bar</foo>", element.OuterXml);

			comment = document.CreateComment ("squonk");
			element.AppendChild (comment);
			AssertEquals ("bar<!--squonk-->", element.InnerXml);
			AssertEquals ("<foo baz=\"quux\">bar<!--squonk--></foo>", element.OuterXml);

			element.RemoveAll();
			element.AppendChild(document.CreateElement("hoge"));
			AssertEquals ("<hoge />", element.InnerXml);
		}

		[Test]
		public void SetGetAttribute ()
		{
			XmlElement element = document.CreateElement ("foo");
			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			AssertEquals ("val1", element.GetAttribute ("attr1"));
			AssertEquals ("val2", element.GetAttribute ("attr2"));
		}

		[Test]
		public void GetElementsByTagNameNoNameSpace ()
		{
			string xml = @"<library><book><title>XML Fun</title><author>John Doe</author>
				<price>34.95</price></book><book><title>Bear and the Dragon</title>
				<author>Tom Clancy</author><price>6.95</price></book><book>
				<title>Bourne Identity</title><author>Robert Ludlum</author>
				<price>9.95</price></book><Fluffer><Nutter><book>
				<title>Bourne Ultimatum</title><author>Robert Ludlum</author>
				<price>9.95</price></book></Nutter></Fluffer></library>";

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList libraryList = document.GetElementsByTagName ("library");
			XmlNode xmlNode = libraryList.Item (0);
			XmlElement xmlElement = xmlNode as XmlElement;
			XmlNodeList bookList = xmlElement.GetElementsByTagName ("book");
			AssertEquals ("GetElementsByTagName (string) returned incorrect count.", 4, bookList.Count);
		}

		[Test]
		public void GetElementsByTagNameUsingNameSpace ()
		{
			StringBuilder xml = new StringBuilder ();
			xml.Append ("<?xml version=\"1.0\" ?><library xmlns:North=\"http://www.foo.com\" ");
			xml.Append ("xmlns:South=\"http://www.goo.com\"><North:book type=\"non-fiction\"> ");
			xml.Append ("<North:title type=\"intro\">XML Fun</North:title> " );
			xml.Append ("<North:author>John Doe</North:author> " );
			xml.Append ("<North:price>34.95</North:price></North:book> " );
			xml.Append ("<South:book type=\"fiction\"> " );
			xml.Append ("<South:title>Bear and the Dragon</South:title> " );
			xml.Append ("<South:author>Tom Clancy</South:author> " );
			xml.Append ("<South:price>6.95</South:price></South:book> " );
			xml.Append ("<South:book type=\"fiction\"><South:title>Bourne Identity</South:title> " );
			xml.Append ("<South:author>Robert Ludlum</South:author> " );
			xml.Append ("<South:price>9.95</South:price></South:book></library>");

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml.ToString ()));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList libraryList = document.GetElementsByTagName ("library");
			XmlNode xmlNode = libraryList.Item (0);
			XmlElement xmlElement = xmlNode as XmlElement;
			XmlNodeList bookList = xmlElement.GetElementsByTagName ("book", "http://www.foo.com");
			AssertEquals ("GetElementsByTagName (string, uri) returned incorrect count.", 1, bookList.Count);
		}

		[Test]
		public void GetElementsByTagNameNs2 ()
		{
			document.LoadXml (@"<root>
			<x:a xmlns:x='urn:foo' id='a'>
			<y:a xmlns:y='urn:foo' id='b'/>
			<x:a id='c' />
			<z id='d' />
			text node
			<?a processing instruction ?>
			<x:w id='e'/>
			</x:a>
			</root>");
			// id='b' has different prefix. Should not caught by (name),
			// while should caught by (name, ns).
			XmlNodeList nl = document.DocumentElement.GetElementsByTagName ("x:a");
			AssertEquals (2, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("c", nl [1].Attributes ["id"].Value);

			nl = document.DocumentElement.GetElementsByTagName ("a", "urn:foo");
			AssertEquals (3, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("b", nl [1].Attributes ["id"].Value);
			AssertEquals ("c", nl [2].Attributes ["id"].Value);

			// name wildcard
			nl = document.DocumentElement.GetElementsByTagName ("*");
			AssertEquals (5, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("b", nl [1].Attributes ["id"].Value);
			AssertEquals ("c", nl [2].Attributes ["id"].Value);
			AssertEquals ("d", nl [3].Attributes ["id"].Value);
			AssertEquals ("e", nl [4].Attributes ["id"].Value);

			// wildcard - local and ns
			nl = document.DocumentElement.GetElementsByTagName ("*", "*");
			AssertEquals (5, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("b", nl [1].Attributes ["id"].Value);
			AssertEquals ("c", nl [2].Attributes ["id"].Value);
			AssertEquals ("d", nl [3].Attributes ["id"].Value);
			AssertEquals ("e", nl [4].Attributes ["id"].Value);

			// namespace wildcard - namespace
			nl = document.DocumentElement.GetElementsByTagName ("*", "urn:foo");
			AssertEquals (4, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("b", nl [1].Attributes ["id"].Value);
			AssertEquals ("c", nl [2].Attributes ["id"].Value);
			AssertEquals ("e", nl [3].Attributes ["id"].Value);

			// namespace wildcard - local only. I dare say, such usage is not XML-ish!
			nl = document.DocumentElement.GetElementsByTagName ("a", "*");
			AssertEquals (3, nl.Count);
			AssertEquals ("a", nl [0].Attributes ["id"].Value);
			AssertEquals ("b", nl [1].Attributes ["id"].Value);
			AssertEquals ("c", nl [2].Attributes ["id"].Value);
		}

		[Test]
		public void OuterXmlWithNamespace ()
		{
			XmlElement element = document.CreateElement ("foo", "bar", "#foo");
			AssertEquals ("<foo:bar xmlns:foo=\"#foo\" />", element.OuterXml);
		}		

		[Test]
		public void RemoveAllAttributes ()
		{
			StringBuilder xml = new StringBuilder ();
			xml.Append ("<?xml version=\"1.0\" ?><library><book type=\"non-fiction\" price=\"34.95\"> ");
			xml.Append ("<title type=\"intro\">XML Fun</title> " );
			xml.Append ("<author>John Doe</author></book></library>");

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml.ToString ()));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList bookList = document.GetElementsByTagName ("book");
			XmlNode xmlNode = bookList.Item (0);
			XmlElement xmlElement = xmlNode as XmlElement;
			xmlElement.RemoveAllAttributes ();
			AssertEquals ("attributes not properly removed.", false, xmlElement.HasAttribute ("type"));
		}

		[Test]
		public void RemoveDoesNotRemoveDefaultAttributes ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root   foo CDATA 'def'   bar CDATA #IMPLIED>]>";
			string xml = dtd + "<root bar='baz' />";
			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			// RemoveAll
			AssertNotNull (document.DocumentElement);
			AssertEquals (2, document.DocumentElement.Attributes.Count);
			AssertEquals ("baz", document.DocumentElement.GetAttribute ("bar"));
			AssertEquals ("def", document.DocumentElement.GetAttribute ("foo"));
			document.DocumentElement.RemoveAll ();
			AssertEquals (1, document.DocumentElement.Attributes.Count);
			AssertEquals ("def", document.DocumentElement.GetAttribute ("foo"));
			AssertEquals (String.Empty, document.DocumentElement.GetAttribute ("bar"));

			// RemoveAllAttributes
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAllAttributes ();
			AssertEquals (1, document.DocumentElement.Attributes.Count);

			// RemoveAttribute(name)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttribute ("foo");
			AssertEquals (2, document.DocumentElement.Attributes.Count);

			// RemoveAttribute(name, ns)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttribute ("foo", String.Empty);
			AssertEquals (2, document.DocumentElement.Attributes.Count);

			// RemoveAttributeAt
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeAt (1);
			AssertEquals (2, document.DocumentElement.Attributes.Count);

			// RemoveAttributeNode
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeNode (document.DocumentElement.Attributes [1]);
			AssertEquals (2, document.DocumentElement.Attributes.Count);

			// RemoveAttributeNode(name, ns)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeNode ("foo", String.Empty);
			AssertEquals (2, document.DocumentElement.Attributes.Count);
		}

		[Test]
		public void SetAttributeNode ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			XmlElement xmlEl = xmlDoc.CreateElement ("TestElement");
			XmlAttribute xmlAttribute = xmlEl.SetAttributeNode ("attr1", "namespace1");
			XmlAttribute xmlAttribute2 = xmlEl.SetAttributeNode ("attr2", "namespace2");
			AssertEquals ("attribute name not properly created.", true, xmlAttribute.Name.Equals ("attr1"));
			AssertEquals ("attribute namespace not properly created.", true, xmlAttribute.NamespaceURI.Equals ("namespace1"));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void SetAttributeNodeError ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:x='urn:foo'/>");
			doc.DocumentElement.SetAttributeNode ("x:lang", "urn:foo");
		}

		[Test]
		public void SetAttributeXmlns ()
		{
			// should not affect Element node's xmlns
			XmlElement el = document.CreateElement ("root");
			el.SetAttribute ("xmlns", "urn:foo");
			AssertEquals (String.Empty, el.NamespaceURI);
		}

		[Test]
		public void InnerTextAndEvent ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root><child>text</child><child2><![CDATA[cdata]]></child2></root>");
			doc.NodeInserted += new XmlNodeChangedEventHandler (
				OnNodeInserted);
			doc.NodeRemoved += new XmlNodeChangedEventHandler (
				OnNodeRemoved);
			// If only one child of the element is Text node,
			// then no events are fired.
			doc.DocumentElement.FirstChild.InnerText = "no events fired.";
			AssertEquals ("NoInsertEventFired", false, Inserted);
			AssertEquals ("NoRemoveEventFired", false, Removed);
			AssertEquals ("SetInnerTextToSingleText", "no events fired.", doc.DocumentElement.FirstChild.InnerText);
			Inserted = false;
			Removed = false;

			// if only one child of the element is CDataSection,
			// then events are fired.
			doc.DocumentElement.LastChild.InnerText = "events are fired.";
			AssertEquals ("InsertedEventFired", true, Inserted);
			AssertEquals ("RemovedEventFired", true, Removed);
			AssertEquals ("SetInnerTextToCDataSection", "events are fired.", doc.DocumentElement.LastChild.InnerText);
		}

		[Test]
		public void InnerXmlSetter ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root/>");
			XmlElement el =  doc.DocumentElement;
			AssertNull ("#Simple", el.FirstChild);
			el.InnerXml = "<foo><bar att='baz'/></foo>";
			XmlElement child = el.FirstChild as XmlElement;
			AssertNotNull ("#Simple.Child", child);
			AssertEquals ("#Simple.Child.Name", "foo", child.LocalName);

			XmlElement grandchild = child.FirstChild as XmlElement;
			AssertNotNull ("#Simple.GrandChild", grandchild);
			AssertEquals ("#Simple.GrandChild.Name", "bar", grandchild.LocalName);
			AssertEquals ("#Simple.GrandChild.Attr", "baz", grandchild.GetAttribute ("att"));

			doc.LoadXml ("<root xmlns='NS0' xmlns:ns1='NS1'><foo/><ns1:bar/><ns2:bar xmlns:ns2='NS2' /></root>");
			el = doc.DocumentElement.FirstChild.NextSibling as XmlElement;	// ns1:bar
			AssertNull ("#Namespaced.Prepare", el.FirstChild);
			el.InnerXml = "<ns1:baz />";
			AssertNotNull ("#Namespaced.Child", el.FirstChild);
			AssertEquals ("#Namespaced.Child.Name", "baz", el.FirstChild.LocalName);
			AssertEquals ("#Namespaced.Child.NSURI", "NS1", el.FirstChild.NamespaceURI);	// important!

			el.InnerXml = "<hoge />";
			AssertEquals ("#Namespaced.VerifyPreviousCleared", "hoge", el.FirstChild.Name);
		}

		[Test]
		public void InnerXmlSetter2 ()
		{
			// See bug #63574
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"<type>QPair&lt;QString,int&gt;::
<ref refid='classQPair'>QPair</ref>
&lt;
<ref refid='classQString'>QString</ref>
,int&gt;
</type>");
			XmlElement typeNode = doc.DocumentElement;
			typeNode.InnerText = "QPair<QString, int>";
			AssertEquals ("QPair<QString, int>", typeNode.InnerText);
		}

		[Test]
		public void IsEmpty ()
		{
			document.LoadXml ("<root><foo/><bar></bar></root>");
			Assertion.AssertEquals ("Empty", true, ((XmlElement) document.DocumentElement.FirstChild).IsEmpty);
			Assertion.AssertEquals ("Empty", false, ((XmlElement) document.DocumentElement.LastChild).IsEmpty);
		}

		[Test]
		public void RemoveAttribute ()
		{
			string xlinkURI = "http://www.w3.org/1999/XLink";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root a1='1' a2='2' xlink:href='urn:foo' xmlns:xlink='" + xlinkURI + "' />");

			XmlElement el =  doc.DocumentElement;
			el.RemoveAttribute ("a1");
			AssertNull ("RemoveAttribute", el.GetAttributeNode ("a1"));
			el.RemoveAttribute ("xlink:href");
			AssertNull ("RemoveAttribute", el.GetAttributeNode ("href", xlinkURI));
			el.RemoveAllAttributes ();
			AssertNull ("RemoveAllAttributes", el.GetAttributeNode ("a2"));
		}

		[Test]
		public void WriteToWithDefaultNamespace ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />");
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			doc.DocumentElement.WriteTo (xtw);
			AssertEquals ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", sw.ToString());
		}

		[Test]
		public void WriteToMakesNonsenseForDefaultNSChildren ()
		{
			XmlDocument d = new XmlDocument ();
			XmlElement x = d.CreateElement ("root");
			d.AppendChild (x);
			XmlElement a = d.CreateElement ("a");
			XmlElement b = d.CreateElement ("b");
			b.SetAttribute ("xmlns","probe");
			x.AppendChild (a);
			x.AppendChild (b);
			XmlElement b2 = d.CreateElement ("p2", "b2", "");
			b.AppendChild (b2);
			AssertEquals ("<root><a /><b xmlns=\"probe\"><b2 /></b></root>", d.OuterXml);
		}

		[Test]
		public void WriteToWithDeletedNamespacePrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy'><foo foo:bar='baz' /></root>");
			doc.DocumentElement.RemoveAllAttributes ();

			Assert (doc.DocumentElement.FirstChild.OuterXml.IndexOf("xmlns:foo") > 0);
		}

		[Test]
		public void WriteToWithDifferentNamespaceAttributes ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy' xmlns:html='http://www.w3.org/1999/xhtml' html:style='font-size: 1em'></root>");
			Assert (doc.OuterXml.IndexOf ("xmlns:html=\"http://www.w3.org/1999/xhtml\"") > 0);
		}

		[Test]
		public void WriteToDefaultAttribute ()
		{
			// default attributes should be ignored.

			string dtd = "<!DOCTYPE root[<!ATTLIST root hoge CDATA 'hoge-def'><!ENTITY foo 'ent-foo'>]>";
			string xml = dtd + "<root>&foo;</root>";
			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document,null);
			xvr.EntityHandling = EntityHandling.ExpandCharEntities;
			xvr.ValidationType = ValidationType.None;
			document.Load (xvr);
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			document.DocumentElement.WriteTo (xtw);
			AssertEquals ("<root>&foo;</root>", sw.ToString ());
		}
	}
}
