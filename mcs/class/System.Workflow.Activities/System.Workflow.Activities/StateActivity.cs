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
	public class StateActivity : CompositeActivity
	{
		public const string StateChangeTrackingDataKey = "StateActivity.StateChange";

		static StateActivity ()
		{

		}

		public StateActivity ()
		{

		}

		public StateActivity (string name)
		{

		}

		//public Activity GetDynamicActivity(string childActivityName);

		protected override void Initialize (IServiceProvider provider)
		{
			base.Initialize (provider);
		}

		protected override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
		{
			// If there is StateInitializationActivity should go first
			StateInitializationActivity init = null;

			foreach (Activity activity in Activities) {
				if (IsBasedOnType (activity, typeof (StateInitializationActivity))) {
					init = (StateInitializationActivity) activity;
					ActivitiesToExecute.Enqueue (init);
					break;
				}
			}

			foreach (Activity activity in Activities) {
				if (activity == init) {
					continue;
				}
				ActivitiesToExecute.Enqueue (activity);
			}

			NeedsExecution = false;
			return ActivityExecutionStatus.Executing;
		}

		// Private methods
		static private bool IsBasedOnType (object obj, Type target)
		{
			for (Type type = obj.GetType (); type != null; type = type.BaseType) {
				if (type == target) {
					return true;
				}
			}

			return false;
		}
	}
}

