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
//
//	This sample should be executed on MS .Net runtime.
//	It helps to understand how the MS. Net runtime workflow
//	executor works internally, how interacts with the sheduler service
//	and so on
//

using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Threading;


class Program
{

	public class ourCodeActivity : Activity
	{
		public WorkflowQueue workflowQueue;

		public ourCodeActivity ()
		{

		}

		public event EventHandler ExecuteCode;

		protected override void Initialize (IServiceProvider provider)
		{
			Console.WriteLine ("***ourCodeActivity.Initialize thread:{0}",
				Thread.CurrentThread.ManagedThreadId);

            		WorkflowQueuingService queuingService = (WorkflowQueuingService)provider.GetService (typeof(WorkflowQueuingService));

            		IComparable queue_name = "LaNostra_Queue";
			WorkflowQueuingService qService = (WorkflowQueuingService) provider.GetService (typeof (WorkflowQueuingService));

		    	if (!qService.Exists (queue_name)) {
			        workflowQueue = qService.CreateWorkflowQueue (queue_name, true);
			}
			else
				workflowQueue = qService.GetWorkflowQueue (queue_name);

            		workflowQueue.QueueItemAvailable += OnQueueItemAvailable;
            		workflowQueue.QueueItemArrived += OnQueueItemArrived;
		}

		protected sealed override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
		{
			Console.WriteLine ("***ourCodeActivity.Execute thread:{0}",
				Thread.CurrentThread.ManagedThreadId);

			ActivityExecutionStatus status;
			//ActivityExecutionContextManager manager = executionContext.ExecutionContextManager;
			//ReadOnlyCollection <ActivityExecutionContext> contexts = manager.ExecutionContexts;

			//Console.WriteLine ("***ourCodeActivity.Execute {0}", contexts.Count);
			object data = workflowQueue.Peek ();
			Console.WriteLine ("OnQueueItemAvailable! {0}", data);

			status = base.Execute (executionContext);
			return status;
		}

		public void OnQueueItemArrived (Object sender, QueueEventArgs args)
		{
			object data = this.workflowQueue.Peek ();
			Console.WriteLine ("OnQueueItemArrived!  event {0}", data);
		}

		public void OnQueueItemAvailable (Object sender, QueueEventArgs args)
		{
			Console.WriteLine ("OnQueueItemAvailable!");
			//ThreadMonitor.WriteToConsole (Thread.CurrentThread, "WaitForMessageActivity",
			//	"WaitForMessageActivity: Processed External Event");

			object data = this.workflowQueue.Peek ();
			Console.WriteLine ("OnQueueItemAvailable event! {0}", data);

			ActivityExecutionContext context = sender as ActivityExecutionContext;
			//context.CloseActivity ();
		}
	}

	public sealed class SequentialWorkflow : SequentialWorkflowActivity
	{
		private ourCodeActivity activity;

		public SequentialWorkflow ()
		{
			InitializeComponent ();
		}

		private void InitializeComponent ()
		{
			CanModifyActivities = true;
			activity = new ourCodeActivity ();

			activity.Name = "activity";
			activity.ExecuteCode += new EventHandler (activity_ExecuteCode);

			Activities.Add (activity);
			CanModifyActivities = false;
		}

		private void activity_ExecuteCode (object sender, EventArgs e)
		{
			Console.WriteLine ("activity_ExecuteCode");
		}
	}

	static AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void Main ()
        {
		// Create the WorkflowRuntime
            	WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

            	Console.WriteLine ("App.Main thread:{0}", Thread.CurrentThread.ManagedThreadId);

		workflowRuntime.StartRuntime ();
		Type type = typeof (SequentialWorkflow);

		// Listen for the workflow events
		workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;
		workflowRuntime.WorkflowTerminated += OnWorkflowTerminated;
		WorkflowInstance wi = workflowRuntime.CreateWorkflow (type);
		wi.Start ();

		Console.WriteLine ("Enquing data");
		wi.EnqueueItem ("LaNostra_Queue", "Hello", null, null);

            	waitHandle.WaitOne ();

            	// Stop the runtime
            	Console.WriteLine ("Program Complete.");
            	//workflowRuntime.Dispose ();
        }

        static void OnWorkflowCompleted(object sender, WorkflowCompletedEventArgs e)
        {
        	Console.WriteLine ("OnWorkflowCompleted");
            	waitHandle.Set ();
        }

        static void OnWorkflowTerminated(object sender, WorkflowTerminatedEventArgs e)
        {
            Console.WriteLine (e.Exception.Message);
            waitHandle.Set ();
        }
}

