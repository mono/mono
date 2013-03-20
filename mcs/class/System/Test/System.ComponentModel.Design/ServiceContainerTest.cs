// ServiceContainerTest.cs - NUnit Test Cases for System.ComponentModel.Design.ServiceContainer
//
// Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Alexandre Pigolkine (pigolkine@gmx.de)
// 

#if !MOBILE

using System;
using System.ComponentModel.Design;
using System.Collections;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel.Design
{
	class NotInSvc
	{
		public NotInSvc ()
		{
		}
	}
	
	class Svc
	{
		public Svc ()
		{
		}
		
		private static int objectsCreatedByCallback;
		
		public static int TotalObjectsCreatedByCallback {
			get { return objectsCreatedByCallback; }
		}

		public static void Reset ()
		{
			objectsCreatedByCallback = 0;
		}
		
		public static object ServiceCreator (IServiceContainer isc, Type tp) 
		{
			Assert.AreEqual (typeof (Svc), tp, "ServiceCreator#01");
			++objectsCreatedByCallback;
			return new Svc ();
		}
	}

	[TestFixture]
	public class ServiceContainerTest
	{
		[SetUp]
		public void SetUp ()
		{
			Svc.Reset ();
		}

		[Test]
		public void AddService1 ()
		{
			object service;
			ServiceContainer parent;
			ServiceContainer sc;

			object serviceInstance1 = new ArrayList ();
			object serviceInstance2 = new Hashtable ();
			object callback1 = new ServiceCreatorCallback (
				Svc.ServiceCreator);

			sc = new ServiceContainer ();
			sc.AddService (typeof (ICollection), serviceInstance1);
			sc.AddService (typeof (IEnumerable), serviceInstance2);
			sc.AddService (typeof (Svc), callback1);

			service = sc.GetService (typeof (ICollection));
			Assert.IsNotNull (service, "#A1");
			Assert.AreSame (serviceInstance1, service, "#A2");

			service = sc.GetService (typeof (IEnumerable));
			Assert.IsNotNull (service, "#B1");
			Assert.AreSame (serviceInstance2, service, "#B2");

			service = sc.GetService (typeof (ArrayList));
			Assert.IsNull (service, "#C1");

			service = sc.GetService (typeof (ICloneable));
			Assert.IsNull (service, "#D1");

			Assert.AreEqual (0, Svc.TotalObjectsCreatedByCallback, "#E1");
			service = sc.GetService (typeof (Svc));
			Assert.IsNotNull (service, "#E2");
			Assert.IsTrue (service is Svc, "#E3");
			Assert.AreEqual (1, Svc.TotalObjectsCreatedByCallback, "#E4");
			Assert.AreSame (service, sc.GetService (typeof (Svc)), "#E5");
			Assert.AreEqual (1, Svc.TotalObjectsCreatedByCallback, "#E6");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);

			sc.AddService (typeof (ICollection), serviceInstance1);

			Assert.AreSame (serviceInstance1, sc.GetService (typeof (ICollection)), "#F1");
			Assert.IsNull (parent.GetService (typeof (ICollection)), "#F2");
		}

#if NET_2_0
		[Test]
		public void AddService1_Disposed ()
		{
			object service;
			ServiceContainer sc;

			object serviceInstance1 = new ArrayList ();
			object serviceInstance2 = new Hashtable ();

			sc = new ServiceContainer ();
			sc.AddService (typeof (ICollection), serviceInstance1);
			service = sc.GetService (typeof (ICollection));
			Assert.IsNotNull (service, "#A1");
			Assert.AreSame (serviceInstance1, service, "#A2");

			sc.Dispose ();

			service = sc.GetService (typeof (ICollection));
			Assert.IsNull (service, "#B");

			sc.AddService (typeof (ICollection), serviceInstance2);
			service = sc.GetService (typeof (ICollection));
			Assert.IsNotNull (service, "#C1");
			Assert.AreSame (serviceInstance2, service, "#C2");
		}
#endif

		[Test] // AddService (Type, Object)
		public void AddService1_ServiceInstance_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.AddService (typeof (IList), (object) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("serviceInstance", ex.ParamName, "#5");
			}
		}

		[Test]
		public void AddService1_ServiceType_Exists ()
		{
			ServiceContainer sc = new ServiceContainer ();

			Svc serviceInstance1 = new Svc ();
			Svc serviceInstance2 = new Svc ();

			sc.AddService (typeof (Svc), serviceInstance1);
			Assert.AreSame (serviceInstance1, sc.GetService (typeof (Svc)), "#A");

			try {
				sc.AddService (typeof (Svc), serviceInstance1);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The service MonoTests.System.ComponentModel.Design.Svc
				// already exists in the service container
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Svc).FullName) != -1, "#B5");
				Assert.AreEqual ("serviceType", ex.ParamName, "#B6");
			}

			try {
				sc.AddService (typeof (Svc), serviceInstance2);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// The service MonoTests.System.ComponentModel.Design.Svc
				// already exists in the service container
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Svc).FullName) != -1, "#C5");
				Assert.AreEqual ("serviceType", ex.ParamName, "#C6");
			}
		}

		[Test] // AddService (Type, Object)
		public void AddService1_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.AddService ((Type) null, new ArrayList ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#5");
			}
		}

		[Test] // AddService (Type, ServiceCreatorCallback)
		public void AddService2_Callback_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();
			ServiceCreatorCallback callback = null;

			try {
				sc.AddService (typeof (IList), callback);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("callback", ex.ParamName, "#5");
			}
		}

