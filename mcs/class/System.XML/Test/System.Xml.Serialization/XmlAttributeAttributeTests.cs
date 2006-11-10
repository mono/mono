//
// Tests for System.Xml.Serialization.XmlAttributeAttribute
//
// Author:
//   Gert Driesen
//
// (C) 2005 Novell
//

using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlAttributeAttributeTests
	{
		[Test]
		public void AttributeNameDefault ()
		{
			XmlAttributeAttribute attr = new XmlAttributeAttribute ();
			Assert.AreEqual (string.Empty, attr.AttributeName, "#1");

			attr.AttributeName = null;
			Assert.AreEqual (string.Empty, attr.AttributeName, "#2");
		}

		[Test]
		public void DataTypeDefault ()
		{
			XmlAttributeAttribute attr = new XmlAttributeAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void FormDefault ()
		{
			XmlAttributeAttribute attr = new XmlAttributeAttribute ();
			Assert.AreEqual (XmlSchemaForm.None, attr.Form);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlAttributeAttribute attr = new XmlAttributeAttribute ();
			Assert.IsNull (attr.Namespace);
		}

		[Test]
		public void TypeDefault ()
		{
			XmlAttributeAttribute attr = new XmlAttributeAttribute ();
			Assert.IsNull (attr.Type);
		}
	}
}
