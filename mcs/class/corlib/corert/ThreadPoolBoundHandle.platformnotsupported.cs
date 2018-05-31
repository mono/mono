namespace System.Threading
{
	public sealed class ThreadPoolBoundHandle : IDisposable
	{
		internal ThreadPoolBoundHandle ()
		{
		}

		public System.Runtime.InteropServices.SafeHandle Handle {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		[System.CLSCompliantAttribute (false)]
		public unsafe System.Threading.NativeOverlapped* AllocateNativeOverlapped (System.Threading.IOCompletionCallback callback, object state, object pinData)
		{
			throw new PlatformNotSupportedException ();
		}

		[System.CLSCompliantAttribute (false)]
		public unsafe System.Threading.NativeOverlapped* AllocateNativeOverlapped (System.Threading.PreAllocatedOverlapped preAllocated)
		{
			throw new PlatformNotSupportedException ();
		}

		public static System.Threading.ThreadPoolBoundHandle BindHandle (System.Runtime.InteropServices.SafeHandle handle)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Dispose ()
		{

		}

		[System.CLSCompliantAttribute (false)]
		public unsafe void FreeNativeOverlapped (System.Threading.NativeOverlapped* overlapped)
		{
			throw new PlatformNotSupportedException ();
		}

		[System.CLSCompliantAttribute (false)]
		public static unsafe object GetNativeOverlappedState (System.Threading.NativeOverlapped* overlapped)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
