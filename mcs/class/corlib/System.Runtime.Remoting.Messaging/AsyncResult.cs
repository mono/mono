// 
// System.Runtime.Remoting.Messaging/AsyncResult.cs 
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Messaging {

public class AsyncResult : IAsyncResult, IMessageSink {

	object async_state;
	WaitHandle handle;
	object async_delegate;
	IntPtr data;
	bool sync_completed;
	bool completed;
	bool endinvoke_called;
		
	public virtual object AsyncState
	{
		get {
			return async_state;
		}
	}

	public virtual WaitHandle AsyncWaitHandle
	{
		get {
			return handle;
		}
	}

	public virtual bool CompletedSynchronously
	{
		get {
			return sync_completed;
		}
	}

	public virtual bool IsCompleted
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
		set {
			endinvoke_called = value;
		}
	}
		
	public virtual object AsyncDelegate
	{
		get {
			return async_delegate;
		}
	}

	[MonoTODO]
	public IMessageSink NextSink {
		get {
			throw new NotImplementedException ();
		}
	}

	[MonoTODO]
	public virtual IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual IMessage GetReplyMessage()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual void SetMessageCtrl (IMessageCtrl mc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual IMessage SyncProcessMessage (IMessage msg)
	{
		throw new NotImplementedException ();
	}
}
}
