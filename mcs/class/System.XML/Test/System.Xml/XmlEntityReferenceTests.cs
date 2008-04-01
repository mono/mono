//
// System.Xml.XmlEntityReference.cs
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Atsushi Enomoto
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlEntityReferenceTests : Assertion
	{
		[Test]
		public void WriteTo ()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root/>");
			XmlEntityReference er = doc.CreateEntityReference("foo");
			doc.DocumentElement.AppendChild(er);
			AssertEquals ("Name", "foo", er.Name);
			AssertEquals ("WriteTo", "<root>&foo;</root>", doc.DocumentElement.OuterXml);
		}

		[Test]
		public void DescendantsRecursively ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root (#PCDATA)*>"
				+ "<!ENTITY ent 'value'>"
				+ "<!ENTITY ent2 'my &ent; string'>"
				+ "]>";
			string xml = dtd + "<root>&ent;</root>";
			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			XmlDocument doc = new XmlDocument ();
			doc.Load (xtr);
			XmlEntity ent = (XmlEntity) doc.DocumentType.Entities.GetNamedItem ("ent2");
			AssertEquals ("ent2", ent.Name);
			AssertEquals ("my ", ent.FirstChild.Value);
			AssertNotNull (ent.FirstChild.NextSibling.FirstChild);
			AssertEquals ("value", ent.FirstChild.NextSibling.FirstChild.Value);
		}

		[Test]
		public void ChildNodes ()
		{
			XmlTextReader xtr = new XmlTextReader ("<!DOCTYPE root [<!ENTITY ent 'ent-value'><!ENTITY el '<foo>hoge</foo><bar/>'>]><root/>",
				XmlNodeType.Document, null);
			XmlDocument doc = new XmlDocument ();

			doc.Load (xtr);
			XmlEntityReference ent = doc.CreateEntityReference ("ent");
			// ChildNodes are not added yet.
			AssertNull (ent.FirstChild);
			doc.DocumentElement.AppendChild (ent);
			// ChildNodes are added here.
			AssertNotNull (ent.FirstChild);

			ent = doc.CreateEntityReference ("foo");
			AssertNull (ent.FirstChild);
			// Entity value is empty when the matching DTD entity 
			// node does not exist.
			doc.DocumentElement.AppendChild (ent);
			AssertNotNull (ent.FirstChild);

			AssertEquals (String.Empty, ent.FirstChild.Value);

			ent = doc.CreateEntityReference ("el");
			AssertEquals ("", ent.InnerText);
			doc.DocumentElement.AppendChild (ent);
			AssertEquals ("<foo>hoge</foo><bar />", ent.InnerXml);
			AssertEquals ("hoge", ent.InnerText);
			AssertEquals (XmlNodeType.Element, ent.FirstChild.NodeType);
		}
	}
}
