//
// System.ComponentModel.Container.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
// (C) 2006 Ivan N. Zlatev
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
using System.Collections.Generic;

namespace System.ComponentModel {

	// <summary>
	//   Container class: encapsulates components.  
	// </summary>
	//
	// <remarks>
	//   
	// </remarks>
	public class Container : IContainer, IDisposable {
		private List<IComponent> c = new List<IComponent> ();

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

			private readonly IComponent component;
			private readonly Container container;
			private string name;
			
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

			public bool DesignMode {
				get {
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
				if (typeof (ISite) == t)
					return this; 

				return container.GetService (t);
			}
		}

		public virtual ComponentCollection Components {
			get {
				IComponent [] a = c.ToArray ();
				return new ComponentCollection (a);
			}
		}

		public virtual void Add (IComponent component)
		{
			Add (component, null);
		}

		public virtual void Add (IComponent component, string name)
		{
			if (component != null) {
				if (component.Site == null || component.Site.Container != this) {
					ValidateName (component, name);
					if (component.Site != null)
						component.Site.Container.Remove (component);
					component.Site = this.CreateSite (component, name);
					c.Add (component);
				}
			}
		}

		protected virtual
		void ValidateName (IComponent component, string name)
		{
			if (component == null)
				throw new ArgumentNullException ("component");
			if (name == null)
				return;
			foreach (IComponent ic in c) {
				if (object.ReferenceEquals (component, ic))
					continue;
				if (ic.Site != null && string.Compare (ic.Site.Name, name, true) == 0)
					throw new ArgumentException (String.Format ("There already is a named component '{0}' in this container", name));
			}
		}

		protected virtual ISite CreateSite (IComponent component, string name)
		{
			return new DefaultSite (name, component, this);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				while (c.Count > 0) {
					int index = c.Count - 1;
					var component = c [index];
					Remove (component);
					component.Dispose ();
				}
			}
		}

		~Container ()
		{
			Dispose (false);
		}

		protected virtual object GetService (Type service)
		{
			if (typeof(IContainer) != service)
				return null;
			return this;
		}

		public virtual void Remove (IComponent component)
		{
			Remove (component, true);
		}

		void Remove (IComponent component, bool unsite)
		{
			if (component != null) {
				if (component.Site != null && component.Site.Container == this) {
					if (unsite)
						component.Site = null;
					c.Remove (component);
				}
			}
		}

		protected void RemoveWithoutUnsiting (IComponent component)
		{
			Remove (component, false);
		}
	}
	
}
