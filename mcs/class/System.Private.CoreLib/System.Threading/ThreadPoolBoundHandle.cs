namespace System.Threading
{
	public sealed partial class ThreadPoolBoundHandle : System.IDisposable
	{
		internal ThreadPoolBoundHandle() { }
		public System.Runtime.InteropServices.SafeHandle Handle { get { throw null; } }
		[System.CLSCompliantAttribute(false)]
		public unsafe System.Threading.NativeOverlapped* AllocateNativeOverlapped(System.Threading.IOCompletionCallback callback, object state, object pinData) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public unsafe System.Threading.NativeOverlapped* AllocateNativeOverlapped(System.Threading.PreAllocatedOverlapped preAllocated) { throw null; }
		public static System.Threading.ThreadPoolBoundHandle BindHandle(System.Runtime.InteropServices.SafeHandle handle) { throw null; }
		public void Dispose() { }
		[System.CLSCompliantAttribute(false)]
		public unsafe void FreeNativeOverlapped(System.Threading.NativeOverlapped* overlapped) { }
		[System.CLSCompliantAttribute(false)]
		public static unsafe object GetNativeOverlappedState(System.Threading.NativeOverlapped* overlapped) { throw null; }
	}
}