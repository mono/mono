//
// System.ComponentModel.Container.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

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

using System.Collections;

namespace System.ComponentModel {

	// <summary>
	//   Container class: encapsulates components.  
	// </summary>
	//
	// <remarks>
	//   
	// </remarks>
	public class Container : IContainer, IDisposable {

		private ArrayList c = new ArrayList ();
		//ComponentCollection cc;

		// <summary>
		//   Auxiliary class to support the default behaviour of CreateSite
		// </summary>
		//
		// <remarks>
		//   This is an internal class that is used to provide a
		//   default implementation of an ISite class.  Container
		//   is just a default implementation of IContainer, and
		//   provides this as a way of getting started
		// </remarks>
		
		class DefaultSite : ISite {
			private IComponent component;
			private Container container;
			private string     name;
			
			public DefaultSite (string name, IComponent component, Container container)
			{
				this.component = component;
				this.container = container;
				this.name = name;
			}

			public IComponent Component {
				get {
					return component;
				}
			}

			public IContainer Container {
				get {
					return container;
				}
			}

			[MonoTODO]
			public bool DesignMode {
				get {
					// FIXME: should we provide a way to set
					// this value?
					return false;
				}
			}

			public string Name {
				get {
					return name;
				}

				set {
					name = value;
				}
			}

			public virtual object GetService (Type t)
			{
				if (typeof(ISite) != t) {
					return null; 
				}
				return container.GetService (t);
			}
		}
		
		// <summary>
		//   Container constructor
		// </summary>
		public Container ()
		{
		}

		public virtual ComponentCollection Components {
			get {
				Array a = c.ToArray(typeof (IComponent));
				return new ComponentCollection((IComponent[]) a);
			}
		}

		public virtual void Add (IComponent component)
		{
			Add (component, null);
		}

		public virtual void Add (IComponent component, string name)
		{
			component.Site = CreateSite (component, name);
			c.Add (component);
		}

		protected virtual ISite CreateSite (IComponent component, string name)
		{
			if (name != null) {
				foreach (IComponent Comp in c) {
					if (Comp.Site != null && Comp.Site.Name == name)
						throw new ArgumentException ("duplicate component name", "name");
				}
			}

			return new DefaultSite (name, component, this);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		bool disposed = false;
		
		protected virtual void Dispose (bool release_all)
		{
			if (disposed)
				return;
			disposed = true;

			if (release_all){
				foreach (IComponent component in c)
					component.Dispose ();
			}

			c = null;
		}

		~Container ()
		{
			Dispose (false);
		}

		protected virtual object GetService (Type service)
		{
			if (typeof(IContainer) != service) {
				return null; 
			}
			return this;
		}

		public virtual void Remove (IComponent component)
		{
			c.Remove (component);
		}
	}
	
}
