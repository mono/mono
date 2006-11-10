//
// Tests for System.Xml.Serialization.SoapSchemaMember
//
// Author:
//   Gert Driesen
//
// (C) 2005 Novell
//

using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class SoapSchemaMemberTests
	{
		[Test]
		public void MemberNameDefault ()
		{
			SoapSchemaMember member = new SoapSchemaMember ();
			Assert.AreEqual (string.Empty, member.MemberName);

			member.MemberName = null;
			Assert.AreEqual (string.Empty, member.MemberName);
		}

		[Test]
		public void MemberTypeDefault ()
		{
			SoapSchemaMember member = new SoapSchemaMember ();
			Assert.AreEqual (XmlQualifiedName.Empty, member.MemberType);
		}
	}
}
