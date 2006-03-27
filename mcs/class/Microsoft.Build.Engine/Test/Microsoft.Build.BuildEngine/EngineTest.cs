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
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class EngineTest {

		Engine engine;
		string binPath;

		[SetUp]
		public void SetUp ()
		{
		    binPath = "binPath";
		}

		[Test]
		public void TestCtor ()
		{
			engine = new Engine (binPath);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException),
		@"Before a project can be instantiated, Engine.BinPath must be set to the location on disk where MSBuild is installed. " +
		"This is used to evaluate $(MSBuildBinPath).")]
		public void TestNewProject ()
		{
			engine = new Engine ();

			engine.CreateNewProject ();
		}

		[Test]
		public void TestBinPath ()
		{
			engine = new Engine (binPath);

			Assert.AreEqual (binPath, engine.BinPath, "A1");
		}

		[Test]
		public void TestBuildEnabled ()
		{
			engine = new Engine (binPath);

			Assert.AreEqual (true, engine.BuildEnabled, "A1");
		}

		[Test]
		public void TestOnlyLogCriticalEvents ()
		{
			engine = new Engine (binPath);

			Assert.AreEqual (false, engine.OnlyLogCriticalEvents, "A1");
		}

		private static string GetPropValue (BuildPropertyGroup bpg, string name)
		{
			foreach (BuildProperty bp in bpg) {
				if (bp.Name == name) {
					return bp.FinalValue;
				}
			}
			return String.Empty;
		}

		[Test]
		public void TestGlobalProperties ()
		{
			engine = new Engine (binPath);
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
	}
}
