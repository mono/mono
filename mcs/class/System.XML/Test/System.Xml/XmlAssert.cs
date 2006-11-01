//
// XPathEditableNavigatorTests.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//

using System;
using System.Xml;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlAssert
	{
		// copy from XmlTextReaderTests
		public static void AssertStartDocument (XmlReader xmlReader,
			string label)
		{
			Assert.AreEqual (ReadState.Initial, xmlReader.ReadState, label + ".ReadState");
			Assert.AreEqual (XmlNodeType.None, xmlReader.NodeType, label + ".NodeType");
			Assert.AreEqual (0, xmlReader.Depth, label + ".Depth");
			Assert.IsFalse (xmlReader.EOF, label + ".EOF");
		}

		public static void AssertNode (
			string label,
			XmlReader xmlReader,
			XmlNodeType nodeType,
			int depth,
			bool isEmptyElement,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value,
			bool hasValue,
			int attributeCount,
			bool hasAttributes)
		{
			label = String.Concat (label, "(", xmlReader.GetType ().Name, ")");
			Assert.AreEqual (nodeType, xmlReader.NodeType, label + ".NodeType");
			Assert.AreEqual (isEmptyElement, xmlReader.IsEmptyElement,
				label + ".IsEmptyElement");

			Assert.AreEqual (name, xmlReader.Name, label + ".Name");

			Assert.AreEqual (prefix, xmlReader.Prefix, label + ".Prefix");

			Assert.AreEqual (localName, xmlReader.LocalName, label + ".LocalName");

			Assert.AreEqual (namespaceURI, xmlReader.NamespaceURI, label + ".NamespaceURI");

			Assert.AreEqual (depth, xmlReader.Depth, label + ".Depth");

			Assert.AreEqual (hasValue, xmlReader.HasValue, label + ".HasValue");

			Assert.AreEqual (value, xmlReader.Value, label + ".Value");

			Assert.AreEqual (hasAttributes, xmlReader.HasAttributes,
				label + "HasAttributes");

			Assert.AreEqual (attributeCount, xmlReader.AttributeCount,
				label + ".AttributeCount");
		}

		public static void AssertAttribute (
			string label,
			XmlReader xmlReader,
			string name,
			string prefix,
			string localName,
			string namespaceURI,
			string value)
		{
			Assert.AreEqual (value, xmlReader [name], label + " [name]");

			Assert.AreEqual (value, xmlReader.GetAttribute (name),
				label + ".GetAttribute(name)");

			if (namespaceURI != String.Empty) {
				Assert.AreEqual (value, xmlReader [localName, namespaceURI], label + " [name]");
				Assert.AreEqual (value, xmlReader.GetAttribute (localName, namespaceURI), label + ".GetAttribute(localName,namespaceURI)");
			}
		}

		public static void AssertEndDocument (XmlReader xmlReader, string label)
		{
			Assert.IsFalse (!xmlReader.Read (), label + ".Read()");
			Assert.AreEqual (XmlNodeType.None, xmlReader.NodeType,
				label + ".NodeType is not XmlNodeType.None");
			Assert.AreEqual (0, xmlReader.Depth, label + ".Depth is not 0");
			Assert.AreEqual (ReadState.EndOfFile, xmlReader.ReadState,
				label + "ReadState is not ReadState.EndOfFile");
			Assert.IsTrue (xmlReader.EOF, label + ".EOF");

			xmlReader.Close ();
			Assert.AreEqual (ReadState.Closed, xmlReader.ReadState,
				label + ".ReadState is not ReadState.Cosed");
		}
	}
}
