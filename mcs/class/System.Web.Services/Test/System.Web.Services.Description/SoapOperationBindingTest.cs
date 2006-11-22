//
// MonoTests.System.Web.Services.Description.SoapOperationBindingTest.cs
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
	public class SoapOperationBindingTest
	{
		SoapOperationBinding sob;

		[SetUp]
		public void InitializeSoapOperationBinding()
		{
			sob = new SoapOperationBinding();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assertion.AssertEquals(String.Empty, sob.SoapAction);
			Assertion.AssertEquals(SoapBindingStyle.Default, sob.Style);
			Assertion.AssertEquals(false, sob.Required);
			Assertion.AssertNull(sob.Parent);
			Assertion.AssertEquals(false, sob.Handled);
		}
	}
}
