//
// Tests for System.Xml.Serialization.SoapAttributeAttribute
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
	public class SoapAttributeAttributeTests
	{
		[Test]
		public void AttributeNameDefault ()
		{
			SoapAttributeAttribute attr = new SoapAttributeAttribute ();
			Assert.AreEqual (string.Empty, attr.AttributeName, "#1");

			attr.AttributeName = null;
			Assert.AreEqual (string.Empty, attr.AttributeName, "#2");
		}

		[Test]
		public void DataTypeDefault ()
		{
			SoapAttributeAttribute attr = new SoapAttributeAttribute ();
			Assert.AreEqual (string.Empty, attr.DataType, "#1");

			attr.DataType = null;
			Assert.AreEqual (string.Empty, attr.DataType, "#2");
		}

		[Test]
		public void NamespaceDefault ()
		{
			SoapAttributeAttribute attr = new SoapAttributeAttribute ();
			Assert.IsNull (attr.Namespace);
		}
	}
}
