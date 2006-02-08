//
// Tests for System.Xml.Serialization.XmlTypeAttribute
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
	public class XmlTypeAttributeTests
	{
		[Test]
		public void IncludeInSchemaDefault ()
		{
			XmlTypeAttribute attr = new XmlTypeAttribute ();
			Assert.AreEqual (true, attr.IncludeInSchema);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlTypeAttribute attr = new XmlTypeAttribute ();
			Assert.IsNull (attr.Namespace);
		}

		[Test]
		public void TypeNameDefault ()
		{
			XmlTypeAttribute attr = new XmlTypeAttribute ();
			Assert.AreEqual (string.Empty, attr.TypeName, "#1");

			attr.TypeName = null;
			Assert.AreEqual (string.Empty, attr.TypeName, "#2");
		}
	}
}
