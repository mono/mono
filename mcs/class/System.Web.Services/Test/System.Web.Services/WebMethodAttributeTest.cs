//
// MonoTests.System.Web.Services.WebMethodAttributeTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using NUnit.Framework;
using System;
using System.Web.Services;
using System.EnterpriseServices;

namespace MonoTests.System.Web.Services {

	public class WebMethodAttributeTest : TestCase {

		public WebMethodAttributeTest () :
			base ("[MonoTests.System.Web.Services.WebMethodAttributeTest]") 
		{
		}

		public WebMethodAttributeTest (string name) :
			base (name) 
		{
		}

		protected override void SetUp ()
		{
		}

		protected override void TearDown ()
		{
		}

		public static ITest Suite {
			get { return new TestSuite (typeof (WebMethodAttributeTest)); }
		}

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
