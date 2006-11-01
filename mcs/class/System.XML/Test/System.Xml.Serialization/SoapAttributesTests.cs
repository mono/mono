//
// System.Xml.XmlAttributesTests
//
// Author:
//   Gert Driesen
//
// (C) 2006 Novell
//

using System;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class SoapAttributesTests
	{
		[Test]
#if NET_2_0
		// in .NET 2.0, SoapDefaultValue should be null by default, but we need
		// more tests before making this change
		[Category ("NotDotNet")]
#endif
		public void Defaults ()
		{
			SoapAttributes atts = new SoapAttributes ();
			Assert.IsNull (atts.SoapAttribute, "#1");
			Assert.IsNotNull (atts.SoapDefaultValue, "#2");
			Assert.AreEqual (DBNull.Value, atts.SoapDefaultValue, "#3");
			Assert.IsNull (atts.SoapElement, "#4");
			Assert.IsNull (atts.SoapEnum, "#5");
			Assert.AreEqual (false, atts.SoapIgnore, "#6");
			Assert.IsNull (atts.SoapType, "#7");
		}

		[Test]
		public void SoapType ()
		{
			SoapAttributes atts = new SoapAttributes (typeof (TestDefault));
			Assert.IsNotNull (atts.SoapType, "#1");
			Assert.AreEqual ("testDefault", atts.SoapType.TypeName, "#2");
			Assert.AreEqual ("urn:myNS", atts.SoapType.Namespace, "#3");
		}

		[Test]
		public void SoapDefaultValue ()
		{
			SoapAttributes atts = new SoapAttributes (typeof (TestDefault).GetMember("strDefault")[0]);
			Assert.IsNotNull (atts.SoapDefaultValue, "#1");
			Assert.AreEqual ("Default Value", atts.SoapDefaultValue, "#2");
		}

		[Test]
		public void SoapEnum ()
		{
			SoapAttributes atts = new SoapAttributes (typeof (FlagEnum_Encoded).GetMember("e1")[0]);
			Assert.IsNotNull (atts.SoapEnum, "#1");
			Assert.AreEqual ("one", atts.SoapEnum.Name, "#2");
		}
	}
}
