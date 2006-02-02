//
// System.ComponentModel.Container test cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
	public class ContainerTest {
		[Test]
		public void GetService1 ()
		{
			TestContainer container = new TestContainer ();
			container.Add (new TestComponent ());
		}
	}
}

