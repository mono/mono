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
// This workflow uses Paralell and Delays activities
//
//

using System;
using NUnit.Framework;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Threading;
using System.Collections.Generic;

namespace MonoTests.System.Workflow.Runtime
{
	public sealed class SequentialWorkflowParallel : SequentialWorkflowActivity
	{
		private CodeActivity CodeCloseMailProgram2;
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
		private CodeActivity CodeCloseMailProgram3;
		private DelayActivity DelayWaitForSentMail3;
		private CodeActivity PrepareMail3;
		private SequenceActivity SeqSendMail3;

		public SequentialWorkflowParallel ()
		{
			InitializeComponent ();
		}

		private void InitializeComponent ()
		{
			Console.WriteLine ("1");

			CanModifyActivities = true;
			CodeCondition codecondition1 = new CodeCondition ();
			CodeCloseMailProgram2 = new CodeActivity ();
			CodeCloseMailProgram3 = new CodeActivity ();
			DelayWaitForSentMail2 = new DelayActivity ();
			DelayWaitForSentMail3 = new DelayActivity ();
			PrepareMail2 = new CodeActivity ();
			PrepareMail3 = new CodeActivity ();
			CodeCloseMailProgram1 = new CodeActivity ();
			DelayWaitForSentMail1 = new DelayActivity ();
			CodePrepareMail1 = new CodeActivity ();
			SeqSendMail2 = new SequenceActivity ();
			SeqSendMail1 = new SequenceActivity ();
			SeqSendMail3 = new SequenceActivity ();
			TerminateFinishNoNeedToReadMail = new TerminateActivity ();
			Parallel = new ParallelActivity ();
			IfElseBranchActivityNoNeed = new IfElseBranchActivity ();
			IfElseBranchActivityNeedToSendMail = new IfElseBranchActivity ();
			NeedToSendMail = new IfElseActivity ();

			PrepareMail3.Name = "PrepareMail3";
			PrepareMail3.ExecuteCode += new EventHandler (PrepareMail3_ExecuteCode);

			CodeCloseMailProgram3.Name = "CodeCloseMailProgram3";
			CodeCloseMailProgram3.ExecuteCode += new EventHandler (CodeCloseMailProgram3_ExecuteCode);

			DelayWaitForSentMail3.Name = "DelayWaitForSentMail3";
			DelayWaitForSentMail3.TimeoutDuration = TimeSpan.Parse ("00:00:03");

			CodeCloseMailProgram2.Name = "CodeCloseMailProgram2";
			CodeCloseMailProgram2.ExecuteCode += new EventHandler (CodeCloseMailProgram2_ExecuteCode);

			DelayWaitForSentMail2.Name = "DelayWaitForSentMail2";
			DelayWaitForSentMail2.TimeoutDuration = TimeSpan.Parse ("00:00:02");

			PrepareMail2.Name = "PrepareMail2";
			PrepareMail2.ExecuteCode += new EventHandler (PrepareMail2_ExecuteCode);

			CodeCloseMailProgram1.Name = "CodeCloseMailProgram1";
			CodeCloseMailProgram1.ExecuteCode += new EventHandler (CodeCloseMailProgram_ExecuteCode);

			DelayWaitForSentMail1.Name = "DelayWaitForSentMail1";
			DelayWaitForSentMail1.TimeoutDuration = TimeSpan.Parse ("00:00:05");
			CodePrepareMail1.Name = "CodePrepareMail1";
			CodePrepareMail1.ExecuteCode += new EventHandler (CodeActivity1_ExecuteCode);

			SeqSendMail2.Activities.Add (PrepareMail2);
			SeqSendMail2.Activities.Add (DelayWaitForSentMail2);
			SeqSendMail2.Activities.Add (CodeCloseMailProgram2);
			SeqSendMail2.Name = "SeqSendMail2";

			SeqSendMail3.Activities.Add (PrepareMail3);
			SeqSendMail3.Activities.Add (DelayWaitForSentMail3);
			SeqSendMail3.Activities.Add (CodeCloseMailProgram3);
			SeqSendMail3.Name = "SeqSendMail3";

			SeqSendMail1.Activities.Add (CodePrepareMail1);
			SeqSendMail1.Activities.Add (DelayWaitForSentMail1);
			SeqSendMail1.Activities.Add (CodeCloseMailProgram1);
			SeqSendMail1.Name = "SeqSendMail1";

			TerminateFinishNoNeedToReadMail.Name = "TerminateFinishNoNeedToReadMail";

			Parallel.Activities.Add (SeqSendMail1);
			Parallel.Activities.Add (SeqSendMail2);
			Parallel.Activities.Add (SeqSendMail3);
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

		private void IfElseCondition (object sender, ConditionalEventArgs e)
		{
			WorkFlowParallelTest.Events.Add ("IfElseCondition");
			e.Result = true;
		}

		private void CodeActivity1_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add ("PrepareMail1_ExecuteCode");
		}

		private void CodeCloseMailProgram_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add  ("CodeCloseMailProgram1_ExecuteCode");
		}

		private void PrepareMail2_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add  ("PrepareMail2_ExecuteCode");
		}

		private void CodeCloseMailProgram2_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add  ("CodeCloseMailProgram2_ExecuteCode");
		}

		private void PrepareMail3_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add  ("PrepareMail3_ExecuteCode");
		}

		private void CodeCloseMailProgram3_ExecuteCode (object sender, EventArgs e)
		{
			WorkFlowParallelTest.Events.Add  ("CodeCloseMailProgram3_ExecuteCode");
		}
	}
	
	[TestFixture]
	public class WorkFlowParallelTest
	{
		static public List <string> events;
		static AutoResetEvent waitHandle = new AutoResetEvent(false);

		[Test]
		public void WorkFlowTest ()
		{
			events = new List <string> ();
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (SequentialWorkflowParallel);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			workflowRuntime.CreateWorkflow (type).Start ();

            		waitHandle.WaitOne ();
            		workflowRuntime.Dispose ();

			Assert.AreEqual ("IfElseCondition", events[0], "C1#1");
			Assert.AreEqual ("PrepareMail1_ExecuteCode", events[1], "C1#2");
			Assert.AreEqual ("PrepareMail2_ExecuteCode", events[2], "C1#3");
			Assert.AreEqual ("PrepareMail3_ExecuteCode", events[3], "C1#4");
			Assert.AreEqual ("CodeCloseMailProgram2_ExecuteCode", events[4], "C1#8");
			Assert.AreEqual ("CodeCloseMailProgram3_ExecuteCode", events[5], "C1#9");
			Assert.AreEqual ("CodeCloseMailProgram1_ExecuteCode", events[6], "C1#10");
		}

		static public List <string> Events {
			get {return events;}
		}

	        void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}
	}
}

