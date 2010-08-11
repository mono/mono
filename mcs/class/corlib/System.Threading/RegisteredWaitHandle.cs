//
// System.Threading.RegisteredWaitHandle.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004, 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.InteropServices;

namespace System.Threading
{
	[ComVisible (true)]
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
			if (_callback != null) {
				try {
					_callback (_state, (bool)timedOut); 
				} catch {}
			}

			lock (this) 
			{
				_callsInProcess--;
				if (_unregistered && _callsInProcess == 0 && _finalEvent != null)
					NativeEventCalls.SetEvent_internal (_finalEvent.Handle);
			}
		}

		[ComVisible (true)]
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

#if ONLY_1_1
		[MonoTODO]
		~RegisteredWaitHandle() {
			// FIXME
		}
#endif
	}
}
