//
// System.ComponentModel.MarshalByValueComponent.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.ComponentModel
{
	/// <summary>
	/// Implements IComponent and provides the base implementation for remotable components that are marshaled by value (a copy of the serialized object is passed).
	/// </summary>
	[DesignerCategory ("Component"), TypeConverter (typeof (ComponentConverter))]
    	[Designer ("System.Windows.Forms.Design.ComponentDocumentDesigner, " + Consts.AssemblySystem_Design, typeof (IRootDesigner))]
	public class MarshalByValueComponent : IComponent, IDisposable, IServiceProvider
	{
		private EventHandlerList eventList;
		private ISite mySite;
		private object disposedEvent = new object ();

		public MarshalByValueComponent ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				// free managed objects contained here
			}

			// Free unmanaged objects
			// Set fields to null
		}

		~MarshalByValueComponent ()
		{
			Dispose (false);
		}
		
		public virtual object GetService (Type service) 
		{
			if (mySite != null) {
				return mySite.GetService(service); 
			}
			return null; 
		}
		
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual IContainer Container {
			get {
				if (mySite == null)
					return null;
				return mySite.Container;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool DesignMode {
			get {
				if (mySite == null)
					return false;
				return mySite.DesignMode;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual ISite Site {
			get { return mySite; }
			set { mySite = value; }
		}

		public override string ToString ()
		{
			if (mySite == null)
				return GetType ().ToString ();
			return String.Format ("{0} [{1}]", mySite.Name, GetType ().ToString ());
		}

		protected EventHandlerList Events {
			get {
				if (eventList == null)
					eventList = new EventHandlerList ();

				return eventList;
			}
		}
		
		public event EventHandler Disposed
		{
			add { Events.AddHandler (disposedEvent, value); }
			remove { Events.RemoveHandler (disposedEvent, value); }
		}
	}
}

