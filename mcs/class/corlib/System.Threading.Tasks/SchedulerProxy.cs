// 
// SchedulerProxy.cs
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

#if NET_4_0 || MOBILE
using System;
using System.Threading;
using System.Reflection;

namespace System.Threading.Tasks
{
	
	internal class SchedulerProxy : IScheduler
	{
		TaskScheduler scheduler;

		Action<Task> participateUntil1;
		Func<Task, ManualResetEventSlim, int, bool> participateUntil2;

		public SchedulerProxy (TaskScheduler scheduler)
		{
			this.scheduler = scheduler;
			FindMonoSpecificImpl ();
		}

		void FindMonoSpecificImpl ()
		{
			Console.WriteLine (scheduler.GetType ());

			// participateUntil1
			FetchMethod<Action<Task>> ("ParticipateUntil",
			                           new[] { typeof(Task) },
			                           ref participateUntil1);
			// participateUntil2
			FetchMethod<Func<Task, ManualResetEventSlim, int, bool>> ("ParticipateUntil",
			                                                          new[] { typeof(Task), typeof(ManualResetEventSlim), typeof(int) },
			                                                          ref participateUntil2);
		}

		void FetchMethod<TDelegate> (string name, Type[] types, ref TDelegate field) where TDelegate : class
		{
			var method = scheduler.GetType ().GetMethod (name,
			                                             BindingFlags.Instance | BindingFlags.Public,
			                                             null,
			                                             types,
			                                             null);
			if (method == null)
				return;
			field = Delegate.CreateDelegate (typeof(TDelegate), scheduler, method) as TDelegate;
			Console.WriteLine ("Created delegate for " + name);
		}
		
		#region IScheduler implementation
		public void AddWork (Task t)
		{
			scheduler.QueueTask (t);
		}
		
		public void ParticipateUntil (Task task)
		{
			if (participateUntil1 != null) {
				participateUntil1 (task);
				return;
			}

			ManualResetEventSlim evt = new ManualResetEventSlim (false);
			task.ContinueWith (_ => evt.Set (), TaskContinuationOptions.ExecuteSynchronously);

			ParticipateUntil (evt, -1);
		}
		
		public bool ParticipateUntil (Task task, ManualResetEventSlim evt, int millisecondsTimeout)
		{
			if (participateUntil2 != null)
				return participateUntil2 (task, evt, millisecondsTimeout);

			bool fromPredicate = true;
			task.ContinueWith (_ => { fromPredicate = false; evt.Set (); }, TaskContinuationOptions.ExecuteSynchronously);

			ParticipateUntil (evt, millisecondsTimeout);

			return fromPredicate;
		}

		void ParticipateUntil (ManualResetEventSlim evt, int millisecondsTimeout)
		{
			evt.Wait (millisecondsTimeout);
		}

		public void PulseAll ()
		{
			
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			scheduler = null;
		}
		#endregion

	}
}
#endif
