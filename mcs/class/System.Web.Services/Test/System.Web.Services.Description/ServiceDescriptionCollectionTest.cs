//
// MonoTests.System.Web.Services.Description.ServiceDescriptionCollectionTest.cs
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
	public class ServiceDescriptionCollectionTest
	{
		ServiceDescriptionCollection sdc;

		[SetUp]
		public void InitializeServiceDescriptionCollection ()
		{
			sdc = new ServiceDescriptionCollection ();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assertion.AssertNull (sdc["hello"]);
			Assertion.AssertEquals (0, sdc.Count);
		}
		
		[Test]
		public void TestAddServiceDescriptionWithoutTargetNS ()
		{
			const string serviceDescriptionNamespace = "testServiceDescription";
			
			ServiceDescription sd = new ServiceDescription ();	
			sdc.Add (sd);

			Assertion.AssertEquals (1, sdc.Count);
			Assertion.AssertNull (sdc[serviceDescriptionNamespace]);
		}

		[Test]
		public void TestAddServiceDescriptionWithTargetNS ()
		{
			const string serviceDescriptionNamespace = "http://some.urn";
			
			ServiceDescription sd = new ServiceDescription ();
			sd.TargetNamespace = serviceDescriptionNamespace;
			
			sdc.Add (sd);

			Assertion.AssertEquals (1, sdc.Count);
			Assertion.AssertEquals (sd, sdc[serviceDescriptionNamespace]);
		}
	}
}
