//
// Tests for System.Xml.Serialization.SoapEnumAttribute
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
	public class SoapEnumAttributeTests
	{
		[Test]
		public void NameDefault ()
		{
			SoapEnumAttribute attr = new SoapEnumAttribute ();
			Assert.AreEqual (string.Empty, attr.Name, "#1");

			attr.Name = null;
			Assert.AreEqual (string.Empty, attr.Name, "#2");
		}
	}
}