#if NET_2_0
		[Test] // AddService (Type, ServiceCreatorCallback)
		public void AddService2_Disposed ()
		{
			object service;
			ServiceContainer sc;

			object callback = new ServiceCreatorCallback (
				Svc.ServiceCreator);

			sc = new ServiceContainer ();
			sc.AddService (typeof (Svc), callback);
			service = sc.GetService (typeof (Svc));
			Assert.IsNotNull (service, "#A");

			sc.Dispose ();

			service = sc.GetService (typeof (Svc));
			Assert.IsNull (service, "#B");

			sc.AddService (typeof (Svc), callback);
			service = sc.GetService (typeof (Svc));
			Assert.IsNotNull (service, "#C");
		}
#endif

		[Test] // AddService (Type, ServiceCreatorCallback)
		public void AddService2_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();
			ServiceCreatorCallback callback = new ServiceCreatorCallback (
				Svc.ServiceCreator);

			try {
				sc.AddService ((Type) null, callback);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#5");
			}
		}

		[Test] // AddService (Type, Object, Boolean)
		public void AddService3 ()
		{
			ServiceContainer sc;
			ServiceContainer parent = new ServiceContainer ();

			ArrayList serviceInstance1 = new ArrayList ();
			ArrayList serviceInstance2 = new ArrayList ();

			Type serviceType1 = typeof (IList);
			Type serviceType2 = typeof (IEnumerable);

			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#A1");
			Assert.IsNull (parent.GetService (serviceType2), "#A2");
			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#A3");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#A4");

			sc = new ServiceContainer ();
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#B1");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#B2");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType1, serviceInstance2, false);

			Assert.AreSame (serviceInstance2, sc.GetService (serviceType1), "#C1");
			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#C2");
		}

#if NET_2_0
		[Test] // AddService (Type, Object, Boolean)
		public void AddService3_Disposed ()
		{
			ServiceContainer sc;
			ServiceContainer parent = new ServiceContainer ();

			ArrayList serviceInstance1 = new ArrayList ();
			ArrayList serviceInstance2 = new ArrayList ();

			Type serviceType1 = typeof (IList);
			Type serviceType2 = typeof (IEnumerable);

			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			sc.Dispose ();

			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#A1");
			Assert.IsNull (parent.GetService (serviceType2), "#A2");
			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#A3");
			Assert.IsNull (sc.GetService (serviceType2), "#A4");

			sc.AddService (serviceType2, serviceInstance2, false);

			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#B1");
			Assert.IsNull (parent.GetService (serviceType2), "#B2");
			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#B3");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#B4");
		}
