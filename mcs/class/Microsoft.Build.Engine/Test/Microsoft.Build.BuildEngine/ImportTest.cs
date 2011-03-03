//
// ImportTest.cs
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
using System.IO;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ImportTest {
		
		Engine			engine;
		Project			project;
		
		[Test]
		public void TestAdd1 ()
		{
			string first = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='second.proj'/>
                                </Project>
";
			using (StreamWriter sw = new StreamWriter ("Test/resources/first.proj")) {
				sw.Write (first);
			}

			string second = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                </Project>
";
			using (StreamWriter sw = new StreamWriter ("Test/resources/second.proj")) {
				sw.Write (second);
			}

                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
						<Import Project='Test\resources\first.proj'/>
						<Import Project='Test\resources\Import.csproj' Condition='false'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Import[] t = new Import [2];
			Assert.AreEqual (2, project.Imports.Count, "Number of imports");
			project.Imports.CopyTo (t, 0);

			string base_dir = Path.Combine (Environment.CurrentDirectory, Path.Combine ("Test", "resources"));

			Assert.IsNull (t [0].Condition, "A1");

			Assert.AreEqual (false, t[0].IsImported, "A5");
			Assert.AreEqual ("Test\\resources\\first.proj", t[0].ProjectPath, "A6");
			Assert.AreEqual (Path.Combine (base_dir, "first.proj"), t[0].EvaluatedProjectPath, "A7");

			Assert.AreEqual (true, t[1].IsImported, "A2");
			Assert.AreEqual ("second.proj", t[1].ProjectPath, "A3");
			Assert.AreEqual (Path.Combine (base_dir, "second.proj"), t[1].EvaluatedProjectPath, "A4");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAdd2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project=''/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAdd3 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/SelfImport.csproj'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual (1, project.Imports.Count, "A1");
		}

		[Test]
		public void TestRelativeImport1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/RelativeImport1.csproj'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual ("B", project.EvaluatedProperties ["A"].FinalValue, "A1");
		}

		[Test]
		public void TestItems1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/Items.csproj'/>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			Assert.IsTrue (groups [0].IsImported, "A1");
			Assert.AreEqual (1, groups [0].Count, "A2");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestMissingImport1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/NonExistantProject.proj'/>
                                </Project>";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString, ProjectLoadSettings.None);
		}

		[Test]
		public void TestMissingImport2 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/NonExistantProject.proj'/>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString, ProjectLoadSettings.IgnoreMissingImports);

			Assert.AreEqual (1, project.Imports.Count, "A1");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestMissingImportDefault ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/NonExistantProject.proj'/>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);
		}

#if NET_4_0
		[Test]
		public void TestImportWildcard ()
		{
			string main_project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
				<ItemGroup>
					<FooItem Include=""From main.proj""/>
				</ItemGroup>

				<Import Project=""tmp\*""/>

				<Target Name=""Build"">
					<Message Text=""FooItem: @(FooItem)""/>
				</Target>
			</Project>";

			string other_project_xml = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
				<ItemGroup>
					<FooItem Include=""From $(MSBuildThisFile)""/>
				</ItemGroup>
			</Project>";

			Engine engine = new Engine (Consts.BinPath);
			MonoTests.Microsoft.Build.Tasks.TestMessageLogger logger =
				new MonoTests.Microsoft.Build.Tasks.TestMessageLogger ();
			engine.RegisterLogger (logger);

			string base_dir = Path.GetFullPath (Path.Combine ("Test", "resources")) + Path.DirectorySeparatorChar;
			string tmp_dir = Path.GetFullPath (Path.Combine (base_dir, "tmp")) + Path.DirectorySeparatorChar;

			string main_project = Path.Combine (base_dir, "main.proj");
			string first_project = Path.Combine (tmp_dir, "first.proj");
			string second_project = Path.Combine (tmp_dir, "second.proj");

			Directory.CreateDirectory (tmp_dir);
			File.WriteAllText (main_project, main_project_xml);
			File.WriteAllText (first_project, other_project_xml);
			File.WriteAllText (second_project, other_project_xml);

			Project project = engine.CreateNewProject ();
			project.Load (main_project);
			try {
				Assert.IsTrue (project.Build ("Build"), "Build failed");

				logger.CheckLoggedMessageHead ("FooItem: From main.proj;From first.proj;From second.proj", "A1");
				Assert.AreEqual (0, logger.NormalMessageCount, "Unexpected extra messages found");
			} catch {
				logger.DumpMessages ();
				throw;
			} finally {
				File.Delete (main_project);
				File.Delete (first_project);
				File.Delete (second_project);
			}
		}
#endif

	}
}
