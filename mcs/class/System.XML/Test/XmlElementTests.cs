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

namespace Ximian.Mono.Tests
{
	public class XmlElementTests : TestCase
	{
		public XmlElementTests () : base ("Ximian.Mono.Tests.XmlElementTests testsuite") { }
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

		public void TestSetGetAttribute ()
		{
			XmlElement element = document.CreateElement ("foo");
			element.SetAttribute ("attr1", "val1");
			element.SetAttribute ("attr2", "val2");
			AssertEquals ("val1", element.GetAttribute ("attr1"));
			AssertEquals ("val2", element.GetAttribute ("attr2"));
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
	}
}
