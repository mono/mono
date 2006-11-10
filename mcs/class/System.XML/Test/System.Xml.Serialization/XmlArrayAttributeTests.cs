//
// Tests for System.Xml.Serialization.XmlArrayAttribute
//
// Author:
//   Gert Driesen
//
// (C) 2005 Novell
//

using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlArrayAttributeTests
	{
		[Test]
		public void ElementNameDefault ()
		{
			XmlArrayAttribute attr = new XmlArrayAttribute ();
			Assert.AreEqual (string.Empty, attr.ElementName, "#1");

			attr.ElementName = null;
			Assert.AreEqual (string.Empty, attr.ElementName, "#2");
		}

		[Test]
		public void FormDefault ()
		{
			XmlArrayAttribute attr = new XmlArrayAttribute ();
			Assert.AreEqual (XmlSchemaForm.None, attr.Form);
		}

		[Test]
		public void IsNullableDefault ()
		{
			XmlArrayAttribute attr = new XmlArrayAttribute ();
			Assert.AreEqual (false, attr.IsNullable);
		}

		[Test]
		public void NamespaceDefault ()
		{
			XmlArrayAttribute attr = new XmlArrayAttribute ();
			Assert.IsNull (attr.Namespace);
		}
	}
}
