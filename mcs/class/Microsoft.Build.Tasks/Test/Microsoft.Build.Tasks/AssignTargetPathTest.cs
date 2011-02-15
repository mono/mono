//
// AssignTargetPathTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2008 Novell, Inc (http://www.novell.com)
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
using System.Text;
using NUnit.Framework;
using Microsoft.Build.BuildEngine;
using System.IO;

namespace MonoTests.Microsoft.Build.Tasks
{
	enum OsType {
		Windows,
		Unix,
		Mac
	}

	[TestFixture]
	public class AssignTargetPathTest
	{
		//inspired from PathTest.cs
		static OsType OS;
		static char DSC = Path.DirectorySeparatorChar;

		[SetUp]
		public void SetUp ()
		{
			if ('/' == DSC) {
				OS = OsType.Unix;
			} else if ('\\' == DSC) {
				OS = OsType.Windows;
			} else {
				OS = OsType.Mac;
				//FIXME: For Mac. figure this out when we need it
			}
		}

		[Test]
		public void TestExecute1()
		{
			if (OS == OsType.Unix) {
				CheckTargetPath(
					new string[] { "/a/b/./abc.cs", "/a/c/def.cs", "a/xyz.cs", "/different/xyz/foo.cs", "rel/bar.resx"},
					new string[] { "b/abc.cs", "c/def.cs", "xyz.cs", "foo.cs", "bar.resx" },
					"/a/./", "A");
			} else if (OS == OsType.Windows) {
				CheckTargetPath(
					new string[] { @"C:\a\b\.\abc.cs", @"C:\a\c\def.cs", "xyz.cs", @"C:\different\xyz\foo.cs", @"rel\bar.resx"},
					new string[] { @"b\abc.cs", @"c\def.cs", "xyz.cs", "foo.cs", "bar.resx" },
					@"C:\a\.\", "A");
			}
		}

		[Test]
		public void TestExecute2()
		{
			string root = Path.GetPathRoot (Environment.CurrentDirectory);
			string cur_dir_minus_root = Environment.CurrentDirectory.Substring (root.Length);

			if (OS == OsType.Unix) {
				CheckTargetPath(
					new string[] { "//a/b/abc.cs", "k/../k/def.cs", "/xyz.cs", "/different/xyz/foo.cs"},
					new string[] { "a/b/abc.cs", Path.Combine (cur_dir_minus_root, "k/def.cs"), "xyz.cs", "different/xyz/foo.cs"},
					"/", "A");
			} else if (OS == OsType.Windows) {
				CheckTargetPath(
					new string[] { root + @"a\b\abc.cs", @"k\..\k\def.cs", root + @"xyz.cs", root + @"different\xyz\foo.cs"},
					new string[] { "a\\b\\abc.cs", cur_dir_minus_root + "\\k\\def.cs", "xyz.cs", "different\\xyz\\foo.cs"},
					root, "A");
			}
		}

		[Test]
		public void TestExecute3()
		{
			string root = Path.GetPathRoot (Environment.CurrentDirectory);
			string cur_dir_minus_root = Environment.CurrentDirectory.Substring (root.Length);

			if (OS == OsType.Unix) {
				CheckTargetPath(
					new string[] { "xyz.cs", "rel/bar.resx" },
					new string[] { Path.Combine (cur_dir_minus_root, "xyz.cs"),
						Path.Combine (cur_dir_minus_root, "rel/bar.resx") },
					"/", "A");
			} else if (OS == OsType.Windows) {
				CheckTargetPath(
					new string[] { "xyz.cs", "rel\\bar.resx" },
					new string[] { Path.Combine (cur_dir_minus_root, "xyz.cs"),
						Path.Combine (cur_dir_minus_root, "rel\\bar.resx") },
					root, "A");
			}
		}

		[Test]
		public void TestLink ()
		{
			string projectText = @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				<ItemGroup>
					<FooFiles Include=""xyz.cs"">
						<Child>Cxyz.cs</Child>
						<Link>Test\Link\xyz.cs</Link>
					</FooFiles>
					<FooFiles Include=""rel\bar.resx"">
						<Child>Crel\bar.resx</Child>
						<Link>Test\Link\bar.resx</Link>
					</FooFiles>
                                        <FooFiles Include=""rel\qwe.txt"">
                                                <Child>Crel\qwe.txt</Child>
                                                <Link>..\Test\Link\qwe.txt</Link>
                                        </FooFiles>
				</ItemGroup>
				<Target Name=""1"">
					<AssignTargetPath Files=""@(FooFiles)"" RootFolder=""/"">
						<Output TaskParameter=""AssignedFiles"" ItemName=""FooPath"" />
					</AssignTargetPath>
				</Target>
			</Project>";
			Engine engine = new Engine(Consts.BinPath);
			Project project = engine.CreateNewProject();

			project.LoadXml(projectText);

			string id = "A";
			Assert.IsTrue(project.Build("1"), id + "1 : Error in building");

			string [] files = new string [] { "xyz.cs", "rel/bar.resx", "rel/qwe.txt"};
			string [] assignedFiles = new string [] { "Test/Link/xyz.cs", "Test/Link/bar.resx", "../Test/Link/qwe.txt"};

			BuildItemGroup include = project.GetEvaluatedItemsByName("FooPath");
			Assert.AreEqual(files.Length, include.Count, id + "2");

			for (int i = 0; i < files.Length; i++) {
			        Assert.AreEqual (files [i], include [i].FinalItemSpec, id + "3, file #" + i);
				Assert.IsTrue (include[i].HasMetadata ("TargetPath"), id + "4, file #" + i + ", TargetPath metadata missing");
				Assert.AreEqual (assignedFiles [i], include[i].GetMetadata("TargetPath"), id + "5, file #" + i);
				Assert.IsTrue (include [i].HasMetadata ("Child"), id + "6, file #" + i + ", Child metadata missing");
				Assert.AreEqual ("C" + files [i], include [i].GetMetadata ("Child"), id + "7, file #" + i + ", Child metadata value incorrect");
			}
		}

		void CheckTargetPath(string[] files, string[] assignedFiles, string rootFolder, string id)
		{
			Engine engine = new Engine(Consts.BinPath);
			Project project = engine.CreateNewProject();

			string projectText = CreateProjectString(files, rootFolder);
			project.LoadXml(projectText);

			Assert.IsTrue(project.Build("1"), id + "1 : Error in building");

			BuildItemGroup include = project.GetEvaluatedItemsByName("FooPath");
			Assert.AreEqual(files.Length, include.Count, id + "2");

			for (int i = 0; i < files.Length; i++) {
			        Assert.AreEqual (files [i], include [i].FinalItemSpec, id + "3, file #" + i);
				Assert.IsTrue (include[i].HasMetadata ("TargetPath"), id + "4, file #" + i + ", TargetPath metadata missing");
				Assert.AreEqual (assignedFiles [i], include[i].GetMetadata("TargetPath"), id + "5, file #" + i);
				Assert.IsTrue (include [i].HasMetadata ("Child"), id + "6, file #" + i + ", Child metadata missing");
				Assert.AreEqual ("C" + files [i], include [i].GetMetadata ("Child"), id + "7, file #" + i + ", Child metadata value incorrect");
			}
		}

		string CreateProjectString(string[] files, string rootFolder)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup>");
			foreach (string file in files)
				sb.AppendFormat("<FooFiles Include=\"{0}\"><Child>C{0}</Child></FooFiles>\n", file);

			sb.AppendFormat(@"</ItemGroup>
			<Target Name=""1"">
				<AssignTargetPath Files=""@(FooFiles)"" RootFolder=""{0}"">
					<Output TaskParameter=""AssignedFiles"" ItemName=""FooPath"" />
				</AssignTargetPath>
			</Target>
		</Project>", rootFolder);

			return sb.ToString();
		}
	}
}
