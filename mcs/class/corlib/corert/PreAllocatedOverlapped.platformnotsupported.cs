namespace System.Threading
{
	public sealed class PreAllocatedOverlapped : IDisposable
	{
		[CLSCompliant (false)]
		public unsafe PreAllocatedOverlapped (IOCompletionCallback callback, object state, object pinData)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Dispose ()
		{
		}
	}
}
