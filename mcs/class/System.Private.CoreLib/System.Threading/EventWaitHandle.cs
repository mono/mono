namespace System.Threading
{
	partial class EventWaitHandle
	{
		public bool Set () => throw new NotImplementedException ();

		public bool Reset () => throw new NotImplementedException ();

		private void CreateEventCore (bool initialState, EventResetMode mode, string name, out bool createdNew) => throw new NotImplementedException ();

		private static OpenExistingResult OpenExistingWorker(string name, out EventWaitHandle result) => throw new NotImplementedException ();
	}
}