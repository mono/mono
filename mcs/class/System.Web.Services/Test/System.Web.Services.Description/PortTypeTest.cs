//
// MonoTests.System.Web.Services.Description.PortTypeTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class PortTypeTest
	{
		PortType portType;

		[SetUp]
		public void InitializePortType()
		{
			portType = new PortType();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assertion.AssertNull(portType.Name);
			Assertion.AssertNotNull(portType.Operations);
			Assertion.AssertEquals(0, portType.Operations.Count);
			Assertion.AssertNull(portType.ServiceDescription);
		}
		
		[Test]
		public void TestEmptyName()
		{
			portType.Name = String.Empty;
			Assertion.AssertEquals(String.Empty, portType.Name);
		}

		[Test]
		public void TestLongName()
		{
			const string LongName = "abcdefghijklmnopqrstuvwxyz";
			portType.Name = LongName;
			Assertion.AssertEquals(LongName, portType.Name);
		}
	}
}
