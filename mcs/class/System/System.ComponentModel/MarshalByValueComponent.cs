//
// System.ComponentModel.MarshalByValueComponent.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.ComponentModel
{
	/// <summary>
	/// Implements IComponent and provides the base implementation for remotable components that are marshaled by value (a copy of the serialized object is passed).
	/// </summary>
	public class MarshalByValueComponent : IComponent, IDisposable, IServiceProvider
	{
		[MonoTODO]
		public MarshalByValueComponent () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose () {
			throw new NotImplementedException ();
		}

		protected virtual void Dispose (bool disposing) {
		}

		public virtual object GetService (Type service) {
			return null;
		}
		
		public virtual IContainer Container {
			get {
				return null;
			}
		}

		public virtual bool DesignMode {
			get {
				return false;
			}
		}

		public virtual ISite Site {
			get {
				return null;
			}
			set {
			}
		}

		[MonoTODO]
		protected EventHandlerList Events {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public event EventHandler Disposed;
	}
}
