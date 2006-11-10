//
// Tests for System.Xml.Serialization.XmlAnyElementAttribute
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
	public class XmlAnyElementAttributeTests
	{
		[Test]
		public void NameDefault ()
		{
			XmlAnyElementAttribute attr = new XmlAnyElementAttribute ();
			Assert.AreEqual (string.Empty, attr.Name, "#1");

			attr.Name = null;
			Assert.AreEqual (string.Empty, attr.Name, "#2");
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlAnyElementAttribute attr = new XmlAnyElementAttribute ();
			Assert.IsNull (attr.Namespace);
		}
	}
}
