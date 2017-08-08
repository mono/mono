namespace System.Threading
{
	public sealed partial class Semaphore
	{
		private static void VerifyNameForCreate (string name)
		{
			throw new NotSupportedException ();
		}

		private void CreateSemaphoreCore (int initialCount, int maximumCount, string name, out bool createdNew)
		{
			throw new NotSupportedException ();
		}

		private static OpenExistingResult OpenExistingWorker (string name, out Semaphore result)
		{
			throw new NotSupportedException ();
		}

		private static int ReleaseCore (IntPtr handle, int releaseCount)
		{
			throw new NotSupportedException ();
		}
	}
}
