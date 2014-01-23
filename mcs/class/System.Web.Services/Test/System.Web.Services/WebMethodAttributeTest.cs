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
#if !MOBILE
using System.EnterpriseServices;
#endif

namespace MonoTests.System.Web.Services {

	[TestFixture]
	public class WebMethodAttributeTest {

		[Test]
		public void TestConstructors ()
		{
			WebMethodAttribute attribute;

			attribute = new WebMethodAttribute ();
			Assert.AreEqual (true, attribute.BufferResponse);
			Assert.AreEqual (0, attribute.CacheDuration);
			Assert.AreEqual (String.Empty, attribute.Description);
			Assert.AreEqual (false, attribute.EnableSession);
			Assert.AreEqual (String.Empty, attribute.MessageName);
			Assert.AreEqual (TransactionOption.Disabled, attribute.TransactionOption);
		}
	}
}
