//
// System.ComponentModel.Component.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.ComponentModel {

	// <summary>
	//   Component class.
	// </summary>
	//
	// <remarks>
	//   Longer description
	// </remarks>
	public class Component : MarshalByRefObject, IComponent, IDisposable {

		IContainer       icontainer;
		bool             design_mode;
		EventHandlerList event_handlers;
		ISite            isite;

		// <summary>
		//   Component Constructor
		// </summary>
		public Component ()
		{
		}

		// <summary>
		//   Get IContainer of this Component
		// </summary>
		public IContainer Container {
			get {
				return icontainer;
			}
		}

		protected bool DesignMode {
			get {
				return design_mode;
			}
		}

		protected EventHandlerList Events {
			get {
				return event_handlers;
			}
		}

		public virtual ISite Site {
			get {
				return isite;
			}

			set {
				isite = value;
			}
		}


		// <summary>
		//   Dispose resources used by this component
		// </summary>
		public virtual void Dispose ()
		{
		}

		// <summary>
		//   Controls disposal of resources used by this.
		// </summary>
		//
		// <param name="release_all"> Controls which resources are released</param>
		//
		// <remarks>
		//   if release_all is set to true, both managed and unmanaged
		//   resources should be released.  If release_all is set to false,
		//   only unmanaged resources should be disposed
		// </remarks>
		protected virtual void Dispose (bool release_all)
		{
		}

		// <summary>
		//   Implements the IServiceProvider interface
		// </summary>
		protected virtual object GetService (Type service)
		{
			// FIXME: Not sure what this should do.
			return null;
		}

		// <summary>
		//   FIXME: Figure out this one.
	        // </summary>
		public event EventHandler Disposed;
	}
	
}
