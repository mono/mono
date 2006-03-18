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
		// Clones a project by reloading from original.Xml
		private Project CloneProject (Project original)
		{
			Project clone;
			
			clone = original.ParentEngine.CreateNewProject ();
			clone.LoadXml (original.Xml);

			return clone;
		}

		[Test]
		public void TestAssignment ()
		{
			Engine engine;
			Project project;
			string binPath = "binPath";
			string documentString =
				"<Project></Project>";
			
			engine = new Engine (binPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
			
			Assert.AreEqual (String.Empty, project.FullFileName, "A1");
		}

		[Test]
		public void TestDefaultTargets ()
		{
			Engine engine;
			Project proj;
			Project cproj;
			string documentString =
				"<Project DefaultTargets=\"Build;Compile\"></Project>";
			
			engine = new Engine ();
			proj = engine.CreateNewProject ();
			proj.LoadXml (documentString);
			
			Assert.AreEqual ("Build;Compile", proj.DefaultTargets, "A1");
			proj.DefaultTargets = "Build";
			Assert.AreEqual ("Build", proj.DefaultTargets, "A2");
			cproj = CloneProject (proj);
			Assert.AreEqual (proj.DefaultTargets, cproj.DefaultTargets, "A3");
		}

		[Test]
		public void TestListProperties ()
		{
			Engine engine = new Engine ();
			Project proj = engine.CreateNewProject ();

			string documentString = @"
				<Project>
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
