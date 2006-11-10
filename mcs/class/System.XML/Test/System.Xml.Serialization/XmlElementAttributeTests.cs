//
// Tests for System.Xml.Serialization.XmlElementAttribute
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
	public class XmlElementAttributeTests
	{
		[Test]
		public void DataTypeDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void ElementNameDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.AreEqual (string.Empty, attr.ElementName, "#1");

			attr.ElementName = null;
			Assert.AreEqual (string.Empty, attr.ElementName, "#2");
		}

		[Test]
		public void FormDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.AreEqual (XmlSchemaForm.None, attr.Form);
		}

		[Test]
		public void IsNullableDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.AreEqual (false, attr.IsNullable);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.IsNull (attr.Namespace);
		}

		[Test]
		public void TypeDefault ()
		{
			XmlElementAttribute attr = new XmlElementAttribute ();
			Assert.IsNull (attr.Type);
		}
	}
}
