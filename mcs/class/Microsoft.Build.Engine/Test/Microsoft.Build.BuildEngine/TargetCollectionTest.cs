//
// TargetCollectionTest.cs
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
	public class TargetCollectionTest {
		
		Engine			engine;
		Project			project;
		
		[Test]
		public void TestEmpty ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual (0, project.Targets.Count, "A1");
			Assert.IsFalse (project.Targets.IsSynchronized, "A2");
			Assert.IsNotNull (project.Targets.SyncRoot, "A3");
		}

		[Test]
		public void TestAddNewTarget1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.AddNewTarget ("name");

			Assert.AreEqual (1, project.Targets.Count, "A1");
			Assert.AreEqual ("name", project.Targets ["name"].Name, "A2");
			Assert.IsFalse (project.Targets ["name"].IsImported, "A3");
			Assert.AreEqual (String.Empty, project.Targets ["name"].Condition, "A4");
			Assert.AreEqual (String.Empty, project.Targets ["name"].DependsOnTargets, "A5");	
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAddNewTarget2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.AddNewTarget (null);
		}

		[Test]
		public void TestAddNewTarget3 ()
		{
			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();

			project.Targets.AddNewTarget ("Name");
			project.Targets.AddNewTarget ("Name");
			Assert.AreEqual (1, project.Targets.Count, "A1");
		}

		[Test]
		public void TestAddNewTarget4 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='A' />
					<Target Name='A' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.AreEqual (1, project.Targets.Count, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestCopyTo1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='a' />
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestCopyTo2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='a' />
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.CopyTo (new Target [1], -1);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void TestCopyTo3 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='a' />
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.CopyTo (new Target [][] { new Target [] { null } }, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo4 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='a' />
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.CopyTo (new Target [1], 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCopyTo5 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='a' />
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.CopyTo (new Target [1], 1);
		}
		
		[Test]
		public void TestExists1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='name' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual (1, project.Targets.Count, "A1");
			Assert.IsTrue (project.Targets.Exists ("name"), "A2");
			Assert.IsTrue (project.Targets.Exists ("NAME"), "A3");
			Assert.IsFalse (project.Targets.Exists ("something_that_doesnt_exist"), "A4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestExists2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='name' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.Exists (null);
		}

		[Test]
		public void TestGetEnumerator ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='first' />
					<Target Name='second' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			ArrayList targets = new ArrayList ();

			foreach (Target t in project.Targets)
				targets.Add (t);

			Assert.AreEqual ("first", ((Target) targets [0]).Name, "A1");
			Assert.AreEqual ("second", ((Target) targets [1]).Name, "A1");

		}

		[Test]
		public void TestRemoveTarget1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='first' />
					<Target Name='second' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.IsTrue (project.Targets.Exists ("first"), "A1");
			Assert.IsTrue (project.Targets.Exists ("second"), "A1");
			
			project.Targets.RemoveTarget (project.Targets ["first"]);
			
			Assert.IsFalse (project.Targets.Exists ("first"), "A1");
			Assert.IsTrue (project.Targets.Exists ("second"), "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		[Category ("NotDotNet")]
		public void TestRemoveTarget2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='first' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			project.Targets.RemoveTarget (null);
		}

		[Test]
		public void TestIndexer1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='first' />
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Target t1 = project.Targets ["first"];

			Assert.AreEqual ("first", t1.Name, "A1");

			Target t2 = project.Targets ["target_that_doesnt_exist"];

			Assert.IsNull (t2, "A2");
		}
	}
}
