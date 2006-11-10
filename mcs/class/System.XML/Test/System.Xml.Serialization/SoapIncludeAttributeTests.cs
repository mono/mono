//
// Tests for System.Xml.Serialization.SoapIncludeAttribute
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
	public class SoapIncludeAttributeTests
	{
		[Test]
		public void TypeDefault ()
		{
			SoapIncludeAttribute attr = new SoapIncludeAttribute (null);
			Assert.IsNull (attr.Type);
		}
	}
}
