//
// Tests for System.Xml.Serialization.XmlArrayItemAttribute
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
	public class XmlArrayItemAttributeTests
	{
		[Test]
		public void DataTypeDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void ElementNameDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.AreEqual (string.Empty, attr.ElementName, "#1");

			attr.ElementName = null;
			Assert.AreEqual (string.Empty, attr.ElementName, "#2");
		}

		[Test]
		public void FormDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.AreEqual (XmlSchemaForm.None, attr.Form);
		}

		[Test]
		public void IsNullableDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.AreEqual (false, attr.IsNullable);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.IsNull (attr.Namespace);
		}

		[Test]
		public void NestingLevelDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.AreEqual (0, attr.NestingLevel);
		}

		[Test]
		public void TypeDefault ()
		{
			XmlArrayItemAttribute attr = new XmlArrayItemAttribute ();
			Assert.IsNull (attr.Type);
		}
	}
}