#endif

		[Test] // AddService (Type, Object, Boolean)
		public void AddService3_ServiceInstance_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.AddService (typeof (IList), (object) null, false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("serviceInstance", ex.ParamName, "#A5");
			}

			try {
				sc.AddService (typeof (IList), (object) null, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("serviceInstance", ex.ParamName, "#B5");
			}
		}

		[Test] // AddService (Type, Object, Boolean)
		public void AddService3_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.AddService ((Type) null, new ArrayList (), false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#A5");
			}

			try {
				sc.AddService ((Type) null, new ArrayList (), true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#B5");
			}
		}

		[Test] // AddService (Type, ServiceCreatorCallback, Boolean)
		public void AddService4_Callback_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();
			ServiceCreatorCallback callback = null;

			try {
				sc.AddService (typeof (IList), callback, false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("callback", ex.ParamName, "#A5");
			}

			try {
				sc.AddService (typeof (IList), callback, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("callback", ex.ParamName, "#B5");
			}
		}

		[Test] // AddService (Type, ServiceCreatorCallback, Boolean)
		public void AddService4_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();
			ServiceCreatorCallback callback = new ServiceCreatorCallback (
				Svc.ServiceCreator);

			try {
				sc.AddService ((Type) null, callback, false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#A5");
			}

			try {
				sc.AddService ((Type) null, callback, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void GeneralTest1 ()
		{
			ServiceContainer sc = new ServiceContainer ();

			sc.AddService (typeof (Svc), new Svc ());
			Svc service1 = sc.GetService (typeof (Svc)) as Svc;
			Assert.IsNotNull (service1, "#1");
			Assert.AreEqual (service1, sc.GetService (typeof (Svc)), "#2");
			Assert.IsNull (sc.GetService (typeof (NotInSvc)), "#3");
		}

		[Test]
		public void GeneralTest2 ()
		{
			ServiceContainer sc = new ServiceContainer ();

			sc.AddService (typeof (Svc), new Svc ());
			Svc service1 = sc.GetService (typeof (Svc)) as Svc;
			Assert.IsNotNull (service1, "#A");
			Assert.AreEqual (service1, sc.GetService (typeof (Svc)), "#2");

			try {
				sc.AddService (typeof (Svc), new Svc ());
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The service MonoTests.System.ComponentModel.Design.Svc
				// already exists in the service container
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (Svc).FullName) != -1, "#B5");
				Assert.AreEqual ("serviceType", ex.ParamName, "#B6");
			}
		}

		[Test]
		public void GetService_DefaultServices ()
		{
			ServiceContainer sc1 = new ServiceContainer ();

			Assert.AreSame (sc1, sc1.GetService (typeof (IServiceContainer)), "#A1");
#if NET_2_0
			Assert.AreSame (sc1, sc1.GetService (typeof(ServiceContainer)), "#A2");
#else
			Assert.IsNull (sc1.GetService (typeof (ServiceContainer)), "#A2");
#endif

			ServiceContainer sc2 = new ServiceContainer ();
			sc1.AddService (typeof (IServiceContainer), sc2);
			sc1.AddService (typeof (ServiceContainer), sc2);

			Assert.AreSame (sc1, sc1.GetService (typeof (IServiceContainer)), "#B1");
#if NET_2_0
			Assert.AreSame (sc1, sc1.GetService (typeof(ServiceContainer)), "#B2");
#else
			Assert.AreSame (sc2, sc1.GetService (typeof (ServiceContainer)), "#B2");
#endif
		}

		[Test]
		public void TestServiceCreator ()
		{
			ServiceContainer sc = new ServiceContainer ();
			sc.AddService (typeof(Svc), new ServiceCreatorCallback (Svc.ServiceCreator));
			Assert.IsNull (sc.GetService (typeof (NotInSvc)), "#A");

			Svc service1 = sc.GetService (typeof (Svc)) as Svc;
			Assert.IsNotNull (service1, "#B1");
			Assert.AreEqual (1, Svc.TotalObjectsCreatedByCallback, "#B2");

			Svc service2 = sc.GetService (typeof (Svc)) as Svc;
			Assert.AreEqual (service1, service2, "#C1");
			Assert.AreEqual (1, Svc.TotalObjectsCreatedByCallback, "#C2");
		}
	
		[Test]
		public void ParentService ()
		{
			ServiceContainer scParent = new ServiceContainer ();
			ServiceContainer sc = new ServiceContainer (scParent);

			scParent.AddService(typeof(Svc), new Svc ());

			Svc service1 = sc.GetService (typeof (Svc)) as Svc;
			Assert.IsNotNull (service1, "#1");
		}

		[Test] // RemoveService (Type)
		public void RemoveService1 ()
		{
			ServiceContainer sc;
			ServiceContainer parent;
			
			ArrayList serviceInstance1 = new ArrayList ();
			ArrayList serviceInstance2 = new ArrayList ();

			Type serviceType1 = typeof (IList);
			Type serviceType2 = typeof (IEnumerable);

			parent = null;
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1);
			sc.AddService (serviceType2, serviceInstance2);

			sc.RemoveService (typeof (DateTime));

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#A1");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#A2");

			sc.RemoveService (serviceType2);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#B1");
			Assert.IsNull (sc.GetService (serviceType2), "#B2");

			sc.RemoveService (serviceType1);

			Assert.IsNull (sc.GetService (serviceType1), "#C1");
			Assert.IsNull (sc.GetService (serviceType2), "#C2");

			sc.AddService (serviceType1, serviceInstance1);
			sc.AddService (serviceType2, serviceInstance2);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#D1");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#D2");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			sc.RemoveService (serviceType1);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#E1");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#E2");

			sc.RemoveService (serviceType2);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#F1");
			Assert.IsNull (sc.GetService (serviceType2), "#F2");

			parent.RemoveService (serviceType1);

			Assert.IsNull (sc.GetService (serviceType1), "#G1");
			Assert.IsNull (sc.GetService (serviceType2), "#G2");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType1, serviceInstance2, false);

			sc.RemoveService (serviceType1);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#H1");
			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#H2");

			sc.RemoveService (serviceType1);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#I1");
			Assert.AreSame (serviceInstance1, parent.GetService (serviceType1), "#I2");

			parent.RemoveService (serviceType1);

			Assert.IsNull (sc.GetService (serviceType1), "#J1");
			Assert.IsNull (parent.GetService (serviceType1), "#J2");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType1, serviceInstance2, false);

			parent.RemoveService (serviceType1);

			Assert.AreSame (serviceInstance2, sc.GetService (serviceType1), "#K1");
			Assert.IsNull (parent.GetService (serviceType1), "#K2");
		}

