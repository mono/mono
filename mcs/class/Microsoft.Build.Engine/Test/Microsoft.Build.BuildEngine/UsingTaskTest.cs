//
// UsingTaskTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class UsingTaskTest {
		
		Engine		engine;
		Project		project;
		
		[Test]
		public void TestAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='SimpleTask'
						Condition='true'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("Test/resources/TestTasks.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual ("true", ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("SimpleTask", ut.TaskName, "A5");
		}

		[Test]
		public void TestAssemblyFile2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='SimpleTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("Test/resources/TestTasks.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual (null, ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("SimpleTask", ut.TaskName, "A5");
		}

		[Test]
		// NOTE: quite hacky test, it works because MSBuild doesn't check type of task at loading
		public void TestAssemblyName ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyName='System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
						TaskName='System.Uri'
						Condition='true'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
			
			UsingTask ut = (UsingTask) en.Current;
			
			Assert.AreEqual ("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", ut.AssemblyName, "A1");
			Assert.IsNull (ut.AssemblyFile, "A2");
			Assert.AreEqual ("true", ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("System.Uri", ut.TaskName, "A5");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestTaskName ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAssemblyNameOrAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						TaskName='SimpleTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		public void TestAssemblyNameOrAssemblyFileConditionFalse ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						TaskName='SimpleTask'
						Condition='false'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			try {
				project.LoadXml (documentString);
			} catch (InvalidProjectFileException) {
				return;
			}
			Assert.Fail ("Project load should've failed");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAssemblyNameOrAssemblyFile2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						TaskName='SimpleTask'
						AssemblyFile='A'
						AssemblyName='B'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestDuplicate1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='TrueTestTask'
					/>
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='TrueTestTask'
					/>

					<Target Name='1'>
						<TrueTestTask/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			Assert.AreEqual (2, project.UsingTasks.Count, "A0");

			foreach (UsingTask ut in project.UsingTasks) {
				Assert.AreEqual ("Test/resources/TestTasks.dll", ut.AssemblyFile, "A1");
				Assert.IsNull (ut.AssemblyName, "A2");
				Assert.AreEqual (null, ut.Condition, "A3");
				Assert.AreEqual (false, ut.IsImported, "A4");
				Assert.AreEqual ("TrueTestTask", ut.TaskName, "A5");
			}
		}


		[Test]
		public void TestLazyLoad1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='NonExistantAssembly.dll'
						TaskName='SimpleTask'
					/>
					<Target Name='1'>
						<Message Text='hello'/>
					</Target>
					<Target Name='2'>
						<SimpleTask Foo='bar'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			if (project.Build ("2"))
				Assert.Fail ("Build should've failed, as a task from a nonexistant assembly is referenced");


			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();

			UsingTask ut = (UsingTask) en.Current;

			Assert.AreEqual ("NonExistantAssembly.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual (null, ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("SimpleTask", ut.TaskName, "A5");
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestLazyLoad2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='Another.SameTask'
					/>
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='Other.SameTask'
					/>

					<Target Name='1'>
						<Other.SameTask>
							<Output TaskParameter='OutputString' ItemName='I0'/>
						</Other.SameTask>
						<Another.SameTask>
							<Output TaskParameter='OutputString' ItemName='I1'/>
						</Another.SameTask>
						<Message Text='I0: @(I0) I1: @(I1)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			logger.CheckLoggedMessageHead ("I0: Other.SameTask I1: Another.SameTask", "A1");
		}

		[Test]
		public void TestLazyLoad3 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='Another.SameTask'
						Condition='false'
					/>

					<Target Name='1'>
						<Another.SameTask />
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (documentString);

			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();

			UsingTask ut = (UsingTask) en.Current;

			Assert.AreEqual ("Test/resources/TestTasks.dll", ut.AssemblyFile, "A1");
			Assert.IsNull (ut.AssemblyName, "A2");
			Assert.AreEqual ("false", ut.Condition, "A3");
			Assert.AreEqual (false, ut.IsImported, "A4");
			Assert.AreEqual ("Another.SameTask", ut.TaskName, "A5");

			if (project.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build should've failed");
			}
		}

	}
}
