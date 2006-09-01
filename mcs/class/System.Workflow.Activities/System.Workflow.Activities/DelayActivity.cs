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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.ComponentModel;
using System.Threading;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities
{
	public sealed class DelayActivity : Activity
	//IEventActivity, IActivityEventListener<QueueEventArgs>
	{
		static public readonly DependencyProperty InitializeTimeoutDurationEvent;
		static public readonly DependencyProperty TimeoutDurationProperty;
		static private AutoResetEvent reset_event = new AutoResetEvent (false);
		private TimeSpan timeout;
		private bool delayed = false;

		public DelayActivity ()
		{

		}

		public DelayActivity (string name) : base (name)
		{

		}

		// Event
		public event EventHandler InitializeTimeoutDuration;

		// Properties
		public TimeSpan TimeoutDuration {
			get {return timeout;}
			set {timeout = value;}
		}

		// Private Properties
		static public AutoResetEvent WaitEvent {
			get {return reset_event;}
		}

		public bool Delayed {
			get {
				return delayed;
			}
		}

		// Methods
		//protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext);

		protected override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
		{
			DateTime expiresAt = DateTime.UtcNow;

			if (InitializeTimeoutDuration != null) {
				InitializeTimeoutDuration (this, new  EventArgs ());
			}

			expiresAt += TimeoutDuration;
			delayed = true;
			SetTimer (executionContext, expiresAt);

			NeedsExecution = false;
			return ActivityExecutionStatus.Executing;
		}

		protected override void OnQueueTimerItemArrived (Object sender, object args)
		{
			delayed = false;
			//Console.WriteLine ("Timer arrived!");
			reset_event.Set ();
		}

		//protected sealed override ActivityExecutionStatus HandleFault (ActivityExecutionContext executionContext, Exception exception)
		//protected override void Initialize (IServiceProvider provider)
		//protected override void OnClosed(IServiceProvider provider);
	}

}

