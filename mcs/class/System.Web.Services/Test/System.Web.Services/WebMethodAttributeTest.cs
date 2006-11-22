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
	public class WebMethodAttributeTest : Assertion {

		[Test]
		public void TestConstructors ()
		{
			WebMethodAttribute attribute;

			attribute = new WebMethodAttribute ();
			AssertEquals (true, attribute.BufferResponse);
			AssertEquals (0, attribute.CacheDuration);
			AssertEquals (String.Empty, attribute.Description);
			AssertEquals (false, attribute.EnableSession);
			AssertEquals (String.Empty, attribute.MessageName);
			AssertEquals (TransactionOption.Disabled, attribute.TransactionOption);
		}
	}
}
