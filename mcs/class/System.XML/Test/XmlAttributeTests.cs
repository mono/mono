// XmlAttributeTests.cs : Tests for the XmlAttribute class
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlAttributeTests : TestCase
	{
		public XmlAttributeTests() : base("Ximian.Mono.Tests.XmlAttributeTests testsuite") { }
		public XmlAttributeTests(string name) : base(name) { }

		XmlDocument doc;
		XmlAttribute attr;

		protected override void SetUp()
		{
			doc = new XmlDocument();
			attr = doc.CreateAttribute("attr1");
			attr.Value = "val1";
		}

		public void TestAttributes()
		{
			AssertNull(attr.Attributes);
		}

		public void TestHasChildNodes()
		{
			Assert("Child nodes not allowed", attr.HasChildNodes);
		}

		public void TestName()
		{
			AssertEquals("attr1", attr.Name);
		}

		public void TestNodeType()
		{
			AssertEquals(XmlNodeType.Attribute, attr.NodeType);
		}

		public void TestOwnerDocument()
		{
			AssertSame(doc, attr.OwnerDocument);
		}

		public void TestParentNode()
		{
			AssertNull("Attr parents not allowed", attr.ParentNode);
		}

		public void TestValue()
		{
			AssertEquals("val1", attr.Value);
		}
	}
}
