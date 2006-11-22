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
			Assertion.AssertEquals(String.Empty, sbb.Encoding);
			Assertion.AssertNull(sbb.Parts);
			Assertion.AssertNull(sbb.PartsString);
			Assertion.AssertEquals(String.Empty, sbb.Namespace);
			Assertion.AssertEquals(SoapBindingUse.Default, sbb.Use);
		}
		
		[Test]
		public void TestAssignNullPartsString()
		{
			sbb.PartsString = null;
			Assertion.AssertNull(sbb.Parts);
			Assertion.AssertEquals(null, sbb.PartsString);
		}

		[Test]
		public void TestAssignEmptyPartsString()
		{
			sbb.PartsString = String.Empty;
			Assertion.AssertNotNull(sbb.Parts);
			Assertion.AssertEquals(1, sbb.Parts.Length);
			Assertion.AssertEquals(String.Empty, sbb.PartsString);
		}

		[Test]
		public void TestAssignSpacesToPartsString()
		{
			const string Spaces = " ";
			sbb.PartsString = Spaces;
			Assertion.AssertNotNull(sbb.Parts);
			Assertion.AssertEquals(2, sbb.Parts.Length);
			Assertion.AssertEquals(Spaces, sbb.PartsString);
		}

		[Test]
		public void TestAssignNullParts()
		{
			sbb.Parts = null;
			Assertion.AssertNull(sbb.Parts);
			Assertion.AssertNull(sbb.PartsString);
		}

		[Test]
		public void TestAssignValueParts()
		{
			string[] vals = {"a", "b"};
			sbb.Parts = vals;
			Assertion.AssertNotNull(sbb.Parts);
			Assertion.AssertEquals("a b", sbb.PartsString);
		}
	}
}
