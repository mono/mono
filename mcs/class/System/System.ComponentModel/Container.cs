//
// System.ComponentModel.Container.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.ComponentModel {

	// <summary>
	//   Container class: encapsulates components.  
	// </summary>
	//
	// <remarks>
	//   
	// </remarks>
	public class Container : IContainer, IDisposable {
		ComponentCollection cc;

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
			IComponent component;
			IContainer container;
			string     name;
			
			void DefaultSite (string name, IComponent component, IContainer container)
			{
				this.component = component;
				this.container = container;
				this.name = name;
			}

			IComponent Component {
				get {
					return component;
				}
			}

			IContainer Container {
				get {
					return container;
				}
			}

			bool DesignMode {
				get {
					// FIXME: should we provide a way to set
					// this value?
					return false;
				}
			}

			string Name {
				get {
					return name;
				}
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
				return cc;
			}
		}

		// <summary>
		//   Adds an IComponent to the Container
		// </summary>
		public virtual void Add (IComponent component)
		{
			// FIXME: Add this component to the ComponentCollection.cc
		}

		// <summary>
		//   Adds an IComponent to the Container.  With a name binding.
		// </summary>
		public virtual void Add (IComponent component, string name)
		{
			// FIXME: Add this component to the ComponentCollection.cc
		}

		// <summary>
		//   Returns an ISite for a component.
		// <summary>
		protected virtual ISite CreateSite (IComponent component, string name)
		{
			return DefaultSite (name, component, this);
		}

		public virtual void Dispose ()
		{
			Dispose (true);
			GC.SupressFinalize (this);
		}

		bool disposed = false;
		
		public virtual void Dispose (bool release_all)
		{
			if (disposed)
				return;

			if (release_all){
				cc.Dispose ();
				cc = null;
			}

			disposed = true;
		}

		protected virtual object GetService (Type service)
		{
			// FIXME: Not clear what GetService does.
			
			return null;
		}

		public virtual void Remove (IComponent component)
		{
			// FIXME: Add this component to the ComponentCollection.cc
		}
	}
	
}
