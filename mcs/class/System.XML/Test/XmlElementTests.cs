//
// XmlElementTests
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlElementTests : TestCase
	{
		public XmlElementTests () : base ("MonoTests.System.Xml.XmlElementTests testsuite") { }
		public XmlElementTests (string name) : base (name) { }

		private XmlDocument document;

		protected override void SetUp()
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

		public void TestCloneNode ()
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

		public void TestCreateElement1 ()
		{
			XmlElement element = document.CreateElement ("name");
			AssertElement (element, String.Empty, "name", String.Empty, 0);
		}

		public void TestCreateElement1WithPrefix ()
		{
			XmlElement element = document.CreateElement ("prefix:localName");
			AssertElement (element, "prefix", "localName", String.Empty, 0);
		}

		public void TestCreateElement2 ()
		{
			XmlElement element = document.CreateElement ("qualifiedName", "namespaceURI");
			AssertElement (element, String.Empty, "qualifiedName",
				       "namespaceURI", 0);
		}

		public void TestCreateElement2WithPrefix ()
		{
			XmlElement element = document.CreateElement ("prefix:localName", "namespaceURI");
			AssertElement (element, "prefix", "localName", "namespaceURI", 0);
		}

		public void TestCreateElement3 ()
		{
			XmlElement element = document.CreateElement ("prefix", "localName", "namespaceURI");
			AssertElement (element, "prefix", "localName", "namespaceURI", 0);
		}

		public void TestCreateElement3WithNullNamespace ()
		{
			// bug #26855, NamespaceURI should NEVER be null.
			XmlElement element = document.CreateElement (null, "localName", null);
			AssertElement (element, String.Empty, "localName", String.Empty, 0);
		}

		public void TestInnerAndOuterXml ()
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


		}

		public void TestSetGetAttribute ()
		{
			XmlElement element = document.CreateElement ("foo");
			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			AssertEquals ("val1", element.GetAttribute ("attr1"));
			AssertEquals ("val2", element.GetAttribute ("attr2"));
		}
	}
}
