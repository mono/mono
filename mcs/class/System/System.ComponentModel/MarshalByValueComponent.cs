//
// System.ComponentModel.MarshalByValueComponent.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

using System;

namespace System.ComponentModel
{
	/// <summary>
	/// Implements IComponent and provides the base implementation for remotable components that are marshaled by value (a copy of the serialized object is passed).
	/// </summary>
	public class MarshalByValueComponent : IComponent, IDisposable, IServiceProvider
	{
		EventHandlerList eventList;

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
		
		[MonoTODO]
		public virtual object GetService (Type service) {
			return null;
		}
		
		public virtual IContainer Container {
			[MonoTODO]
			get {
				return null;
			}
		}

		public virtual bool DesignMode {
			[MonoTODO]
			get {
				return false;
			}
		}

		public virtual ISite Site {
			[MonoTODO]
			get {
				// TODO: need to get Site
				return null;
			}

			[MonoTODO]
			set {
				// TODO: need to set Site
			}
		}

		protected EventHandlerList Events {
			get {
				if (eventList == null)
					eventList = new EventHandlerList ();

				return eventList;
			}
		}
		
		public event EventHandler Disposed;
	}
}
