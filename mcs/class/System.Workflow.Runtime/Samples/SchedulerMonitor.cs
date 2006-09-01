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
using System.Workflow.ComponentModel.Compiler;
using System.ComponentModel.Design;

class Program
{
	public class ourDefaultWorkflowSchedulerService : DefaultWorkflowSchedulerService
	{
		public ourDefaultWorkflowSchedulerService ()
		{

		}

		protected override void OnStarted ()
		{
			base.OnStarted ();
			Console.WriteLine ("*** ourDefaultWorkflowSchedulerService::ourDefaultWorkflowSchedulerService.OnStarted",
				Environment.StackTrace);
		}

		protected override void Schedule (WaitCallback callback, Guid workflowInstanceId)
		{
			Console.WriteLine ("*** ourDefaultWorkflowSchedulerService::Schedule {0} {1}",
				callback, workflowInstanceId);

			base.Schedule (callback, workflowInstanceId);
		}

      		protected override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
      		{
      			Console.WriteLine ("*** ourDefaultWorkflowSchedulerService::Schedule {0} {1} {2}",
				callback, workflowInstanceId, whenUtc);

			base.Schedule (callback, workflowInstanceId, whenUtc, timerId);
      		}

      		protected override void Stop ()
      		{
      			Console.WriteLine ("*** ourDefaultWorkflowSchedulerService::Stop");
			base.Stop ();
      		}
	}

	public class ourCodeActivity : Activity
	{
		public ourCodeActivity ()
		{

		}

		public event EventHandler ExecuteCode;

		protected override void Initialize (IServiceProvider provider)
		{
			Console.WriteLine ("***ourCodeActivity.IServiceProvider {0}",
				Environment.StackTrace);


		}

		protected sealed override ActivityExecutionStatus Execute (ActivityExecutionContext executionContext)
		{
			ActivityExecutionStatus status;
			ActivityExecutionContextManager manager = executionContext.ExecutionContextManager;
			ReadOnlyCollection <ActivityExecutionContext> contexts = manager.ExecutionContexts;

			Console.WriteLine ("***ourCodeActivity.Execute {0}", contexts.Count);

			IComparable queue_name = "our_queue";

			WorkflowQueuingService qService = executionContext.GetService<WorkflowQueuingService> ();

		    	if (!qService.Exists (queue_name)) {
		    		Console.WriteLine ("CreatingQue");
			        qService.CreateWorkflowQueue (queue_name, true);
			}

			status = base.Execute (executionContext);
			return status;
		}
	}

	public sealed  class SequentialWorkflow : SequentialWorkflowActivity
	{
		private ourCodeActivity CodeCloseMailProgram2;
		private DelayActivity DelayWaitForSentMail2;
		private CodeActivity PrepareMail2;
		private CodeActivity CodeCloseMailProgram1;
		private DelayActivity DelayWaitForSentMail1;
		private CodeActivity CodePrepareMail1;
		private SequenceActivity SeqSendMail2;
		private SequenceActivity SeqSendMail1;
		private ParallelActivity Parallel;
		private IfElseBranchActivity IfElseBranchActivityNoNeed;
		private IfElseBranchActivity IfElseBranchActivityNeedToSendMail;
		private TerminateActivity TerminateFinishNoNeedToReadMail;
		private IfElseActivity NeedToSendMail;

		public SequentialWorkflow ()
		{
			InitializeComponent ();
		}

		private void InitializeComponent ()
		{

			CanModifyActivities = true;
			CodeCondition codecondition1 = new CodeCondition ();
			CodeCloseMailProgram2 = new ourCodeActivity ();
			DelayWaitForSentMail2 = new DelayActivity ();
			PrepareMail2 = new CodeActivity ();
			CodeCloseMailProgram1 = new CodeActivity ();
			DelayWaitForSentMail1 = new DelayActivity ();
			CodePrepareMail1 = new CodeActivity ();
			SeqSendMail2 = new SequenceActivity ();
			SeqSendMail1 = new SequenceActivity ();
			TerminateFinishNoNeedToReadMail = new TerminateActivity ();
			Parallel = new ParallelActivity ();
			IfElseBranchActivityNoNeed = new IfElseBranchActivity ();
			IfElseBranchActivityNeedToSendMail = new IfElseBranchActivity ();
			NeedToSendMail = new IfElseActivity ();

			CodeCloseMailProgram2.Name = "CodeCloseMailProgram2";
			CodeCloseMailProgram2.ExecuteCode += new EventHandler (CodeCloseMailProgram2_ExecuteCode);

			DelayWaitForSentMail2.Name = "DelayWaitForSentMail2";
			DelayWaitForSentMail2.TimeoutDuration = System.TimeSpan.Parse ("00:00:05");
			DelayWaitForSentMail2.InitializeTimeoutDuration += new EventHandler (DelayWaitForSentMail2_InitializeTimeoutDuration);

			PrepareMail2.Name = "PrepareMail2";
			PrepareMail2.ExecuteCode += new EventHandler (PrepareMail2_ExecuteCode);

			CodeCloseMailProgram1.Name = "CodeCloseMailProgram1";
			CodeCloseMailProgram1.ExecuteCode += new EventHandler (CodeCloseMailProgram_ExecuteCode);

			DelayWaitForSentMail1.Name = "DelayWaitForSentMail1";
			DelayWaitForSentMail1.TimeoutDuration = System.TimeSpan.Parse ("00:00:03");
			DelayWaitForSentMail1.InitializeTimeoutDuration += new EventHandler (DelayWaitForSentMail1_InitializeTimeoutDuration);

			CodePrepareMail1.Name = "CodePrepareMail1";
			CodePrepareMail1.ExecuteCode += new EventHandler (CodeActivity1_ExecuteCode);

			SeqSendMail2.Activities.Add (PrepareMail2);
			SeqSendMail2.Activities.Add (DelayWaitForSentMail2);
			SeqSendMail2.Activities.Add (CodeCloseMailProgram2);
			SeqSendMail2.Name = "SeqSendMail2";

			SeqSendMail1.Activities.Add (CodePrepareMail1);
			//SeqSendMail1.Activities.Add (DelayWaitForSentMail1);
			SeqSendMail1.Activities.Add (CodeCloseMailProgram1);
			SeqSendMail1.Name = "SeqSendMail1";

			TerminateFinishNoNeedToReadMail.Name = "TerminateFinishNoNeedToReadMail";

			Parallel.Activities.Add (SeqSendMail1);
			Parallel.Activities.Add (SeqSendMail2);
			Parallel.Name = "Parallel";

			IfElseBranchActivityNoNeed.Activities.Add (TerminateFinishNoNeedToReadMail);
			IfElseBranchActivityNoNeed.Name = "IfElseBranchActivityNoNeed";

			IfElseBranchActivityNeedToSendMail.Activities.Add (Parallel);
			codecondition1.Condition += new EventHandler <ConditionalEventArgs>(IfElseCondition);
			IfElseBranchActivityNeedToSendMail.Condition = codecondition1;
			IfElseBranchActivityNeedToSendMail.Name = "IfElseBranchActivityNeedToSendMail";

			NeedToSendMail.Activities.Add (IfElseBranchActivityNeedToSendMail);
			NeedToSendMail.Activities.Add (IfElseBranchActivityNoNeed);
			NeedToSendMail.Name = "NeedToSendMail";

			Activities.Add (NeedToSendMail);
			Name = "IfElseParalellWorkFlow";
			CanModifyActivities = false;

		}



