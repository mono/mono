//
// YieldAwaitable.cs
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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,)
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_4_5

using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
	public struct YieldAwaitable
	{
		public struct YieldAwaiter : ICriticalNotifyCompletion
		{
			public bool IsCompleted {
				get {
					return false;
				}
			}

			public void OnCompleted (Action continuation)
			{
				OnCompleted (continuation, false);
			}

			public void UnsafeOnCompleted (Action continuation)
			{
				OnCompleted (continuation, true);
			}

			void OnCompleted (Action continuation, bool isUnsafe)
			{
				if (continuation == null)
					throw new ArgumentNullException ("continuation");

				var ctx = SynchronizationContext.Current;
				if (ctx != null) {
					ctx.Post (l => ((Action) l) (), continuation);
					return;
				}

				if (TaskScheduler.Current == TaskScheduler.Default) {
					//
					// Pass continuation as an argument to avoid allocating
					// hoisting class
					//
					WaitCallback callBack = l => ((Action) l) ();
					if (isUnsafe) {
						ThreadPool.UnsafeQueueUserWorkItem (callBack, continuation);
					} else {
						ThreadPool.QueueUserWorkItem (callBack, continuation);
					}
					return;
				}

				new Task (continuation).Start (TaskScheduler.Current);
			}

			public void GetResult ()
			{
			}
		}

		public YieldAwaitable.YieldAwaiter GetAwaiter ()
		{
			return new YieldAwaiter ();
		}
	}
}

#endif