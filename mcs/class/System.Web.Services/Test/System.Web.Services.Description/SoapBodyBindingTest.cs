//
// MonoTests.System.Web.Services.Description.SoapBodyBindingTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class SoapBodyBindingTest
	{
		SoapBodyBinding sbb;

		[SetUp]
		public void InitializeSoapBodyBinding()
		{
			sbb = new SoapBodyBinding();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.AreEqual (String.Empty, sbb.Encoding);
			Assert.IsNull (sbb.Parts);
			Assert.IsNull (sbb.PartsString);
			Assert.AreEqual (String.Empty, sbb.Namespace);
			Assert.AreEqual (SoapBindingUse.Default, sbb.Use);
		}
		
		[Test]
		public void TestAssignNullPartsString()
		{
			sbb.PartsString = null;
			Assert.IsNull (sbb.Parts);
			Assert.AreEqual (null, sbb.PartsString);
		}

		[Test]
		public void TestAssignEmptyPartsString()
		{
			sbb.PartsString = String.Empty;
			Assert.IsNotNull (sbb.Parts);
			Assert.AreEqual (1, sbb.Parts.Length);
			Assert.AreEqual (String.Empty, sbb.PartsString);
		}

		[Test]
		public void TestAssignSpacesToPartsString()
		{
			const string Spaces = " ";
			sbb.PartsString = Spaces;
			Assert.IsNotNull (sbb.Parts);
			Assert.AreEqual (2, sbb.Parts.Length);
			Assert.AreEqual (Spaces, sbb.PartsString);
		}

		[Test]
		public void TestAssignNullParts()
		{
			sbb.Parts = null;
			Assert.IsNull (sbb.Parts);
			Assert.IsNull (sbb.PartsString);
		}

		[Test]
		public void TestAssignValueParts()
		{
			string[] vals = {"a", "b"};
			sbb.Parts = vals;
			Assert.IsNotNull (sbb.Parts);
			Assert.AreEqual ("a b", sbb.PartsString);
		}
	}
}
