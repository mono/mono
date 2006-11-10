//
// Tests for System.Xml.Serialization.XmlChoiceIdentifierAttribute
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
	public class XmlChoiceIdentifierAttributeTests
	{
		[Test]
		public void MemerNameDefault ()
		{
			XmlChoiceIdentifierAttribute attr = new XmlChoiceIdentifierAttribute ();
			Assert.AreEqual (string.Empty, attr.MemberName, "#1");

			attr.MemberName = null;
			Assert.AreEqual (string.Empty, attr.MemberName, "#2");
		}
	}
}
