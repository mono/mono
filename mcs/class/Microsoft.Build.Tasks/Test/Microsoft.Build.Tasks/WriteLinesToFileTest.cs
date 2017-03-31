//
// WriteLinesToFileTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2010 Novell, Inc (http://www.novell.com)
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
using System.Text;

namespace MonoTests.Microsoft.Build.Tasks {

	[TestFixture]
	public class WriteLinesToFileTest
	{
		string full_path, full_filepath;

		[SetUp]
		public void Setup ()
		{
			full_path = Path.GetFullPath (Path.Combine ("Test", "resources"));
			full_filepath = Path.Combine (full_path, "foo.txt");
			File.Delete (full_filepath);
		}

		[Test]
		public void TestDefault ()
		{
			CreateProjectAndCheck (full_filepath, null, true, false, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, null);
			});
		}

		[Test]
		public void TestDefaultWithExistingFile ()
		{
			File.WriteAllText (full_filepath, "xyz");
			CreateProjectAndCheck (full_filepath, null, true, false, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"xyz"});
			});
		}

		[Test]
		public void TestOverwriteFile ()
		{
			string[] lines = new string[] { "abc", "def" };
			CreateProjectAndCheck (full_filepath, lines, true, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, lines);
			});
		}

		[Test]
		public void TestOverwriteFileWithExistingFile ()
		{
			File.WriteAllText (full_filepath, "xyz");
			string[] lines = new string[] { "abc", "def" };
			CreateProjectAndCheck (full_filepath, lines, true, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, lines);
			});
		}

		[Test]
		[Category("NotWorking")] // this fails due to an xbuild bug, it works on MS.NET
		public void TestLineWithEscapedSemicolon ()
		{
			string[] lines = new string[] { "abc%3Btest%3B%3B", "%3Bdef" };
			CreateProjectAndCheck (full_filepath, lines, false, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"abc;test;;", ";def"});
			});
		}

		[Test]
		[Category("NotWorking")] // this fails due to an xbuild bug, it works on MS.NET
		public void TestLineWithEscapedSpace ()
		{
			string[] lines = new string[] { "  %20%20abc%20test  ", "  def%20%20" };
			CreateProjectAndCheck (full_filepath, lines, false, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"  abc test", "def  "});
			});
		}

		[Test]
		public void TestLineWithEscapedQuote ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix) {
				Assert.Ignore ("Throws \"Illegal characters in path\" on Windows since \" is not a legal Windows path character");
			}
			string[] lines = new string[] { "%22abc test%22 123 %22def%22" };
			CreateProjectAndCheck (full_filepath, lines, false, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"\"abc test\" 123 \"def\""});
			});
		}

		[Test]
		public void TestNoOverwrite ()
		{
			string[] lines = new string[] { "abc", "def" };
			CreateProjectAndCheck (full_filepath, lines, false, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"abc", "def"});
			});
		}

		[Test]
		// appends in this case
		public void TestNoOverwriteWithExistingFile ()
		{
			File.WriteAllText (full_filepath, "xyz");
			string[] lines = new string[] { "abc", "def" };
			CreateProjectAndCheck (full_filepath, lines, false, true, delegate () {
				CheckFileExists (full_filepath, true);
				CheckLines (full_filepath, new string [] {"xyzabc", "def"});
			});
		}

		[Test]
		public void TestEmptyLinesOverwrite ()
		{
			CreateProjectAndCheck (full_filepath, new string[0], true, true,
				delegate () {
					CheckFileExists (full_filepath, false);
				});
		}

		[Test]
		public void TestEmptyLinesOverwriteWithExisting ()
		{
			File.WriteAllText (full_filepath, "xyz");
			CreateProjectAndCheck (full_filepath, new string[0], true, true,
				delegate () {
					CheckFileExists (full_filepath, false);
				});
		}


		[Test]
		public void TestEmptyLinesNoOverwrite ()
		{
			CreateProjectAndCheck (full_filepath, new string[0], false, true,
				delegate () {
					CheckFileExists (full_filepath, true);
					CheckLines (full_filepath, new string[0]);
				});
		}

		[Test]
		public void TestEmptyLinesNoOverwriteWithExisting ()
		{
			File.WriteAllText (full_filepath, "xyz");
			CreateProjectAndCheck (full_filepath, new string[0], false, true,
				delegate () {
					CheckFileExists (full_filepath, true);
					CheckLines (full_filepath, new string [] {"xyz"});
				});
		}

		void CreateProjectAndCheck (string file, string[] lines, bool overwrite, bool use_overwrite, Action action)
		{
			Engine engine;
			Project project;

			StringBuilder sb = new StringBuilder ();
			sb.Append (@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" " + Consts.ToolsVersionString + @">
	<ItemGroup>
");

			if (lines != null)
				foreach (string line in lines)
					sb.AppendFormat ("\t\t<Lines Include='{0}'/>\n", line);

			sb.AppendFormat (@"</ItemGroup>
					<Target Name='1'>
						<WriteLinesToFile File='{0}' Lines='@(Lines)'", file);

			if (use_overwrite)
				sb.AppendFormat (" Overwrite='{0}' ", overwrite);
			sb.Append (@"/>
					</Target>
				</Project>");

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger logger = new TestMessageLogger ();
			engine.RegisterLogger (logger);

			project.LoadXml (sb.ToString ());

			try {
				if (!project.Build ("1"))
					Assert.Fail ("Build failed");

				if (action != null)
					action.Invoke ();
			} catch (AssertionException) {
				logger.DumpMessages ();
				Console.WriteLine (sb.ToString ());
				throw;
			} finally {
				File.Delete (file);
			}
		}

		static void CheckFileExists (string file, bool should_exist)
		{
			Assert.AreEqual (should_exist, File.Exists (file), "File existence");
		}

		static void CheckLines (string full_filepath, string[] expected)
		{
			string[] actual = File.ReadAllLines (full_filepath);
			Assert.AreEqual (expected != null ? expected.Length : 0, actual.Length, "Number of lines written don't match");

			if (expected == null)
				return;
			int i = 0;
			foreach (string line in actual)
				Assert.AreEqual (expected[i++], line, "Z#" + i.ToString ());
		}
	}
}
