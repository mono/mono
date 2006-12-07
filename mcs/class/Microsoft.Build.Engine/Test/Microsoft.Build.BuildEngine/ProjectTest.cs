//
// ProjectTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
//
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

using System;
using System.Collections;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {

	class TestLogger : Logger {
		int target_started_events = 0;
		int target_finished_events = 0;

		public override void Initialize (IEventSource eventSource)
		{
			eventSource.TargetStarted += new TargetStartedEventHandler(TargetStarted);
			eventSource.TargetFinished += new TargetFinishedEventHandler(TargetFinished);
			eventSource.MessageRaised += new BuildMessageEventHandler(Message);
			eventSource.WarningRaised += new BuildWarningEventHandler(Warning);
		}

		void TargetStarted (object sender, TargetStartedEventArgs args)
		{
			target_started_events++;
		}

		void TargetFinished (object sender, TargetFinishedEventArgs args)
		{
			target_finished_events++;
		}

		void Message (object sender, BuildMessageEventArgs args)
		{
		}
		
		void Warning (object sender, BuildWarningEventArgs args)
		{
		}

		public int TargetStartedEvents { get { return target_started_events; } }

		public int TargetFinishedEvents { get { return target_finished_events; } }
	}

	[TestFixture]
	public class ProjectTest {

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException),
		@"The default XML namespace of the project must be the MSBuild XML namespace." + 
		" If the project is authored in the MSBuild 2003 format, please add " +
		"xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" to the <Project> element. " +
		"If the project has been authored in the old 1.0 or 1.2 format, please convert it to MSBuild 2003 format.  ")]
		public void TestAssignment ()
		{
			Engine engine;
			Project project;
			string documentString =
				"<Project></Project>";
			
			engine = new Engine (Consts.BinPath);
			DateTime time = DateTime.Now;
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.BuildEnabled, "A1");
			Assert.AreEqual (String.Empty, project.DefaultTargets, "A2");
			Assert.AreEqual (String.Empty, project.FullFileName, "A3");
			Assert.AreEqual (false, project.IsDirty, "A4");
			Assert.AreEqual (false, project.IsValidated, "A5");
			Assert.AreEqual (engine, project.ParentEngine, "A6");
			Assert.IsTrue (time <= project.TimeOfLastDirty, "A7");
			Assert.IsTrue (String.Empty != project.Xml, "A8");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAddNewItemGroup ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup big = project.AddNewItemGroup ();
			Assert.IsNotNull (big, "A1");
			Assert.AreEqual (String.Empty, big.Condition, "A2");
			Assert.AreEqual (0, big.Count, "A3");
			Assert.AreEqual (false, big.IsImported, "A4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAddNewPropertyGroup ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildPropertyGroup bpg = project.AddNewPropertyGroup (false);
			Assert.IsNotNull (bpg, "A1");
			Assert.AreEqual (String.Empty, bpg.Condition, "A2");
			Assert.AreEqual (0, bpg.Count, "A3");
			Assert.AreEqual (false, bpg.IsImported, "A4");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestBuild1 ()
		{
			Engine engine;
			Project project;
			IDictionary hashtable = new Hashtable ();
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='Main'>
						<Microsoft.Build.Tasks.Message Text='Text' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.Build (new string[] { "Main" }, hashtable), "A1");
			Assert.AreEqual (1, hashtable.Count, "A2");
		}

		[Test]
		public void TestBuild2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T' Inputs='Test\resources\TestTasks.cs' Outputs='Test\resources\TestTasks.dll'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			TestLogger tl = new TestLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build ("T");
			project.Build ("T");

			Assert.AreEqual (2, tl.TargetStartedEvents, "A1");
			Assert.AreEqual (2, tl.TargetFinishedEvents, "A1");
		}

		[Test]
		public void TestBuild3 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T' Inputs='Test\resources\TestTasks.cs' Outputs='Test\resources\TestTasks.dll'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			TestLogger tl = new TestLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build (new string [1] { "T" }, null, BuildSettings.None);
			project.Build (new string [1] { "T" }, null, BuildSettings.None);

			Assert.AreEqual (2, tl.TargetStartedEvents, "A1");
			Assert.AreEqual (2, tl.TargetFinishedEvents, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestBuild4 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T' Inputs='Test\resources\TestTasks.cs' Outputs='Test\resources\TestTasks.dll'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			TestLogger tl = new TestLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);
			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);

			Assert.AreEqual (1, tl.TargetStartedEvents, "A1");
			Assert.AreEqual (1, tl.TargetFinishedEvents, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGetConditionedPropertyValues ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup Condition='true'>
						<A>A</A>
						<B Condition='true'>A</B>
					</PropertyGroup>
					<PropertyGroup>
						<C Condition='true'>A</C>
						<C Condition='false'>B</C>
						<C Condition='!false'>C</C>
						<D>A</D>
						<E Condition="" '$(C)' == 'A' "">E</E>
					</PropertyGroup>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (0, project.GetConditionedPropertyValues ("A").Length, "A1");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("B").Length, "A2");
			Assert.AreEqual (1, project.GetConditionedPropertyValues ("C").Length, "A3");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("D").Length, "A4");
			Assert.AreEqual (0, project.GetConditionedPropertyValues ("E").Length, "A5");
			Assert.AreEqual ("A", project.GetConditionedPropertyValues ("C") [0], "A6");
		}

		[Test]
		public void TestGetProjectExtensions ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<ProjectExtensions>
						<Node>Text</Node>
					</ProjectExtensions>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (String.Empty, project.GetProjectExtensions (null), "A1");
			Assert.AreEqual (String.Empty, project.GetProjectExtensions (String.Empty), "A2");
			Assert.AreEqual (String.Empty, project.GetProjectExtensions ("something"), "A3");
			Assert.AreEqual ("Text", project.GetProjectExtensions ("Node"), "A4");
		}

		[Test]
		public void TestGlobalProperties1 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (0, project.GlobalProperties.Count, "A1");
		}

		[Test]
		public void TestGlobalProperties2 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			engine.GlobalProperties.SetProperty ("Property", "Value");
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.GlobalProperties.Count, "A1");
			Assert.AreEqual ("Property", project.GlobalProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].FinalValue, "A4");
			Assert.AreEqual ("Property", project.EvaluatedProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].FinalValue, "A4");
		}

		[Test]
		[Ignore ("NullRefException under MS .NET 2.0")]
		public void TestGlobalProperties3 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.GlobalProperties = null;
		}

		[Test]
		[Ignore ("NullRefException under MS .NET 2.0")]
		public void TestGlobalProperties4 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);

			project.GlobalProperties = groups [0];
			project.GlobalProperties = project.EvaluatedProperties;
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGlobalProperties5 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);
			project.GlobalProperties = groups [0];
		}

		[Test]
		public void TestParentEngine ()
		{
			Engine engine;
			Project project;
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			Assert.AreEqual (engine, project.ParentEngine, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestResetBuildStatus ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='T' Inputs='Test\resources\TestTasks.cs' Outputs='Test\resources\TestTasks.dll'>
						<Message Text='text' />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			TestLogger tl = new TestLogger ();
			engine.RegisterLogger (tl);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.Build ("T");
			project.ResetBuildStatus ();
			project.Build (new string [1] { "T" }, null, BuildSettings.DoNotResetPreviouslyBuiltTargets);

			Assert.AreEqual (2, tl.TargetStartedEvents, "A1");
			Assert.AreEqual (2, tl.TargetFinishedEvents, "A1");
		}
		
		[Test]
		public void TestSchemaFile ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsNull (project.SchemaFile, "A1");
		}
	}
}
