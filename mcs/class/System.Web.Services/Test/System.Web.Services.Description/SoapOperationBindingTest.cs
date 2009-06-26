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
			Assert.AreEqual (String.Empty, sob.SoapAction);
			Assert.AreEqual (SoapBindingStyle.Default, sob.Style);
			Assert.AreEqual (false, sob.Required);
			Assert.IsNull (sob.Parent);
			Assert.AreEqual (false, sob.Handled);
		}
	}
}
