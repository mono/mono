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

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class UsingTaskCollectionTest {
		
		string		binPath;
		Engine		engine;
		Project		project;
		
		[SetUp]
		public void SetUp ()
		{
			binPath = "../../tools/xbuild/xbuild";
		}
		
		[Test]
		public void TestAssemblyFile1 ()
		{
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<UsingTask
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='SimpleTask'
					/>
				</Project>
			";

                        engine = new Engine (binPath);

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
						AssemblyFile='Test/resources/TestTasks.dll'
						TaskName='SimpleTask'
					/>
				</Project>
			";

			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			IEnumerator en = project.UsingTasks.GetEnumerator ();
			en.MoveNext ();
		}

	}
}
