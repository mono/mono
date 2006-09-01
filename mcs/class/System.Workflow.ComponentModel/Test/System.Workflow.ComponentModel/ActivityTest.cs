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
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Workflow.ComponentModel;
using System.Workflow.Activities;
using System.Collections;
using System.Collections.Generic;

namespace MonoTests.System.Workflow.ComponentModel
{
	[TestFixture]
	public class ActivityTest
	{
		[Test]
		public void GetActivityByNameAndParent ()
		{
			SequentialWorkflowActivity sq = new SequentialWorkflowActivity ();
			IfElseActivity ifelse_activity = new IfElseActivity ();
			IfElseBranchActivity branch1 = new IfElseBranchActivity ();
			CodeActivity code_branch1 = new CodeActivity ();
			CodeActivity code_branch2 = new CodeActivity ();
			IfElseBranchActivity branch2 = new IfElseBranchActivity ();
			Activity activity;

			code_branch1.Name ="Code1";
			code_branch2.Name ="Code2";
			ifelse_activity.Name = "IfElse";
			sq.Name = "Root";
			branch1.Activities.Add (code_branch1);
			branch2.Activities.Add (code_branch2);
			ifelse_activity.Activities.Add (branch1);
			ifelse_activity.Activities.Add (branch2);
			sq.Activities.Add (ifelse_activity);

			// Getting Code1 activity from root
			activity = sq.GetActivityByName ("Code1", true);
			Assert.AreEqual (code_branch1, activity, "C1#1");

			activity = sq.GetActivityByName ("Code1", false);
			Assert.AreEqual (code_branch1, activity, "C1#2");

			// Getting Root activity from IfElse
			activity = ifelse_activity.GetActivityByName ("Root", true);
			Assert.AreEqual (null, activity, "C1#3");

			activity = ifelse_activity.GetActivityByName ("Root", false);
			Assert.AreEqual (sq, activity, "C1#4");

			// Getting Root activity from Code1
			activity = code_branch1.GetActivityByName ("Root", true);
			Assert.AreEqual (null, activity, "C1#5");

			activity = code_branch1.GetActivityByName ("Root", false);
			Assert.AreEqual (sq, activity, "C1#6");

			// Getting Ifelse activity from Code1
			activity = code_branch1.GetActivityByName ("IfElse", true);
			Assert.AreEqual (null, activity, "C1#7");

			activity = code_branch2.GetActivityByName ("IfElse", false);
			Assert.AreEqual (ifelse_activity, activity, "C1#8");

			// Parent checks
			Assert.AreEqual (ifelse_activity, branch1.Parent, "C1#9");
			Assert.AreEqual (ifelse_activity, branch2.Parent, "C1#10");
			Assert.AreEqual (null, sq.Parent, "C1#11");
		}

		[Test]
		public void SetGet ()
		{
			Activity activity = new Activity ();

			Assert.AreEqual ("Activity", activity.Name, "C1#1");
			Assert.AreEqual ("", activity.Description, "C1#2");
			Assert.AreEqual (true, activity.Enabled, "C1#3");
			Assert.AreEqual (activity.ExecutionResult, ActivityExecutionResult.None, "C1#4");
			Assert.AreEqual (activity.ExecutionStatus, ActivityExecutionStatus.Initialized, "C1#5");
			Assert.AreEqual ("Activity", activity.QualifiedName, "C1#6");
			Assert.AreEqual ("Activity [System.Workflow.ComponentModel.Activity]", activity.ToString (), "C1#7");
		}
	}
}

