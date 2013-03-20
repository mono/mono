//
// System.ComponentModel.Container test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Ivan N. Zlatev (contact i-nZ.net)

// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2006 Ivan N. Zlatev
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace MonoTests.System.ComponentModel
{
	class TestService {
	}
	
	class TestContainer : Container {
		ServiceContainer _services = new ServiceContainer ();
		bool allowDuplicateNames;
		
		public TestContainer()
		{
			_services.AddService (typeof (TestService), new TestService ());
		}

		public bool AllowDuplicateNames {
			get { return allowDuplicateNames; }
			set { allowDuplicateNames = value; }
		}

		protected override object GetService (Type serviceType)
		{
			return _services.GetService (serviceType);
		}

#if NET_2_0
		public new void RemoveWithoutUnsiting (IComponent component)
		{
			base.RemoveWithoutUnsiting (component);
		}

		public void InvokeValidateName (IComponent component, string name)
		{
			ValidateName (component, name);
		}

		protected override void ValidateName (IComponent component, string name)
		{
			if (AllowDuplicateNames)
				return;
			base.ValidateName (component, name);
		}
#endif

		public bool Contains (IComponent component)
		{
			bool found = false;
			
			foreach (IComponent c in Components) {
				if (component.Equals (c)) {
					found = true;
					break;
				}
			}
			return found;
		}

		public new void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
	
	class TestComponent : Component {
		public override ISite Site {
			get {
				return base.Site;
			}
			set {
				base.Site = value;
				if (value != null) {
					Assert.IsNotNull (value.GetService (typeof (ISite)), "ISite");
					Assert.IsNotNull (value.GetService (typeof (TestService)), "TestService");
				}
			}
		}

		public bool IsDisposed {
			get { return disposed; }
		}

		public bool ThrowOnDispose {
			get { return throwOnDispose; }
			set { throwOnDispose = value; }
		}

		protected override void Dispose (bool disposing)
		{
			if (ThrowOnDispose)
				throw new InvalidOperationException ();

			base.Dispose (disposing);
			disposed = true;
		}

		private bool disposed;
		private bool throwOnDispose;
	}

	[TestFixture]
	public class ContainerTest
	{
		private TestContainer _container;

		[SetUp]
		public void Init ()
		{
			_container = new TestContainer ();
		}

		[Test] // Add (IComponent)
		public void Add1 ()
		{
			TestContainer containerA = new TestContainer ();
			TestContainer containerB = new TestContainer ();

			ISite siteA;
			ISite siteB;

			TestComponent compA = new TestComponent ();
			Assert.IsNull (compA.Site);
			TestComponent compB = new TestComponent ();
			Assert.IsNull (compB.Site);
			Assert.AreEqual (0, containerA.Components.Count);
			Assert.AreEqual (0, containerB.Components.Count);

			containerA.Add (compA);
			siteA = compA.Site;
			Assert.IsNotNull (siteA);
			Assert.AreSame (compA, siteA.Component);
			Assert.AreSame (containerA, siteA.Container);
			Assert.IsFalse (siteA.DesignMode);
			Assert.IsNull (siteA.Name);
			containerA.Add (compB);
			siteB = compB.Site;
			Assert.IsNotNull (siteB);
			Assert.AreSame (compB, siteB.Component);
			Assert.AreSame (containerA, siteB.Container);
			Assert.IsFalse (siteB.DesignMode);
			Assert.IsNull (siteB.Name);

			Assert.IsFalse (object.ReferenceEquals (siteA, siteB));
			Assert.AreEqual (2, containerA.Components.Count);
			Assert.AreEqual (0, containerB.Components.Count);
			Assert.AreSame (compA, containerA.Components [0]);
			Assert.AreSame (compB, containerA.Components [1]);

			// check effect of adding component that is already member of
			// another container
			containerB.Add (compA);
			Assert.IsFalse (object.ReferenceEquals (siteA, compA.Site));
			siteA = compA.Site;
			Assert.IsNotNull (siteA);
			Assert.AreSame (compA, siteA.Component);
			Assert.AreSame (containerB, siteA.Container);
			Assert.IsFalse (siteA.DesignMode);
			Assert.IsNull (siteA.Name);

			Assert.AreEqual (1, containerA.Components.Count);
			Assert.AreEqual (1, containerB.Components.Count);
			Assert.AreSame (compB, containerA.Components [0]);
			Assert.AreSame (compA, containerB.Components [0]);

			// check effect of add component twice to same container
			containerB.Add (compA);
			Assert.AreSame (siteA, compA.Site);

			Assert.AreEqual (1, containerA.Components.Count);
			Assert.AreEqual (1, containerB.Components.Count);
			Assert.AreSame (compB, containerA.Components [0]);
			Assert.AreSame (compA, containerB.Components [0]);
		}

		[Test]
		public void Add1_Component_Null ()
		{
			_container.Add ((IComponent) null);
			Assert.AreEqual (0, _container.Components.Count);
		}

		[Test] // Add (IComponent, String)
		public void Add2 ()
		{
			TestContainer containerA = new TestContainer ();
			TestContainer containerB = new TestContainer ();

			ISite siteA;
			ISite siteB;

			TestComponent compA = new TestComponent ();
			Assert.IsNull (compA.Site);
			TestComponent compB = new TestComponent ();
			Assert.IsNull (compB.Site);
			Assert.AreEqual (0, containerA.Components.Count);
			Assert.AreEqual (0, containerB.Components.Count);

			containerA.Add (compA, "A");
			siteA = compA.Site;
			Assert.IsNotNull (siteA);
			Assert.AreSame (compA, siteA.Component);
			Assert.AreSame (containerA, siteA.Container);
			Assert.IsFalse (siteA.DesignMode);
			Assert.AreEqual ("A", siteA.Name);
			containerA.Add (compB, "B");
			siteB = compB.Site;
			Assert.IsNotNull (siteB);
			Assert.AreSame (compB, siteB.Component);
			Assert.AreSame (containerA, siteB.Container);
			Assert.IsFalse (siteB.DesignMode);
			Assert.AreEqual ("B", siteB.Name);

			Assert.IsFalse (object.ReferenceEquals (siteA, siteB));
			Assert.AreEqual (2, containerA.Components.Count);
			Assert.AreEqual (0, containerB.Components.Count);
			Assert.AreSame (compA, containerA.Components [0]);
			Assert.AreSame (compB, containerA.Components [1]);

			// check effect of adding component that is already member of
			// another container
			containerB.Add (compA, "A2");
			Assert.IsFalse (object.ReferenceEquals (siteA, compA.Site));
			siteA = compA.Site;
			Assert.IsNotNull (siteA);
			Assert.AreSame (compA, siteA.Component);
			Assert.AreSame (containerB, siteA.Container);
			Assert.IsFalse (siteA.DesignMode);
			Assert.AreEqual ("A2", siteA.Name);

			Assert.AreEqual (1, containerA.Components.Count);
			Assert.AreEqual (1, containerB.Components.Count);
			Assert.AreSame (compB, containerA.Components [0]);
			Assert.AreSame (compA, containerB.Components [0]);

			// check effect of add component twice to same container
			containerB.Add (compA, "A2");
			Assert.AreSame (siteA, compA.Site);
			Assert.AreEqual ("A2", siteA.Name);

			Assert.AreEqual (1, containerA.Components.Count);
			Assert.AreEqual (1, containerB.Components.Count);
			Assert.AreSame (compB, containerA.Components [0]);
			Assert.AreSame (compA, containerB.Components [0]);

			// add again with different name
			containerB.Add (compA, "A3");
			Assert.AreSame (siteA, compA.Site);
			Assert.AreEqual ("A2", siteA.Name);

			Assert.AreEqual (1, containerA.Components.Count);
			Assert.AreEqual (1, containerB.Components.Count);
			Assert.AreSame (compB, containerA.Components [0]);
			Assert.AreSame (compA, containerB.Components [0]);

			// check effect of add component twice to same container
			containerB.Add (compA, "A2");
			Assert.AreSame (siteA, compA.Site);
			Assert.AreEqual ("A2", siteA.Name);
		}

		[Test]
		public void Add2_Component_Null ()
		{
			_container.Add ((IComponent) null, "A");
			Assert.AreEqual (0, _container.Components.Count);
			_container.Add (new TestComponent (), "A");
			Assert.AreEqual (1, _container.Components.Count);
			_container.Add ((IComponent) null, "A");
			Assert.AreEqual (1, _container.Components.Count);
		}

		[Test]
		public void Add2_Name_Duplicate ()
		{
			TestContainer container = new TestContainer ();
			TestComponent c1 = new TestComponent ();
			container.Add (c1, "dup");

			// new component, same case
			TestComponent c2 = new TestComponent ();
			try {
				container.Add (c2, "dup");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'dup'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'dup'") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
				Assert.AreEqual (1, container.Components.Count, "#A7");
			}

			// new component, different case
			TestComponent c3 = new TestComponent ();
			try {
				container.Add (c3, "duP");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'duP'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'duP'") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
				Assert.AreEqual (1, container.Components.Count, "#B7");
			}

			// existing component, same case
			TestComponent c4 = new TestComponent ();
			container.Add (c4, "C4");
			Assert.AreEqual (2, container.Components.Count, "#C1");
			container.Add (c4, "dup");
			Assert.AreEqual (2, container.Components.Count, "#C2");
			Assert.AreEqual ("C4", c4.Site.Name, "#C3");

			// component of other container, same case
			TestContainer container2 = new TestContainer ();
			TestComponent c5 = new TestComponent ();
			container2.Add (c5, "C5");
			try {
				container.Add (c5, "dup");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'dup'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'dup'") != -1, "#D5");
				Assert.IsNull (ex.ParamName, "#D6");
				Assert.AreEqual (2, container.Components.Count, "#D7");
			}
			Assert.AreEqual (1, container2.Components.Count, "#D8");
			Assert.AreSame (c5, container2.Components [0], "#D9");

#if NET_2_0
			container.AllowDuplicateNames = true;
			TestComponent c6 = new TestComponent ();
			container.Add (c6, "dup");
			Assert.AreEqual (3, container.Components.Count, "#E1");
			Assert.IsNotNull (c1.Site, "#E2");
			Assert.AreEqual ("dup", c1.Site.Name, "#E3");
			Assert.IsNotNull (c6.Site, "#E4");
			Assert.AreEqual ("dup", c6.Site.Name, "#E5");
			Assert.IsFalse (object.ReferenceEquals (c1.Site, c6.Site), "#E6");
#endif
		}

		[Test]
		public void AddRemove ()
		{
			TestComponent component = new TestComponent ();
			
			_container.Add (component);
			Assert.IsNotNull (component.Site, "#1");
			Assert.IsTrue (_container.Contains (component), "#2");
			
			_container.Remove (component);
			Assert.IsNull (component.Site, "#3");
			Assert.IsFalse (_container.Contains (component), "#4");
		}

		[Test] // Dispose ()
		public void Dispose1 ()
		{
			TestComponent compA;
			TestComponent compB;

			compA = new TestComponent ();
			_container.Add (compA);
			compB = new TestComponent ();
			_container.Add (compB);

			_container.Dispose ();

			Assert.AreEqual (0, _container.Components.Count, "#A1");
			Assert.IsTrue (compA.IsDisposed, "#A2");
			Assert.IsNull (compA.Site, "#A3");
			Assert.IsTrue (compB.IsDisposed, "#A4");
			Assert.IsNull (compB.Site, "#A5");

			_container = new TestContainer ();
			compA = new TestComponent ();
			compA.ThrowOnDispose = true;
			_container.Add (compA);
			compB = new TestComponent ();
			_container.Add (compB);

			// assert that component is not removed from components until after
			// Dispose of component has succeeded
			try {
				_container.Dispose ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException) {
				Assert.AreEqual (0, _container.Components.Count, "#B2");
				Assert.IsFalse (compA.IsDisposed, "#B4");
				Assert.IsNull (compA.Site, "#B5");
				Assert.IsTrue (compB.IsDisposed, "#B6");
				Assert.IsNull (compB.Site, "#B7");
			} finally {
				compA.ThrowOnDispose = false;
			}

			_container = new TestContainer ();
			compA = new TestComponent ();
			_container.Add (compA);
			compB = new TestComponent ();
			compB.ThrowOnDispose = true;
			_container.Add (compB);

			try {
				_container.Dispose ();
				Assert.Fail ("#C1");
			} catch (InvalidOperationException) {
				Assert.AreEqual (1, _container.Components.Count, "#C2");
				Assert.AreSame (compA, _container.Components [0], "#C3");
				Assert.IsFalse (compA.IsDisposed, "#C4");
				Assert.IsNotNull (compA.Site, "#C5");
				Assert.IsFalse (compB.IsDisposed, "#C6");
				Assert.IsNull (compB.Site, "#C7");
			} finally {
				compB.ThrowOnDispose = false;
			}
		}

		[Test] // Dispose (Boolean)
		public void Dispose2 ()
		{
			TestComponent compA;
			TestComponent compB;

			compA = new TestComponent ();
			_container.Add (compA);
			compB = new TestComponent ();
			_container.Add (compB);

			_container.Dispose (false);

			Assert.AreEqual (2, _container.Components.Count, "#A1");
			Assert.IsFalse (compA.IsDisposed, "#A2");
			Assert.IsNotNull (compA.Site, "#A3");
			Assert.IsFalse (compB.IsDisposed, "#A4");
			Assert.IsNotNull (compB.Site, "#A5");

			_container.Dispose (true);

			Assert.AreEqual (0, _container.Components.Count, "#B1");
			Assert.IsTrue (compA.IsDisposed, "#B2");
			Assert.IsNull (compA.Site, "#B3");
			Assert.IsTrue (compB.IsDisposed, "#B4");
			Assert.IsNull (compB.Site, "#B5");

			compA = new TestComponent ();
			_container.Add (compA);
			compB = new TestComponent ();
			_container.Add (compB);

			Assert.AreEqual (2, _container.Components.Count, "#C1");
			Assert.IsFalse (compA.IsDisposed, "#C2");
			Assert.IsNotNull (compA.Site, "#C3");
			Assert.IsFalse (compB.IsDisposed, "#C4");
			Assert.IsNotNull (compB.Site, "#C5");

			_container.Dispose (true);

			Assert.AreEqual (0, _container.Components.Count, "#D1");
			Assert.IsTrue (compA.IsDisposed, "#D2");
			Assert.IsNull (compA.Site, "#D3");
			Assert.IsTrue (compB.IsDisposed, "#D4");
			Assert.IsNull (compB.Site, "#D5");
		}

		[Test] // bug #522474
		public void Dispose_Recursive ()
		{
			MyComponent comp = new MyComponent ();
			Container container = comp.CreateContainer ();
			comp.Dispose ();
			Assert.AreEqual (0, container.Components.Count);
		}

		[Test]
		public void GetService ()
		{
			object service;

			GetServiceContainer container = new GetServiceContainer ();
			container.Add (new MyComponent ());
			service = container.GetService (typeof (MyComponent));
			Assert.IsNull (service, "#1");
			service = container.GetService (typeof (Component));
			Assert.IsNull (service, "#2");
			service = container.GetService (typeof (IContainer));
			Assert.AreSame (container, service, "#3");
			service = container.GetService ((Type) null);
			Assert.IsNull (service, "#4");
		}

		[Test]
		public void Remove ()
		{
			TestComponent compA;
			TestComponent compB;
			ISite siteA;
			ISite siteB;

			compA = new TestComponent ();
			_container.Add (compA);
			siteA = compA.Site;
			compB = new TestComponent ();
			_container.Add (compB);
			siteB = compB.Site;
			_container.Remove (compB);
			Assert.AreSame (siteA, compA.Site, "#A1");
			Assert.IsNull (compB.Site, "#A2");
			Assert.AreEqual (1, _container.Components.Count, "#A3");
			Assert.AreSame (compA, _container.Components [0], "#A4");

			// remove component with no site
			compB = new TestComponent ();
			_container.Remove (compB);
			Assert.AreSame (siteA, compA.Site, "#B1");
			Assert.IsNull (compB.Site, "#B2");
			Assert.AreEqual (1, _container.Components.Count, "#B3");
			Assert.AreSame (compA, _container.Components [0], "#B4");

			// remove component associated with other container
			TestContainer container2 = new TestContainer ();
			compB = new TestComponent ();
			container2.Add (compB);
			siteB = compB.Site;
			_container.Remove (compB);
			Assert.AreSame (siteA, compA.Site, "#C1");
			Assert.AreSame (siteB, compB.Site, "#C2");
			Assert.AreEqual (1, _container.Components.Count, "#C3");
			Assert.AreSame (compA, _container.Components [0], "#C4");
			Assert.AreEqual (1, container2.Components.Count, "#C5");
			Assert.AreSame (compB, container2.Components [0], "#C6");
		}

		[Test]
		public void Remove_Component_Null ()
		{
			_container.Add (new TestComponent ());
			_container.Remove ((IComponent) null);
			Assert.AreEqual (1, _container.Components.Count);
		}

#if NET_2_0
		[Test]
		public void RemoveWithoutUnsiting ()
		{
			TestComponent compA;
			TestComponent compB;
			ISite siteA;
			ISite siteB;

			compA = new TestComponent ();
			_container.Add (compA);
			siteA = compA.Site;
			compB = new TestComponent ();
			_container.Add (compB);
			siteB = compB.Site;
			_container.RemoveWithoutUnsiting (compB);
			Assert.AreSame (siteA, compA.Site, "#A1");
			Assert.AreSame (siteB, compB.Site, "#A2");
			Assert.AreEqual (1, _container.Components.Count, "#A3");
			Assert.AreSame (compA, _container.Components [0], "#A4");

			// remove component with no site
			compB = new TestComponent ();
			_container.RemoveWithoutUnsiting (compB);
			Assert.AreSame (siteA, compA.Site, "#B1");
			Assert.IsNull (compB.Site, "#B2");
			Assert.AreEqual (1, _container.Components.Count, "#B3");
			Assert.AreSame (compA, _container.Components [0], "#B4");

			// remove component associated with other container
			TestContainer container2 = new TestContainer ();
			compB = new TestComponent ();
			container2.Add (compB);
			siteB = compB.Site;
			_container.RemoveWithoutUnsiting (compB);
			Assert.AreSame (siteA, compA.Site, "#C1");
			Assert.AreSame (siteB, compB.Site, "#C2");
			Assert.AreEqual (1, _container.Components.Count, "#C3");
			Assert.AreSame (compA, _container.Components [0], "#C4");
			Assert.AreEqual (1, container2.Components.Count, "#C5");
			Assert.AreSame (compB, container2.Components [0], "#C6");
		}

		[Test]
		public void RemoveWithoutUnsiting_Component_Null ()
		{
			ISite site;
			TestComponent component;

			component = new TestComponent ();
			_container.Add (component);
			site = component.Site;
			_container.RemoveWithoutUnsiting ((IComponent) null);
			Assert.AreSame (site, component.Site, "#1");
			Assert.AreEqual (1, _container.Components.Count, "#2");
			Assert.AreSame (component, _container.Components [0], "#3");
		}

		[Test]
		public void ValidateName_Component_Null ()
		{
			try {
				_container.InvokeValidateName ((IComponent) null, "A");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("component", ex.ParamName, "#5");
			}
		}

		[Test]
		public void ValidateName_Name_Null ()
		{
			TestComponent compA = new TestComponent ();
			_container.Add (compA, (string) null);
			TestComponent compB = new TestComponent ();
			_container.InvokeValidateName (compB, (string) null);
		}

		[Test]
		public void ValidateName_Name_Duplicate ()
		{
			TestComponent compA = new TestComponent ();
			_container.Add (compA, "dup");

			// same component, same case
			_container.InvokeValidateName (compA, "dup");

			// existing component, same case
			TestComponent compB = new TestComponent ();
			_container.Add (compB, "B");
			try {
				_container.InvokeValidateName (compB, "dup");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'duP'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'dup'") != -1, "#A5");
				Assert.IsNull (ex.ParamName, "#A6");
				Assert.AreEqual (2, _container.Components.Count, "#A7");
			}
			_container.InvokeValidateName (compB, "whatever");

			// new component, different case
			TestComponent compC = new TestComponent ();
			try {
				_container.InvokeValidateName (compC, "dup");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'duP'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'dup'") != -1, "#B5");
				Assert.IsNull (ex.ParamName, "#B6");
				Assert.AreEqual (2, _container.Components.Count, "#B7");
			}
			_container.InvokeValidateName (compC, "whatever");

			// component of other container, different case
			TestContainer container2 = new TestContainer ();
			TestComponent compD = new TestComponent ();
			container2.Add (compD, "B");
			try {
				_container.InvokeValidateName (compD, "dup");
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Duplicate component name 'duP'.  Component names must be
				// unique and case-insensitive
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("'dup'") != -1, "#C5");
				Assert.IsNull (ex.ParamName, "#C6");
				Assert.AreEqual (2, _container.Components.Count, "#C7");
			}
			_container.InvokeValidateName (compD, "whatever");
			Assert.AreEqual (1, container2.Components.Count, "#C8");
			Assert.AreSame (compD, container2.Components [0], "#C9");
		}
#endif

		class MyComponent : Component
		{
			private Container container;

			protected override void Dispose (bool disposing)
			{
				if (container != null)
					container.Dispose ();
				base.Dispose (disposing);
			}

			public Container CreateContainer ()
			{
				if (container != null)
					throw new InvalidOperationException ();
				container = new Container ();
				container.Add (new MyComponent ());
				container.Add (this);
				return container;
			}

			public Container Container {
				get { return container; }
			}
		}

		class MyContainer : IContainer
		{
			private ComponentCollection components = new ComponentCollection (
				new Component [0]);

			public ComponentCollection Components {
				get { return components; }
			}

			public void Add (IComponent component)
			{
			}

			public void Add (IComponent component, string name)
			{
			}

			public void Remove (IComponent component)
			{
			}

			public void Dispose ()
			{
			}
		}

		public class GetServiceContainer : Container
		{
			public new object GetService (Type service)
			{
				return base.GetService (service);
			}
		}
	}
}

#endif
