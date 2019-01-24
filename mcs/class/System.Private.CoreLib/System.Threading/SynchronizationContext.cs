namespace System.Threading
{
	public partial class SynchronizationContext
	{
		public static System.Threading.SynchronizationContext Current { get { throw null; } }
		public virtual System.Threading.SynchronizationContext CreateCopy() { throw null; }
		public bool IsWaitNotificationRequired() { throw null; }
		public virtual void OperationCompleted() { }
		public virtual void OperationStarted() { }
		public virtual void Post(System.Threading.SendOrPostCallback d, object state) { }
		public virtual void Send(System.Threading.SendOrPostCallback d, object state) { }
		public static void SetSynchronizationContext(System.Threading.SynchronizationContext syncContext) { }
		protected void SetWaitNotificationRequired() { }
		[System.CLSCompliantAttribute(false)]
		public virtual int Wait(System.IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) { throw null; }
		[System.CLSCompliantAttribute(false)]
		protected static int WaitHelper(System.IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) { throw null; }
	}
}