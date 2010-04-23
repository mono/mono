//
// ChangeMonitor.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc. (http://novell.com/)
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

namespace System.Runtime.Caching
{
	public abstract class ChangeMonitor : IDisposable
	{
		bool initializationComplete;
		bool notifyOnChangedCalled;
		OnChangedCallback onChangedCallback;
		object onChangedState;

		public bool HasChanged { get; private set; }
		public bool IsDisposed { get; private set; }
		public abstract string UniqueId { get; }
		
		protected ChangeMonitor ()
		{
		}

		public void Dispose ()
		{
			if (IsDisposed)
				return;

			if (!initializationComplete)
				// TODO: check if Dispose (bool) is called in this case
				throw new InvalidOperationException ("Initialization is not complete in the derived change-monitor class that called the base Dispose method.");

			try {
				try {
					InvokeOnChangedCallback (onChangedState);
				} catch {
					// ignore
					// TODO: check what happens if the callback throws an exception - is
					// Dispose (true) called then?
				}
		
				Dispose (true);
			} finally {	
				IsDisposed = true;
			}
		}

		protected abstract void Dispose (bool disposing);

		protected void InitializationComplete ()
		{
			initializationComplete = true;
			if (HasChanged)
				Dispose ();
		}

		void InvokeOnChangedCallback (object state)
		{
			if (onChangedCallback == null)
				return;

			try {
				onChangedCallback (state);
			} finally {
				onChangedCallback = null;
				onChangedState = null;
			}
		}
		
		public void NotifyOnChanged (OnChangedCallback onChangedCallback)
		{
			if (onChangedCallback == null)
				throw new ArgumentNullException ("onChangedCallback");

			if (notifyOnChangedCalled)
				throw new InvalidOperationException ("The callback method has already been invoked.");

			notifyOnChangedCalled = true;
			this.onChangedCallback = onChangedCallback;
			if (HasChanged) {
				InvokeOnChangedCallback (onChangedState);
				return;
			}
			
		}

		protected void OnChanged (object state)
		{
			HasChanged = true;
			try {
				if (onChangedCallback == null)
					onChangedState = state;
				else
					InvokeOnChangedCallback (state);
			} catch {
				// ignore
				// TODO: check what happens if callback throws an exception - is
				// Dispose below called?
			} finally {
				if (initializationComplete)
					Dispose ();
			}
		}
	}
}
