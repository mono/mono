namespace System.Threading
{
	public class Overlapped
	{
		public Overlapped() { }
		[System.ObsoleteAttribute("This constructor is not 64-bit compatible.  Use the constructor that takes an IntPtr for the event handle.  https://go.microsoft.com/fwlink/?linkid=14202")]
		public Overlapped(int offsetLo, int offsetHi, int hEvent, System.IAsyncResult ar) { }
		public Overlapped(int offsetLo, int offsetHi, System.IntPtr hEvent, System.IAsyncResult ar) { }
		public System.IAsyncResult AsyncResult { get { throw null; } set { } }
		[System.ObsoleteAttribute("This property is not 64-bit compatible.  Use EventHandleIntPtr instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
		public int EventHandle { get { throw null; } set { } }
		public System.IntPtr EventHandleIntPtr { get { throw null; } set { } }
		public int OffsetHigh { get { throw null; } set { } }
		public int OffsetLow { get { throw null; } set { } }
		[System.CLSCompliantAttribute(false)]
		public static unsafe void Free(System.Threading.NativeOverlapped* nativeOverlappedPtr) { }
		[System.CLSCompliantAttribute(false)]
		[System.ObsoleteAttribute("This method is not safe.  Use Pack (iocb, userData) instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
		public unsafe System.Threading.NativeOverlapped* Pack(System.Threading.IOCompletionCallback iocb) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public unsafe System.Threading.NativeOverlapped* Pack(System.Threading.IOCompletionCallback iocb, object userData) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public static unsafe System.Threading.Overlapped Unpack(System.Threading.NativeOverlapped* nativeOverlappedPtr) { throw null; }
		[System.CLSCompliantAttribute(false)]
		[System.ObsoleteAttribute("This method is not safe.  Use UnsafePack (iocb, userData) instead.  https://go.microsoft.com/fwlink/?linkid=14202")]
		public unsafe System.Threading.NativeOverlapped* UnsafePack(System.Threading.IOCompletionCallback iocb) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public unsafe System.Threading.NativeOverlapped* UnsafePack(System.Threading.IOCompletionCallback iocb, object userData) { throw null; }
	}
}