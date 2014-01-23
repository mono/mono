//
// MonoTests.System.Web.Services.WebServiceTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

#if !MOBILE
using NUnit.Framework;
using System;
using System.Web.Services;

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

#endif