namespace System.Threading
{
	partial class Mutex
	{
		void CreateMutexCore (bool initiallyOwned, string name, out bool createdNew) => throw new NotImplementedException ();

		static OpenExistingResult OpenExistingWorker (string name, out Mutex result) => throw new NotImplementedException ();

		public void ReleaseMutex ()
		{
			throw new NotImplementedException ();
		}
	}
}