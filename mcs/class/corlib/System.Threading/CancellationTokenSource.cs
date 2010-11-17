// 
// CancellationTokenSource.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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

#if NET_4_0 || BOOTSTRAP_NET_4_0
using System;
using System.Collections.Generic;

namespace System.Threading
{
	
	public sealed class CancellationTokenSource : IDisposable
	{
		bool canceled;
		bool processed;
		
		int currId = int.MinValue;
		
		Dictionary<CancellationTokenRegistration, Action> callbacks
			= new Dictionary<CancellationTokenRegistration, Action> ();
		
		ManualResetEvent handle = new ManualResetEvent (false);
		
		object syncRoot = new object ();
		
		internal static readonly CancellationTokenSource NoneSource = new CancellationTokenSource ();
		
		public void Cancel ()
		{
			Cancel (false);
		}
		
		// If parameter is true we throw exception as soon as they appear otherwise we aggregate them
		public void Cancel (bool throwOnFirst)
		{
			canceled = true;
			handle.Set ();
			
			List<Exception> exceptions = null;
			if (!throwOnFirst)
				exceptions = new List<Exception> ();
			
			lock (callbacks) {
				foreach (KeyValuePair<CancellationTokenRegistration, Action> item in callbacks) {
					if (throwOnFirst) {
						item.Value ();
					} else {
						try {
							item.Value ();
						} catch (Exception e) {
							exceptions.Add (e);
						}
					}
				}
			}
			
			Thread.MemoryBarrier ();
			processed = true;
			
			if (exceptions != null && exceptions.Count > 0)
				throw new AggregateException (exceptions);
		}
		
		public void Dispose ()
		{
			
		}
		
		public static CancellationTokenSource CreateLinkedTokenSource (CancellationToken token1, CancellationToken token2)
		{
			return CreateLinkedTokenSource (new CancellationToken[] { token1, token2 });
		}
		
		public static CancellationTokenSource CreateLinkedTokenSource (params CancellationToken[] tokens)
		{
			CancellationTokenSource src = new CancellationTokenSource ();
			Action action = src.Cancel;
			
			foreach (CancellationToken token in tokens)
				token.Register (action);
			
			return src;
		}
		
		public CancellationToken Token {
			get {
				return CreateToken ();
			}
		}
		
		public bool IsCancellationRequested {
			get {
				return canceled;
			}
		}
		
		internal WaitHandle WaitHandle {
			get {
				return handle;
			}
		}
		
		internal CancellationTokenRegistration Register (Action callback, bool useSynchronizationContext)
		{
			CancellationTokenRegistration tokenReg = GetTokenReg ();
			if (canceled) {
				callback ();
			} else {
				bool temp = false;
				lock (syncRoot) {
					if (!(temp = canceled))
						callbacks.Add (tokenReg, callback);
				}
				if (temp)
					callback ();
			}
			
			return tokenReg;
		}
		
		internal void RemoveCallback (CancellationTokenRegistration tokenReg)
		{
			if (!canceled) {
				lock (syncRoot) {
					if (!canceled) {
						callbacks.Remove (tokenReg);
						return;
					}
				}
			}
			
			SpinWait sw = new SpinWait ();
			while (!processed)
				sw.SpinOnce ();
			
		}

		CancellationTokenRegistration GetTokenReg ()
		{
			CancellationTokenRegistration registration
				= new CancellationTokenRegistration (Interlocked.Increment (ref currId), this);
			
			return registration;
		}
		
		CancellationToken CreateToken ()
		{
			CancellationToken tk = new CancellationToken (canceled);
			tk.Source = this;
			
			return tk;
		}
	}
}
#endif
