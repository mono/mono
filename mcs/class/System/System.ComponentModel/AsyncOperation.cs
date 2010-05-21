//
// AsyncOperation.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
//

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
#if NET_2_0
using System.Threading;

namespace System.ComponentModel
{
	public sealed class AsyncOperation
	{
		internal AsyncOperation (SynchronizationContext ctx,
					 object state)
		{
			this.ctx = ctx;
			this.state = state;

			ctx.OperationStarted ();
		}

		~AsyncOperation ()
		{
			if (!done && ctx != null)
				ctx.OperationCompleted ();
		}

		SynchronizationContext ctx;
		object state;
		bool done;

		public SynchronizationContext SynchronizationContext {
			get { return ctx; }
		}

		public object UserSuppliedState {
			get { return state; }
		}

		public void OperationCompleted ()
		{
			if (done)
				throw new InvalidOperationException ("This task is already completed. Multiple call to OperationCompleted is not allowed.");

			ctx.OperationCompleted ();

			done = true;
		}

		public void Post (SendOrPostCallback d, object arg)
		{
			if (d == null)
				throw new ArgumentNullException ("d");

			if (done)
				throw new InvalidOperationException ("This task is already completed. Multiple call to Post is not allowed.");

			ctx.Post (d, arg);
		}

		public void PostOperationCompleted (
			SendOrPostCallback d, object arg)
		{
			if (d == null)
				throw new ArgumentNullException ("d");

			if (done)
				throw new InvalidOperationException ("This task is already completed. Multiple call to PostOperationCompleted is not allowed.");

			Post (d, arg);
			OperationCompleted ();
		}
	}
}
#endif
