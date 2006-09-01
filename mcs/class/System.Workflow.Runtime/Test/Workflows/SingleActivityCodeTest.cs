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
// This a simple Sequential WorkFlow with a Code Activity
//

using System;
using NUnit.Framework;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Threading;

namespace MonoTests.System.Workflow.Runtime
{
	public sealed class SequentialWorkflow : SequentialWorkflowActivity
	{
		public SequentialWorkflow ()
		{
			CodeActivity code = new CodeActivity ();
			code.ExecuteCode += OnCodeExecute;
			Activities.Add (code);
		}

		private void OnCodeExecute (object sender, EventArgs e)
		{
			SingleActivityCodeTest.code_execute = true;
		}
	}

	[TestFixture]
	public class SingleActivityCodeTest
	{
		static public bool code_execute = false;
		static AutoResetEvent waitHandle = new AutoResetEvent(false);

		[Test]
		public void WorkFlowTest ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (SequentialWorkflow);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();
            		workflowRuntime.Dispose ();

			Assert.AreEqual (true, code_execute, "C1#1");
		}

	        void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}
	}
}

