//
// WebServicesInteroperabilityTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.
//

#if NET_2_0

using NUnit.Framework;

using System;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class WebServicesInteroperabilityTest
	{
		[Test]
		public void ResolveImport () // should not result in an error
		{
			BasicProfileViolationCollection bc = new BasicProfileViolationCollection ();
			WebServicesInteroperability.CheckConformance (
				WsiProfiles.BasicProfile1_1,
				ServiceDescription.Read ("Test/System.Web.Services.Description/check-import.wsdl"), bc);
		}

		[Test]
		public void CheckR2305_1 () // bug #443095
		{
			BasicProfileViolationCollection bc = new BasicProfileViolationCollection ();
			WebServicesInteroperability.CheckConformance (
				WsiProfiles.BasicProfile1_1,
				ServiceDescription.Read ("Test/System.Web.Services.Description/443095.wsdl"), bc);
		}

		[Test]
		public void CheckEmptyOutput () // bug #6041
		{
			BasicProfileViolationCollection bc = new BasicProfileViolationCollection ();
			WebServicesInteroperability.CheckConformance (
				WsiProfiles.BasicProfile1_1,
				ServiceDescription.Read ("Test/System.Web.Services.Description/6041.wsdl"), bc);
		}

	}
}

#endif
