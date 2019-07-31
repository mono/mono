using System.Runtime.InteropServices;

namespace System
{
	[StructLayout (LayoutKind.Sequential)]
	public abstract class MarshalByRefObject
	{
#region Keep this code, it is used by the runtime
#pragma warning disable 169, 649
		private object _identity;
#pragma warning restore 169, 649
#endregion

		protected MarshalByRefObject ()
		{
		}

		public object GetLifetimeService ()
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual object InitializeLifetimeService ()
		{
			throw new PlatformNotSupportedException ();
		}

		protected MarshalByRefObject MemberwiseClone (bool cloneIdentity)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
