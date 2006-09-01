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
using System.Threading;
using NUnit.Framework;
using System.Security.Permissions;
using System.Workflow.Activities;
using System.Workflow.Runtime;

namespace MonoTests.System.Workflow.Activities
{
	public class WorkFlowIfElseDoubleCondition : WorkFlowIfElse
	{
		public WorkFlowIfElseDoubleCondition ()
		{
			CodeCondition ifelse_condition2 = new CodeCondition ();
			ifelse_condition2.Condition += new EventHandler <ConditionalEventArgs> (IfElseCondition2);
			branch2.Condition = ifelse_condition2;
		}

		private void IfElseCondition2 (object sender, ConditionalEventArgs e)
		{
			e.Result = IfElseActivityTest.ifelse_condition2;
		}
	}

	public class WorkFlowIfElse : SequentialWorkflowActivity
	{
		protected IfElseBranchActivity branch2;

		public WorkFlowIfElse ()
		{
			IfElseActivity ifelse_activity = new IfElseActivity ();
			IfElseBranchActivity branch1 = new IfElseBranchActivity ();
			CodeCondition ifelse_condition1 = new CodeCondition ();
			CodeActivity code_branch1 = new CodeActivity ();
			CodeActivity code_branch2 = new CodeActivity ();
			branch2 = new IfElseBranchActivity ();

			code_branch1.Name ="Code1";
			code_branch2.Name ="Code2";
			code_branch1.ExecuteCode += new EventHandler (ExecuteCode1);
			code_branch2.ExecuteCode += new EventHandler (ExecuteCode2);

			branch1.Activities.Add (code_branch1);
			branch2.Activities.Add (code_branch2);

			ifelse_condition1.Condition += new EventHandler <ConditionalEventArgs> (IfElseCondition1);
			branch1.Condition = ifelse_condition1;

			ifelse_activity.Activities.Add (branch1);
			ifelse_activity.Activities.Add (branch2);

			Activities.Add (ifelse_activity);
		}

		private void IfElseCondition1 (object sender, ConditionalEventArgs e)
		{
			e.Result = IfElseActivityTest.ifelse_condition1;
		}

		private void ExecuteCode1 (object sender, EventArgs e)
	        {
	        	IfElseActivityTest.executed1 = true;
	        }

	        private void ExecuteCode2 (object sender, EventArgs e)
	        {
	        	IfElseActivityTest.executed2 = true;
	        }
	}

	[TestFixture]
	public class IfElseActivityTest
	{
		static public bool ifelse_condition1;
		static public bool ifelse_condition2;
		static public bool executed1;
		static public bool executed2;
		static AutoResetEvent waitHandle = new AutoResetEvent(false);

		[Test]
		public void IfElseConditionFalse ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (WorkFlowIfElse);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			ifelse_condition1 = false;
			executed1 = false;
			executed2 = false;
			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();

			Assert.AreEqual (false, executed1, "C1#1");
			Assert.AreEqual (true, executed2, "C1#2");

			workflowRuntime.Dispose ();
		}

		[Test]
		public void IfElseConditionTrue ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (WorkFlowIfElse);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			ifelse_condition1 = true;
			executed1 = false;
			executed2 = false;
			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();

			Assert.AreEqual (true, executed1, "C1#1");
			Assert.AreEqual (false, executed2, "C1#2");

			workflowRuntime.Dispose ();
		}

		[Test]
		public void IfElseConditionDoubleTrue ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (WorkFlowIfElseDoubleCondition);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			ifelse_condition1 = true;
			ifelse_condition2 = true;
			executed1 = false;
			executed2 = false;
			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();

			Assert.AreEqual (true, executed1, "C1#1");
			Assert.AreEqual (false, executed2, "C1#2");

			workflowRuntime.Dispose ();
		}

		[Test]
		public void IfElseConditionDoubleFalse ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (WorkFlowIfElseDoubleCondition);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			ifelse_condition1 = true;
			ifelse_condition2 = false;
			executed1 = false;
			executed2 = false;
			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();

			Assert.AreEqual (true, executed1, "C1#1");
			Assert.AreEqual (false, executed2, "C1#2");

			workflowRuntime.Dispose ();
		}

	        void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}
	}
}