#if NET_2_0
		[Test] // RemoveService (Type)
		public void RemoveService1_Disposed ()
		{
			ServiceContainer sc;
			ServiceContainer parent;
			
			ArrayList serviceInstance1 = new ArrayList ();

			Type serviceType1 = typeof (IList);

			parent = null;
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1);
			sc.Dispose ();

			sc.RemoveService (typeof (DateTime));
		}
#endif

		[Test] // RemoveService (Type)
		public void RemoveService1_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.RemoveService ((Type) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#5");
			}
		}

		[Test] // RemoveService (Type, Boolean)
		public void RemoveService2 ()
		{
			ServiceContainer sc;
			ServiceContainer parent;

			ArrayList serviceInstance1 = new ArrayList ();
			ArrayList serviceInstance2 = new ArrayList ();

			Type serviceType1 = typeof (IList);
			Type serviceType2 = typeof (IEnumerable);

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			sc.RemoveService (serviceType1, false);
			sc.RemoveService (serviceType2, false);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#A1");
			Assert.IsNull (sc.GetService (serviceType2), "#A2");

			sc.RemoveService (serviceType1, true);
			sc.RemoveService (serviceType2, true);

			Assert.IsNull (sc.GetService (serviceType1), "#B1");
			Assert.IsNull (sc.GetService (serviceType2), "#B2");

			sc.AddService (serviceType1, serviceInstance1, true);
			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#C1");
			parent.RemoveService (serviceType1);
			Assert.IsNull (sc.GetService (serviceType1), "#C2");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			sc.RemoveService (serviceType1, true);
			sc.RemoveService (serviceType2, true);

			Assert.IsNull (sc.GetService (serviceType1), "#D1");
			Assert.AreSame (serviceInstance2, sc.GetService (serviceType2), "#D2");

			sc = new ServiceContainer ();
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.RemoveService (serviceType1, true);
			Assert.IsNull (sc.GetService (serviceType1), "#E");

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType1, serviceInstance2, false);

			sc.RemoveService (serviceType1, true);

			Assert.AreSame (serviceInstance2, sc.GetService (serviceType1), "#F1");
			Assert.IsNull (parent.GetService (serviceType1), "#F2");

			sc.RemoveService (serviceType1, true);

			Assert.AreSame (serviceInstance2, sc.GetService (serviceType1), "#G1");
			Assert.IsNull (parent.GetService (serviceType1), "#G2");

			sc.RemoveService (serviceType1, false);

			Assert.IsNull (sc.GetService (serviceType1), "#H1");
			Assert.IsNull (parent.GetService (serviceType1), "#H2");
		}

#if NET_2_0
		[Test] // RemoveService (Type, Boolean)
		public void RemoveService2_Disposed ()
		{
			ServiceContainer sc;
			ServiceContainer parent;

			ArrayList serviceInstance1 = new ArrayList ();
			ArrayList serviceInstance2 = new ArrayList ();

			Type serviceType1 = typeof (IList);
			Type serviceType2 = typeof (IEnumerable);

			parent = new ServiceContainer ();
			sc = new ServiceContainer (parent);
			sc.AddService (serviceType1, serviceInstance1, true);
			sc.AddService (serviceType2, serviceInstance2, false);

			sc.Dispose ();

			sc.RemoveService (serviceType1, false);
			sc.RemoveService (serviceType2, false);

			Assert.AreSame (serviceInstance1, sc.GetService (serviceType1), "#A1");
			Assert.IsNull (sc.GetService (serviceType2), "#A2");

			sc.RemoveService (serviceType1, true);
			sc.RemoveService (serviceType2, true);

			Assert.IsNull (sc.GetService (serviceType1), "#B1");
			Assert.IsNull (sc.GetService (serviceType2), "#B2");
		}
#endif

		[Test] // RemoveService (Type, Boolean)
		public void RemoveService2_ServiceType_Null ()
		{
			ServiceContainer sc = new ServiceContainer ();

			try {
				sc.RemoveService ((Type) null, false);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#A5");
			}

			try {
				sc.RemoveService ((Type) null, true);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("serviceType", ex.ParamName, "#B5");
			}
		}
	}
}

#endif