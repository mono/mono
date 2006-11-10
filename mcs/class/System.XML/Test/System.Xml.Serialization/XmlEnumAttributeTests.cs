//
// Tests for System.Xml.Serialization.XmlEnumAttribute
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
	public class XmlEnumAttributeTests
	{
		[Test]
		public void NameDefault ()
		{
			XmlEnumAttribute attr = new XmlEnumAttribute ();
			Assert.IsNull (attr.Name);
		}
	}
}
