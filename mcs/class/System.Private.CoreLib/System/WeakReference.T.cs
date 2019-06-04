using System.Runtime.InteropServices;

namespace System
{
	partial class WeakReference<T>
	{
		GCHandle handle;
		bool trackResurrection;

		T Target {
			get {
				GCHandle h = handle;
				return h.IsAllocated ? (T) h.Target : null;
			}
		}

		~WeakReference ()
		{
			handle.Free ();
		}

		void Create (T target, bool trackResurrection)
		{
			if (trackResurrection) {
				trackResurrection = true;
				handle = GCHandle.Alloc (target, GCHandleType.WeakTrackResurrection);
			} else {
				handle = GCHandle.Alloc (target, GCHandleType.Weak);
			}
		}

		public void SetTarget (T target)
		{
			handle.Target = target;
		}		

		bool IsTrackResurrection () => trackResurrection;
	}
}