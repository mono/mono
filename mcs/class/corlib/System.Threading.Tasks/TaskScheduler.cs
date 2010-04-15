#if NET_4_0 || BOOTSTRAP_NET_4_0
// 
// TaskScheduler.cs
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
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public abstract class TaskScheduler
	{
		static TaskScheduler defaultScheduler = new Scheduler ();
		
		[ThreadStatic]
		static TaskScheduler currentScheduler;
		
		int id;
		static int lastId = int.MinValue;
		
		public static event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;
		
		protected TaskScheduler ()
		{
			this.id = Interlocked.Increment (ref lastId);
		}

		// FIXME: Probably not correct
		public static TaskScheduler FromCurrentSynchronizationContext ()
		{
			return Current;
		}
		
		public static TaskScheduler Default  {
			get {
				return defaultScheduler;
			}
		}
		
		public static TaskScheduler Current  {
			get {
				if (currentScheduler != null)
					return currentScheduler;
				
				return defaultScheduler;
			}
			internal set {
				currentScheduler = value;
			}
		}
		
		public int Id {
			get {
				return id;
			}
		}
		
		public virtual int MaximumConcurrencyLevel {
			get {
				return Environment.ProcessorCount;
			}
		}
		
		protected abstract IEnumerable<Task> GetScheduledTasks ();
		protected internal abstract void QueueTask (Task task);
		protected internal virtual bool TryDequeue (Task task)
		{
			throw new NotSupportedException ();
		}

		internal protected bool TryExecuteTask (Task task)
		{
			throw new NotSupportedException ();
		}

		protected abstract bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued);
		
		internal UnobservedTaskExceptionEventArgs FireUnobservedEvent (AggregateException e)
		{
			UnobservedTaskExceptionEventArgs args = new UnobservedTaskExceptionEventArgs (e);
			
			EventHandler<UnobservedTaskExceptionEventArgs> temp = UnobservedTaskException;
			if (temp == null)
				return args;
			
			temp (this, args);
			
			return args;
		}
	}
}
#endif
