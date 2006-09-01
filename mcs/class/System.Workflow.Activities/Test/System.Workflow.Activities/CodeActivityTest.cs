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
// Test Code Activity with two handlers
//

using System;
using System.Threading;
using NUnit.Framework;
using System.Security.Permissions;
using System.Workflow.Activities;
using System.Workflow.Runtime;

namespace MonoTests.System.Workflow.Activities
{
	public sealed class SimpleWorkFlow : SequentialWorkflowActivity
	{
        	public SimpleWorkFlow ()
		{
			CodeActivity ca1 = new CodeActivity ();
			ca1.ExecuteCode += new EventHandler (ExecuteCode1);
			ca1.ExecuteCode += new EventHandler (ExecuteCode2);
			Activities.Add (ca1);
		}

	        private void ExecuteCode1 (object sender, EventArgs e)
	        {
	        	CodeActivityTest.executed1 = true;
	        }

	        private void ExecuteCode2 (object sender, EventArgs e)
	        {
	        	CodeActivityTest.executed2 = true;
	        }
	}

	[TestFixture]
	public class CodeActivityTest
	{
		static public bool executed1 = false;
		static public bool executed2 = false;
		static AutoResetEvent waitHandle = new AutoResetEvent(false);

		[Test]
		public void WorkFlowTest ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (SimpleWorkFlow);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();
            		workflowRuntime.Dispose ();

			Assert.AreEqual (true, executed1, "C1#1");
			Assert.AreEqual (true, executed2, "C1#2");
		}

	        void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}
	}

}

