// ServiceContainerTest.cs - NUnit Test Cases for System.ComponentModel.Design.ServiceContainer
//
// Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Alexandre Pigolkine (pigolkine@gmx.de)
// 

using NUnit.Framework;
using System;
using System.ComponentModel.Design;

namespace MonoTests.System.ComponentModel.Design
{

// Helper classes
	
	class NotInSvc 
	{
		public NotInSvc() 
		{
		}
	}
	
	class Svc 
	{
		public Svc () 
		{
		}
		
		private static int objectsCreatedByCallback = 0;
		
		public static int TotalObjectsCreatedByCallback {
			get { return objectsCreatedByCallback; }
		}
		
		public static object ServiceCreator (IServiceContainer isc, Type tp) 
		{
			Assertion.AssertEquals ("ServiceCreator#01", tp, typeof (Svc));
			++objectsCreatedByCallback;
			return new Svc();
		}
	}
	
[TestFixture]
public class ServiceContainerTest : Assertion {
	
	[Test]
	public void GeneralTest1 () 
	{
		ServiceContainer sc = new ServiceContainer ();
			
		sc.AddService (typeof (Svc), new Svc());
		Svc service1 = sc.GetService (typeof (Svc)) as Svc;
		AssertNotNull ("GT1#01", service1);
		AssertEquals ("GT1#02", service1, sc.GetService (typeof (Svc)));	
		AssertNull ("GT1#04", sc.GetService (typeof (NotInSvc)));
	}

	[Test, ExpectedException (typeof (ArgumentException))]
	public void GeneralTest2 () 
	{
		ServiceContainer sc = new ServiceContainer ();
			
		sc.AddService (typeof (Svc), new Svc());
		Svc service1 = sc.GetService (typeof (Svc)) as Svc;
		AssertNotNull ("GT1#01", service1);
		AssertEquals ("GT1#02", service1, sc.GetService (typeof (Svc)));
			
		sc.AddService (typeof (Svc), new Svc());
	}

	[Test]
	public void TestServiceCreator () 
	{
		ServiceContainer sc = new ServiceContainer ();
		sc.AddService(typeof(Svc), new ServiceCreatorCallback(Svc.ServiceCreator));
		AssertNull ("TSC#01", sc.GetService (typeof(NotInSvc)));
		
		Svc service1 = sc.GetService (typeof(Svc)) as Svc;
		AssertNotNull ("TSC#02", service1);
		AssertEquals ("TSC#03", Svc.TotalObjectsCreatedByCallback, 1);
		
		Svc service2 = sc.GetService (typeof(Svc)) as Svc;
		AssertEquals ("TSC#04", service2, service1);
		AssertEquals ("TSC#05", Svc.TotalObjectsCreatedByCallback, 1);
	}
	
	[Test]
	public void TestParentService () 
	{
		ServiceContainer scParent = new ServiceContainer();
		ServiceContainer sc = new ServiceContainer(scParent);
		
		scParent.AddService(typeof(Svc), new Svc());
			
		Svc service1 = sc.GetService (typeof(Svc)) as Svc;
		AssertNotNull ("TPS#01", service1);
		
	}

}
}
