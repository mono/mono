//
// MonoTests.System.Web.Services.WebServiceAttributeTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using NUnit.Framework;
using System;
using System.Web.Services;

namespace MonoTests.System.Web.Services {

	public class WebServiceAttributeTest : TestCase {

		public WebServiceAttributeTest () :
			base ("[MonoTests.System.Web.Services.WebServiceAttributeTest]") 
		{
		}

		public WebServiceAttributeTest (string name) :
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
			get { return new TestSuite (typeof (WebServiceAttributeTest)); }
		}

		public void TestConstructors ()
		{
			WebServiceAttribute attribute;

			attribute = new WebServiceAttribute ();
			AssertEquals (String.Empty, attribute.Description);
			AssertEquals (String.Empty, attribute.Name);
			AssertEquals ("http://tempuri.org/", attribute.Namespace);
		}
	}
}