		// The event handler that executes on ExecuteCode event of the ApprovePO activity
		private void ExecutingCode (object sender, EventArgs e)
		{
			//Console.WriteLine ("**Executing. {0}", Environment.StackTrace);
			Console.WriteLine ("**Executing.");
		}

		// The event handler that executes on ExecuteCode event of the ApprovePO activity
		private void OnApproved(object sender, EventArgs e)
		{
			Console.WriteLine ("**Purchase Order Approved.");
		}


		// Code condition to evaluate whether to take the first branch, YesIfElseBranch
		// Since it always returns true, the first branch is always taken.
		private void IsCondition (object sender, ConditionalEventArgs e)
		{
		    	e.Result = true;
		    	//e.Result = false;
		    	Console.WriteLine ("**IsCondition called {0}", e.Result);
		}


		private void IfElseCondition (object sender, ConditionalEventArgs e)
		{
			Console.WriteLine ("IfElseCondition");
			e.Result = true;
		}

		private void CodeActivity1_ExecuteCode (object sender, EventArgs e)
		{
			Console.WriteLine ("PrepareMail1_ExecuteCode");
		}

		private void DelayWaitForSentMail2_InitializeTimeoutDuration (object sender, EventArgs e)
		{
			Console.WriteLine ("DelayWaitForSentMail2_InitializeTimeoutDuration");
		}

		private void CodeCloseMailProgram_ExecuteCode (object sender, EventArgs e)
		{
			Console.WriteLine ("CodeCloseMailProgram1_ExecuteCode");
		}

		private void PrepareMail2_ExecuteCode (object sender, EventArgs e)
		{
			Console.WriteLine ("PrepareMail2_ExecuteCode");
		}

		private void CodeCloseMailProgram2_ExecuteCode (object sender, EventArgs e)
		{
			Console.WriteLine ("CodeCloseMailProgram2_ExecuteCode");
		}

		private void DelayWaitForSentMail1_InitializeTimeoutDuration (object sender, EventArgs e)
		{
			Console.WriteLine ("DelayWaitForSentMail1_InitializeTimeoutDuration");
		}
	}

	static AutoResetEvent waitHandle = new AutoResetEvent(false);

        static void Main ()
        {
		// Create the WorkflowRuntime
            	WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

            	workflowRuntime.AddService ((object) new ourDefaultWorkflowSchedulerService ());

		workflowRuntime.StartRuntime ();
		Type type = typeof (SequentialWorkflow);

		// Listen for the workflow events
		workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;
		workflowRuntime.WorkflowTerminated += OnWorkflowTerminated;
		WorkflowInstance wi = workflowRuntime.CreateWorkflow (type);
		wi.Start ();

            	waitHandle.WaitOne ();

            	// Stop the runtime
            	Console.WriteLine("Program Complete.");
            	//workflowRuntime.Dispose ();
        }

          // This method will be called when a workflow instance is completed; since we have started only a single
        // instance we are ignoring the event args and signaling the waitHandle so the main thread can continue
        static void OnWorkflowCompleted(object sender, WorkflowCompletedEventArgs e)
        {

        	//Console.WriteLine ("-->{0}", Environment.StackTrace);
        	Console.WriteLine ("OnWorkflowCompleted");
            	waitHandle.Set();
        }

        // This method is called when the workflow terminates and does not complete
        // This should not occur in this sample; however, it is good practice to include a
        // handler for this event so the host application can manage workflows that are
        // unexpectedly terminated (e.g. unhandled workflow exception).
        // waitHandle is set so the main thread can continue
        static void OnWorkflowTerminated(object sender, WorkflowTerminatedEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
            waitHandle.Set();
        }
}

