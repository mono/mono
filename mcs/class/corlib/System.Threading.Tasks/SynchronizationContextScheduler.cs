// 
// SynchronizationContextScheduler.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Novell
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

#if NET_4_0 || MOBILE

using System;
using System.Threading;

namespace System.Threading.Tasks
{
	public class SynchronizationContextScheduler : TaskScheduler
	{
		readonly SynchronizationContext ctx;
		static readonly SendOrPostCallback callback = TaskLaunchWrapper;

		public SynchronizationContextScheduler (SynchronizationContext ctx)
		{
			this.ctx = ctx;
		}

		protected internal override void QueueTask (Task task)
		{
			ctx.Post (callback, task);
		}

		static void TaskLaunchWrapper (object obj)
		{
			((Task)obj).Execute (null);
		}

		protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks ()
		{
			throw new System.NotImplementedException();
		}

		protected internal override bool TryDequeue (Task task)
		{
			return false;
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			if (task.IsCompleted)
				return false;

			if (task.Status == TaskStatus.WaitingToRun) {
				task.Execute (null);
				return true;
			}

			return false;
		}

		public override int MaximumConcurrencyLevel {
			get {
				return base.MaximumConcurrencyLevel;
			}
		}
	}
}

#endif
