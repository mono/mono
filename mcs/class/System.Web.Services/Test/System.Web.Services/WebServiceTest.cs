//
// MonoTests.System.Web.Services.WebServiceTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

using NUnit.Framework;
using System;
using System.Web.Services;
using System.EnterpriseServices;

namespace MonoTests.System.Web.Services 
{
	[TestFixture]
	public class WebServiceTest
	{
		[Test] // test for bug #331183(!)
		public void Constructor ()
		{
			new WebService ();
		}
	}
}
