//
// Tests for System.Xml.Serialization.XmlIncludeAttribute
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
	public class XmlIncludeAttributeTests
	{
		[Test]
		public void TypeDefault ()
		{
			XmlIncludeAttribute attr = new XmlIncludeAttribute (null);
			Assert.IsNull (attr.Type);
		}
	}
}
