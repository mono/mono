//
//  MonoTests.System.ComponentModel.NestedContainerTest
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE

using System;
using System.ComponentModel;
using System.Collections;
using NUnit.Framework;


namespace MonoTests.System.ComponentModel
{

	[TestFixture]
	public class NestedContainerTest : NestedContainer
	{

		private class ContainerWithService : Container
		{

			public class DesignModeEnabledSite : ISite {

				private IComponent _component;
				private ContainerWithService _container;
				private string _name;
				
				public DesignModeEnabledSite (string name, IComponent component, ContainerWithService container)
				{
					_component = component;
					_container = container;
					_name = name;
				}
	
				public IComponent Component {
					get { return _component; }
				}
	
				public IContainer Container {
					get { return _container; }
				}
	
				public bool DesignMode {
					get { return true; }
				}
	
				public string Name {
					get { return _name; }
					set { _name = value; }
				}
	
				public virtual object GetService (Type t)
				{
					if (typeof (ISite) == t)
						return this; 
	
					return _container.GetService (t);
				}
			}

			protected override object GetService (Type type)
			{
				if (typeof (ContainerWithService) == type)
					return this;
				else
					return null;
			}

			protected override ISite CreateSite (IComponent component, string name)
			{
				return new ContainerWithService.DesignModeEnabledSite (name, component, this);
			}
		}                      		

		public NestedContainerTest () : base (new Component())
		{
		}

		public NestedContainerTest (IComponent owner) : base (owner)
		{
		}

		[Test]
		public void NameTest ()
		{
			Container container = new Container ();
			Component owner = new Component ();
			container.Add (owner, "OwnerName");
			NestedContainerTest nestedContainer = new NestedContainerTest (owner);
			Component nestedComponent = new Component ();
			nestedContainer.Add (nestedComponent, "NestedComponentName");

			Assert.AreEqual ("OwnerName", nestedContainer.OwnerName, "#1");
			Assert.AreEqual ("OwnerName.NestedComponentName", ((INestedSite)nestedComponent.Site).FullName, "#2");
			// Prooves that MSDN is wrong.
			Assert.AreEqual ("NestedComponentName", nestedComponent.Site.Name, "#3");

			container.Remove (owner);
			Assert.AreEqual (null, nestedContainer.OwnerName, "#4");
			Assert.AreEqual ("NestedComponentName", ((INestedSite)nestedComponent.Site).FullName, "#5");
		}

		[Test]
		public void GetServiceTest ()
		{
			ContainerWithService container = new ContainerWithService ();
			Component owner = new Component ();
			container.Add (owner, "OwnerName");
			NestedContainerTest nestedContainer = new NestedContainerTest (owner);
			Component nestedComponent = new Component ();
			nestedContainer.Add (nestedComponent, "NestedComponentName");

			Assert.IsNotNull (nestedComponent.Site.GetService (typeof (INestedContainer)), "#1");
			// test who provides the ISite service.
			Assert.AreEqual (nestedComponent.Site, nestedComponent.Site.GetService (typeof (ISite)), "#2");
			// test GetService forwarding to owner. Prooves that MSDN is wrong
			Assert.IsNull (nestedComponent.Site.GetService (typeof (ContainerWithService)), "#3");
		}

		[Test]
		public void DesignModeTest ()
		{
			ContainerWithService container = new ContainerWithService ();
			Component owner = new Component ();
			container.Add (owner, "OwnerName");
			NestedContainerTest nestedContainer = new NestedContainerTest (owner);
			Component nestedComponent = new Component ();
			nestedContainer.Add (nestedComponent, "NestedComponentName");

			Assert.IsTrue (nestedComponent.Site.DesignMode, "#1");
		}
	}
}
#endif
