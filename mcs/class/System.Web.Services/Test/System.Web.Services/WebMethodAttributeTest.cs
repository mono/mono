//
// MonoTests.System.Web.Services.WebMethodAttributeTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Dave Bettin (dave@opendotnet.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Dave Bettin, 2003
//

using NUnit.Framework;
using System;
using System.Web.Services;
using System.EnterpriseServices;

namespace MonoTests.System.Web.Services {

	[TestFixture]
	public class WebMethodAttributeTest {

		[Test]
		public void TestConstructors ()
		{
			WebMethodAttribute attribute;

			attribute = new WebMethodAttribute ();
			Assertion.AssertEquals (true, attribute.BufferResponse);
			Assertion.AssertEquals (0, attribute.CacheDuration);
			Assertion.AssertEquals (String.Empty, attribute.Description);
			Assertion.AssertEquals (false, attribute.EnableSession);
			Assertion.AssertEquals (String.Empty, attribute.MessageName);
			Assertion.AssertEquals (TransactionOption.Disabled, attribute.TransactionOption);
		}
	}
}
