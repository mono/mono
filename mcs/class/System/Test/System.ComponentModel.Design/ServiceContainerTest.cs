// ServiceContainer.cs - NUnit Test Cases for System.ComponentModel.Design.ServiceContainer
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
	
	[SetUp]
	public void GetReady() {}

	[TearDown]
	public void Clean() {}

	[Test]
	public void GeneralTest1 () 
	{
		ServiceContainer sc = new ServiceContainer ();
			
		sc.AddService (typeof (Svc), new Svc());
		Svc service1 = sc.GetService (typeof (Svc)) as Svc;
		Assertion.AssertNotNull ("GT1#01", service1);
		Assertion.AssertEquals ("GT1#02", service1, sc.GetService (typeof (Svc)));
			
		bool exceptionThrown = false;
		try {
			sc.AddService (typeof (Svc), new Svc());
		}
		catch (ArgumentException ex){
			exceptionThrown = true;
		}
		Assertion.AssertEquals ("GT1#03", exceptionThrown, true);
			
		Assertion.AssertNull ("GT1#04", sc.GetService (typeof (NotInSvc)));
	}

	[Test]
	public void TestServiceCreator () 
	{
		ServiceContainer sc = new ServiceContainer ();
		sc.AddService(typeof(Svc), new ServiceCreatorCallback(Svc.ServiceCreator));
		Assertion.AssertNull ("TSC#01", sc.GetService (typeof(NotInSvc)));
		
		Svc service1 = sc.GetService (typeof(Svc)) as Svc;
		Assertion.AssertNotNull ("TSC#02", service1);
		Assertion.AssertEquals ("TSC#03", Svc.TotalObjectsCreatedByCallback, 1);
		
		Svc service2 = sc.GetService (typeof(Svc)) as Svc;
		Assertion.AssertEquals ("TSC#04", service2, service1);
		Assertion.AssertEquals ("TSC#05", Svc.TotalObjectsCreatedByCallback, 1);
	}
	
	[Test]
	public void TestParentService () 
	{
		ServiceContainer scParent = new ServiceContainer();
		ServiceContainer sc = new ServiceContainer(scParent);
		
		scParent.AddService(typeof(Svc), new Svc());
			
		Svc service1 = sc.GetService (typeof(Svc)) as Svc;
		Assertion.AssertNotNull ("TPS#01", service1);
		
	}

}
}
