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

#if NET_4_0

using System;
using System.Threading;

namespace System.Threading.Tasks
{
	public class SynchronizationContextScheduler : TaskScheduler, IScheduler
	{
		readonly SynchronizationContext ctx;
		static readonly SendOrPostCallback callback = TaskLaunchWrapper;

		public SynchronizationContextScheduler (SynchronizationContext ctx)
		{
			this.ctx = ctx;
		}

		public void AddWork (Task t)
		{
			ctx.Post (callback, t);
		}

		static void TaskLaunchWrapper (object obj)
		{
			((Task)obj).Execute (null);
		}

		public void ParticipateUntil (Task task)
		{
			if (task.IsCompleted)
				return;

			ManualResetEventSlim evt = new ManualResetEventSlim (false);
			task.ContinueWith (_ => evt.Set (), TaskContinuationOptions.ExecuteSynchronously);
			if (evt.IsSet || task.IsCompleted)
				return;

			ParticipateUntilInternal (task, evt, -1);
		}

		public bool ParticipateUntil (Task task, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			if (task.IsCompleted)
				return false;

			bool isFromPredicate = true;
			task.ContinueWith (_ => { isFromPredicate = false; evt.Set (); }, TaskContinuationOptions.ExecuteSynchronously);

			ParticipateUntilInternal (task, evt, millisecondsTimeout);

			if (task.IsCompleted)
				return false;

			return isFromPredicate;
		}

		internal void ParticipateUntilInternal (Task self, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			evt.Wait (millisecondsTimeout);
		}

		public void PulseAll ()
		{
		}

		public void Dispose ()
		{
		}

		protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks ()
		{
			throw new System.NotImplementedException();
		}

		protected internal override void QueueTask (Task task)
		{
			AddWork (task);
		}

		protected internal override bool TryDequeue (Task task)
		{
			return false;
		}

		protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
		{
			task.Execute (null);
			return true;
		}

		public override int MaximumConcurrencyLevel {
			get {
				return base.MaximumConcurrencyLevel;
			}
		}
	}
}

#endif