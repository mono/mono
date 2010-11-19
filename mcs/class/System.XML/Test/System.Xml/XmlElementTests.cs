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
			Assert.AreEqual (prefix != String.Empty ? prefix + ":" + localName : localName, element.Name);
			Assert.AreEqual (prefix, element.Prefix);
			Assert.AreEqual (localName, element.LocalName);
			Assert.AreEqual (namespaceURI, element.NamespaceURI);
			//Assert.AreEqual (attributesCount, element.Attributes.Count);
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
			// Assert.AreEqual (deep.OuterXml, element.OuterXml, "These should be the same"); 
			Assert.IsNull (deep.ParentNode, "This is not null");
			Assert.IsTrue (!Object.ReferenceEquals (element, deep), "Copies, not pointers");

			XmlNode shallow = element.CloneNode (false);
			Assert.IsNull (shallow.ParentNode, "This is not null");
			Assert.IsTrue (!Object.ReferenceEquals (element, shallow), "Copies, not pointers");
			Assert.AreEqual (false, shallow.HasChildNodes, "Shallow clones shalt have no children!");
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
			Assert.AreEqual (String.Empty, element.InnerXml);
			Assert.AreEqual ("<foo />", element.OuterXml);

			text = document.CreateTextNode ("bar");
			element.AppendChild (text);
			Assert.AreEqual ("bar", element.InnerXml);
			Assert.AreEqual ("<foo>bar</foo>", element.OuterXml);

			element.SetAttribute ("baz", "quux");
			Assert.AreEqual ("bar", element.InnerXml);
			Assert.AreEqual ("<foo baz=\"quux\">bar</foo>", element.OuterXml);

			comment = document.CreateComment ("squonk");
			element.AppendChild (comment);
			Assert.AreEqual ("bar<!--squonk-->", element.InnerXml);
			Assert.AreEqual ("<foo baz=\"quux\">bar<!--squonk--></foo>", element.OuterXml);

			element.RemoveAll();
			element.AppendChild(document.CreateElement("hoge"));
			Assert.AreEqual ("<hoge />", element.InnerXml);
		}

		[Test]
		public void SetGetAttribute ()
		{
			XmlElement element = document.CreateElement ("foo");
			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			Assert.AreEqual ("val1", element.GetAttribute ("attr1"));
			Assert.AreEqual ("val2", element.GetAttribute ("attr2"));
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
			Assert.AreEqual (4, bookList.Count, "GetElementsByTagName (string) returned incorrect count.");
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
			Assert.AreEqual (1, bookList.Count, "GetElementsByTagName (string, uri) returned incorrect count.");
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
			Assert.AreEqual (2, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [1].Attributes ["id"].Value);

			nl = document.DocumentElement.GetElementsByTagName ("a", "urn:foo");
			Assert.AreEqual (3, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);

			// name wildcard
			nl = document.DocumentElement.GetElementsByTagName ("*");
			Assert.AreEqual (5, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("d", nl [3].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [4].Attributes ["id"].Value);

			// wildcard - local and ns
			nl = document.DocumentElement.GetElementsByTagName ("*", "*");
			Assert.AreEqual (5, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("d", nl [3].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [4].Attributes ["id"].Value);

			// namespace wildcard - namespace
			nl = document.DocumentElement.GetElementsByTagName ("*", "urn:foo");
			Assert.AreEqual (4, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
			Assert.AreEqual ("e", nl [3].Attributes ["id"].Value);

			// namespace wildcard - local only. I dare say, such usage is not XML-ish!
			nl = document.DocumentElement.GetElementsByTagName ("a", "*");
			Assert.AreEqual (3, nl.Count);
			Assert.AreEqual ("a", nl [0].Attributes ["id"].Value);
			Assert.AreEqual ("b", nl [1].Attributes ["id"].Value);
			Assert.AreEqual ("c", nl [2].Attributes ["id"].Value);
		}

		[Test]
		public void OuterXmlWithNamespace ()
		{
			XmlElement element = document.CreateElement ("foo", "bar", "#foo");
			Assert.AreEqual ("<foo:bar xmlns:foo=\"#foo\" />", element.OuterXml);
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
			Assert.AreEqual (false, xmlElement.HasAttribute ("type"), "attributes not properly removed.");
		}

		[Test]
#if NET_2_0
		[Ignore ("This test is very implementation dependent and thus .NET 2.0 does not pass. That's why I said http://primates.ximian.com/~atsushi/blog/archives/000416.html and http://svn.myrealbox.com/viewcvs/trunk/mono/web/xml-classes?rev=23598")]
#endif
		public void RemoveDoesNotRemoveDefaultAttributes ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root   foo CDATA 'def'   bar CDATA #IMPLIED>]>";
			string xml = dtd + "<root bar='baz' />";
			XmlValidatingReader xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			// RemoveAll
			Assert.IsNotNull (document.DocumentElement);
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #01");
			Assert.AreEqual ("baz", document.DocumentElement.GetAttribute ("bar"));
			Assert.AreEqual ("def", document.DocumentElement.GetAttribute ("foo"));
			Assert.AreEqual (false, document.DocumentElement.GetAttributeNode ("foo").Specified);
			document.DocumentElement.RemoveAll ();
			Assert.AreEqual (1, document.DocumentElement.Attributes.Count, "attrCount #02");
			Assert.AreEqual ("def", document.DocumentElement.GetAttribute ("foo"));
			Assert.AreEqual (String.Empty, document.DocumentElement.GetAttribute ("bar"));

			// RemoveAllAttributes
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAllAttributes ();
			Assert.AreEqual (1, document.DocumentElement.Attributes.Count, "attrCount #03");

			// RemoveAttribute(name)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttribute ("foo");
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #04");

			// RemoveAttribute(name, ns)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttribute ("foo", String.Empty);
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #05");

			// RemoveAttributeAt
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeAt (1);
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #06");

			// RemoveAttributeNode
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeNode (document.DocumentElement.Attributes [1]);
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #07");

			// RemoveAttributeNode(name, ns)
			xvr = new XmlValidatingReader (xml, XmlNodeType.Document, null);
			document.Load (xvr);
			document.DocumentElement.RemoveAttributeNode ("foo", String.Empty);
			Assert.AreEqual (2, document.DocumentElement.Attributes.Count, "attrCount #08");
		}

		[Test]
		public void SetAttributeNode ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			XmlElement xmlEl = xmlDoc.CreateElement ("TestElement");
			XmlAttribute xmlAttribute = xmlEl.SetAttributeNode ("attr1", "namespace1");
			XmlAttribute xmlAttribute2 = xmlEl.SetAttributeNode ("attr2", "namespace2");
			Assert.AreEqual (true, xmlAttribute.Name.Equals ("attr1"), "attribute name not properly created.");
			Assert.AreEqual (true, xmlAttribute.NamespaceURI.Equals ("namespace1"), "attribute namespace not properly created.");
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
			Assert.AreEqual (String.Empty, el.NamespaceURI);
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
			Assert.AreEqual (false, Inserted, "NoInsertEventFired");
			Assert.AreEqual (false, Removed, "NoRemoveEventFired");
			Assert.AreEqual ("no events fired.", doc.DocumentElement.FirstChild.InnerText, "SetInnerTextToSingleText");
			Inserted = false;
			Removed = false;

			// if only one child of the element is CDataSection,
			// then events are fired.
			doc.DocumentElement.LastChild.InnerText = "events are fired.";
			Assert.AreEqual (true, Inserted, "InsertedEventFired");
			Assert.AreEqual (true, Removed, "RemovedEventFired");
			Assert.AreEqual ("events are fired.", doc.DocumentElement.LastChild.InnerText, "SetInnerTextToCDataSection");
		}

		[Test]
		public void InnerXmlSetter ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root/>");
			XmlElement el =  doc.DocumentElement;
			Assert.IsNull (el.FirstChild, "#Simple");
			el.InnerXml = "<foo><bar att='baz'/></foo>";
			XmlElement child = el.FirstChild as XmlElement;
			Assert.IsNotNull (child, "#Simple.Child");
			Assert.AreEqual ("foo", child.LocalName, "#Simple.Child.Name");

			XmlElement grandchild = child.FirstChild as XmlElement;
			Assert.IsNotNull (grandchild, "#Simple.GrandChild");
			Assert.AreEqual ("bar", grandchild.LocalName, "#Simple.GrandChild.Name");
			Assert.AreEqual ("baz", grandchild.GetAttribute ("att"), "#Simple.GrandChild.Attr");

			doc.LoadXml ("<root xmlns='NS0' xmlns:ns1='NS1'><foo/><ns1:bar/><ns2:bar xmlns:ns2='NS2' /></root>");
			el = doc.DocumentElement.FirstChild.NextSibling as XmlElement;	// ns1:bar
			Assert.IsNull (el.FirstChild, "#Namespaced.Prepare");
			el.InnerXml = "<ns1:baz />";
			Assert.IsNotNull (el.FirstChild, "#Namespaced.Child");
			Assert.AreEqual ("baz", el.FirstChild.LocalName, "#Namespaced.Child.Name");
			Assert.AreEqual ("NS1", el.FirstChild.NamespaceURI, "#Namespaced.Child.NSURI");	// important!

			el.InnerXml = "<hoge />";
			Assert.AreEqual ("hoge", el.FirstChild.Name, "#Namespaced.VerifyPreviousCleared");
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
			Assert.AreEqual ("QPair<QString, int>", typeNode.InnerText);
		}

		[Test]
		public void IsEmpty ()
		{
			document.LoadXml ("<root><foo/><bar></bar></root>");
			Assert.AreEqual (true, ((XmlElement) document.DocumentElement.FirstChild).IsEmpty, "Empty");
			Assert.AreEqual (false, ((XmlElement) document.DocumentElement.LastChild).IsEmpty, "Empty");
		}

		[Test]
		public void RemoveAttribute ()
		{
			string xlinkURI = "http://www.w3.org/1999/XLink";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root a1='1' a2='2' xlink:href='urn:foo' xmlns:xlink='" + xlinkURI + "' />");

			XmlElement el =  doc.DocumentElement;
			el.RemoveAttribute ("a1");
			Assert.IsNull (el.GetAttributeNode ("a1"), "RemoveAttribute");
			el.RemoveAttribute ("xlink:href");
			Assert.IsNull (el.GetAttributeNode ("href", xlinkURI), "RemoveAttribute");
			el.RemoveAllAttributes ();
			Assert.IsNull (el.GetAttributeNode ("a2"), "RemoveAllAttributes");
		}

		[Test]
		public void WriteToWithDefaultNamespace ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />");
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			doc.DocumentElement.WriteTo (xtw);
			Assert.AreEqual ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", sw.ToString());
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
			Assert.AreEqual ("<root><a /><b xmlns=\"probe\"><b2 /></b></root>", d.OuterXml);
		}

		[Test]
		public void WriteToWithDeletedNamespacePrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy'><foo foo:bar='baz' /></root>");
			doc.DocumentElement.RemoveAllAttributes ();

			Assert.IsTrue (doc.DocumentElement.FirstChild.OuterXml.IndexOf("xmlns:foo") > 0);
		}

		[Test]
		public void WriteToWithDifferentNamespaceAttributes ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns:foo='urn:dummy' xmlns:html='http://www.w3.org/1999/xhtml' html:style='font-size: 1em'></root>");
			Assert.IsTrue (doc.OuterXml.IndexOf ("xmlns:html=\"http://www.w3.org/1999/xhtml\"") > 0);
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
			Assert.AreEqual ("<root>&foo;</root>", sw.ToString ());
		}

		[Test]
