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

		EventHandlerList event_handlers;
		ISite            mySite;

		// <summary>
		//   Component Constructor
		// </summary>
		public Component ()
		{
			event_handlers = null;
		}

		// <summary>
		//   Get IContainer of this Component
		// </summary>
		public IContainer Container {
			get {
				return mySite.Container;
			}
		}

		protected bool DesignMode {
			get {
				return mySite.DesignMode;
			}
		}

		protected EventHandlerList Events {
			get {
				// Note: space vs. time tradeoff
				// We create the object here if it's never be accessed before.  This potentially 
				// saves space. However, we must check each time the propery is accessed to
				// determine whether we need to create the object, which increases overhead.
				// We could put the creation in the contructor, but that would waste space
				// if it were never used.  However, accessing this property would be faster.
				if (null == event_handlers)
				{
					event_handlers = new EventHandlerList();
				}
				return event_handlers;
			}
		}

		public virtual ISite Site {
			get {
				return mySite;
			}

			set {
				mySite = value;
			}
		}

		[MonoTODO]
		~Component()
		{
			// FIXME: Not sure this is correct.
			Dispose(true);
		}

		// <summary>
		//   Dispose resources used by this component
		// </summary>
		[MonoTODO]
		public void Dispose ()
		{
			// FIXME: Not sure this is correct.
			Dispose(false);
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
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
		[MonoTODO]
		protected virtual object GetService (Type service)
		{
			// FIXME: Not sure what this should do.
			return null;
		}

		public override string ToString ()
		{
			if (mySite == null)
				return GetType ().ToString ();
			return String.Format ("{0} [{1}]", mySite.Name, GetType ().ToString ());
		}
		// <summary>
		//   FIXME: Figure out this one.
	        // </summary>
		[MonoTODO ("Figure this out")]
		public event EventHandler Disposed;
	}
	
}
