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
using System.IO;

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
		string secondProject;

		static string GetPropValue (BuildPropertyGroup bpg, string name)
		{
			foreach (BuildProperty bp in bpg) {
				if (bp.Name == name) {
					return bp.FinalValue;
				}
			}
			return String.Empty;
		}

		[SetUp]
		public void Setup ()
		{
			secondProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
	<PropertyGroup Condition=""'$(foo)' == 'hello'"">
		<A>FooWasHello</A>
	</PropertyGroup>
	<Target Name=""TargetA"">
		<Message Text=""(TargetA) foo: $(foo) A: $(A) External: $(External)""/>
	</Target>

	<Target Name=""TargetB"">
		<Message Text=""(TargetB) foo: $(foo) A: $(A) External: $(External)""/>
	</Target>
</Project>";

		}

		[Test]
		public void TestCtor ()
		{
			engine = new Engine (Consts.BinPath);
		}

		// Before a project can be instantiated, Engine.BinPath must be set to the location on disk where MSBuild is installed.
		// This is used to evaluate $(MSBuildBinPath).
		/* This isn't valid for 3.5

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestNewProject ()
		{
			engine = new Engine ();

			engine.CreateNewProject ();
		}*/

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
		[Category ("NotDotNet")]
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
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBuildProjectNull1 ()
		{
			engine = new Engine (Consts.BinPath);
			engine.BuildProject (null, "foo");
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void TestBuildProjectNull2 ()
		{
			engine = new Engine (Consts.BinPath);
			engine.BuildProject (null, (string)null);
		}

		// Tests to check global properties behavior
		[Test]
		public void TestGlobalProperties1 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
				+ GetUsingTask ("MSBuild")
				+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
				+ GetUsingTask ("MSBuild")
				+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
	</Target>
</Project>
";

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				9, 7, 13,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo:  A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second" });
		}

		[Test]
		public void TestGlobalProperties1a ()
		{
			Directory.CreateDirectory ("Test/resources/foo");
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";
			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				10, 7, 14,
				 new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo:  A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		[Test]
		public void TestGlobalProperties1b ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
	</Target>
</Project>
";
			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				10, 7, 14,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetA) foo:  A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		[Test]
		public void TestGlobalProperties2 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				10, 7, 14,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo:  A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		[Test]
		public void TestGlobalProperties3 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<CallTarget Targets=""Call2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
	<Target Name=""Call2"">
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				10, 8, 15,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo:  A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		//externally set global properties
		[Test]
		public void TestGlobalProperties4 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<CallTarget Targets=""Call2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
	<Target Name=""Call2"">
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			BuildPropertyGroup globalprops = new BuildPropertyGroup ();
			globalprops.SetProperty ("foo", "hello");
			engine.GlobalProperties = globalprops;

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				globalprops, null, 10, 8, 15,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo: hello A: FooWasHello External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		//externally set global properties, merge with explicit
		[Test]
		public void TestGlobalProperties4a ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<CallTarget Targets=""Call2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
	<Target Name=""Call2"">
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			BuildPropertyGroup globalprops = new BuildPropertyGroup ();
			globalprops.SetProperty ("external", "ExternalValue");

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				globalprops, null,
				10, 8, 15,
				new string [] {
					"(TargetA) foo: bar A:  External: ExternalValue",
					"(TargetB) foo: foofoo A:  External: ExternalValue",
					"(TargetA) foo:  A:  External: ExternalValue",
					"(TargetB) foo: foofoo1 A:  External: ExternalValue",
					"second"});
		}

		//set global properties on _project_, merge with explicit
		[Test]
		public void TestGlobalProperties4b ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<CallTarget Targets=""Call2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
	<Target Name=""Call2"">
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			BuildPropertyGroup globalprops = new BuildPropertyGroup ();
			globalprops.SetProperty ("external", "ExternalValue");

			BuildPropertyGroup project_globalprops = new BuildPropertyGroup ();
			project_globalprops.SetProperty ("external", "ProjExternalValue");
			project_globalprops.SetProperty ("foo", "ProjFooValue");

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				globalprops, project_globalprops,
				10, 8, 15,
				new string [] {
					"(TargetA) foo: bar A:  External: ProjExternalValue",
					"(TargetB) foo: foofoo A:  External: ProjExternalValue",
					"(TargetA) foo: ProjFooValue A:  External: ProjExternalValue",
					"(TargetB) foo: foofoo1 A:  External: ProjExternalValue",
					"second"});
		}

		//set global properties on _project_, and engine and explicit via msbuild
		[Test]
		public void TestGlobalProperties4c ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name=""main"">
		<MSBuild Projects=""first.proj"" Targets = ""1""/>
		<CallTarget Targets=""Call2""/>
		<Message Text=""second""/>
		<MSBuild Projects=""first.proj"" Targets = ""1;2""/>
	</Target>
	<Target Name=""Call2"">
		<MSBuild Projects=""first.proj"" Targets = ""2""/>
	</Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
	<Target Name = ""1"">
		<MSBuild Projects=""second.proj"" Properties=""foo=bar""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo""/>
	</Target>
	<Target Name=""2"">
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetA""/>
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo"" />
		<MSBuild Projects=""second.proj"" Targets = ""TargetB"" Properties=""foo=foofoo1"" />
	</Target>
