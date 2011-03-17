//
// FindAppConfigFileTest.cs
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

#if NET_3_5

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;
using System.Text;

namespace MonoTests.Microsoft.Build.Tasks {

	[TestFixture]
	public class FindAppConfigFileTest {

		[Test]
		public void TestExecution1 ()
		{
			CheckOutput (new string [] {"app.config"}, new string [] {"app.config"}, "AppConfigWithTargetPath: app.config List1 Foo.exe.config");
		}

		[Test]
		public void TestExecution2 ()
		{
			CheckOutput (new string [] {"foobar"}, new string [] {"app.config"}, "AppConfigWithTargetPath: app.config List2 Foo.exe.config");
		}

		[Test]
		public void TestExecution3 ()
		{
			CheckOutput (new string [] {"foo\\app.config"}, new string [] {"app.config"}, "AppConfigWithTargetPath: app.config List2 Foo.exe.config");
		}

		[Category ("NotWorking")]
		[Test]
		public void TestExecution4 ()
		{
			CheckOutput (new string[] { "foo\\app.config" }, new string[] { "bar\\app.config" }, "AppConfigWithTargetPath: foo\\app.config List1 Foo.exe.config");
		}

		[Category ("NotWorking")]
		[Test]
		public void TestExecution5 ()
		{
			CheckOutput (new string[] { "foobar" }, new string[] { "bar\\app.config" }, "AppConfigWithTargetPath: bar\\app.config List2 Foo.exe.config");
		}

		[Test]
		public void TestExecution6 ()
		{
			CheckOutput (new string[] { "foobar" }, new string[] { "bar\\foo.config" }, "AppConfigWithTargetPath:   ");
		}

		[Test]
		public void TestExecution7 () {
			CheckOutput (new string[] { ".\\app.config" }, new string[] { "app.config" }, "AppConfigWithTargetPath: app.config List2 Foo.exe.config");
		}

		void CheckOutput (string[] primary_list, string[] secondary_list, string expected) {
			StringBuilder sb = new StringBuilder ();

			sb.Append (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + ">");
			sb.Append ("\t<ItemGroup>");
			if (primary_list != null)
				foreach (string s in primary_list)
					sb.AppendFormat ("\t\t<List1 Include=\"{0}\"><Source>List1</Source></List1>", s);

			if (secondary_list != null)
				foreach (string s in secondary_list)
					sb.AppendFormat ("\t\t<List2 Include=\"{0}\"><Source>List2</Source></List2>", s);
			sb.Append ("\t</ItemGroup>");

			sb.Append (@"
					<Target Name='1'>
						<FindAppConfigFile PrimaryList=""@(List1)"" SecondaryList=""@(List2)"" TargetPath=""Foo.exe.config"">
							<Output TaskParameter=""AppConfigFile"" ItemName=""AppConfigWithTargetPath""/>
						</FindAppConfigFile>

						<Message Text=""AppConfigWithTargetPath: %(AppConfigWithTargetPath.Identity) %(AppConfigWithTargetPath.Source) %(AppConfigWithTargetPath.TargetPath)""/>
					</Target>
				</Project>");

			string projectXml = sb.ToString ();
			Engine engine = new Engine (Consts.BinPath);
			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			Project project = engine.CreateNewProject ();
			project.LoadXml (projectXml);
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			Assert.AreEqual (1, testLogger.NormalMessageCount, "Expected number of messages");
			testLogger.CheckLoggedMessageHead (expected, "A1");
		}
	}
}	
#endif
