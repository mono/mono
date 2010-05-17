//
// CancellationToken.cs
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

using System;
using System.Threading;

#if NET_4_0 || BOOTSTRAP_NET_4_0
namespace System.Threading
{
	public struct CancellationToken
	{
		public CancellationToken (bool canceled)
			: this ()
		{
			// dummy, this is actually set by CancellationTokenSource when the token is created
			Source = null;
		}

		public static CancellationToken None {
			get {
				return CancellationTokenSource.NoneSource.Token;
			}
		}

		public CancellationTokenRegistration Register (Action callback)
		{
			return Register (callback, false);
		}

		public CancellationTokenRegistration Register (Action callback, bool useSynchronizationContext)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Source.Register (callback, useSynchronizationContext);
		}

		public CancellationTokenRegistration Register (Action<object> callback, object state)
		{
			return Register (callback, state, false);
		}

		public CancellationTokenRegistration Register (Action<object> callback, object state, bool useSynchronizationContext)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Register (() => callback (state), useSynchronizationContext);
		}

		public void ThrowIfCancellationRequested ()
		{
			Source.ThrowIfCancellationRequested ();
		}

		public bool Equals (CancellationToken other)
		{
			return this.Source == other.Source;
		}

		public override bool Equals (object obj)
		{
			return (obj is CancellationToken) ? Equals ((CancellationToken)obj) : false;
		}

		public override int GetHashCode ()
		{
			return Source.GetHashCode ();
		}

		public static bool operator == (CancellationToken lhs, CancellationToken rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator != (CancellationToken lhs, CancellationToken rhs)
		{
			return !lhs.Equals (rhs);
		}

		public bool CanBeCanceled {
			get {
				return true;
			}
		}

		public bool IsCancellationRequested {
			get {
				return Source.IsCancellationRequested;
			}
		}

		public WaitHandle WaitHandle {
			get {
				return Source.WaitHandle;
			}
		}

		internal CancellationTokenSource Source {
			get;
			set;
		}
	}
}
#endif
