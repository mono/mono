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
			foreach (IComponent Comp in c) {
				if (Comp.Site != null)
				if (Comp.Site.Name == name)
					throw new ArgumentException ("duplicate component name", "name");
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
				//??
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
