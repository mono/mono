//
// MonoTests.System.Web.Services.WebServiceAttributeTest.cs
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

namespace MonoTests.System.Web.Services {

	[TestFixture]
	public class WebServiceAttributeTest : Assertion {

		[Test]
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
