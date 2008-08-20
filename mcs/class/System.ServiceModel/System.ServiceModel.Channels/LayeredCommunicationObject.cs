//
// SecurityReplyChannel.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceModel.Channels
{
	internal abstract class LayeredCommunicationObject : ICommunicationObject, IDisposable
	{
		ICommunicationObject inner;

		protected LayeredCommunicationObject (ICommunicationObject source)
		{
			inner = source;
		}

		public abstract ChannelManagerBase ChannelManager { get; }

		IDefaultCommunicationTimeouts Timeouts {
			get { return (IDefaultCommunicationTimeouts) ChannelManager; }
		}

		// ICommunicationObject

		public virtual CommunicationState State {
			get { return inner.State; }
		}

		public virtual void Abort ()
		{
			inner.Abort ();
		}

		public IAsyncResult BeginClose (AsyncCallback callback, object state)
		{
			return BeginClose (Timeouts.CloseTimeout, callback, state);
		}

		public IAsyncResult BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return OnBeginClose (timeout, callback, state);
		}

		protected virtual IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginClose (timeout, callback, state);
		}

		public virtual void EndClose (IAsyncResult result)
		{
			OnEndClose (result);
		}

		protected void OnEndClose (IAsyncResult result)
		{
			inner.EndClose (result);
		}

		public void Close ()
		{
			Close (Timeouts.CloseTimeout);
		}

		public void Close (TimeSpan timeout)
		{
			OnClose (timeout);
		}

		protected virtual void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		public void Dispose ()
		{
			Close ();
		}

		public IAsyncResult BeginOpen (AsyncCallback callback, object state)
		{
			return BeginOpen (Timeouts.OpenTimeout, callback, state);
		}

		public IAsyncResult BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return OnBeginOpen (timeout, callback, state);
		}

		protected virtual IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginOpen (timeout, callback, state);
		}

		public virtual void EndOpen (IAsyncResult result)
		{
			OnEndOpen (result);
		}

		protected virtual void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}

		public void Open ()
		{
			Open (Timeouts.OpenTimeout);
		}

		public void Open (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected virtual Type GetCommunicationObjectType ()
		{
			return GetType ();
		}

		protected void ThrowIfNotOpen ()
		{
			if (State != CommunicationState.Opened)
				throw new InvalidOperationException (String.Format ("The communication object {0} must be at opened state.", GetCommunicationObjectType ()));
		}

		protected void ThrowIfImmutable ()
		{
			if (State != CommunicationState.Created)
				throw new InvalidOperationException (String.Format ("The communication object {0} is being closed while it is not opened.", GetType ()));
		}

		public event EventHandler Closing {
			add { inner.Closing += value; }
			remove { inner.Closing -= value; }
		}

		public event EventHandler Closed {
			add { inner.Closed += value; }
			remove { inner.Closed -= value; }
		}

		public event EventHandler Opening {
			add { inner.Opening += value; }
			remove { inner.Opening -= value; }
		}

		public event EventHandler Opened {
			add { inner.Opened += value; }
			remove { inner.Opened -= value; }
		}

		public event EventHandler Faulted {
			add { inner.Faulted += value; }
			remove { inner.Faulted -= value; }
		}
	}
}
