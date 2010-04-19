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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting.Messaging {

[System.Runtime.InteropServices.ComVisible (true)]
public class AsyncResult : IAsyncResult, IMessageSink {

#pragma warning disable 169, 414, 649
	object async_state;
	WaitHandle handle;
	object async_delegate;
	IntPtr data;
	object object_data;
	bool sync_completed;
	bool completed;
	bool endinvoke_called;
	object async_callback;
	ExecutionContext current;
	ExecutionContext original;
	long add_time;
#pragma warning restore 169, 414, 649

	// not part of MonoAsyncResult...
	MonoMethodMessage call_message;
#pragma warning disable 0414
	IMessageCtrl message_ctrl;
#pragma warning restore
	IMessage reply_message;
	
	internal AsyncResult ()
	{
	}

	internal AsyncResult (WaitCallback cb, object state, bool capture_context)
	{
		async_state = state;
		async_delegate = cb;
		if (capture_context)
			current = ExecutionContext.Capture ();
	}

	public virtual object AsyncState
	{
		get {
			return async_state;
		}
	}

	public virtual WaitHandle AsyncWaitHandle {
		get {
			lock (this) {
				if (handle == null)
					handle = new ManualResetEvent (completed);

				return handle;
			}
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

	public IMessageSink NextSink {
		get {
			return null;
		}
	}

	public virtual IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
	{
		// Never called
		throw new NotSupportedException ();
	}

	public virtual IMessage GetReplyMessage()
	{
		return reply_message;
	}

	public virtual void SetMessageCtrl (IMessageCtrl mc)
	{
		message_ctrl = mc;
	}

	internal void SetCompletedSynchronously (bool completed)
	{
		sync_completed = completed;
	}

	internal IMessage EndInvoke ()
	{
		lock (this) {
			if (completed)
				return reply_message;
		}

		AsyncWaitHandle.WaitOne ();
		return reply_message;
	}

	public virtual IMessage SyncProcessMessage (IMessage msg)
	{
		reply_message = msg;

		lock (this) {
			completed = true;
			if (handle != null)
				((ManualResetEvent) AsyncWaitHandle).Set ();
		}
		
		if (async_callback != null) {
			AsyncCallback ac = (AsyncCallback) async_callback;
			ac (this);
		}

		return null;
	}
	
	internal MonoMethodMessage CallMessage
	{
		get { return call_message; }
		set { call_message = value; }
	}
}
}
