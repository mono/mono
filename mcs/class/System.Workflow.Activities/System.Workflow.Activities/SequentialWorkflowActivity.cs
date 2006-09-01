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
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities
{
	public class SequentialWorkflowActivity : SequenceActivity
	{
		public SequentialWorkflowActivity ()
		{

		}

		public SequentialWorkflowActivity (string name) : base (name)
		{

		}

		//protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext);

		protected override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
		{
			foreach (Activity activity in Activities) {
				ActivitiesToExecute.Enqueue (activity);
			}

			NeedsExecution = false;
			return ActivityExecutionStatus.Closed;
		}

		//protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception);
		//protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity);
		//protected virtual void OnSequenceComplete(ActivityExecutionContext executionContext);
		//protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext);

	}

}

