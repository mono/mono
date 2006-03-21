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
using System.Xml;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ProjectTest {

        string binPath;

        [SetUp]
        public void SetUp ()
        {
            binPath = "binPath";
        }

		// Clones a project by reloading from original.Xml
		private Project CloneProject (Project original)
		{
			Project clone;
			
			clone = original.ParentEngine.CreateNewProject ();
			clone.LoadXml (original.Xml);

			return clone;
		}

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
			
			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		public void TestDefaultTargets ()
		{
			Engine engine;
			Project proj;
			Project cproj;
			string documentString = @"
                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" DefaultTargets=""Build;Compile"">
                </Project>
            ";
			
			engine = new Engine (binPath);
			proj = engine.CreateNewProject ();
			proj.LoadXml (documentString);
			
			Assert.AreEqual ("Build; Compile", proj.DefaultTargets, "A1");
			proj.DefaultTargets = "Build";
			Assert.AreEqual ("Build", proj.DefaultTargets, "A2");
			cproj = CloneProject (proj);
			Assert.AreEqual (proj.DefaultTargets, cproj.DefaultTargets, "A3");
		}

		[Test]
		public void TestListProperties ()
		{
			Engine engine = new Engine (binPath);
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<Prop1>value1</Prop1>
					</PropertyGroup>
				</Project>
			";

			proj.LoadXml (documentString);
			Assert.AreEqual (proj.PropertyGroups.Count, 1, "A1");
		}
	}
}