</Project>
";

			BuildPropertyGroup globalprops = new BuildPropertyGroup ();
			globalprops.SetProperty ("foo", "EngineFooValue");

			BuildPropertyGroup project_globalprops = new BuildPropertyGroup ();
			project_globalprops.SetProperty ("foo", "ProjFooValue");

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				globalprops, project_globalprops,
				10, 8, 15,
				new string [] {
					"(TargetA) foo: bar A:  External: ",
					"(TargetB) foo: foofoo A:  External: ",
					"(TargetA) foo: ProjFooValue A:  External: ",
					"(TargetB) foo: foofoo1 A:  External: ",
					"second"});
		}

		// Check for global properties in case of Import

		[Test]
		public void TestGlobalPropertiesImport1 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""main"">
					<MSBuild Projects=""first.proj"" Targets = ""1"" Properties='Prop=test'/>
				</Target>
			</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name = ""1"">
					<Message Text='Prop: $(Prop)'/>
				</Target>
				<Import Project='$(Prop).proj'/>
			</Project>";

			CreateAndCheckGlobalPropertiesImportTest (mainProject, firstProject);
		}

		[Test]
		public void TestGlobalPropertiesImport2 ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""main"">
					<MSBuild Projects=""first.proj"" Targets = ""1"" Properties='Prop=test'/>
				</Target>
			</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<PropertyGroup>
					<Prop>invalid</Prop>
				</PropertyGroup>
				<Target Name = ""1"">
					<Message Text='Prop: $(Prop)'/>
				</Target>
				<Import Project='$(Prop).proj'/>
			</Project>";

			CreateAndCheckGlobalPropertiesImportTest (mainProject, firstProject);
		}

		[Test]
		public void TestGlobalPropertiesImport3()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<Target Name=""main"">
					<MSBuild Projects=""first.proj"" Targets = ""1""/>
				</Target>
			</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<PropertyGroup>
					<Prop>test</Prop>
				</PropertyGroup>
				<Target Name = ""1"">
					<Message Text='Prop: $(Prop)'/>
				</Target>
				<Import Project='$(Prop).proj'/>
			</Project>";

			CreateAndCheckGlobalPropertiesImportTest (mainProject, firstProject);
		}

	        [Test]
		public void TestMSBuildOutputs ()
		{
			string mainProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">"
					+ GetUsingTask ("MSBuild")
					+ @"
        <ItemGroup>
                <ProjectRef Include=""first.proj"">
                        <Prop3>value</Prop3>
                        <Unique>true</Unique>
                </ProjectRef>
                <ProjectRef Include=""first.proj"">
                        <Prop3>value2</Prop3>
                        <Unique>false</Unique>
                </ProjectRef>
		
		<ProjectRef Include=""second.proj"">
                        <Prop3>value3</Prop3>
                        <Unique>unique</Unique>
                </ProjectRef>

        </ItemGroup>

        <Target Name='Main'>
                <MSBuild Projects=""@(ProjectRef)"" Targets=""GetData"">
                        <Output TaskParameter=""TargetOutputs"" ItemName=""F""/>
                </MSBuild>
                <Message Text=""@(F): F.Unique: %(F.Unique)""/>
                <Message Text=""@(F): F.Prop1: %(F.Prop1)""/>
                <Message Text=""@(F): F.Prop2: %(F.Prop2)""/>
                <Message Text=""@(F): F.Prop3: %(F.Prop3)""/>
        </Target>
</Project>";

			string firstProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
        <ItemGroup>
                <A Include=""foofoo"">
                        <Prop1>false</Prop1>
                        <Prop2>false</Prop2>
                        <Prop3>foo value</Prop3>
                </A>
		
		<A Include=""barbar"">
                        <Prop1>bar_false</Prop1>
                        <Prop2>bar_false</Prop2>
                        <Prop3>bar value</Prop3>
                </A>

        </ItemGroup>

        <Target Name=""GetData"" Outputs=""@(AbcOutputs)"">
                <CreateItem Include=""@(A)"">
                        <Output TaskParameter=""Include"" ItemName=""AbcOutputs""/>
                </CreateItem>
        </Target>
</Project>
";
			string secondProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
        <ItemGroup>
                <A Include=""from_second"">
                        <Prop1>false</Prop1>
                        <Prop2>false</Prop2>
                        <Prop3>new value</Prop3>
                </A>
        </ItemGroup>

        <Target Name=""GetData"" Outputs=""@(AbcOutputs)"">
                <CreateItem Include=""@(A)"">
                        <Output TaskParameter=""Include"" ItemName=""AbcOutputs""/>
                </CreateItem>
        </Target>
</Project>
";

			CreateAndCheckGlobalPropertiesTest (mainProject, firstProject, secondProject,
				null, null,
				4, 3, 13,
				new string [] {
					"foofoo;barbar: F.Unique: true",
					"foofoo;barbar: F.Unique: false",
					"from_second: F.Unique: unique",
					"foofoo;foofoo;from_second: F.Prop1: false",
					"barbar;barbar: F.Prop1: bar_false",
					"foofoo;foofoo;from_second: F.Prop2: false",
					"barbar;barbar: F.Prop2: bar_false",
					"foofoo;foofoo: F.Prop3: foo value",
					"barbar;barbar: F.Prop3: bar value",
					"from_second: F.Prop3: new value",
				});
		}

		[Test]
		public void TestGetLoadedProject1()
		{
			Project project = Engine.GlobalEngine.GetLoadedProject("foo.proj");
			Assert.IsNull(project);
		}

		void CreateAndCheckGlobalPropertiesImportTest (string main, string first)
		{
			string basePath = Path.Combine ("Test", "resources");

			string testProject = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>";

			File.WriteAllText (Path.Combine (basePath, "main.proj"), main);
			File.WriteAllText (Path.Combine (basePath, "first.proj"), first);
			File.WriteAllText (Path.Combine (basePath, "test.proj"), testProject);

			try {
				Engine engine = new Engine ();
				MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
					new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
				engine.RegisterLogger (logger);

				Project project = engine.CreateNewProject ();
				project.Load (Path.Combine (basePath, "main.proj"));

				bool result = project.Build ();
				if (!result) {
					logger.DumpMessages ();
					Assert.Fail ("Build failed");
				}

				logger.CheckAny ("Prop: test", MessageImportance.Normal);
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} finally {
				File.Delete (Path.Combine (basePath, "main.proj"));
				File.Delete (Path.Combine (basePath, "first.proj"));
				File.Delete (Path.Combine (basePath, "test.proj"));
			}
		}

		// Helper Methods for TestGlobalProperties*

		void CreateAndCheckGlobalPropertiesTest (string main, string first, string second,
			int project_count, int target_count, int task_count, string [] messages)
		{
			CreateAndCheckGlobalPropertiesTest (main, first, second, null, null,
				project_count, target_count, task_count, messages);
		}

		void CreateAndCheckGlobalPropertiesTest (string main, string first, string second,
			BuildPropertyGroup engine_globals, BuildPropertyGroup project_globals,
			int project_count, int target_count, int task_count, string [] messages)
		{
			WriteGlobalPropertiesProjects (main, first, second);

			Engine engine = new Engine (Consts.BinPath);
			if (engine_globals != null)
				engine.GlobalProperties = engine_globals;
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			Project project = engine.CreateNewProject ();
			project.Load (Path.Combine ("Test", Path.Combine ("resources", "main.proj")));
			if (project_globals != null)
				project.GlobalProperties = project_globals;

			bool result = project.Build ();
			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			CheckEventCounts (logger, project_count, target_count, task_count);

			CheckLoggedMessages (logger, messages, "A1");
		}

		void CheckEventCounts (MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger,
			int project, int target, int task)
		{
			try {
				Assert.AreEqual (project, logger.ProjectStarted, "#project started events");
				Assert.AreEqual (project, logger.ProjectFinished, "#project finished events");
				Assert.AreEqual (target, logger.TargetStarted, "#target started events");
				Assert.AreEqual (target, logger.TargetFinished, "#target finished events");
				Assert.AreEqual (task, logger.TaskStarted, "#task started events");
				Assert.AreEqual (task, logger.TaskFinished, "#task finished events");
				Assert.AreEqual (1, logger.BuildStarted, "#build started events");
				Assert.AreEqual (1, logger.BuildFinished, "#build finished events");
			} catch (AssertionException) {
				logger.DumpMessages ();
				throw;
			}
		}

		void CheckLoggedMessages (MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger, string [] messages,
			string prefix)
		{
			try {
				for (int i = 0; i < messages.Length; i++) {
					logger.CheckLoggedMessageHead (messages [i], String.Format ("{0} #{1}", prefix, i));
				}
			} catch {
				logger.DumpMessages ();
				throw;
			}

			Assert.AreEqual (0, logger.NormalMessageCount, "Number of remaining messages");
		}

		// helper methods for TestGlobalProperties*
		void WriteGlobalPropertiesProjects (string mainProject, string firstProject, string secondProject)
		{
			Directory.CreateDirectory (Path.Combine ("Test", "resources"));
			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "main.proj")))) {
				sw.Write (mainProject);
			}

			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "first.proj")))) {
				sw.Write (firstProject);
			}

			using (StreamWriter sw = new StreamWriter (Path.Combine ("Test", Path.Combine ("resources", "second.proj")))) {
				sw.Write (secondProject);
			}
		}

		public static string GetUsingTask (string taskName)
		{
			return "<UsingTask TaskName='Microsoft.Build.Tasks." + taskName + "' AssemblyFile='" + Consts.GetTasksAsmPath () + "' />";
		}
	}
}
