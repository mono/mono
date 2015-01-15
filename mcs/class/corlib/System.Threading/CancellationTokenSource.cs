// 
// CancellationTokenSource.cs
//  
// Authors:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Marek Safar (marek.safar@gmail.com)
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading
{
	public class CancellationTokenSource : IDisposable
	{
		const int StateValid = 0;
		const int StateCanceled = 1 << 1;
		const int StateDisposed = 1 << 2;

		int state;
		int currId = int.MinValue;
		ConcurrentDictionary<CancellationTokenRegistration, Action> callbacks;
		CancellationTokenRegistration[] linkedTokens;

		ManualResetEvent handle;
		
		internal static readonly CancellationTokenSource NoneSource = new CancellationTokenSource ();
		internal static readonly CancellationTokenSource CanceledSource = new CancellationTokenSource ();
		
		static readonly TimerCallback timer_callback;
		Timer timer;

		static CancellationTokenSource ()
		{
			CanceledSource.state = StateCanceled;

			timer_callback = token => {
				var cts = (CancellationTokenSource) token;
				cts.CancelSafe ();
			};
		}

		public CancellationTokenSource ()
		{
			callbacks = new ConcurrentDictionary<CancellationTokenRegistration, Action> ();
			handle = new ManualResetEvent (false);
		}

		public CancellationTokenSource (int millisecondsDelay)
			: this ()
		{
			if (millisecondsDelay < -1)
				throw new ArgumentOutOfRangeException ("millisecondsDelay");

			if (millisecondsDelay != Timeout.Infinite)
				timer = new Timer (timer_callback, this, millisecondsDelay, Timeout.Infinite);
		}

		public CancellationTokenSource (TimeSpan delay)
			: this (CheckTimeout (delay))
		{
		}

		public CancellationToken Token {
			get {
				CheckDisposed ();
				return new CancellationToken (this);
			}
		}
		
		public bool IsCancellationRequested {
			get {
				return (state & StateCanceled) != 0;
			}
		}
		
		internal WaitHandle WaitHandle {
			get {
				CheckDisposed ();
				return handle;
			}
		}
		
		public void Cancel ()
		{
			Cancel (false);
		}
		
		// If parameter is true we throw exception as soon as they appear otherwise we aggregate them
		public void Cancel (bool throwOnFirstException)
		{
			CheckDisposed ();
			Cancellation (throwOnFirstException);
		}

		//
		// Don't throw ObjectDisposedException if the callback
		// is called concurrently with a Dispose
		//
		void CancelSafe ()
		{
			if (state == StateValid)
				Cancellation (true);
		}

		void Cancellation (bool throwOnFirstException)
		{
			if (Interlocked.CompareExchange (ref state, StateCanceled, StateValid) != StateValid)
				return;

			handle.Set ();

			if (linkedTokens != null)
				UnregisterLinkedTokens ();

			var cbs = callbacks;
			if (cbs == null)
				return;

			List<Exception> exceptions = null;

			try {
				Action cb;
				for (int id = currId; id != int.MinValue; id--) {
					if (!cbs.TryRemove (new CancellationTokenRegistration (id, this), out cb))
						continue;
					if (cb == null)
						continue;

					if (throwOnFirstException) {
						cb ();
					} else {
						try {
							cb ();
						} catch (Exception e) {
							if (exceptions == null)
								exceptions = new List<Exception> ();

							exceptions.Add (e);
						}
					}
				}
			} finally {
				cbs.Clear ();
			}

			if (exceptions != null)
				throw new AggregateException (exceptions);
		}

		public void CancelAfter (TimeSpan delay)
		{
			CancelAfter (CheckTimeout (delay));
		}

		public void CancelAfter (int millisecondsDelay)
		{
			if (millisecondsDelay < -1)
				throw new ArgumentOutOfRangeException ("millisecondsDelay");

			CheckDisposed ();

			if (IsCancellationRequested || millisecondsDelay == Timeout.Infinite)
				return;

			if (timer == null) {
				// Have to be carefull not to create secondary background timer
				var t = new Timer (timer_callback, this, Timeout.Infinite, Timeout.Infinite);
				if (Interlocked.CompareExchange (ref timer, t, null) != null)
					t.Dispose ();
			}

			timer.Change (millisecondsDelay, Timeout.Infinite);
		}

		public static CancellationTokenSource CreateLinkedTokenSource (CancellationToken token1, CancellationToken token2)
		{
			return CreateLinkedTokenSource (new [] { token1, token2 });
		}
		
		public static CancellationTokenSource CreateLinkedTokenSource (params CancellationToken[] tokens)
		{
			if (tokens == null)
				throw new ArgumentNullException ("tokens");

			if (tokens.Length == 0)
				throw new ArgumentException ("Empty tokens array");

			CancellationTokenSource src = new CancellationTokenSource ();
			Action action = src.CancelSafe;
			var registrations = new List<CancellationTokenRegistration> (tokens.Length);

			foreach (CancellationToken token in tokens) {
				if (token.CanBeCanceled)
					registrations.Add (token.Register (action));
			}
			src.linkedTokens = registrations.ToArray ();
			
			return src;
		}

		static int CheckTimeout (TimeSpan delay)
		{
			try {
				return checked ((int) delay.TotalMilliseconds);
			} catch (OverflowException) {
				throw new ArgumentOutOfRangeException ("delay");
			}
		}

		void CheckDisposed ()
		{
			if ((state & StateDisposed) != 0)
				throw new ObjectDisposedException (GetType ().Name);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual
		void Dispose (bool disposing)
		{
			if (disposing && (state & StateDisposed) == 0) {
				if (Interlocked.CompareExchange (ref state, StateDisposed, StateValid) == StateValid) {
					UnregisterLinkedTokens ();
					callbacks = null;
				} else {
					if (handle != null)
						handle.WaitOne ();

					state |= StateDisposed;
					Thread.MemoryBarrier ();
				}
				if (timer != null)
					timer.Dispose ();

				handle.Dispose ();
				handle = null;
			}
		}
		
		// extracted from ../../../../external/referencesource/mscorlib/system/threading/CancellationTokenSource.cs
		/// <summary>
		/// A simple helper to determine whether disposal has occured.
		/// </summary>
		internal bool IsDisposed
		{
			get { return (state & StateDisposed) != 0; }
		}

		void UnregisterLinkedTokens ()
		{
			var registrations = Interlocked.Exchange (ref linkedTokens, null);
			if (registrations == null)
				return;
			foreach (var linked in registrations)
				linked.Dispose ();
		}
		
		internal CancellationTokenRegistration Register (Action callback, bool useSynchronizationContext)
		{
			CheckDisposed ();

			var tokenReg = new CancellationTokenRegistration (Interlocked.Increment (ref currId), this);

			/* If the source is already canceled we execute the callback immediately
			 * if not, we try to add it to the queue and if it is currently being processed
			 * we try to execute it back ourselves to be sure the callback is ran
			 */
			if (IsCancellationRequested)
				callback ();
			else {
				callbacks.TryAdd (tokenReg, callback);
				if (IsCancellationRequested && callbacks.TryRemove (tokenReg, out callback))
					callback ();
			}
			
			return tokenReg;
		}

		internal void RemoveCallback (CancellationTokenRegistration reg)
		{
			// Ignore call if the source has been disposed
			if ((state & StateDisposed) != 0)
				return;
			Action dummy;
			var cbs = callbacks;
			if (cbs != null)
				cbs.TryRemove (reg, out dummy);
		}
	}
}
