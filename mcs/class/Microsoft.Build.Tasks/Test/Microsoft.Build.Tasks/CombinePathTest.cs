//
// CombinePath.cs
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
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	[TestFixture]
	public class CombinePathTest {

		[Test]
		public void TestAssignment ()
		{
			CombinePath cp = new CombinePath ();

			cp.BasePath = "a";
			cp.Paths = new ITaskItem [] { new TaskItem ("b")};

			Assert.AreEqual ("a", cp.BasePath, "A1");
			Assert.AreEqual ("b", cp.Paths [0].ItemSpec, "A2");
		}

		[Test]
		public void TestExecution1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Dir Include='b' />
						<Dir Include='c' />
						<Dir Include='d\e' />
					</ItemGroup>
					<Target Name='1'>
						<CombinePath BasePath='a' Paths='@(Dir)'>
							<Output
								TaskParameter='CombinedPaths'
								ItemName='Out'
							/>
						</CombinePath>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			BuildItemGroup output = project.GetEvaluatedItemsByName ("Out");
			Assert.AreEqual (3, output.Count, "A2");
			Assert.AreEqual (Path.Combine ("a", "b"), output [0].FinalItemSpec, "A3");
			Assert.AreEqual (Path.Combine ("a", "c"), output [1].FinalItemSpec, "A4");
			Assert.AreEqual (Path.Combine ("a", Path.Combine ("d", "e")), output [2].FinalItemSpec, "A5");

		}

		[Test]
		public void TestExecution2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Dir Include='a\b' />
					</ItemGroup>
					<Target Name='1'>
						<CombinePath Paths='@(Dir)'>
							<Output
								TaskParameter='CombinedPaths'
								ItemName='Out'
							/>
						</CombinePath>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			Assert.IsTrue (project.Build ("1"), "A1");

			BuildItemGroup output = project.GetEvaluatedItemsByName ("Out");
			Assert.AreEqual (1, output.Count, "A2");
			Assert.AreEqual (Path.Combine ("a", "b"), output [0].FinalItemSpec, "A3");
		}
	}
}

