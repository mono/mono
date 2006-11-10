//
// Tests for System.Xml.Serialization.SoapTypeAttribute
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
	public class SoapTypeAttributeTests
	{
		[Test]
		public void IncludeInSchemaDefault ()
		{
			SoapTypeAttribute attr = new SoapTypeAttribute ();
			Assert.AreEqual (true, attr.IncludeInSchema);
		}

		[Test]
		public void NamespaceDefault ()
		{
			SoapTypeAttribute attr = new SoapTypeAttribute ();
			Assert.IsNull (attr.Namespace);
		}

		[Test]
		public void TypeNameDefault ()
		{
			SoapTypeAttribute attr = new SoapTypeAttribute ();
			Assert.AreEqual (string.Empty, attr.TypeName, "#1");

			attr.TypeName = null;
			Assert.AreEqual (string.Empty, attr.TypeName, "#2");
		}
	}
}
