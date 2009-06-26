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
	public class WebServiceAttributeTest {

		[Test]
		public void TestConstructors ()
		{
			WebServiceAttribute attribute;

			attribute = new WebServiceAttribute ();
			Assert.AreEqual (String.Empty, attribute.Description);
			Assert.AreEqual (String.Empty, attribute.Name);
			Assert.AreEqual ("http://tempuri.org/", attribute.Namespace);
		}
	}
}
