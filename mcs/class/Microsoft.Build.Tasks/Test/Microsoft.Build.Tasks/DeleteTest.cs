//
// DeleteTest.cs
//  
// Author:
//   Jonathan Chambers (joncham@gmail.com)
//
// (C) 2008 Jonathan Chambers
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
	public class DeleteTest {
		string path;

		[SetUp]
		public void CreateDir ()
		{
			path = Path.Combine (Path.Combine ("Test", "resources"), "Delete");
			Directory.CreateDirectory (path);
		}

		[TearDown]
		public void RemoveDirectories ()
		{
			Directory.Delete (path, true);
		}

		[Test]
		public void TestDelete1 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine(path, "delete.txt");

			using (File.CreateText (file_path)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Delete Files='" + file_path + @"'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
		}

		[Test]
		public void TestDelete2 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");
			string file_path2 = Path.Combine (path, "delete2.txt");

			using (File.CreateText (file_path)) { }
			using (File.CreateText (file_path2)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Delete Files='" + file_path + ";" + file_path2 + @"'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
			Assert.IsTrue (!File.Exists (file_path), "A3");
		}

		[Test]
		public void TestDelete3 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");

			using (File.CreateText (file_path)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<FileToDelete>" + file_path + @"</FileToDelete>
					</PropertyGroup>
					<Target Name='1'>
						<Delete Files='$(FileToDelete)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
		}

		[Test]
		public void TestDelete4 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");
			string file_path2 = Path.Combine (path, "delete2.txt");

			using (File.CreateText (file_path)) { }
			using (File.CreateText (file_path2)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<FileToDelete>" + file_path + @"</FileToDelete>
						<FileToDelete2>" + file_path2 + @"</FileToDelete2>
					</PropertyGroup>
					<Target Name='1'>
						<Delete Files='$(FileToDelete);$(FileToDelete2)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
			Assert.IsTrue (!File.Exists (file_path2), "A3");
		}

		[Test]
		public void TestDelete5 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");

			using (File.CreateText (file_path)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<File Include='" + file_path + @"' />
					</ItemGroup>
					<Target Name='1'>
						<Delete Files='@(File)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
		}


		[Test]
		public void TestDelete6 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");
			string file_path2 = Path.Combine (path, "delete2.txt");

			using (File.CreateText (file_path)) { }
			using (File.CreateText (file_path2)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<File Include='" + file_path + @"' />
						<File Include='" + file_path2 + @"' />
					</ItemGroup>
					<Target Name='1'>
						<Delete Files='@(File)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
			Assert.IsTrue (!File.Exists (file_path2), "A3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestDelete7 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (path, "delete.txt");

			using (File.CreateText (file_path)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<FolderToDelete>" + path + @"</FolderToDelete>
						<FileToDelete>delete</FileToDelete>
						<ExtToDelete>txt</ExtToDelete>
					</PropertyGroup>
					<Target Name='1'>
						<Delete Files='$(FolderToDelete)\$(FileToDelete).$(ExtToDelete)'/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			Assert.IsTrue (project.Build ("1"), "A1");
			Assert.IsTrue (!File.Exists (file_path), "A2");
		}

	}
}
