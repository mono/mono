//
// AssignProjectConfigurationTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2009 Novell, Inc (http://www.novell.com)
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
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class AssignProjectConfigurationTest
	{
		[Test]
		public void TestValidCase () {
			string[] guids = new string[] {
				"{88932AF5-A0AF-44F3-A202-5C88152F25CA}",
				"{88932AF5-A0AF-44F3-A202-5C88152FABC1}",
				"{3653C4D3-60C0-4657-8289-3922D0DFB933}",
				"{DAE34193-B5C7-4488-A911-29EE15C84CB8}",
				"{23F291D9-78DF-4133-8CF2-78CE104DDE63}",
				"asd"
			};

			string[] project_ref_guids = new string[] {
				"{88932AF5-A0AF-44F3-A202-5C88152F25CA}",
				"{88932AF5-A0AF-44F3-A202-5C88152faBC1}",
				"{3653C4D3-60C0-4657-8289-3922D0DFB933}",
				"{DAE34193-B5C7-4488-A911-29EE15C84CB8}",
				"{DAE34193-B5C7-4488-A911-29EE15C84CBE}"
			};

			CreateAndCheckProject (guids, new bool[] {true, true, true, true, true, true},
					project_ref_guids, new string[] {
					"AssignedProjects : foo0.csproj;foo1.csproj;foo2.csproj;foo3.csproj;foo4.csproj: SetConfig: Configuration=Release",
					"AssignedProjects : foo0.csproj: SetPlatform: Platform=AnyCPU0",
					"AssignedProjects : foo1.csproj: SetPlatform: Platform=AnyCPU1",
					"AssignedProjects : foo2.csproj: SetPlatform: Platform=AnyCPU2",
					"AssignedProjects : foo3.csproj: SetPlatform: Platform=AnyCPU3",
					"AssignedProjects : foo4.csproj: SetPlatform: Platform=AnyCPU4",
					"UnassignedProjects : "},
					true,
					 "A1#");
		}

		[Test]
		public void TestNoGuidAndNoAbsolutePathFound()
		{
			string[] guids = new string[] {
				"asd"
			};

			string[] project_ref_guids = new string[] {
				"{DAE34193-B5C7-4488-A911-29EE15C84CB8}",
				"invalid guid",
				""
			};

			CreateAndCheckProject (guids, new bool[]{false},
					project_ref_guids,
					new string[] {
						"AssignedProjects : : SetConfig: ",
						"AssignedProjects : : SetPlatform: ",
						"UnassignedProjects : foo0.csproj;foo1.csproj;foo2.csproj"
					},
					true, "A1#");
		}

		[Test]
		public void TestInvalidProjectGuidWithAbsolutePath ()
		{
			string[] guids = new string[] {
				null, // no AbsPath
				"another invalid guid",	// has AbsPath
			};

			string[] project_ref_guids = new string[] {
				"1234zxc", // this won't match because no AbsPath
				"xzxoiu",  // match with the second project, foo1.csproj
				"{23F291D9-78DF-4133-8CF2-78CE104DDE63}",
				"badref"   // no corresponding project at all
			};

			CreateAndCheckProject (guids, new bool[]{false, true},
					project_ref_guids,
					new string[] {
						"AssignedProjects : foo1.csproj: SetConfig: Configuration=Release",
						"AssignedProjects : foo1.csproj: SetPlatform: Platform=AnyCPU1",
						"UnassignedProjects : foo0.csproj;foo2.csproj;foo3.csproj"
					},
					true, "A1#");
		}

		[Test]
		public void TestNoGuidWithAbsolutePath ()
		{
			string[] guids = new string[] {
				"",
				null
			};

			string[] project_ref_guids = new string[] {
				"{DAE34193-B5C7-4488-A911-29EE15C84CB8}",
				"{23F291D9-78DF-4133-8CF2-78CE104DDE63}",
				"invalid guid"
			};

			CreateAndCheckProject (guids, new bool[]{true, false},
					project_ref_guids,
					new string[] {
						"AssignedProjects : foo0.csproj: SetConfig: Configuration=Release",
						"AssignedProjects : foo0.csproj: SetPlatform: Platform=AnyCPU0",
						"UnassignedProjects : foo1.csproj;foo2.csproj"
					},
					true, "A1#");
		}

		[Test]
		public void TestInvalidProjectGuidInSolutionConfigContents () {
			string[] guids = new string[] {
				"{23F291D9-78DF-4133-8CF2-78CE104DDE63}",
				"invalid guid"
			};

			string[] project_ref_guids = new string[] {
				"{DAE34193-B5C7-4488-A911-29EE15C84CB8}",
				"{23F291D9-78DF-4133-8CF2-78CE104DDE63}"
			};

			CreateAndCheckProject (guids, new bool[]{false, true},
				project_ref_guids,
				new string [] {
					"AssignedProjects : foo1.csproj: SetConfig: Configuration=Release",
					"AssignedProjects : foo1.csproj: SetPlatform: Platform=AnyCPU0",
					"UnassignedProjects : foo0.csproj"
				}, true, "A1#");
		}


		void CreateAndCheckProject (string[] guids, bool[] set_project_paths, string[] project_ref_guids, string[] messages, bool build_result, string prefix)
		{
			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			string projectString = CreateProject (guids, set_project_paths, project_ref_guids);
			project.LoadXml (projectString);

			try {
				Assert.AreEqual (build_result, project.Build (), "Build " + (build_result ? "failed" : "should've failed"));
				if (!build_result || messages == null)
					// build failed as expected, don't check outputs
					return;
				for (int i = 0; i < messages.Length; i++)
					testLogger.CheckLoggedMessageHead (messages [i], prefix + i.ToString ());
				Assert.AreEqual (0, testLogger.NormalMessageCount);
			} catch (AssertionException) {
				Console.WriteLine (projectString);
				testLogger.DumpMessages ();
				throw;
			}
		}

		string CreateProject (string[] guids, bool[] set_project_paths, string[] project_ref_guids)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">");
			sb.Append ("\n" + GetUsingTask ("AssignProjectConfiguration"));
			sb.AppendFormat (@"<PropertyGroup>{0}</PropertyGroup>", CreateSolutionConfigurationProperty (guids, set_project_paths, "Release|AnyCPU"));
			sb.Append (CreateProjectReferencesItemGroup (project_ref_guids));

			sb.Append ("\n\t<Target Name=\"1\">\n");
			sb.Append ("\t\t<AssignProjectConfiguration ProjectReferences=\"@(ProjectReference)\" " +
					" SolutionConfigurationContents=\"$(CurrentSolutionConfigurationContents)\">\n");
			sb.Append ("\t\t\t<Output TaskParameter=\"AssignedProjects\" ItemName = \"AssignedProjects\" />\n");
			sb.Append ("\t\t\t<Output TaskParameter=\"UnassignedProjects\" ItemName = \"UnassignedProjects\" />\n");
			sb.Append ("\t\t</AssignProjectConfiguration>\n");
			sb.Append ("<Message Text=\"AssignedProjects : @(AssignedProjects): SetConfig: %(AssignedProjects.SetConfiguration)\"/>\n");
			sb.Append ("<Message Text=\"AssignedProjects : @(AssignedProjects): SetPlatform: %(AssignedProjects.SetPlatform)\"/>\n");
			sb.Append ("<Message Text=\"UnassignedProjects : @(UnassignedProjects)\"/>\n");
			sb.Append ("</Target>\n");
			sb.Append ("</Project>");

			return sb.ToString ();
		}

		string CreateSolutionConfigurationProperty (string[] guids, bool[] set_project_paths, string config_str)
		{
			string abs_proj_path_prefix = Path.GetFullPath ("foo");
			StringBuilder sb = new StringBuilder ();
			sb.Append ("\n<CurrentSolutionConfigurationContents>\n");
				sb.Append ("\t<foo xmlns=\"\">\n");
				for (int i = 0; i < guids.Length; i++) {
					sb.Append ("\t\t<bar");
					if (guids[i] != null)
						sb.AppendFormat (" Project=\"{0}\"", guids[i]);
					if (set_project_paths[i])
						sb.AppendFormat (" AbsolutePath=\"{0}{1}.csproj\" ", abs_proj_path_prefix, i);
					sb.AppendFormat (">{1}{2}</bar>\n",
						guids[i], config_str, i);
				}
				sb.Append ("\t</foo>\n");

			sb.Append ("</CurrentSolutionConfigurationContents>\n");
			return sb.ToString ();
		}

		string CreateProjectReferencesItemGroup (string[] guids)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("\n<ItemGroup>\n");
			for (int i = 0; i < guids.Length; i ++) {
				sb.AppendFormat ("\t<ProjectReference Include=\"foo{0}.csproj\">", i);
				if (guids[i] != null)
					sb.AppendFormat ("<Project>{0}</Project>", guids[i]);
				sb.Append ("</ProjectReference>\n");
			}
			sb.Append ("</ItemGroup>\n");
			return sb.ToString ();
		}
		
		string GetUsingTask (string taskName)
		{
			return "<UsingTask TaskName='Microsoft.Build.Tasks." + taskName + "' AssemblyFile='" + Consts.GetTasksAsmPath () + "' />";
		}

	}
}
