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
	public class XmlElementTests 
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
			Assertion.AssertEquals (prefix != String.Empty ? prefix + ":" + localName : localName, element.Name);
			Assertion.AssertEquals (prefix, element.Prefix);
			Assertion.AssertEquals (localName, element.LocalName);
			Assertion.AssertEquals (namespaceURI, element.NamespaceURI);
			//Assertion.AssertEquals (attributesCount, element.Attributes.Count);
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
			// Assertion.AssertEquals ("These should be the same", deep.OuterXml, element.OuterXml); 
			Assertion.AssertNull ("This is not null", deep.ParentNode);
			Assertion.Assert ("Copies, not pointers", !Object.ReferenceEquals (element,deep));

			XmlNode shallow = element.CloneNode (false);
			Assertion.AssertNull ("This is not null", shallow.ParentNode);
			Assertion.Assert ("Copies, not pointers", !Object.ReferenceEquals (element,shallow));
			Assertion.AssertEquals ("Shallow clones shalt have no children!", false, shallow.HasChildNodes);
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
			Assertion.AssertEquals (String.Empty, element.InnerXml);
			Assertion.AssertEquals ("<foo />", element.OuterXml);

			text = document.CreateTextNode ("bar");
			element.AppendChild (text);
			Assertion.AssertEquals ("bar", element.InnerXml);
			Assertion.AssertEquals ("<foo>bar</foo>", element.OuterXml);

			element.SetAttribute ("baz", "quux");
			Assertion.AssertEquals ("bar", element.InnerXml);
			Assertion.AssertEquals ("<foo baz=\"quux\">bar</foo>", element.OuterXml);

			comment = document.CreateComment ("squonk");
			element.AppendChild (comment);
			Assertion.AssertEquals ("bar<!--squonk-->", element.InnerXml);
			Assertion.AssertEquals ("<foo baz=\"quux\">bar<!--squonk--></foo>", element.OuterXml);

			element.RemoveAll();
			element.AppendChild(document.CreateElement("hoge"));
			Assertion.AssertEquals ("<hoge />", element.InnerXml);
		}

		[Test]
		public void SetGetAttribute ()
		{
			XmlElement element = document.CreateElement ("foo");
			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			Assertion.AssertEquals ("val1", element.GetAttribute ("attr1"));
			Assertion.AssertEquals ("val2", element.GetAttribute ("attr2"));
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
			Assertion.AssertEquals ("GetElementsByTagName (string) returned incorrect count.", 4, bookList.Count);
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
			Assertion.AssertEquals ("GetElementsByTagName (string, uri) returned incorrect count.", 1, bookList.Count);
		}

		[Test]
		public void OuterXmlWithNamespace ()
		{
			XmlElement element = document.CreateElement ("foo", "bar", "#foo");
			Assertion.AssertEquals ("<foo:bar xmlns:foo=\"#foo\" />", element.OuterXml);
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
			Assertion.AssertEquals ("attributes not properly removed.", false, xmlElement.HasAttribute ("type"));
		}

		[Test]
		public void SetAttributeNode ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			XmlElement xmlEl = xmlDoc.CreateElement ("TestElement");
			XmlAttribute xmlAttribute = xmlEl.SetAttributeNode ("attr1", "namespace1");
			XmlAttribute xmlAttribute2 = xmlEl.SetAttributeNode ("attr2", "namespace2");
			Assertion.AssertEquals ("attribute name not properly created.", true, xmlAttribute.Name.Equals ("attr1"));
			Assertion.AssertEquals ("attribute namespace not properly created.", true, xmlAttribute.NamespaceURI.Equals ("namespace1"));
		}

		[Test]
		public void InnerXmlSetter ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root/>");
			XmlElement el =  doc.DocumentElement;
			Assertion.AssertNull ("#Simple", el.FirstChild);
			el.InnerXml = "<foo><bar att='baz'/></foo>";
			XmlElement child = el.FirstChild as XmlElement;
			Assertion.AssertNotNull ("#Simple.Child", child);
			Assertion.AssertEquals ("#Simple.Child.Name", "foo", child.LocalName);

			XmlElement grandchild = child.FirstChild as XmlElement;
			Assertion.AssertNotNull ("#Simple.GrandChild", grandchild);
			Assertion.AssertEquals ("#Simple.GrandChild.Name", "bar", grandchild.LocalName);
			Assertion.AssertEquals ("#Simple.GrandChild.Attr", "baz", grandchild.GetAttribute ("att"));

			doc.LoadXml ("<root xmlns='NS0' xmlns:ns1='NS1'><foo/><ns1:bar/><ns2:bar xmlns:ns2='NS2' /></root>");
			el = doc.DocumentElement.FirstChild.NextSibling as XmlElement;	// ns1:bar
			Assertion.AssertNull ("#Namespaced.Prepare", el.FirstChild);
			el.InnerXml = "<ns1:baz />";
			Assertion.AssertNotNull ("#Namespaced.Child", el.FirstChild);
			Assertion.AssertEquals ("#Namespaced.Child.Name", "baz", el.FirstChild.LocalName);
			Assertion.AssertEquals ("#Namespaced.Child.NSURI", "NS1", el.FirstChild.NamespaceURI);	// important!

			el.InnerXml = "<hoge />";
			Assertion.AssertEquals ("#Namespaced.VerifyPreviousCleared", "hoge", el.FirstChild.Name);
		}

		[Test]
		public void RemoveAttribute ()
		{
			string xlinkURI = "http://www.w3.org/1999/XLink";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root a1='1' a2='2' xlink:href='urn:foo' xmlns:xlink='" + xlinkURI + "' />");

			XmlElement el =  doc.DocumentElement;
			el.RemoveAttribute ("a1");
			Assertion.AssertNull ("RemoveAttribute", el.GetAttributeNode ("a1"));
			el.RemoveAttribute ("xlink:href");
			Assertion.AssertNull ("RemoveAttribute", el.GetAttributeNode ("href", xlinkURI));
			el.RemoveAllAttributes ();
			Assertion.AssertNull ("RemoveAllAttributes", el.GetAttributeNode ("a2"));
		}

		[Test]
		public void WriteToWithDefaultNamespace ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<RetrievalElement URI=\"\"xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />");
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			doc.DocumentElement.WriteTo (xtw);
			Assertion.AssertEquals ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", sw.ToString());
		}

		[Test]
		public void WriteToWithDeletedNamespacePrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy'><foo foo:bar='baz' /></root>");
			doc.DocumentElement.RemoveAllAttributes ();

			Assertion.Assert (doc.DocumentElement.FirstChild.OuterXml.IndexOf("xmlns:foo") > 0);
		}

		[Test]
		public void WriteToWithDifferentNamespaceAttributes ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy' xmlns:html='http://www.w3.org/1999/xhtml' html:style='font-size: 1em'></root>");
			Assertion.Assert (doc.OuterXml.IndexOf ("xmlns:html=\"http://www.w3.org/1999/xhtml\"") > 0);
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
			Assertion.AssertEquals ("NoInsertEventFired", false, Inserted);
			Assertion.AssertEquals ("NoRemoveEventFired", false, Removed);
			Assertion.AssertEquals ("SetInnerTextToSingleText", "no events fired.", doc.DocumentElement.FirstChild.InnerText);
			Inserted = false;
			Removed = false;

			// if only one child of the element is CDataSection,
			// then events are fired.
			doc.DocumentElement.LastChild.InnerText = "events are fired.";
			Assertion.AssertEquals ("InsertedEventFired", true, Inserted);
			Assertion.AssertEquals ("RemovedEventFired", true, Removed);
			Assertion.AssertEquals ("SetInnerTextToCDataSection", "events are fired.", doc.DocumentElement.LastChild.InnerText);
		}
	}
}