#if ONLY_1_1
		[ExpectedException (typeof (ArgumentNullException))]
#endif
		public void SetNullPrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root/>");
			doc.DocumentElement.Prefix = null;

#if NET_2_0
			Assert.AreEqual (string.Empty, doc.DocumentElement.Prefix, "#1");
			AssertClearPrefix ((string) null);
#endif
		}

		[Test]
		public void SetEmptyStringPrefix ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<root />");
			doc.DocumentElement.Prefix = String.Empty;
			Assert.AreEqual (string.Empty, doc.DocumentElement.Prefix, "#1");

			AssertClearPrefix (string.Empty);

		}

		private void AssertClearPrefix (string newPrefix)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<x:root xmlns:x=\"http://somenamespace.com\" />");
			Assert.AreEqual ("<x:root xmlns:x=\"http://somenamespace.com\" />", doc.OuterXml, "#Clear1");
			Assert.AreEqual ("<x:root xmlns:x=\"http://somenamespace.com\" />", doc.DocumentElement.OuterXml, "#Clear2");
			Assert.AreEqual ("x", doc.DocumentElement.Prefix, "#Clear3");
			doc.DocumentElement.Prefix = newPrefix;
			Assert.AreEqual ("<root xmlns:x=\"http://somenamespace.com\" xmlns=\"http://somenamespace.com\" />", doc.OuterXml, "#Clear4");
			Assert.AreEqual ("<root xmlns:x=\"http://somenamespace.com\" xmlns=\"http://somenamespace.com\" />", doc.DocumentElement.OuterXml, "#Clear5");
			Assert.AreEqual (string.Empty, doc.DocumentElement.Prefix, "#Clear6");
		}

		[Test]
		public void NullPrefix ()
		{
			new MyXmlElement ("foo", "urn:foo", new XmlDocument ());
		}

		[Test] // bug #380720
		[Category ("Networking")]
		public void SetAttributeWithIdentity ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd' []>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head></head>
<body><div id='xxx'>XXX</div><div id='yyy'>YYY</div></body>
</html>");
			XmlElement xxx = (XmlElement) doc.GetElementsByTagName ("div") [0];
			XmlElement yyy = (XmlElement) doc.GetElementsByTagName ("div") [1];
			yyy.ParentNode.RemoveChild (yyy);
			yyy.SetAttribute ("id", "xxx");
		}

		[Test]
		public void SetAttributeExistingNoInsert () // bug #464394
		{
			XmlDocument doc = new XmlDocument ();
			bool changed = false;
			doc.LoadXml (@"<MyNode Key='ABC' ClientName='xxx' DateIssued='yyy' />");
			doc.NodeChanged += delegate {
				changed = true;
			};
			doc.DocumentElement.SetAttribute ("Key", "");
			Assert.IsTrue (changed);
		}

		class MyXmlElement : XmlElement
		{
			public MyXmlElement (string localName, string ns, XmlDocument doc)
				: base (null, localName, ns, doc)
			{
			}
		}
	}
}
