//
// System.ComponentModel.NestedContainer
//
// Authors:		
//		Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.ComponentModel
{
	// Differences compared to Container:
	// * Site's DesignMode property is routed through the owning component's site. Note that even though MSDN 
	//   says that GetService is routed as well, it actually isn't. The unit tests proof that.
	// * According to MSDN the site's Name property is a qualified name that includes the owning component's name followed by a
	//	period (.) and the child component's name. It is not again according to the tests.
	// * GetService provides support for the INestedContainer as a service.
	// * When the owning component is disposed, the container is disposed as well.
	
	public class NestedContainer : Container, INestedContainer, IContainer, IDisposable
	{

#region Site : INestedSite, ISite, IServiceProvider

		private class Site : INestedSite, ISite, IServiceProvider
		{		 

			private IComponent _component;
			private NestedContainer _nestedContainer;
			private string _siteName;
			
			public Site (IComponent component, NestedContainer container, string name)
			{
				_component = component;
				_nestedContainer = container;
				_siteName = name;
			}
			
			public IComponent Component {
				get { return _component; }
			}

			public IContainer Container {
				get { return _nestedContainer; }
			}

			public bool DesignMode {
				get {
					if (_nestedContainer.Owner != null
						&& _nestedContainer.Owner.Site != null) {

						return _nestedContainer.Owner.Site.DesignMode;
					}
					else {
						return false;
					}
				}
			}

			public string Name {
				get { return _siteName; }				
				set { _siteName = value; }
			}

			// [owner].[component]
			//
			public string FullName {
				get {
					if (_siteName == null) {
						return null;
					}
					if (_nestedContainer.OwnerName == null) {
						return _siteName;
					}

					return _nestedContainer.OwnerName + "." + _siteName;
				}
			}

			public virtual object GetService (Type service)
			{
				if (service == typeof (ISite)) {
					return this; 
				}
				
				return _nestedContainer.GetService (service);
			}
		} // Site

#endregion

		
		
		private IComponent _owner;
		
		
		public NestedContainer (IComponent owner)
		{
			if (owner == null)
				throw new ArgumentNullException ("owner");

			_owner = owner;
			_owner.Disposed += new EventHandler (OnOwnerDisposed);
		}
		
		public IComponent Owner {
			get { return _owner; }
		}

		protected virtual string OwnerName {
			get {
				if (_owner.Site is INestedSite)
					return ((INestedSite) _owner.Site).FullName;
				if (_owner == null || _owner.Site == null)
					return null;
			
				return _owner.Site.Name;
			}
		}		

		protected override ISite CreateSite (IComponent component, string name)
		{
			if (component == null)
				throw new ArgumentNullException("component");
			
			return new NestedContainer.Site (component, this, name);
		}
		
		protected override object GetService (Type service)
		{
			if (service == typeof (INestedContainer))
				return this;
			
			return base.GetService (service);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				_owner.Disposed -= new EventHandler (OnOwnerDisposed);
			
			base.Dispose (disposing);
		}

		private void OnOwnerDisposed (object sender, EventArgs e)
		{
			this.Dispose ();
		}
	}
	
}
