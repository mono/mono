namespace System.Threading
{
	public sealed class PreAllocatedOverlapped : System.IDisposable
	{
		[System.CLSCompliantAttribute(false)]
		public PreAllocatedOverlapped(System.Threading.IOCompletionCallback callback, object state, object pinData) { }
		public void Dispose() { }
	}
}