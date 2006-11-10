//
// Tests for System.Xml.Serialization.SoapElementAttribute
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
	public class SoapElementAttributeTests
	{
		[Test]
		public void DataTypeDefault ()
		{
			SoapElementAttribute attr = new SoapElementAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void ElementNameDefault ()
		{
			SoapElementAttribute attr = new SoapElementAttribute ();
			Assert.AreEqual (string.Empty, attr.ElementName, "#1");

			attr.ElementName = null;
			Assert.AreEqual (string.Empty, attr.ElementName, "#2");
		}

		[Test]
		public void IsNullableDefault ()
		{
			SoapElementAttribute attr = new SoapElementAttribute ();
			Assert.AreEqual (false, attr.IsNullable);
		}
	}
}
