//
// CommunicationObject.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel;
using System.Threading;

namespace System.ServiceModel.Channels
{
	public abstract class CommunicationObject : ICommunicationObject
	{
		object mutex;
		CommunicationState state = CommunicationState.Created;
		TimeSpan default_open_timeout = TimeSpan.FromMinutes (1), default_close_timeout = TimeSpan.FromMinutes (1);
		bool aborted;

		protected CommunicationObject ()
			: this (new object ())
		{
		}

		protected CommunicationObject (object mutex)
		{
			this.mutex = mutex;
		}

		#region Events

		public event EventHandler Closed;

		public event EventHandler Closing;

		public event EventHandler Faulted;

		public event EventHandler Opened;

		public event EventHandler Opening;

		#endregion

		#region Properties

		public CommunicationState State {
			get { return state; }
		}

		protected bool IsDisposed {
			get { return state == CommunicationState.Closed; }
		}

		protected object ThisLock {
			get { return mutex; }
		}

		protected internal abstract TimeSpan DefaultCloseTimeout { get; }

		protected internal abstract TimeSpan DefaultOpenTimeout { get; }

		#endregion

		#region Methods

		[MonoTODO]
		public void Abort ()
		{
			OnAbort ();
		}

		[MonoTODO]
		protected void Fault ()
		{
			state = CommunicationState.Faulted;
			OnFaulted ();
		}

		public IAsyncResult BeginClose (AsyncCallback callback,
			object state)
		{
			return BeginClose (default_close_timeout, callback, state);
		}

		public IAsyncResult BeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			ProcessClosing ();
			return OnBeginClose (timeout, callback, state);
		}

		public IAsyncResult BeginOpen (AsyncCallback callback,
			object state)
		{
			return BeginOpen (default_open_timeout, callback, state);
		}

		public IAsyncResult BeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			ProcessOpening ();
			return OnBeginOpen (timeout, callback, state);
		}

		public void Close ()
		{
			Close (default_close_timeout);
		}

		public void Close (TimeSpan timeout)
		{
			ProcessClosing ();
			OnClose (timeout);
			ProcessClosed ();
		}

		public void EndClose (IAsyncResult result)
		{
			OnEndClose (result);
			ProcessClosed ();
		}

		public void EndOpen (IAsyncResult result)
		{
			OnEndOpen (result);
			ProcessOpened ();
		}

		public void Open ()
		{
			Open (default_open_timeout);
		}

		public void Open (TimeSpan timeout)
		{
			ProcessOpening ();
			OnOpen (timeout);
			ProcessOpened ();
		}

		protected abstract void OnAbort ();

		protected abstract IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state);

		protected abstract IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state);

		protected abstract void OnClose (TimeSpan timeout);

		void ProcessClosing ()
		{
			if (State == CommunicationState.Faulted)
				throw new CommunicationObjectFaultedException ();
			state = CommunicationState.Closing;
			OnClosing ();
		}

		protected virtual void OnClosing ()
		{
			// This means, if this method is overriden, then
			// Opening event is surpressed.
			if (Closing != null)
				Closing (this, new EventArgs ());
		}

		void ProcessClosed ()
		{
			state = CommunicationState.Closed;
			OnClosed ();
		}

		protected virtual void OnClosed ()
		{
			// This means, if this method is overriden, then
			// Closed event is surpressed.
			if (Closed != null)
				Closed (this, new EventArgs ());
		}

		protected abstract void OnEndClose (IAsyncResult result);

		protected abstract void OnEndOpen (IAsyncResult result);

		[MonoTODO]
		protected virtual void OnFaulted ()
		{
			// This means, if this method is overriden, then
			// Opened event is surpressed.
			if (Faulted != null)
				Faulted (this, new EventArgs ());
		}

		protected abstract void OnOpen (TimeSpan timeout);

		void ProcessOpened ()
		{
			state = CommunicationState.Opened;
			OnOpened ();
		}

		protected virtual void OnOpened ()
		{
			// This means, if this method is overriden, then
			// Opened event is surpressed.
			if (Opened != null)
				Opened (this, new EventArgs ());
		}

		void ProcessOpening ()
		{
			ThrowIfDisposedOrImmutable ();
			state = CommunicationState.Opening;
			OnOpening ();
		}

		protected virtual void OnOpening ()
		{
			// This means, if this method is overriden, then
			// Opening event is surpressed.
			if (Opening != null)
				Opening (this, new EventArgs ());
		}

		protected void ThrowIfDisposed ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException (String.Format ("This communication object {0} is already disposed.", GetCommunicationObjectType ()));
		}

		protected void ThrowIfDisposedOrNotOpen ()
		{
			ThrowIfDisposed ();
			if (State == CommunicationState.Faulted)
				throw new CommunicationObjectFaultedException ();
			if (State != CommunicationState.Opened)
				throw new InvalidOperationException (String.Format ("The communication object {0} must be at opened state.", GetCommunicationObjectType ()));
		}

		protected void ThrowIfDisposedOrImmutable ()
		{
			ThrowIfDisposed ();
			// hmm, according to msdn, Closing is OK here.
			switch (State) {
			case CommunicationState.Faulted:
				throw new CommunicationObjectFaultedException ();
			case CommunicationState.Opening:
			case CommunicationState.Opened:
				throw new InvalidOperationException (String.Format ("The communication object {0} is not at created state.", GetType ()));
			}
		}

		protected virtual Type GetCommunicationObjectType ()
		{
			return GetType ();
		}

		#endregion


		class SimpleAsyncResult : IAsyncResult
		{
			CommunicationState comm_state;
			object async_state;

			public SimpleAsyncResult (
				CommunicationState communicationState,
				TimeSpan timeout, AsyncCallback callback,
				object asyncState)
			{
				comm_state = communicationState;
				async_state = asyncState;
			}

			public object AsyncState {
				get { return async_state; }
			}

			// FIXME: implement
			public WaitHandle AsyncWaitHandle {
				get { throw new NotImplementedException (); }
			}

			// FIXME: implement
			public bool CompletedSynchronously {
				get { throw new NotImplementedException (); }
			}

			// FIXME: implement
			public bool IsCompleted {
				get { throw new NotImplementedException (); }
			}
		}
	}
}
