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
using System.Collections;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ProjectTest {

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
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestBuild1 ()
		{
			Engine engine;
			Project project;
			IDictionary hashtable = new Hashtable ();
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<Target Name='Main'>
						<Microsoft.Build.Tasks.Message Text='Text' />
					</Target>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (true, project.Build (new string[] { "Main" }, hashtable), "A1");
			Assert.AreEqual (1, hashtable.Count, "A2");
		}

		[Test]
		public void TestGlobalProperties1 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (0, project.GlobalProperties.Count, "A1");
		}

		[Test]
		public void TestGlobalProperties2 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			engine.GlobalProperties.SetProperty ("Property", "Value");
			
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.AreEqual (1, project.GlobalProperties.Count, "A1");
			Assert.AreEqual ("Property", project.GlobalProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.GlobalProperties ["Property"].FinalValue, "A4");
			Assert.AreEqual ("Property", project.EvaluatedProperties ["Property"].Name, "A2");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].Value, "A3");
			Assert.AreEqual ("Value", project.EvaluatedProperties ["Property"].FinalValue, "A4");
		}

		[Test]
		[Ignore ("NullRefException under MS .NET 2.0")]
		public void TestGlobalProperties3 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			project.GlobalProperties = null;
		}

		[Test]
		[Ignore ("NullRefException under MS .NET 2.0")]
		public void TestGlobalProperties4 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);

			project.GlobalProperties = groups [0];
			project.GlobalProperties = project.EvaluatedProperties;
		}

		[Test]
		[Category ("NotWorking")]
		public void TestGlobalProperties5 ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
					<PropertyGroup>
						<Property>a</Property>
					</PropertyGroup>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildPropertyGroup[] groups = new BuildPropertyGroup [1];
			project.PropertyGroups.CopyTo (groups, 0);
			project.GlobalProperties = groups [0];
		}

		[Test]
		public void TestParentEngine ()
		{
			Engine engine;
			Project project;
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			Assert.AreEqual (engine, project.ParentEngine, "A1");
		}
		
		[Test]
		public void TestSchemaFile ()
		{
			Engine engine;
			Project project;
			
			string documentString = @"
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>
			";
			
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsNull (project.SchemaFile, "A1");
		}
	}
}
