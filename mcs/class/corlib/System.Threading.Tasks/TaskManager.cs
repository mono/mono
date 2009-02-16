#if NET_4_0
// TaskManager.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

using System;
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public class TaskManager: IDisposable
	{
		static TaskManager tdefault = new TaskManager();
		static TaskManager tcurrent = tdefault;
		
		TaskManagerPolicy policy;
		
		IScheduler        sched;
		
		public TaskManager(): this(new TaskManagerPolicy())
		{
		}
		
		public TaskManager(TaskManagerPolicy policy): 
			this(policy, new Scheduler(policy.IdealProcessors * policy.IdealThreadsPerProcessor,
			                           policy.MaxStackSize, policy.ThreadPriority))
		{
		}
		
		internal TaskManager(TaskManagerPolicy policy, IScheduler sched)
		{
			this.policy = policy;
			this.sched = sched;
		}
		
		~TaskManager()
		{
			Dispose(false);
		}
		
		public void Dispose()
		{
			Dispose(true);
		}
		
		protected virtual void Dispose(bool managedRes)
		{
			if (managedRes)
				sched.Dispose();
		}
		
		internal void AddWork(Task t)
		{
			sched.AddWork(t);
		}
		
		internal void WaitForTask(Task task)
		{
			sched.ParticipateUntil(task);
		}
		
		internal bool WaitForTaskWithPredicate(Task task, Func<bool> predicate)
		{
			return sched.ParticipateUntil(task, predicate);
		}
		
		internal void WaitForTasksUntil(Func<bool> predicate)
		{
			sched.ParticipateUntil(predicate);
		}
		
		public TaskManagerPolicy Policy {
			get {
				return policy;
			}
		}

		public static TaskManager Default {
			get {
				return tdefault;
			}
		}

		public static TaskManager Current {
			get {
				return tcurrent;
			}
			set {
				if (value != null)
					tcurrent = value;	
			}
		}
		
		// May be used if we want to feed a big bunch of Tasks without waking up Workers
		/*internal bool IsInhibited {
			set {
				if (value)
					sched.InhibitPulse();
				else {
					sched.UnInhibitPulse();
					sched.PulseAll();
				}
			}
		}*/
	}
}
#endif
