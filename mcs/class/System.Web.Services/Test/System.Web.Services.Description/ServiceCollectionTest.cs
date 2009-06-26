//
// MonoTests.System.Web.Services.Description.ServiceCollectionTest.cs
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
	public class ServiceCollectionTest
	{
		ServiceCollection sc;

		[SetUp]
		public void InitializeServiceCollection ()
		{
			// workaround for internal constructor
			ServiceDescription desc = new ServiceDescription ();
			sc = desc.Services;
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.IsNull (sc["hello"]);
			Assert.AreEqual (0, sc.Count);
		}
		
		[Test]
		public void TestAddService ()
		{
			const string serviceName = "testService";
			
			Service s = new Service ();
			s.Name = serviceName;
			
			sc.Add (s);

			Assert.AreEqual (1, sc.Count);
			Assert.AreEqual (s, sc[serviceName]);
		}
	}
}
