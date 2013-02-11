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

#if NET_4_0
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading
{
#if !NET_4_5
	sealed
#endif
	public class CancellationTokenSource : IDisposable
	{
		bool canceled;
		bool disposed;
		
		int currId = int.MinValue;
		ConcurrentDictionary<CancellationTokenRegistration, Action> callbacks;
		CancellationTokenRegistration[] linkedTokens;

		ManualResetEvent handle;
		
		internal static readonly CancellationTokenSource NoneSource = new CancellationTokenSource ();
		internal static readonly CancellationTokenSource CanceledSource = new CancellationTokenSource ();
		
#if NET_4_5
		static readonly TimerCallback timer_callback;
		Timer timer;
#endif

		static CancellationTokenSource ()
		{
			CanceledSource.canceled = true;

#if NET_4_5
			timer_callback = token => {
				var cts = (CancellationTokenSource) token;
				cts.Cancel ();
			};
#endif
		}

		public CancellationTokenSource ()
		{
			callbacks = new ConcurrentDictionary<CancellationTokenRegistration, Action> ();
			handle = new ManualResetEvent (false);
		}

#if NET_4_5
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
#endif

		public CancellationToken Token {
			get {
				CheckDisposed ();
				return new CancellationToken (this);
			}
		}
		
		public bool IsCancellationRequested {
			get {
				return canceled;
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

			if (canceled)
				return;

			Thread.MemoryBarrier ();
			canceled = true;
			
			handle.Set ();
			if (linkedTokens != null)
				UnregisterLinkedTokens ();
			
			List<Exception> exceptions = null;
			
			try {
				Action cb;
				for (int id = int.MinValue + 1; id <= currId; id++) {
					if (!callbacks.TryRemove (new CancellationTokenRegistration (id, this), out cb))
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
				callbacks.Clear ();
			}

			if (exceptions != null)
				throw new AggregateException (exceptions);
		}

		/* This is the callback registered on linked tokens
		 * so that they don't throw an ODE if the callback
		 * is called concurrently with a Dispose
		 */
		void SafeLinkedCancel ()
		{
			try {
				Cancel ();
			} catch (ObjectDisposedException) {}
		}

#if NET_4_5
		public void CancelAfter (TimeSpan delay)
		{
			CancelAfter (CheckTimeout (delay));
		}

		public void CancelAfter (int millisecondsDelay)
		{
			if (millisecondsDelay < -1)
				throw new ArgumentOutOfRangeException ("millisecondsDelay");

			CheckDisposed ();

			if (canceled || millisecondsDelay == Timeout.Infinite)
				return;

			if (timer == null) {
				// Have to be carefull not to create secondary background timer
				var t = new Timer (timer_callback, this, Timeout.Infinite, Timeout.Infinite);
				if (Interlocked.CompareExchange (ref timer, t, null) != null)
					t.Dispose ();
			}

			timer.Change (millisecondsDelay, Timeout.Infinite);
		}
#endif

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
			Action action = src.SafeLinkedCancel;
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
			if (disposed)
				throw new ObjectDisposedException (GetType ().Name);
		}

		public void Dispose ()
		{
			Dispose (true);
		}

#if NET_4_5
		protected virtual
#endif
		void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				Thread.MemoryBarrier ();
				disposed = true;

				if (!canceled) {
					Thread.MemoryBarrier ();
					UnregisterLinkedTokens ();
					callbacks = null;
				}
#if NET_4_5
				if (timer != null)
					timer.Dispose ();
#endif
				handle.Dispose ();
			}
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
			if (canceled)
				callback ();
			else {
				callbacks.TryAdd (tokenReg, callback);
				if (canceled && callbacks.TryRemove (tokenReg, out callback))
					callback ();
			}
			
			return tokenReg;
		}

		internal void RemoveCallback (CancellationTokenRegistration reg)
		{
			// Ignore call if the source has been disposed
			if (disposed)
				return;
			Action dummy;
			var cbs = callbacks;
			if (cbs != null)
				cbs.TryRemove (reg, out dummy);
		}
	}
}
#endif
