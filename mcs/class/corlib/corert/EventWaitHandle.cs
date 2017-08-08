namespace System.Threading
{
	public partial class EventWaitHandle
	{
		private static void VerifyNameForCreate (string name)
		{
			throw new NotSupportedException ();
		}

		private void CreateEventCore (bool initialState, EventResetMode mode, string name, out bool createdNew)
		{
			throw new NotSupportedException ();
		}

		private static OpenExistingResult OpenExistingWorker (string name, out EventWaitHandle result)
		{
			throw new NotSupportedException ();
		}

		private static bool ResetCore (IntPtr handle)
		{
			throw new NotSupportedException ();
		}

		private static bool SetCore (IntPtr handle)
		{
			throw new NotSupportedException ();
		}
	}
}