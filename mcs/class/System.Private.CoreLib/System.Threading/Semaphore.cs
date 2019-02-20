namespace System.Threading
{
    partial class Semaphore
    {
		int ReleaseCore (int releaseCount) => throw new NotImplementedException ();

		static OpenExistingResult OpenExistingWorker(string name, out Semaphore result) => throw new NotImplementedException ();

		void CreateSemaphoreCore(int initialCount, int maximumCount, string name, out bool createdNew) => throw new NotImplementedException ();
    }
}