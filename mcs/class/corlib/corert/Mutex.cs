namespace System.Threading
{
    public sealed partial class Mutex
    {
		private static void VerifyNameForCreate (string name)
		{
			throw new NotSupportedException ();
		}

		private void CreateMutexCore (bool initiallyOwned, string name, out bool createdNew)
		{
			throw new NotSupportedException ();
		}

		private static OpenExistingResult OpenExistingWorker (string name, out Mutex result)
		{
			throw new NotSupportedException ();
		}

		private static void ReleaseMutexCore (IntPtr handle)
		{
			throw new NotSupportedException ();
		}
	}
}
