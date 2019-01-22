using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Threading
{
	public sealed class Timer : System.MarshalByRefObject, System.IDisposable
	{
		public Timer(System.Threading.TimerCallback callback) { }
		public Timer(System.Threading.TimerCallback callback, object state, int dueTime, int period) { }
		public Timer(System.Threading.TimerCallback callback, object state, long dueTime, long period) { }
		public Timer(System.Threading.TimerCallback callback, object state, System.TimeSpan dueTime, System.TimeSpan period) { }
		[System.CLSCompliantAttribute(false)]
		public Timer(System.Threading.TimerCallback callback, object state, uint dueTime, uint period) { }
		public bool Change(int dueTime, int period) { throw null; }
		public bool Change(long dueTime, long period) { throw null; }
		public bool Change(System.TimeSpan dueTime, System.TimeSpan period) { throw null; }
		[System.CLSCompliantAttribute(false)]
		public bool Change(uint dueTime, uint period) { throw null; }
		public void Dispose() { }
		public bool Dispose(System.Threading.WaitHandle notifyObject) { throw null; }
		public System.Threading.Tasks.ValueTask DisposeAsync() { throw null; }
	}

	internal class TimerQueueTimer
	{
		public TimerQueueTimer (TimerCallback timerCallback, object state, uint dueTime, uint period, bool flowExecutionContext)
		{
		}

		internal bool Change(uint dueTime, uint period)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
		}
	}
}

namespace System.Reflection
{
	abstract class RuntimeModule : Module
	{
	}
}
