//
// System.ComponentModel.Component.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.ComponentModel {

	[DesignerCategory ("Component")]
	public class Component : MarshalByRefObject, IComponent, IDisposable
	{

		private EventHandlerList event_handlers;
		private ISite mySite;
		private object disposedEvent = new object ();

		public Component ()
		{
			event_handlers = null;
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IContainer Container {
			get {
				if (mySite == null)
					return null;
				return mySite.Container;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected bool DesignMode {
			get {
				if (mySite == null)
					return false;
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
					event_handlers = new EventHandlerList();

				return event_handlers;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual ISite Site {
			get { return mySite; }
			set { mySite = value; }
		}

		~Component()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
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
			if (release_all) {
				EventHandler eh = (EventHandler) Events [disposedEvent];
				if (eh != null)
					eh (this, EventArgs.Empty);
			}
			mySite = null;
		}

		protected virtual object GetService (Type service)
		{
			if (mySite != null) {
				return mySite.GetService(service); 
			}
			return null; 
		}

		public override string ToString ()
		{
			if (mySite == null)
				return GetType ().ToString ();
			return String.Format ("{0} [{1}]", mySite.Name, GetType ().ToString ());
		}

		[Browsable (false), EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler Disposed {
			add { Events.AddHandler (disposedEvent, value); }
			remove { Events.RemoveHandler (disposedEvent, value); }
		}
	}
	
}
