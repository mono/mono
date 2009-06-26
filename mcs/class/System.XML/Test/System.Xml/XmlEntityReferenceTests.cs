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
	public class XmlEntityReferenceTests
	{
		[Test]
		public void WriteTo ()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root/>");
			XmlEntityReference er = doc.CreateEntityReference("foo");
			doc.DocumentElement.AppendChild(er);
			Assert.AreEqual ("foo", er.Name, "Name");
			Assert.AreEqual ("<root>&foo;</root>", doc.DocumentElement.OuterXml, "WriteTo");
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
			Assert.AreEqual ("ent2", ent.Name);
			Assert.AreEqual ("my ", ent.FirstChild.Value);
			Assert.IsNotNull (ent.FirstChild.NextSibling.FirstChild);
			Assert.AreEqual ("value", ent.FirstChild.NextSibling.FirstChild.Value);
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
			Assert.IsNull (ent.FirstChild);
			doc.DocumentElement.AppendChild (ent);
			// ChildNodes are added here.
			Assert.IsNotNull (ent.FirstChild);

			ent = doc.CreateEntityReference ("foo");
			Assert.IsNull (ent.FirstChild);
			// Entity value is empty when the matching DTD entity 
			// node does not exist.
			doc.DocumentElement.AppendChild (ent);
			Assert.IsNotNull (ent.FirstChild);

			Assert.AreEqual (String.Empty, ent.FirstChild.Value);

			ent = doc.CreateEntityReference ("el");
			Assert.AreEqual ("", ent.InnerText);
			doc.DocumentElement.AppendChild (ent);
			Assert.AreEqual ("<foo>hoge</foo><bar />", ent.InnerXml);
			Assert.AreEqual ("hoge", ent.InnerText);
			Assert.AreEqual (XmlNodeType.Element, ent.FirstChild.NodeType);
		}
	}
}
