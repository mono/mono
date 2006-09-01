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
//

using System;
using System.Threading;
using System.CodeDom;
using NUnit.Framework;
using System.Security.Permissions;
using System.Workflow.Activities;
using System.Workflow.Runtime;
using System.Workflow.Activities.Rules;


namespace MonoTests.System.Workflow.Activities
{
	public class WorkFlowIfElseRule : SequentialWorkflowActivity
	{
		private IfElseBranchActivity branch2;
		private IfElseBranchActivity branch1;
		private RuleDefinitions definitions = new RuleDefinitions();

		public WorkFlowIfElseRule ()
		{
			IfElseActivity ifelse_activity = new IfElseActivity ();
			branch1 = new IfElseBranchActivity ();

			CodeActivity code_branch1 = new CodeActivity ();
			CodeActivity code_branch2 = new CodeActivity ();
			branch2 = new IfElseBranchActivity ();

			code_branch1.Name ="Code1";
			code_branch2.Name ="Code2";
			code_branch1.ExecuteCode += new EventHandler (ExecuteCode1);
			code_branch2.ExecuteCode += new EventHandler (ExecuteCode2);

			branch1.Activities.Add (code_branch1);
			branch2.Activities.Add (code_branch2);

			RuleConditionReference condition1 = new RuleConditionReference ();
			condition1.ConditionName = "Condition1";
			RuleExpressionCondition rc =  new RuleExpressionCondition ("Condition1",
				RulesTest.check_condition);

			definitions.Conditions.Add (rc);
			branch1.Condition = condition1;

			ifelse_activity.Activities.Add (branch1);
			ifelse_activity.Activities.Add (branch2);

			SetValue (RuleDefinitions.RuleDefinitionsProperty, definitions);
			Activities.Add (ifelse_activity);
		}

		private void ExecuteCode1 (object sender, EventArgs e)
	        {
	        	RulesTest.executed1 = true;
	        }

	        private void ExecuteCode2 (object sender, EventArgs e)
	        {
	        	RulesTest.executed2 = true;
	        }
	}

	[TestFixture]
	public class RulesTest
	{
		static public bool ifelse_condition1;
		static public bool ifelse_condition2;
		static public bool executed1;
		static public bool executed2;
		static public CodeBinaryOperatorExpression check_condition;
		static AutoResetEvent waitHandle = new AutoResetEvent(false);

		private void CheckRule (CodeBinaryOperatorExpression expression, bool executed, string error)
		{
			WorkflowInstance wi;
			WorkflowRuntime workflowRuntime = new WorkflowRuntime ();
			Type type = typeof (WorkFlowIfElseRule);
			workflowRuntime.WorkflowCompleted += OnWorkflowCompleted;

			executed1 = false;
			executed2 = false;
			check_condition = expression;
			wi = workflowRuntime.CreateWorkflow (type);
			wi.Start ();
            		waitHandle.WaitOne ();

            		if (executed) {
				Assert.AreEqual (true, executed1, error);
				Assert.AreEqual (false, executed2, error);
			} else {
				Assert.AreEqual (false, executed1, error);
				Assert.AreEqual (true, executed2, error);
			}

			workflowRuntime.Dispose ();
		}

		[Test]
		public void RuleGreaterThanOrEqual ()
		{
			CodeBinaryOperatorExpression check = new CodeBinaryOperatorExpression ();
			check.Operator = CodeBinaryOperatorType.GreaterThanOrEqual;

			check.Left = new CodePrimitiveExpression ((Int32) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int16) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int64) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((sbyte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((float) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((byte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32)6);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int32)5);
			RuleCheckWithDifferentTypes (check, true);
		}

		[Test]
		public void RuleGreaterThan ()
		{
			CodeBinaryOperatorExpression check = new CodeBinaryOperatorExpression ();
			check.Operator = CodeBinaryOperatorType.GreaterThan;

			check.Left = new CodePrimitiveExpression ((Int32) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int16) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int64) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((sbyte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((float) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((byte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32)6);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int32)5);
			RuleCheckWithDifferentTypes (check, false);
		}

		[Test]
		public void RuleLessThan ()
		{
			CodeBinaryOperatorExpression check = new CodeBinaryOperatorExpression ();
			check.Operator = CodeBinaryOperatorType.LessThan;

			check.Left = new CodePrimitiveExpression ((Int32) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int16) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int64) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((sbyte) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((float) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((byte) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int32) 6);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32) 5);
			RuleCheckWithDifferentTypes (check, false);
		}

		[Test]
		public void LessThanOrEqual ()
		{
			CodeBinaryOperatorExpression check = new CodeBinaryOperatorExpression ();
			check.Operator = CodeBinaryOperatorType.LessThanOrEqual;

			check.Left = new CodePrimitiveExpression ((Int32) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int16) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int64) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((sbyte) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((float) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((byte) 4);
			RuleCheckWithDifferentTypes (check, true);

			check.Left = new CodePrimitiveExpression ((Int32) 6);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32) 5);
			RuleCheckWithDifferentTypes (check, true);
		}

		[Test]
		public void ValueEquality ()
		{
			CodeBinaryOperatorExpression check = new CodeBinaryOperatorExpression ();
			check.Operator = CodeBinaryOperatorType.ValueEquality;

			check.Left = new CodePrimitiveExpression ((Int32) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int16) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int64) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((sbyte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((float) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((byte) 4);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32) 6);
			RuleCheckWithDifferentTypes (check, false);

			check.Left = new CodePrimitiveExpression ((Int32) 5);
			RuleCheckWithDifferentTypes (check, true);
		}

	        private void OnWorkflowCompleted (object sender, WorkflowCompletedEventArgs e)
        	{
          		waitHandle.Set ();
       		}

       		private void RuleCheckWithDifferentTypes (CodeBinaryOperatorExpression check, bool result)
		{
			check.Right = new CodePrimitiveExpression ((Int32) 5);
			CheckRule (check, result, "C1#");

			check.Right = new CodePrimitiveExpression ((Int16) 5);
			CheckRule (check, result, "C2#");

			check.Right = new CodePrimitiveExpression ((Int64) 5);
			CheckRule (check, result, "C3#");

			check.Right = new CodePrimitiveExpression ((sbyte) 5);
			CheckRule (check, result, "C4#");

			check.Right = new CodePrimitiveExpression ((float) 5);
			CheckRule (check, result, "C5#");

			check.Right = new CodePrimitiveExpression ((byte) 5);
			CheckRule (check, result, "C6#");
		}
	}
}

