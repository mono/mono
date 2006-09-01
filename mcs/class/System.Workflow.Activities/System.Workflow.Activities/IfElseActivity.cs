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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities
{
	public sealed class IfElseActivity : CompositeActivity
	{
		public IfElseActivity ()
		{

		}

	      	public IfElseActivity (string name) : base (name)
	      	{

	      	}

	      	// Methods
	      	public IfElseBranchActivity AddBranch (ICollection <Activity> activities)
	      	{
			return AddBranch (activities, null);
	      	}

	      	public IfElseBranchActivity AddBranch (ICollection <Activity> activities, ActivityCondition branchCondition)
	      	{
	      		IfElseBranchActivity branch_activity = new IfElseBranchActivity ();
	      		branch_activity.Condition = branchCondition;

	      		foreach (Activity activity in activities) {
	      			branch_activity.Activities.Add (activity);
	      		}

			return 	branch_activity;
	      	}

	      	//protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)

	      	protected override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
	      	{
	      		bool condition_true = false;

			foreach (IfElseBranchActivity activity in Activities) {

				if (activity == null || activity.Condition == null)
					continue;

				if (activity.Condition.Evaluate (this, executionContext) == true) {
					condition_true = true;
					break;
				}
			}

			if (Activities.Count > 1) {
				if (condition_true == true) {
					ActivitiesToExecute.Enqueue (Activities[0]);
				} else {
					ActivitiesToExecute.Enqueue (Activities[1]);
				}
			}

			NeedsExecution = false;
			return ActivityExecutionStatus.Closed;
	      	}
	}
}

