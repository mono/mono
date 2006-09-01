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
// This a simple Sequential WorkFlow with a Delay, While and Code activities
//
//

using System;
using NUnit.Framework;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Threading;

namespace MonoTests.System.Workflow.Runtime
{

	public sealed class SimpleWorkFlowDelay : SequentialWorkflowActivity
	{
		private WhileActivity WhileFilesToBackup;
        	private CodeActivity BackUpFile;
        	private DelayActivity DelaySystemReady;

        	public SimpleWorkFlowDelay ()
		{
			CanModifyActivities = true;
			CodeCondition codecondition1 = new CodeCondition ();
			BackUpFile = new CodeActivity ();
			WhileFilesToBackup = new WhileActivity ();
			DelaySystemReady = new DelayActivity ();

			BackUpFile.ExecuteCode += new EventHandler (BackUpFile_ExecuteCode);

			WhileFilesToBackup.Activities.Add(BackUpFile);
			codecondition1.Condition += new EventHandler <ConditionalEventArgs>(MoreFiles);

			WhileFilesToBackup.Condition =  codecondition1;

			DelaySystemReady.TimeoutDuration = TimeSpan.Parse ("00:00:02");

			Activities.Add (DelaySystemReady);
			Activities.Add (WhileFilesToBackup);
			Name = "SimpleWorkFlowDelay";
			CanModifyActivities = false;
		}

	        private void MoreFiles (object sender, ConditionalEventArgs e)
	        {
	        	SimpleWorkFlowDelayTest.files_counted++;
	        	if (SimpleWorkFlowDelayTest.files_counted < 3) {
				e.Result = true;
			}
			else {
				e.Result = false;
			}
	        }

	        private void BackUpFile_ExecuteCode (object sender, EventArgs e)
	        {
			SimpleWorkFlowDelayTest.backup_executed++;
	        }
	}

	[TestFixture]
	public class SimpleWorkFlowDelayTest
	{
		static public int files_counted = 0;
		static public int backup_executed = 0;
		static AutoResetEvent waitHandle = new AutoResetEvent (false);

		[Test]
		public void WorkFlowTest ()
		{
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();

			Type type = typeof (SimpleWorkFlowDelay);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			workflowRuntime.CreateWorkflow (type).Start ();
            		waitHandle.WaitOne ();
            		workflowRuntime.Dispose ();

			Assert.AreEqual (3, files_counted, "C1#1");
			Assert.AreEqual (3, backup_executed, "C1#2");
		}

	        void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}
	}
}

