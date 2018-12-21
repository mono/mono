namespace System.Threading
{
	partial class Thread
	{
		public ExecutionContext ExecutionContext {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public SynchronizationContext SynchronizationContext {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}
	}
}