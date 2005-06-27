//
// FilterConfigurationTest.cs: FilterConfiguration Unit Tests
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Configuration;
using System.Xml;

using Microsoft.Web.Services.Configuration;
using Microsoft.Web.Services.Security;
using Microsoft.Web.Services.Timestamp;
using Microsoft.Web.Services.Referral;
using Microsoft.Web.Services.Routing;

using NUnit.Framework;

namespace MonoTests.MS.Web.Services.Configuration {

	[TestFixture]
	public class FilterConfigurationTest : Assertion {

		[Test]
		public void InputFilters () 
		{
			FilterConfiguration fc = WebServicesConfiguration.FilterConfiguration;
			AssertEquals ("InputFilters.Count", 4, fc.InputFilters.Count);
			// order is important (think so)
			Assert ("InputFilters.0.SecurityInputFilter", fc.InputFilters [0] is SecurityInputFilter);
			Assert ("InputFilters.1.TimestampInputFilter", fc.InputFilters [1] is TimestampInputFilter);
			Assert ("InputFilters.2.ReferralInputFilter", fc.InputFilters [2] is ReferralInputFilter);
			Assert ("InputFilters.3.RoutingInputFilter", fc.InputFilters [3] is RoutingInputFilter);
		}

		[Test]
		public void OutputFilters () 
		{
			FilterConfiguration fc = WebServicesConfiguration.FilterConfiguration;
			AssertEquals ("OutputFilters.Count", 4, fc.OutputFilters.Count);
			// order is important (think so)
			Assert ("InputFilters.0.SecurityOutputFilter", fc.OutputFilters [0] is SecurityOutputFilter);
			Assert ("InputFilters.1.TimestampOutputFilter", fc.OutputFilters [1] is TimestampOutputFilter);
			Assert ("InputFilters.2.ReferralOutputFilter", fc.OutputFilters [2] is ReferralOutputFilter);
			Assert ("InputFilters.3.RoutingOutputFilter", fc.OutputFilters [3] is RoutingOutputFilter);
		}
	}
}
