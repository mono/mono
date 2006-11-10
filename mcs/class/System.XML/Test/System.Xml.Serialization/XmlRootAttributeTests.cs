//
// Tests for System.Xml.Serialization.XmlRootAttribute
//
// Author:
//   Gert Driesen
//
// (C) 2005 Novell
//

using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlRootAttributeTests
	{
		[Test]
		public void DataTypeDefault ()
		{
			XmlRootAttribute attr = new XmlRootAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void ElementNameDefault ()
		{
			XmlRootAttribute attr = new XmlRootAttribute ();
			Assert.AreEqual (string.Empty, attr.ElementName, "#1");

			attr.ElementName = null;
			Assert.AreEqual (string.Empty, attr.ElementName, "#2");
		}

		[Test]
		public void IsNullableDefault ()
		{
			XmlRootAttribute attr = new XmlRootAttribute ();
			Assert.AreEqual (true, attr.IsNullable);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlRootAttribute attr = new XmlRootAttribute ();
			Assert.IsNull (attr.Namespace);
		}
	}
}
