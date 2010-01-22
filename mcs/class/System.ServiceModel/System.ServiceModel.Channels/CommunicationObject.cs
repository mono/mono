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
using System.Reflection;
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

		public void Abort ()
		{
			if (State != CommunicationState.Closed) {
				OnAbort ();
				ProcessClosed ();
			}
		}

		protected void Fault ()
		{
			ProcessFaulted ();
		}

		public IAsyncResult BeginClose (AsyncCallback callback,
			object state)
		{
			return BeginClose (default_close_timeout, callback, state);
		}

		public IAsyncResult BeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (State == CommunicationState.Created)
				return new EventHandler (delegate { Abort (); }).BeginInvoke (null, null, callback, state);
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
			if (State == CommunicationState.Created)
				Abort ();
			else {
				ProcessClosing ();
				OnClose (timeout);
				ProcessClosed ();
			}
		}

		public void EndClose (IAsyncResult result)
		{
			if (State == CommunicationState.Created || State == CommunicationState.Closed) {
				if (!result.IsCompleted)
					result.AsyncWaitHandle.WaitOne ();
			} else {
				OnEndClose (result);
				ProcessClosed ();
			}
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
			lock (ThisLock) {
				if (State == CommunicationState.Faulted)
					throw new CommunicationObjectFaultedException ();
				OnClosing ();
				if (state != CommunicationState.Closing) {
					state = CommunicationState.Faulted;
					throw new InvalidOperationException (String.Format ("Communication object {0} has an overriden OnClosing method that does not call base OnClosing method (declared in {1} type).", this.GetType (), GetType ().GetMethod ("OnClosing", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType));
				}
			}
		}

		protected virtual void OnClosing ()
		{
			state = CommunicationState.Closing;
			// This means, if this method is overriden, then
			// Opening event is surpressed.
			if (Closing != null)
				Closing (this, new EventArgs ());
		}

		void ProcessClosed ()
		{
			lock (ThisLock) {
				OnClosed ();
				if (state != CommunicationState.Closed) {
					state = CommunicationState.Faulted;
					throw new InvalidOperationException (String.Format ("Communication object {0} has an overriden OnClosed method that does not call base OnClosed method (declared in {1} type).", this.GetType (), GetType ().GetMethod ("OnClosed", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType));
				}
			}
		}

		protected virtual void OnClosed ()
		{
			state = CommunicationState.Closed;
			// This means, if this method is overriden, then
			// Closed event is surpressed.
			if (Closed != null)
				Closed (this, new EventArgs ());
		}

		protected abstract void OnEndClose (IAsyncResult result);

		protected abstract void OnEndOpen (IAsyncResult result);

		void ProcessFaulted ()
		{
			lock (ThisLock) {
				if (State == CommunicationState.Faulted)
					throw new CommunicationObjectFaultedException ();
				OnFaulted ();
				if (state != CommunicationState.Faulted) {
					state = CommunicationState.Faulted; // FIXME: am not sure if this makes sense ...
					throw new InvalidOperationException (String.Format ("Communication object {0} has an overriden OnFaulted method that does not call base OnFaulted method (declared in {1} type).", this.GetType (), GetType ().GetMethod ("OnFaulted", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType));
				}
			}
		}

		protected virtual void OnFaulted ()
		{
			state = CommunicationState.Faulted;
			// This means, if this method is overriden, then
			// Faulted event is surpressed.
			if (Faulted != null)
				Faulted (this, new EventArgs ());
		}

		protected abstract void OnOpen (TimeSpan timeout);

		void ProcessOpened ()
		{
			lock (ThisLock) {
				OnOpened ();
				if (state != CommunicationState.Opened) {
					state = CommunicationState.Faulted;
					throw new InvalidOperationException (String.Format ("Communication object {0} has an overriden OnOpened method that does not call base OnOpened method (declared in {1} type).", this.GetType (), GetType ().GetMethod ("OnOpened", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType));
				}
			}
		}

		protected virtual void OnOpened ()
		{
			state = CommunicationState.Opened;
			if (Opened != null)
				Opened (this, new EventArgs ());
		}

		void ProcessOpening ()
		{
			lock (ThisLock) {
				ThrowIfDisposedOrImmutable ();
				OnOpening ();
				if (state != CommunicationState.Opening) {
					state = CommunicationState.Faulted;
					throw new InvalidOperationException (String.Format ("Communication object {0} has an overriden OnOpening method that does not call base OnOpening method (declared in {1} type).", this.GetType (), GetType ().GetMethod ("OnOpening", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType));
				}
			}
		}

		protected virtual void OnOpening ()
		{
			state = CommunicationState.Opening;
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
				throw new InvalidOperationException (String.Format ("The communication object {0} is not at created state but at {1} state.", GetType (), State));
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
