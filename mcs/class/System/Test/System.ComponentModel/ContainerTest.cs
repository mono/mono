//
// System.ComponentModel.Container test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Ivan N. Zlatev (contact i-nZ.net)

// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2006 Ivan N. Zlatev
//

using NUnit.Framework;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace MonoTests.System.ComponentModel
{
	class TestService {
	}
	
	class TestContainer : Container {
		ServiceContainer _services = new ServiceContainer();
		
		public TestContainer() {
			_services.AddService( typeof(TestService), new TestService() );
		}
		
		protected override object GetService( Type serviceType ) {
			return _services.GetService( serviceType );
		}

#if NET_2_0
		public void Remove_WithoutUnsiting (IComponent component)
		{
			base.RemoveWithoutUnsiting (component);
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
	}
	
	class TestComponent : Component {
		public override ISite Site {
			get {
				return base.Site;
			}
			set {
				base.Site = value;
				if (value != null) {
					Assert.IsNotNull (value.GetService (typeof (ISite)));
					Assert.IsNotNull (value.GetService (typeof (TestService)));
				}
			}
		}
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
			
#if NET_2_0
			_container.Add (component);
			_container.Remove_WithoutUnsiting (component);
			Assert.IsNotNull (component.Site, "#5");
			Assert.IsFalse (_container.Contains (component), "#6");
#endif
		}

		[Test]
		public void GetService1 ()
		{
			_container.Add (new TestComponent ());
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ValidateName ()
		{
			TestContainer container = new TestContainer ();
			TestComponent c1 = new TestComponent ();
			container.Add (c1, "dup");
			TestComponent c2 = new TestComponent ();
			container.Add (c2, "dup");
		}
#endif
	}
}
