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
			Assertion.AssertEquals ("not all attributes removed.", false, xmlElement.HasAttribute ("type"));
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
			Assertion.AssertEquals ("attribute name not properly created.", true, xmlAttribute3.Name.Equals ("attr3"));
			Assertion.AssertEquals ("attribute namespace not properly created.", true, xmlAttribute3.NamespaceURI.Equals ("namespace1"));
		}

		[Test]
		public void CopyTo () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root a1='garnet' a2='amethyst' a3='Bloodstone' a4='diamond' a5='emerald' a6='pearl' a7='ruby' a8='sapphire' a9='moonstone' a10='opal' a11='topaz' a12='turquoize' />");
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;
			XmlAttribute[] array = new XmlAttribute[24];
			col.CopyTo(array, 0);
			Assertion.AssertEquals("garnet", array[0].Value);
			Assertion.AssertEquals("moonstone", array[8].Value);
			Assertion.AssertEquals("turquoize", array[11].Value);
			col.CopyTo(array, 12);
			Assertion.AssertEquals("garnet", array[12].Value);
			Assertion.AssertEquals("moonstone", array[20].Value);
			Assertion.AssertEquals("turquoize", array[23].Value);
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
			Assertion.AssertEquals("SetNamedItem.Normal", "bloodstone", el.GetAttribute("b3"));

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "aquamaline";
			col.SetNamedItem(attr);
			Assertion.AssertEquals("SetNamedItem.Override", "aquamaline", el.GetAttribute("b3"));
			Assertion.AssertEquals("SetNamedItem.Override.Count.1", 1, el.Attributes.Count);
			Assertion.AssertEquals("SetNamedItem.Override.Count.2", 1, col.Count);
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
			Assertion.AssertEquals("InsertAfterNull", "garnet", el.GetAttributeNode("b1").Value);
			Assertion.AssertEquals("InsertAfterNull.Pos", el.GetAttribute("b1"), col[0].Value);

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "bloodstone";
			col.InsertAfter(attr, el.GetAttributeNode("b2"));
			Assertion.AssertEquals("InsertAfterAttr", "bloodstone", el.GetAttributeNode("b3").Value);
			Assertion.AssertEquals("InsertAfterAttr.Pos", el.GetAttribute("b3"), col[2].Value);

			attr = xmlDoc.CreateAttribute("b4");
			attr.Value = "diamond";
			col.InsertBefore(attr, null);
			Assertion.AssertEquals("InsertBeforeNull", "diamond", el.GetAttributeNode("b4").Value);
			Assertion.AssertEquals("InsertBeforeNull.Pos", el.GetAttribute("b4"), col[3].Value);

			attr = xmlDoc.CreateAttribute("warning");
			attr.Value = "mixed modern and traditional;-)";
			col.InsertBefore(attr, el.GetAttributeNode("b1"));
			Assertion.AssertEquals("InsertBeforeAttr", "mixed modern and traditional;-)", el.GetAttributeNode("warning").Value);
			Assertion.AssertEquals("InsertBeforeAttr.Pos", el.GetAttributeNode("warning").Value, col[0].Value);

			attr = xmlDoc.CreateAttribute("about");
			attr.Value = "lists of birthstone.";
			col.Prepend(attr);
			Assertion.AssertEquals("Prepend", "lists of birthstone.", col[0].Value);
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
			Assertion.AssertEquals("Remove", 11, col.Count);
			Assertion.AssertEquals("Remove.Removed", "a12", attr.Name);

			// RemoveAt
			attr = col.RemoveAt(5);
			Assertion.AssertEquals("RemoveAt", null, el.GetAttributeNode("a6"));
			Assertion.AssertEquals("Remove.Removed", "pearl", attr.Value);
		}
	}
}
