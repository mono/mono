namespace System.Threading
{
	partial class TimerQueue
	{
		private TimerQueue (int id)
		{
		}

		static int TickCount {
			get {
				throw new NotImplementedException ();
			}
		}

		bool SetTimer (uint actualDuration)
		{
			throw new NotImplementedException ();
		}
	}
}