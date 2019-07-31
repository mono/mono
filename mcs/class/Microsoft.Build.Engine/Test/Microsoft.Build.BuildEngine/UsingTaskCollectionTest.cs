//
// UsingTaskCollectionTest.cs
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

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class UsingTaskCollectionTest {
		
		Engine		engine;
		Project		project;
		
		[Test]
		public void TestAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);
                        
                        Assert.AreEqual (1, project.UsingTasks.Count, "A1");
                        Assert.AreEqual (false, project.UsingTasks.IsSynchronized, "A2");
                        Assert.AreEqual (typeof (object), project.UsingTasks.SyncRoot.GetType (), "A3");
		}
		
		[Test]
		public void TestGetEnumerator ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='FalseTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll"), ((UsingTask) en.Current).AssemblyFile, "A1");
			Assert.AreEqual ("TrueTestTask", ((UsingTask) en.Current).TaskName, "A2");

			en.MoveNext ();

			Assert.AreEqual (TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll"), ((UsingTask) en.Current).AssemblyFile, "A3");
			Assert.AreEqual ("FalseTestTask", ((UsingTask) en.Current).TaskName, "A4");

			Assert.IsFalse (en.MoveNext ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCopyTo1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.UsingTasks.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestCopyTo2 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.UsingTasks.CopyTo (new UsingTask [1], -1);
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (InvalidCastException))]
		public void TestCopyTo3 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.UsingTasks.CopyTo (new UsingTask [][] { new UsingTask [] {
				null}}, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo4 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.UsingTasks.CopyTo (new UsingTask [1], 2);
		}
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo5 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.UsingTasks.CopyTo (new UsingTask [1], 1);
		}

		[Test]
		public void TestCopyTo6 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			UsingTask[] array = new UsingTask [1];
			project.UsingTasks.CopyTo (array, 0);

			Assert.AreEqual ("TrueTestTask", array [0].TaskName, "A1");
		}

		[Test]
		public void TestCopyTo7 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='" + TestResourceHelper.GetFullPathOfResource ("Test/resources/TestTasks.dll") + @"'
						TaskName='TrueTestTask'
					/>
				</Project>
			";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			UsingTask [] array = new UsingTask [1];
			project.UsingTasks.CopyTo ((Array) array, 0);
		}
	}
}

