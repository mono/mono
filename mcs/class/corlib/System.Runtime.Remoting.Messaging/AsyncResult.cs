// 
// System.Runtime.Remoting.Messaging/AsyncResult.cs 
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Messaging {

public class AsyncResult : IAsyncResult {

	object async_state;
	WaitHandle handle;
	object async_delegate;
	IntPtr data;
	bool sync_completed;
	bool completed;
	bool endinvoke_called;
		
	public object AsyncState
	{
		get {
			return async_state;
		}
	}

	public WaitHandle AsyncWaitHandle
	{
		get {
			return handle;
		}
	}

	public bool CompletedSynchronously
	{
		get {
			return sync_completed;
		}
	}

	public bool IsCompleted
	{
		get {
			return completed;
		}
	}
		
	public bool EndInvokeCalled
	{
		get {
			return endinvoke_called;
		}
	}
		
	public object AsyncDelegate
	{
		get {
			return async_delegate;
		}
	}

}
}
