// TpScheduler.cs
//
// Copyright (c) 2011 Jérémie "Garuma" Laval
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
//
//

#if NET_4_0 || MOBILE
using System;
using System.Collections.Concurrent;

namespace System.Threading.Tasks
{
	internal class TpScheduler: TaskScheduler, IScheduler
	{
		static readonly WaitCallback callback = TaskExecuterCallback;

		public TpScheduler ()
			: this (Environment.ProcessorCount, ThreadPriority.Normal)
		{
		}

		public TpScheduler (int maxWorker, ThreadPriority priority)
		{
		}

		public void AddWork (Task t)
		{
			ThreadPool.QueueUserWorkItem (callback, task);
		}

		static void TaskExecuterCallback (object obj)
		{
			Task task = obj as Task;
			if (task == null)
				return;
			task.Execute (null);
		}

		public void ParticipateUntil (Task task)
		{
			if (task.Status == TaskStatus.WaitingToRun)
				task.Execute (null);

			if (task.IsCompleted)
				return;

			ParticipateUntil (task, new ManualResetEventSlim (false), -1);
		}

		public bool ParticipateUntil (Task task, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			if (task.IsCompleted)
				return false;

			bool isFromPredicate = true;
			task.ContinueWith (_ => { isFromPredicate = false; evt.Set (); }, TaskContinuationOptions.ExecuteSynchronously);

			evt.Wait (millisecondsTimeout);

			return isFromPredicate;
		}

		static bool TaskCompletedPredicate (Task self)
		{
			return self.IsCompleted;
		}

		public void PulseAll ()
		{
		}

		public void Dispose ()
		{
		}
#region Scheduler dummy stubs
		protected override System.Collections.Generic.IEnumerable<Task> GetScheduledTasks ()
		{
			throw new System.NotImplementedException();
		}

		protected internal override void QueueTask (Task task)
		{
			throw new System.NotImplementedException();
		}

		protected internal override bool TryDequeue (Task task)
		{
			throw new System.NotImplementedException();
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
#endregion
	}
}
#endif
