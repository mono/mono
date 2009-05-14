//
// EngineTest.cs:
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {

	class CheckUnregisterLogger : Logger {
		bool anything = false;

		public override void Initialize (IEventSource eventSource)
		{
			eventSource.AnyEventRaised += delegate { anything = true; };
			eventSource.BuildFinished += delegate { anything = true; };
			eventSource.BuildStarted += delegate { anything = true; };
			eventSource.CustomEventRaised += delegate { anything = true; };
			eventSource.ErrorRaised += delegate { anything = true; };
			eventSource.MessageRaised += delegate { anything = true; };
			eventSource.ProjectFinished += delegate { anything = true; };
			eventSource.ProjectStarted += delegate { anything = true; };
			eventSource.StatusEventRaised += delegate { anything = true; };
			eventSource.TargetFinished += delegate { anything = true; };
			eventSource.TargetStarted += delegate { anything = true; };
			eventSource.TaskFinished += delegate { anything = true; };
			eventSource.TaskStarted += delegate { anything = true; };
			eventSource.WarningRaised += delegate { anything = true; };
		}

		public bool Anything { get { return anything; } }
	}

	[TestFixture]
	public class EngineTest {

		Engine engine;

		static string GetPropValue (BuildPropertyGroup bpg, string name)
		{
			foreach (BuildProperty bp in bpg) {
				if (bp.Name == name) {
					return bp.FinalValue;
				}
			}
			return String.Empty;
		}

		[Test]
		public void TestCtor ()
		{
			engine = new Engine (Consts.BinPath);
		}

		// Before a project can be instantiated, Engine.BinPath must be set to the location on disk where MSBuild is installed.
		// This is used to evaluate $(MSBuildBinPath).
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestNewProject ()
		{
			engine = new Engine ();

			engine.CreateNewProject ();
		}

		[Test]
		public void TestBinPath ()
		{
			engine = new Engine (Consts.BinPath);

			Assert.AreEqual (Consts.BinPath, engine.BinPath, "A1");
		}

		[Test]
		public void TestBuildEnabled ()
		{
			engine = new Engine (Consts.BinPath);

			Assert.AreEqual (true, engine.BuildEnabled, "A1");
		}

		[Test]
		public void TestOnlyLogCriticalEvents ()
		{
			engine = new Engine (Consts.BinPath);

			Assert.AreEqual (false, engine.OnlyLogCriticalEvents, "A1");
		}

		[Test]
		public void TestGlobalProperties ()
		{
			engine = new Engine (Consts.BinPath);
			Project project;

			Assert.IsNotNull (engine.GlobalProperties, "A1");
			Assert.AreEqual (0, engine.GlobalProperties.Count, "A2");
			Assert.AreEqual (String.Empty, engine.GlobalProperties.Condition, "A3");
			Assert.IsFalse (engine.GlobalProperties.IsImported, "A4");
			
			engine.GlobalProperties.SetProperty ("GlobalA", "value1");
			Assert.AreEqual (1, engine.GlobalProperties.Count, "A5");
			engine.GlobalProperties.SetProperty ("GlobalB", "value1");
			Assert.AreEqual (2, engine.GlobalProperties.Count, "A6");
			engine.GlobalProperties.SetProperty ("GlobalA", "value2");
			Assert.AreEqual (2, engine.GlobalProperties.Count, "A7");

			project = engine.CreateNewProject ();
			Assert.AreEqual (2, project.GlobalProperties.Count, "A8");
			project.GlobalProperties.SetProperty ("GlobalC", "value3");
			Assert.AreEqual (3, project.GlobalProperties.Count, "A9");
			Assert.AreEqual (2, engine.GlobalProperties.Count, "A10");

			project.GlobalProperties.SetProperty ("GlobalA", "value3");
			Assert.AreEqual ("value2", GetPropValue(engine.GlobalProperties, "GlobalA"), "A11");
			engine.GlobalProperties.SetProperty ("GlobalB", "value3");
			Assert.AreEqual ("value1", GetPropValue(project.GlobalProperties, "GlobalB"), "A12");

			engine.GlobalProperties.SetProperty ("GlobalC", "value4");
			engine.GlobalProperties.SetProperty ("GlobalD", "value5");
			Assert.AreEqual (4, engine.GlobalProperties.Count, "A13");
			Assert.AreEqual (3, project.GlobalProperties.Count, "A14");

			project = new Project (engine);
			Assert.AreEqual (4, project.GlobalProperties.Count, "A15");
		}

		[Test]
		public void TestGlobalEngine ()
		{
			engine = new Engine ();
			Assert.IsFalse (engine == Engine.GlobalEngine, "1");
			Assert.IsNotNull (Engine.GlobalEngine, "2");
			engine = Engine.GlobalEngine;
			Assert.AreSame (engine, Engine.GlobalEngine, "3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestRegisterLogger ()
		{
			engine = new Engine (Consts.BinPath);
			engine.RegisterLogger (null);
		}

		// The "Project" object specified does not belong to the correct "Engine" object.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestUnloadProject1 ()
		{
			Engine a = new Engine (Consts.BinPath);
			Engine b = new Engine (Consts.BinPath);

			Project p = a.CreateNewProject ();

			b.UnloadProject (p);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestUnloadProject2 ()
		{
			Engine a = new Engine (Consts.BinPath);

			a.UnloadProject (null);
		}

		// This project object has been unloaded from the MSBuild engine and is no longer valid.
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestUnloadProject3 ()
		{
			Engine a = new Engine (Consts.BinPath);
			Project p = a.CreateNewProject ();

			a.UnloadProject (p);
			a.UnloadProject (p);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestUnregisterAllLoggers ()
		{
			engine = new Engine (Consts.BinPath);
			CheckUnregisterLogger cul = new CheckUnregisterLogger ();
			engine.RegisterLogger (cul);

			engine.UnregisterAllLoggers ();

			Assert.IsFalse (cul.Anything, "A1");
		}

		[Test]
		public void TestBuildError1 ()
		{
			engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			Assert.IsFalse (project.Build (), "A1");
			Assert.IsFalse (project.Build ((string)null), "A2");
			Assert.IsFalse (project.Build ((string [])null), "A3");
			Assert.IsFalse (project.Build (new string [0]), "A4");
			Assert.IsFalse (project.Build (null, null), "A5");
			Assert.IsFalse (project.Build (null, null, BuildSettings.None), "A6");
			//FIXME: Add test for Build (null, non-null-target)
		}

		[Test]
		public void TestBuildProjectFile1 ()
		{
			engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			project.LoadXml (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Message Text='Target 1 called'/>
					</Target>
				</Project>");

			Assert.IsTrue (project.Build ((string)null), "A1");
			Assert.IsTrue (project.Build ((string [])null), "A2");
			Assert.IsTrue (project.Build (new string [0]), "A3");
			Assert.IsTrue (project.Build (null, null), "A4");
			Assert.IsTrue (project.Build (null, null, BuildSettings.None), "A5");
			//FIXME: Add test for Build (null, non-null-target)
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBuildProject1 ()
		{
			engine = new Engine (Consts.BinPath);
			engine.BuildProject (null);
		}

		[Test]
		public void TestBuildProject2 ()
		{
			engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();

			Assert.IsFalse (engine.BuildProject (project, (string)null), "#A1");
			Assert.IsFalse (engine.BuildProject (project, (string [])null), "#A2");
			Assert.IsFalse (engine.BuildProject (project, (string [])null, null), "#A3");
			Assert.IsFalse (engine.BuildProject (project, (string [])null, null, BuildSettings.None), "#A4");

			bool caught_exception = false;
			try {
				//null string in targetNames [] param
				engine.BuildProject (project, new string [] {null}, null);
			} catch {
				caught_exception = true;
			}
			if (!caught_exception)
				Assert.Fail ("Expected exception for Engine.BuildProject");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBuildProjectNull1 ()
		{
			engine = new Engine (Consts.BinPath);
			engine.BuildProject (null, "foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBuildProjectNull2 ()
		{
			engine = new Engine (Consts.BinPath);
			engine.BuildProject (null, (string)null);
		}

	}
}
