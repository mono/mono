// XmlAttributeCollectionTests.cs : Tests for the XmlAttributeCollection class
//
// Author: Matt Hunter <xrkune@tconl.com>
//
// <c> 2002 Matt Hunter

using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlAttributeCollectionTests : TestCase
	{
		public XmlAttributeCollectionTests() : base("MonoTests.System.Xml.XmlAttributeCollectionTests testsuite") { }
		public XmlAttributeCollectionTests(string name) : base(name) { }

		private XmlDocument document;

		protected override void SetUp()
		{
			document = new XmlDocument ();
		}
		public void TestRemoveAll ()
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
			AssertEquals ("not all attributes removed.", false, xmlElement.HasAttribute ("type"));
		}

		public void TestAppend () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			XmlElement xmlEl = xmlDoc.CreateElement ("TestElement");
			XmlAttribute xmlAttribute = xmlEl.SetAttributeNode ("attr1", "namespace1");
			XmlNode xmlNode = xmlDoc.CreateNode (XmlNodeType.Attribute, "attr3", "namespace1");
			XmlAttribute xmlAttribute3 = xmlNode as XmlAttribute;
			XmlAttributeCollection attributeCol = xmlEl.Attributes;
			xmlAttribute3 = attributeCol.Append (xmlAttribute3);
			AssertEquals ("attribute name not properly created.", true, xmlAttribute3.Name.Equals ("attr3"));
			AssertEquals ("attribute namespace not properly created.", true, xmlAttribute3.NamespaceURI.Equals ("namespace1"));
		}

		public void TestCopyTo () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root a1='garnet' a2='amethyst' a3='Bloodstone' a4='diamond' a5='emerald' a6='pearl' a7='ruby' a8='sapphire' a9='moonstone' a10='opal' a11='topaz' a12='turquoize' />");
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;
			XmlAttribute[] array = new XmlAttribute[24];
			col.CopyTo(array, 0);
			AssertEquals("garnet", array[0].Value);
			AssertEquals("moonstone", array[8].Value);
			AssertEquals("turquoize", array[11].Value);
			col.CopyTo(array, 12);
			AssertEquals("garnet", array[12].Value);
			AssertEquals("moonstone", array[20].Value);
			AssertEquals("turquoize", array[23].Value);
		}

		public void TestSetNamedItem ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;

			XmlAttribute attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "bloodstone";
			col.SetNamedItem(attr);
			AssertEquals("SetNamedItem.Normal", "bloodstone", el.GetAttribute("b3"));

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "aquamaline";
			col.SetNamedItem(attr);
			AssertEquals("SetNamedItem.Override", "aquamaline", el.GetAttribute("b3"));
			AssertEquals("SetNamedItem.Override.Count.1", 1, el.Attributes.Count);
			AssertEquals("SetNamedItem.Override.Count.2", 1, col.Count);
		}

		public void TestInsertBeforeAfterPrepend () 
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root b2='amethyst' />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = xmlDoc.DocumentElement.Attributes;
			XmlAttribute attr = xmlDoc.CreateAttribute("b1");
			attr.Value = "garnet";
			col.InsertAfter(attr, null);
			AssertEquals("InsertAfterNull", "garnet", el.GetAttributeNode("b1").Value);
			AssertEquals("InsertAfterNull.Pos", el.GetAttribute("b1"), col[0].Value);

			attr = xmlDoc.CreateAttribute("b3");
			attr.Value = "bloodstone";
			col.InsertAfter(attr, el.GetAttributeNode("b2"));
			AssertEquals("InsertAfterAttr", "bloodstone", el.GetAttributeNode("b3").Value);
			AssertEquals("InsertAfterAttr.Pos", el.GetAttribute("b3"), col[2].Value);

			attr = xmlDoc.CreateAttribute("b4");
			attr.Value = "diamond";
			col.InsertBefore(attr, null);
			AssertEquals("InsertBeforeNull", "diamond", el.GetAttributeNode("b4").Value);
			AssertEquals("InsertBeforeNull.Pos", el.GetAttribute("b4"), col[3].Value);

			attr = xmlDoc.CreateAttribute("warning");
			attr.Value = "mixed modern and traditional;-)";
			col.InsertBefore(attr, el.GetAttributeNode("b1"));
			AssertEquals("InsertBeforeAttr", "mixed modern and traditional;-)", el.GetAttributeNode("warning").Value);
			AssertEquals("InsertBeforeAttr.Pos", el.GetAttributeNode("warning").Value, col[0].Value);

			attr = xmlDoc.CreateAttribute("about");
			attr.Value = "lists of birthstone.";
			col.Prepend(attr);
			AssertEquals("Prepend", "lists of birthstone.", col[0].Value);
		}

		public void TestRemove ()
		{
			XmlDocument xmlDoc = new XmlDocument ();
			xmlDoc.LoadXml("<root a1='garnet' a2='amethyst' a3='bloodstone' a4='diamond' a5='emerald' a6='pearl' a7='ruby' a8='sapphire' a9='moonstone' a10='opal' a11='topaz' a12='turquoize' />");
			XmlElement el = xmlDoc.DocumentElement;
			XmlAttributeCollection col = el.Attributes;

			// Remove
			XmlAttribute attr = col.Remove(el.GetAttributeNode("a12"));
			AssertEquals("Remove", 11, col.Count);
			AssertEquals("Remove.Removed", "a12", attr.Name);

			// RemoveAt
			attr = col.RemoveAt(5);
			AssertEquals("RemoveAt", null, el.GetAttributeNode("a6"));
			AssertEquals("Remove.Removed", "pearl", attr.Value);
		}
	}
}
