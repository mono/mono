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
// This is a State Machine workflow test
//
//

using System;
using NUnit.Framework;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoTests.Workflow.Runtime
{
	public sealed class DocumentCreation : StateMachineWorkflowActivity
	{
		private SetStateActivity DocumentSetState;
		private CodeActivity CodeDocument;
		private EventDrivenActivity CreateDriven;
		private EventDrivenActivity ProofReadEventDriven;
		private StateActivity ProofRead;
		private SetStateActivity ProofReadSetStateQualityNotOK;
		private SetStateActivity ProofReadSetStateQualityOK;
		private IfElseBranchActivity ProofReadElseBranchActivity2;
		private IfElseBranchActivity ProofReadElseBranch;
		private IfElseActivity ProofReadIfElse;
		private EventDrivenActivity PrintEventDriven;
		private StateActivity Print;
		private CodeActivity PrintCode;
		private DelayActivity PrintDelay;
		private StateActivity End;
		private SetStateActivity PrintSetState;
		private DelayActivity ProofReadDelay;
		private DelayActivity CreateDelay;
		private CodeActivity CreateInitCode;
		private StateInitializationActivity CreateInitialization;
		private CodeActivity PrintFinalizationCode;
		private StateFinalizationActivity PrintFinalization;
		private StateActivity Create;

		public DocumentCreation ()
		{
			InitializeComponent  ();
		}

		private void InitializeComponent ()
		{
			CanModifyActivities = true;
			CodeCondition codecondition1 = new CodeCondition ();
			ProofReadSetStateQualityNotOK = new SetStateActivity ();
			ProofReadSetStateQualityOK = new SetStateActivity ();
			ProofReadElseBranchActivity2 = new IfElseBranchActivity ();
			ProofReadElseBranch = new IfElseBranchActivity ();
			PrintFinalizationCode = new CodeActivity ();
			PrintSetState = new SetStateActivity ();
			PrintCode = new CodeActivity ();
			PrintDelay = new DelayActivity ();
			ProofReadIfElse = new IfElseActivity ();
			ProofReadDelay = new DelayActivity ();
			CreateInitCode = new CodeActivity ();
			DocumentSetState = new SetStateActivity ();
			CodeDocument = new CodeActivity ();
			CreateDelay = new DelayActivity ();
			PrintFinalization = new StateFinalizationActivity ();
			PrintEventDriven = new EventDrivenActivity ();
			ProofReadEventDriven = new EventDrivenActivity ();
			CreateInitialization = new StateInitializationActivity ();
			CreateDriven = new EventDrivenActivity ();
			End = new StateActivity ();
			Print = new StateActivity ();
			ProofRead = new StateActivity ();
			Create = new StateActivity ();

			// ProofReadSetStateQualityNotOK
			ProofReadSetStateQualityNotOK.Name = "ProofReadSetStateQualityNotOK";
			ProofReadSetStateQualityNotOK.TargetStateName = "Print";

			// ProofReadSetStateQualityOK
			ProofReadSetStateQualityOK.Name = "ProofReadSetStateQualityOK";
			ProofReadSetStateQualityOK.TargetStateName = "End";

			// ProofReadElseBranchActivity2
			ProofReadElseBranchActivity2.Activities.Add (ProofReadSetStateQualityNotOK);
			ProofReadElseBranchActivity2.Name = "ProofReadElseBranchActivity2";

			// ProofReadElseBranch
			ProofReadElseBranch.Activities.Add (ProofReadSetStateQualityOK);
			codecondition1.Condition += new EventHandler <ConditionalEventArgs> (ProofReadIfElseConditionFunction);
			ProofReadElseBranch.Condition = codecondition1;
			ProofReadElseBranch.Name = "ProofReadElseBranch";

			// PrintFinalizationCode
			PrintFinalizationCode.Name = "PrintFinalizationCode";
			PrintFinalizationCode.ExecuteCode += new EventHandler (PrintFinalizationCodeFunction);

			// PrintSetState
			PrintSetState.Name = "PrintSetState";
			PrintSetState.TargetStateName = "End";

			// PrintCode
			PrintCode.Name = "PrintCode";
			PrintCode.ExecuteCode += new EventHandler (PrintCodeFunction);

			// PrintDelay
			PrintDelay.Name = "PrintDelay";
			PrintDelay.TimeoutDuration = TimeSpan.Parse ("00:00:02");

			// ProofReadIfElse
			ProofReadIfElse.Activities.Add (ProofReadElseBranch);
			ProofReadIfElse.Activities.Add (ProofReadElseBranchActivity2);
			ProofReadIfElse.Description = "Quality is OK?";
			ProofReadIfElse.Name = "ProofReadIfElse";

			// ProofReadDelay
			ProofReadDelay.Name = "ProofReadDelay";
			ProofReadDelay.TimeoutDuration = TimeSpan.Parse ("00:00:01");

			// CreateInitCode
			CreateInitCode.Name = "CreateInitCode";
			CreateInitCode.ExecuteCode += new EventHandler (CreateInitCodeFunction);

			// DocumentSetState
			DocumentSetState.Name = "DocumentSetState";
			DocumentSetState.TargetStateName = "ProofRead";

			// CodeDocument
			CodeDocument.Name = "CodeDocument";
			CodeDocument.ExecuteCode += new EventHandler (CodeDocumentFunction);

			// CreateDelay
			CreateDelay.Name = "CreateDelay";
			CreateDelay.TimeoutDuration = TimeSpan.Parse ("00:00:01");

			// PrintFinalization
			PrintFinalization.Activities.Add (PrintFinalizationCode);
			PrintFinalization.Name = "PrintFinalization";

			// PrintEventDriven
			PrintEventDriven.Activities.Add (PrintDelay);
			PrintEventDriven.Activities.Add (PrintCode);
			PrintEventDriven.Activities.Add (PrintSetState);
			PrintEventDriven.Name = "PrintEventDriven";

			// ProofReadEventDriven
			ProofReadEventDriven.Activities.Add (ProofReadDelay);
			ProofReadEventDriven.Activities.Add (ProofReadIfElse);
			ProofReadEventDriven.Name = "ProofReadEventDriven";

			// CreateInitialization
			CreateInitialization.Activities.Add (CreateInitCode);
			CreateInitialization.Name = "CreateInitialization";

			// CreateDriven
			CreateDriven.Activities.Add (CreateDelay);
			CreateDriven.Activities.Add (CodeDocument);
			CreateDriven.Activities.Add (DocumentSetState);
			CreateDriven.Name = "CreateDriven";

			// End
			End.Name = "End";

			// Print
			Print.Activities.Add (PrintEventDriven);
			Print.Activities.Add (PrintFinalization);
			Print.Name = "Print";

			// ProofRead
			ProofRead.Activities.Add (ProofReadEventDriven);
			ProofRead.Name = "ProofRead";

			// Create
			Create.Activities.Add (CreateDriven);
			Create.Activities.Add (CreateInitialization);
			Create.Name = "Create";

			// DocumentCreation
			Activities.Add (Create);
			Activities.Add (ProofRead);
			Activities.Add (Print);
			Activities.Add (End);
			CompletedStateName = "End";
			InitialStateName = "Create";
			Name = "DocumentCreation";
			CanModifyActivities = false;
		}

		private void PrintCodeFunction (object sender, EventArgs e)
		{
			WorkFlowMachineStatusTest.Events.Add ("PrintCodeFunction");
		}

		private void ProofReadIfElseConditionFunction (object sender, ConditionalEventArgs e)
		{
			e.Result = false;
			WorkFlowMachineStatusTest.Events.Add ("ProofReadIfElseConditionFunction");

			Activity activity = (Activity) sender;
			while (activity.Parent != null) {
				activity = activity.Parent;
			}

			DocumentCreation doc = (DocumentCreation) activity;
			WorkFlowMachineStatusTest.Events.Add ("State:" + doc.CurrentStateName);
		}

		private void CodeDocumentFunction (object sender, EventArgs e)
		{
			WorkFlowMachineStatusTest.Events.Add ("CodeDocumentFunction");
		}

		private void CreateInitCodeFunction (object sender, EventArgs e)
		{
			WorkFlowMachineStatusTest.Events.Add ("CreateInitCodeFunction");
		}

		private void PrintFinalizationCodeFunction (object sender, EventArgs e)
		{
			WorkFlowMachineStatusTest.Events.Add ("PrintFinalizationCodeFunction");

			Activity activity = (Activity) sender;
			while (activity.Parent != null) {
				activity = activity.Parent;
			}

			DocumentCreation doc = (DocumentCreation) activity;
			WorkFlowMachineStatusTest.Events.Add ("State:" + doc.CurrentStateName);
			WorkFlowMachineStatusTest.Events.Add ("Previous state:" + doc.PreviousStateName);
		}
	}

	[TestFixture]
	public class WorkFlowMachineStatusTest
	{
		static public List <string> events;

		static public List <string> Events {
			get {return events;}
		}

		[Test]
		public void WorkFlowTest ()
		{
			events = new List <string> ();
			using (WorkflowRuntime workflowRuntime = new WorkflowRuntime ())
			{
				AutoResetEvent waitHandle = new AutoResetEvent (false);
				workflowRuntime.WorkflowCompleted += delegate (object sender, WorkflowCompletedEventArgs e) {waitHandle.Set ();};
				workflowRuntime.WorkflowTerminated += delegate (object sender, WorkflowTerminatedEventArgs e)
				{
					Console.WriteLine (e.Exception.Message);
					waitHandle.Set  ();
				};

				WorkflowInstance instance = workflowRuntime.CreateWorkflow  (typeof (DocumentCreation));
				instance.Start  ();

				waitHandle.WaitOne  ();
			}

			Assert.AreEqual ("CreateInitCodeFunction", events[0], "C1#1");
			Assert.AreEqual ("CodeDocumentFunction", events[1], "C1#2");
			Assert.AreEqual ("ProofReadIfElseConditionFunction", events[2], "C1#3");
			Assert.AreEqual ("State:ProofRead", events[3], "C1#4");
			Assert.AreEqual ("PrintCodeFunction", events[4], "C1#5");
			Assert.AreEqual ("PrintFinalizationCodeFunction", events[5], "C1#6");
			Assert.AreEqual ("State:End", events[6], "C1#7");
			Assert.AreEqual ("Previous state:Print", events[7], "C1#8");
			Assert.AreEqual (8, events.Count, "C1#9");
		}
	}
}

