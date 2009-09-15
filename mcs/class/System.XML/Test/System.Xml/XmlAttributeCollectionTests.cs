// XmlAttributeCollectionTests.cs : Tests for the XmlAttributeCollection class
//
// Author: Matt Hunter <xrkune@tconl.com>
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Matt Hunter
// (C) 2003 Martin Willemoes Hansen

using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlAttributeCollectionTests
	{
		private XmlDocument document;

		[SetUp]
		public void GetReady()
		{
			document = new XmlDocument ();
		}

		[Test]
		public void RemoveAll ()
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
			XmlAttributeCollection attributes = xmlElement.Attributes;
			attributes.RemoveAll ();
			Assert.AreEqual (false, xmlElement.HasAttribute ("type"), "not all attributes removed.");
		}

		[Test]
		public void Append () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			XmlElement xmlEl = xmlDoc.CreateElement ("TestElement");
			XmlAttribute xmlAttribute = xmlEl.SetAttributeNode ("attr1", "namespace1");
			XmlNode xmlNode = xmlDoc.CreateNode (XmlNodeType.Attribute, "attr3", "namespace1");
			XmlAttribute xmlAttribute3 = xmlNode as XmlAttribute;
			XmlAttributeCollection attributeCol = xmlEl.Attributes;
			xmlAttribute3 = attributeCol.Append (xmlAttribute3);
			Assert.AreEqual (true, xmlAttribute3.Name.Equals ("attr3"), "attribute name not properly created.");
			Assert.AreEqual (true, xmlAttribute3.NamespaceURI.Equals ("namespace1"), "attribute namespace not properly created.");
		}

		[Test]
		public void CopyTo () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root a1='garnet' a2='amethyst' a3='Bloodstone' a4='diamond' a5='emerald' a6='pearl' a7='ruby' a8='sapphire' a9='moonstone' a10='opal' a11='topaz' a12='turquoize' />");
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;
			XmlAttribute[] array = new XmlAttribute[24];
			col.CopyTo(array, 0);
			Assert.AreEqual ("garnet", array[0].Value);
			Assert.AreEqual ("moonstone", array[8].Value);
			Assert.AreEqual ("turquoize", array[11].Value);
			col.CopyTo(array, 12);
			Assert.AreEqual ("garnet", array[12].Value);
			Assert.AreEqual ("moonstone", array[20].Value);
			Assert.AreEqual ("turquoize", array[23].Value);
		}

		[Test]
		public void SetNamedItem ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;

			XmlAttribute attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "bloodstone";
			col.SetNamedItem(attr);
			Assert.AreEqual ("bloodstone", el.GetAttribute("b3"), "SetNamedItem.Normal");

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "aquamaline";
			col.SetNamedItem(attr);
			Assert.AreEqual ("aquamaline", el.GetAttribute("b3"), "SetNamedItem.Override");
			Assert.AreEqual (1, el.Attributes.Count, "SetNamedItem.Override.Count.1");
			Assert.AreEqual (1, col.Count, "SetNamedItem.Override.Count.2");
		}

		[Test]
		public void InsertBeforeAfterPrepend () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root b2='amethyst' />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;
			XmlAttribute attr = xmlDoc.CreateAttribute("b1");
			attr.Value = "garnet";
			col.InsertAfter(attr, null);
			Assert.AreEqual ("garnet", el.GetAttributeNode("b1").Value, "InsertAfterNull");
			Assert.AreEqual (el.GetAttribute("b1"), col[0].Value, "InsertAfterNull.Pos");

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "bloodstone";
			col.InsertAfter(attr, el.GetAttributeNode("b2"));
			Assert.AreEqual ("bloodstone", el.GetAttributeNode("b3").Value, "InsertAfterAttr");
			Assert.AreEqual (el.GetAttribute("b3"), col[2].Value, "InsertAfterAttr.Pos");

			attr = xmlDoc.CreateAttribute("b4");
			attr.Value = "diamond";
			col.InsertBefore(attr, null);
			Assert.AreEqual ("diamond", el.GetAttributeNode("b4").Value, "InsertBeforeNull");
			Assert.AreEqual (el.GetAttribute("b4"), col[3].Value, "InsertBeforeNull.Pos");

			attr = xmlDoc.CreateAttribute("warning");
			attr.Value = "mixed modern and traditional;-)";
			col.InsertBefore(attr, el.GetAttributeNode("b1"));
			Assert.AreEqual ("mixed modern and traditional;-)", el.GetAttributeNode("warning").Value, "InsertBeforeAttr");
			Assert.AreEqual (el.GetAttributeNode("warning").Value, col[0].Value, "InsertBeforeAttr.Pos");

			attr = xmlDoc.CreateAttribute("about");
			attr.Value = "lists of birthstone.";
			col.Prepend(attr);
			Assert.AreEqual ("lists of birthstone.", col[0].Value, "Prepend");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InsertAfterError ()
		{
			this.document.LoadXml ("<root><elem a='1'/></root>");
			XmlAttribute attr = document.CreateAttribute ("foo");
			attr.Value = "test";
			document.DocumentElement.Attributes.InsertAfter (attr, document.DocumentElement.FirstChild.Attributes [0]);
		}

		[Test]
		public void InsertAfterReplacesInCorrectOrder ()
		{
			XmlDocument testDoc = new XmlDocument ();
			XmlElement testElement = testDoc.CreateElement ("TestElement" );
			testDoc.AppendChild (testElement);

			XmlAttribute testAttr1 = testDoc.CreateAttribute ("TestAttribute1");
			testAttr1.Value = "First attribute";
			testElement.Attributes.Prepend (testAttr1);

			XmlAttribute testAttr2 = testDoc.CreateAttribute ("TestAttribute2");
			testAttr2.Value = "Second attribute";
			testElement.Attributes.InsertAfter (testAttr2, testAttr1);

			XmlAttribute testAttr3 = testDoc.CreateAttribute ("TestAttribute3");
			testAttr3.Value = "Third attribute";
			testElement.Attributes.InsertAfter (testAttr3, testAttr2);

			XmlAttribute outOfOrder = testDoc.CreateAttribute ("TestAttribute2");
			outOfOrder.Value = "Should still be second attribute";
			testElement.Attributes.InsertAfter (outOfOrder, testElement.Attributes [0]);

			Assert.AreEqual ("First attribute", testElement.Attributes [0].Value);
			Assert.AreEqual ("Should still be second attribute", testElement.Attributes [1].Value);
			Assert.AreEqual ("Third attribute", testElement.Attributes [2].Value);
		}

		[Test]
		public void Remove ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root a1='garnet' a2='amethyst' a3='bloodstone' a4='diamond' a5='emerald' a6='pearl' a7='ruby' a8='sapphire' a9='moonstone' a10='opal' a11='topaz' a12='turquoize' />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = el.Attributes;

			// Remove
			XmlAttribute attr = col.Remove(el.GetAttributeNode("a12"));
			Assert.AreEqual (11, col.Count, "Remove");
			Assert.AreEqual ("a12", attr.Name, "Remove.Removed");

			// RemoveAt
			attr = col.RemoveAt(5);
			Assert.AreEqual (null, el.GetAttributeNode("a6"), "RemoveAt");
			Assert.AreEqual ("pearl", attr.Value, "Remove.Removed");
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // enbug
#endif
		public void RemoveDefaultAttribute ()
		{
			string dtd = "<!DOCTYPE root [<!ELEMENT root EMPTY><!ATTLIST root attr CDATA 'default'>]>";
			string xml = dtd + "<root/>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml (xml);

			doc.DocumentElement.Attributes ["attr"].Value = "modified";
			doc.DocumentElement.RemoveAttribute("attr");

			XmlAttribute defAttr = doc.DocumentElement.Attributes ["attr"];
			Assert.IsNotNull (defAttr);
			Assert.AreEqual ("default", defAttr.Value);

			defAttr.Value = "default"; // same value as default
			Assert.AreEqual (true, defAttr.Specified);
		}

		[Test]
		public void AddIdenticalAttrToTheSameNode ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"<blah><br><mynode txt='blah/drinks1'/></br><br><mynode txt='hello world'/></br></blah>");
			XmlAttribute a = doc.CreateAttribute ("MyAttribute");
			doc.SelectNodes ("//mynode") [0].Attributes.Append (a);
			doc.SelectNodes ("//mynode") [0].Attributes.Append (a);
		}

		[Test]
		public void AddIdentityDuplicate ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"
				<!DOCTYPE eticGlossList [<!ELEMENT item (item*)><!ATTLIST item id ID #IMPLIED>]>
				<group><item></item><item id=""tom""><f/></item></group>");
			XmlNodeList nodes = doc.SelectNodes ("group/item");
			int n = 1;
			foreach (XmlNode node in nodes) {
				XmlAttribute idAttr = doc.CreateAttribute ("id");

				idAttr.Value = "id";
				node.Attributes.Append (idAttr);
				Assert.AreEqual (1, node.Attributes.Count, "#" + n++);
			}
		}
	}
}
