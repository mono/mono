//
// Tests for System.Xml.Serialization.XmlTextAttribute
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
	public class XmlTextAttributeTests
	{
		[Test]
		public void DataTypeDefault ()
		{
			XmlTextAttribute attr = new XmlTextAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void TypeDefault ()
		{
			XmlTextAttribute attr = new XmlTextAttribute ();
			Assert.IsNull (attr.Type);
		}
	}
}
