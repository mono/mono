//
// System.Threading.RegisteredWaitHandle.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class RegisteredWaitHandle : MarshalByRefObject
	{
		WaitHandle _waitObject;
		WaitOrTimerCallback _callback;
		TimeSpan _timeout;
		object _state;
		bool _executeOnlyOnce;
		WaitHandle _finalEvent;
		ManualResetEvent _cancelEvent;
		int _callsInProcess;
		bool _unregistered;

		internal RegisteredWaitHandle (WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce)
		{
			_waitObject = waitObject;
			_callback = callback;
			_state = state;
			_timeout = timeout;
			_executeOnlyOnce = executeOnlyOnce;
			_finalEvent = null;
			_cancelEvent = new ManualResetEvent (false);
			_callsInProcess = 0;
			_unregistered = false;
		}

		internal void Wait (object state)
		{
			try
			{
				WaitHandle[] waits = new WaitHandle[] {_waitObject, _cancelEvent};
				do 
				{
					int signal = WaitHandle.WaitAny (waits, _timeout, false);
					if (!_unregistered)
					{
						lock (this) { _callsInProcess++; }
						ThreadPool.QueueUserWorkItem (new WaitCallback (DoCallBack), (signal == WaitHandle.WaitTimeout));
					}
				} 
				while (!_unregistered && !_executeOnlyOnce);
			}
			catch {}

			lock (this) {
				_unregistered = true;
				if (_callsInProcess == 0 && _finalEvent != null)
					NativeEventCalls.SetEvent_internal (_finalEvent.Handle);
			}
		}

		private void DoCallBack (object timedOut)
		{
			if (_callback != null)
				_callback (_state, (bool)timedOut); 

			lock (this) 
			{
				_callsInProcess--;
				if (_unregistered && _callsInProcess == 0 && _finalEvent != null)
					NativeEventCalls.SetEvent_internal (_finalEvent.Handle);
			}
		}

		public bool Unregister(WaitHandle waitObject) 
		{
			lock (this) 
			{
				if (_unregistered) return false;
				_finalEvent = waitObject;
				_unregistered = true;
				_cancelEvent.Set();
				return true;
			}
		}

		[MonoTODO]
		~RegisteredWaitHandle() {
			// FIXME
		}
	}
}
