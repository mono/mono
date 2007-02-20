//
// Build.cs: Various build tests.
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

namespace MonoTests.Microsoft.Build.BuildEngine.Various {
	[TestFixture]
	public class Build {
		[Test]
		public void TestBuild1 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='1' DependsOnTargets='2'>
					</Target>
					<Target Name='2' DependsOnTargets='1'>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsFalse (project.Build ("1"), "A1");
		}

		[Test]
		public void TestBuild2 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='1' DependsOnTargets='2'>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsFalse (project.Build ("1"));
		}

		[Test]
		public void TestBuild3 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='A'>
						<Error Text='text' />
						<OnError ExecuteTargets='B' />
					</Target>
					<Target Name='B'>
						<CreateProperty Value='B'>
							<Output TaskParameter='Value' PropertyName='B'/>
						</CreateProperty>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsFalse (project.Build ("A"), "A1");
			Assert.IsNotNull (project.EvaluatedProperties ["B"], "A2");
		}

		[Test]
		public void TestBuild4 ()
		{
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='A'>
						<Error ContinueOnError='true' Text='text' />
						<CreateProperty Value='A'>
							<Output TaskParameter='Value' PropertyName='A'/>
						</CreateProperty>
						<OnError ExecuteTargets='B' />
					</Target>
					<Target Name='B'>
						<CreateProperty Value='B'>
							<Output TaskParameter='Value' PropertyName='B'/>
						</CreateProperty>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("A"), "A1");
			Assert.IsNotNull (project.EvaluatedProperties ["A"], "A2");
			Assert.IsNull (project.EvaluatedProperties ["B"], "A3");
		}
	}
}
