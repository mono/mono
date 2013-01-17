//
// AsyncVoidMethodBuilder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System.Threading;

namespace System.Runtime.CompilerServices
{
	public struct AsyncVoidMethodBuilder
	{
		static readonly SynchronizationContext null_context = new SynchronizationContext ();

		readonly SynchronizationContext context;
		IAsyncStateMachine stateMachine;

		private AsyncVoidMethodBuilder (SynchronizationContext context)
		{
			this.context = context;
			this.stateMachine = null;
		}

		public void AwaitOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : INotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			var action = new Action (stateMachine.MoveNext);
			awaiter.OnCompleted (action);
		}
		
		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> (ref TAwaiter awaiter, ref TStateMachine stateMachine)
			where TAwaiter : ICriticalNotifyCompletion
			where TStateMachine : IAsyncStateMachine
		{
			var action = new Action (stateMachine.MoveNext);
			awaiter.UnsafeOnCompleted (action);
		}

		public static AsyncVoidMethodBuilder Create ()
		{
			var ctx = SynchronizationContext.Current ?? null_context;
			ctx.OperationStarted ();

			return new AsyncVoidMethodBuilder (ctx);
		}

		public void SetException (Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException ("exception");

			try {
				context.Post (l => { throw (Exception) l; }, exception);
			} finally {
				SetResult ();
			}
		}

		public void SetStateMachine (IAsyncStateMachine stateMachine)
		{
			if (stateMachine == null)
				throw new ArgumentNullException ("stateMachine");
			
			if (this.stateMachine != null)
				throw new InvalidOperationException ("The state machine was previously set");
			
			this.stateMachine = stateMachine;
		}

		public void SetResult ()
		{
			context.OperationCompleted ();
		}
		
		public void Start<TStateMachine> (ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			if (stateMachine == null)
				throw new ArgumentNullException ("stateMachine");
			
			stateMachine.MoveNext ();
		}
	}
}

#endif