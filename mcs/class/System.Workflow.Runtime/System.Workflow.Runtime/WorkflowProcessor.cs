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

//#define DEBUG_EXECUTIONLOOP

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Activities;

namespace System.Workflow.Runtime
{
	internal class WorkflowProcessor
	{
		static WorkflowProcessor ()
		{

		}

		// Workflow processor
		internal static void RunWorkflow (Object stateInfo)
		{
			Stack <Activity> stack = new Stack <Activity> ();
			WorkflowInstance wi = WorkflowRuntime.GetInstanceFromGuid ((Guid)stateInfo);
			wi.timer_subscriptions = new Timer (TimerSubscriptionsCallback, wi, 0, 1000);
			Activity activity = wi.GetWorkflowDefinition ();
			Activity next_activity;
			ActivityExecutionContextManager manager = new ActivityExecutionContextManager (wi);
			ActivityExecutionContext context;
			List <DelayActivity> waiting = new List <DelayActivity> ();
			bool wait = false;
			StateMachineWorkflowActivity state_machine = null;

		#if DEBUG_EXECUTIONLOOP
			Console.WriteLine ("Initiating thread for activity {0}", wi.GetWorkflowDefinition ());
		#endif
			context = manager.CreateExecutionContext (activity);

			// Main Workflow execution loop
			while (activity != null) {

				next_activity = null;
				if (activity.NeedsExecution) {
				#if DEBUG_EXECUTIONLOOP
					Console.WriteLine ("*** Executing {0}, parallel {1}", activity, activity.ParallelParent);
				#endif
					context.ExecuteActivity (activity);
				}

				// If this a state machine changing its statge update StateMachineWorkflowActivity
				if (state_machine != null && IsBasedOnType (activity, typeof (SetStateActivity))) {
					state_machine.SetCurrentStateName (((SetStateActivity) activity).TargetStateName);
				}


			#if DEBUG_EXECUTIONLOOP
				Console.WriteLine ("  ActivitiesToExecute.Count {0}, stack {1}, waiting {2}",
					activity.ActivitiesToExecute.Count, stack.Count, waiting.Count);
			#endif
				wait = false;

				// State machine workflow, first activity is InitialStateName
				if (IsBasedOnType (activity, typeof (StateMachineWorkflowActivity))) {
					state_machine = (StateMachineWorkflowActivity) activity;
					stack.Push (activity.GetActivityByName (state_machine.InitialStateName));
					state_machine.SetCurrentStateName (state_machine.InitialStateName);

				#if DEBUG_EXECUTIONLOOP
					Console.WriteLine ("  StateMachineWorkflowActivity, pushing {0}",
						activity.GetActivityByName (sm.InitialStateName));
				#endif
				}

				// TODO: if (IsBasedOnType (current, typeof (CompositeActivity))) {
				if (activity.GetType () == typeof (DelayActivity)) {
					if (activity.ParallelParent == null) {
						wi.WorkflowRuntime.OnWorkflowIdled (wi);
						waiting.Add ((DelayActivity) activity);
						wait = true;
					} else {
						// Continue from parent activities
						// TODO: This can be moved to the Execute method
						// of the paralell activity
						if (activity.ParallelParent.ActivitiesToExecute.Count > 0) {
							stack.Push (activity.ParallelParent);
						#if DEBUG_EXECUTIONLOOP
							Console.WriteLine ("Pushing parent {0}", activity.ParallelParent);
						#endif
							waiting.Add ((DelayActivity) activity);
						} else { // If not possible, wait for the delay
						#if DEBUG_EXECUTIONLOOP
							Console.WriteLine ("Schedule Waiting");
						#endif
							waiting.Add ((DelayActivity) activity);
							wait = true;
						}
					}
				}

				if (activity.NeedsExecution) { // ex. While
					stack.Push (activity);
				}

				if (activity.ActivitiesToExecute.Count == 0 && stack.Count == 0 && waiting.Count == 0) {
				#if DEBUG_EXECUTIONLOOP
					Console.WriteLine ("Exiting...");
				#endif
					break;
				}

				// Does it have sub-activities to run?
				// Delay is not composite, cannot have children activities
				if (wait == false) {
					if (activity.ActivitiesToExecute.Count > 0) {
						next_activity = activity.ActivitiesToExecute.Dequeue ();
					#if DEBUG_EXECUTIONLOOP
						Console.WriteLine ("Next Activity A {0}", next_activity);
					#endif
						if (activity.ActivitiesToExecute.Count > 0) {
							stack.Push (activity);
						}
					} else {
						if (stack.Count > 0) {
							next_activity = stack.Pop ();
						}

						if (next_activity != null && next_activity.NeedsExecution == false) {
							if (next_activity.ActivitiesToExecute.Count > 0) {
								next_activity = next_activity.ActivitiesToExecute.Dequeue ();
							}
						}

					#if DEBUG_EXECUTIONLOOP
						Console.WriteLine ("Next Activity B {0}", next_activity);
					#endif
					}
				}

				if (next_activity == null) {
					if (waiting.Count > 0) {
					#if DEBUG_EXECUTIONLOOP
						Console.WriteLine ("Waiting for {0} handles...", waiting.Count);
					#endif
						wi.WorkflowRuntime.OnWorkflowIdled (wi);
						DelayActivity.WaitEvent.WaitOne ();
					}
				}

				// Do we have delay activities no longer waiting?
				foreach (DelayActivity delay in waiting) {
					if (delay.Delayed == false) {
						bool flag = false;
						// Continue with the list of activities pending in the parent
						next_activity = delay.Parent;
						waiting.Remove (delay);
					#if DEBUG_EXECUTIONLOOP
						Console.WriteLine ("Delayed Parent {0}", next_activity);
					#endif
						if (next_activity.ActivitiesToExecute.Count > 0) {
							if (next_activity.ActivitiesToExecute.Count > 1)
								flag = true;

							if (next_activity != null) {
								next_activity = next_activity.ActivitiesToExecute.Dequeue ();

								if (flag == true) {
									stack.Push (delay.Parent);
								}
							}
						}
						break;
					}
				}

			#if DEBUG_EXECUTIONLOOP
				Console.WriteLine ("Next activity to process {0}", next_activity);
			#endif
				activity = next_activity;
			}
			wi.WorkflowRuntime.OnWorkflowCompleted (wi);
		}

		// This is called by the timer to process the TimeEventSubcriptionCollection
		static private void TimerSubscriptionsCallback (object state)
		{
			WorkflowInstance wi = (WorkflowInstance) state;

			if (wi.TimerEventSubscriptionCollection.Count == 0) {
				return;
			}

			if (wi.TimerEventSubscriptionCollection.Peek ().ExpiresAt > DateTime.UtcNow) {
				return;
			}

			TimerEventSubscription ti = wi.TimerEventSubscriptionCollection [0];
			// Event has arrived, send a message to the queue
			wi.EnqueueItem (ti.QueueName, ti, null, null);
			wi.TimerEventSubscriptionCollection.Remove (ti);
		}

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

