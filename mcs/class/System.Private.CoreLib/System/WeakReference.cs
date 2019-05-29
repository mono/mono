using System.Runtime.InteropServices;

namespace System
{
	partial class WeakReference
	{
		bool trackResurrection;
		GCHandle handle;

		public virtual bool IsAlive {
			get {
				return Target != null;
			}
		}

		public virtual object Target {
			get {
				if (!handle.IsAllocated)
					return null;
				return handle.Target;
			}
			set {
				handle.Target = value;
			}
		}

		~WeakReference ()
		{
			handle.Free ();
		}

		void Create (object target, bool trackResurrection)
		{
			if (trackResurrection) {
				this.trackResurrection = true;
				handle = GCHandle.Alloc (target, GCHandleType.WeakTrackResurrection);
			} else {
				handle = GCHandle.Alloc (target, GCHandleType.Weak);
			}
		}

		bool IsTrackResurrection () => trackResurrection;
	}
}