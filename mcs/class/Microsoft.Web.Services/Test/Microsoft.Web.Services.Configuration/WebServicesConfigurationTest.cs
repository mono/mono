//
// WebServicesConfigurationTest.cs: WebServicesConfiguration Unit Tests
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

using Microsoft.Web.Services.Configuration;

using NUnit.Framework;

namespace MonoTests.MS.Web.Services.Configuration {

	[TestFixture]
	public class WebServicesConfigurationTest : Assertion {

		[Test]
		public void FilterConfiguration ()
		{
			FilterConfiguration fc = WebServicesConfiguration.FilterConfiguration;
			AssertNotNull ("FilterConfiguration", fc);
		}
	}
}
