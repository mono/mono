//
// CopyTest.cs
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
using System.IO;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;
using System.Text;
using System.Threading;
using Microsoft.Build.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MonoTests.Microsoft.Build.Tasks {

	[TestFixture]
	public class CopyTest {
		string source_path, target_path;

		[SetUp]
		public void CreateDir ()
		{
			source_path = Path.Combine (Path.Combine ("Test", "resources"), "Copy");
			Directory.CreateDirectory (source_path);
			target_path = Path.Combine (Path.Combine ("Test", "resources"), "Target");
			Directory.CreateDirectory (target_path);
		}

		[TearDown]
		public void RemoveDirectories ()
		{
			Directory.Delete (source_path, true);
			Directory.Delete (target_path, true);
		}

		[Test]
		public void TestCopy_MissingSourceFile ()
		{
			Copy copy = new Copy ();
			copy.BuildEngine = new TestEngine ();
			copy.SourceFiles = new ITaskItem [1];
			copy.SourceFiles [0] = new TaskItem ("SourceDoesNotExist");
			copy.DestinationFiles = new ITaskItem [1];
			copy.DestinationFiles [0] = new TaskItem ("DestDoesNotExist");
			Assert.IsFalse (copy.Execute ());
		}

		[Test]
		public void TestCopy1 ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (source_path, "copy.txt");
			string target_file = Path.Combine (target_path, "copy.txt");

			using (File.CreateText (file_path)) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup><DestFile>" + target_file + @"</DestFile></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_path + @"'><Md>1</Md></SFiles>
						<DFiles Include='$(DestFile)'><Mde>2</Mde></DFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFiles='@(DFiles)' SkipUnchangedFiles='true' >
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
						<Message Text=""I0 : @(I0), I1: @(I1)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				var sb = new StringBuilder ();
				testLogger.DumpMessages (sb);
				Assert.Fail ("Build failed " + sb.ToString ());
			}
			Assert.IsTrue (File.Exists (target_file), "A2");

			BuildItemGroup big = project.GetEvaluatedItemsByName ("I0");
			Assert.AreEqual (1, big.Count, "A3");
			BuildItem bi = big [0];
			Assert.AreEqual (target_file, bi.FinalItemSpec, "A4");
			Assert.AreEqual ("1", bi.GetMetadata ("Md"), "A4");
			Assert.AreEqual ("2", bi.GetMetadata ("Mde"), "A5");

			big = project.GetEvaluatedItemsByName ("I1");
			Assert.AreEqual (1, big.Count, "A10");
			bi = big [0];
			Assert.AreEqual (target_file, bi.FinalItemSpec, "A11");
			Assert.AreEqual ("1", bi.GetMetadata ("Md"), "A12");
			Assert.AreEqual ("2", bi.GetMetadata ("Mde"), "A13");

			// build again, this time files won't get copied because
			// of SkipUnchangedFiles=true
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed #2");
			}
			Assert.IsTrue (File.Exists (target_file), "A20");

			big = project.GetEvaluatedItemsByName ("I0");
			Assert.AreEqual (1, big.Count, "A21");
			bi = big [0];
			Assert.AreEqual (target_file, bi.FinalItemSpec, "A22");
			Assert.AreEqual ("1", bi.GetMetadata ("Md"), "A23");
			Assert.AreEqual ("2", bi.GetMetadata ("Mde"), "A24");

			big = project.GetEvaluatedItemsByName ("I1");
			Assert.AreEqual (1, big.Count, "A25");
			bi = big [0];
			Assert.AreEqual (target_file, bi.FinalItemSpec, "A26");
			Assert.AreEqual ("1", bi.GetMetadata ("Md"), "A27");
			Assert.AreEqual ("2", bi.GetMetadata ("Mde"), "A28");
		}

		[Test]
		public void TestCopy2 ()
		{
			Engine engine;
			Project project;
			string [] file_paths = new string [] {
				Path.Combine (source_path, "copy1.txt"),
				Path.Combine (source_path, "copy2.txt")
			};

			using (File.CreateText (file_paths[0])) { }
			using (File.CreateText (file_paths[1])) { }

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup><TargetPath>" + target_path + @"</TargetPath></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_paths [0] + @"'><Md>1</Md></SFiles>
						<SFiles Include='" + file_paths [1] + @"'><Md>2</Md></SFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFolder='$(TargetPath)' SkipUnchangedFiles='true' >
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
					</Target>
				</Project>
			";
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			CheckCopyBuildItems (project, file_paths, target_path, "A1");

			// build again, this time files won't get copied because
			// of SkipUnchangedFiles=true
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed #2");
			}
			CheckCopyBuildItems (project, file_paths, target_path, "A2");
		}

		[Test]
		public void TestCopy_EmptySources () {
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='1'>
						<Copy SourceFiles='@(NonExistantSourceFiles)' DestinationFolder='$(TargetPath)' SkipUnchangedFiles='true' >
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
					</Target>
				</Project>
			";
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			
			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}
		}

		[Test]
		public void TestCopy_EmptyDestFolder () {
			Engine engine;
			Project project;

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<SFiles Include='foo.txt'><Md>1</Md></SFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFolder='@(NonExistant)' DestinationFiles='@(NonExistant)' SkipUnchangedFiles='true' >
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
					</Target>
				</Project>
			";
			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);
			if (project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build should have failed");
			}
		}

		[Test]
		public void TestCopy_ReadOnlyUpdate ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (source_path, "copyro.txt");
			string target_file = Path.Combine (target_path, "copyro.txt");			

			using (File.CreateText (file_path)) { }
			
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup><DestFile>" + target_file + @"</DestFile></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_path + @"'><Md>1</Md></SFiles>
						<DFiles Include='$(DestFile)'><Mde>2</Mde></DFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFiles='@(DFiles)' >
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
						<Message Text=""I0 : @(I0), I1: @(I1)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				testLogger.DumpMessages ();
				Assert.Fail ("Build failed");
			}
			Assert.IsTrue (File.Exists (target_file), "A2");
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				Assert.AreEqual (FileAttributes.Normal, File.GetAttributes (target_file), "A3");
			else
				// On Windows the Archive attribute will be set, not the Normal attribute.
				Assert.AreEqual (FileAttributes.Archive, File.GetAttributes (target_file), "A3");
		}

		[Test]
		public void TestCopy_OverwriteReadOnlyTrue ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (source_path, "copyro1.txt");
			string target_file = Path.Combine (target_path, "copyro1.txt");			

			using (File.CreateText (file_path)) { }
			using (File.CreateText (target_file)) { }

			File.SetAttributes (target_file, FileAttributes.ReadOnly);
			Assert.AreEqual (FileAttributes.ReadOnly, File.GetAttributes (target_file), "A1");
			
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<PropertyGroup><DestFile>" + target_file + @"</DestFile></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_path + @"'><Md>1</Md></SFiles>
						<DFiles Include='$(DestFile)'><Mde>2</Mde></DFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFiles='@(DFiles)' OverwriteReadOnlyFiles='true'>
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
						<Message Text=""I0 : @(I0), I1: @(I1)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			if (!project.Build ("1")) {
				var sb = new StringBuilder ();
				testLogger.DumpMessages (sb);
				Assert.Fail ("Build failed " + sb.ToString ());
			}
			Assert.IsTrue (File.Exists (target_file), "A2");
			var target_file_attrs = File.GetAttributes (target_file);
			if (Environment.OSVersion.Platform == PlatformID.Unix)
				Assert.AreEqual (FileAttributes.Normal, File.GetAttributes (target_file), "A3");
			else
				// On Windows the Archive attribute will be set, not the Normal attribute.
				Assert.AreEqual (FileAttributes.Archive, File.GetAttributes (target_file), "A3");
		}

		[Test]
		public void TestCopy_OverwriteReadOnlyFalse ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (source_path, "copyro2.txt");
			string target_file = Path.Combine (target_path, "copyro2.txt");			

			using (File.CreateText (file_path)) { }
			using (File.CreateText (target_file)) { }

			File.SetAttributes (target_file, FileAttributes.ReadOnly);
			Assert.AreEqual (FileAttributes.ReadOnly, File.GetAttributes (target_file), "A1");
			
			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup><DestFile>" + target_file + @"</DestFile></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_path + @"'><Md>1</Md></SFiles>
						<DFiles Include='$(DestFile)'><Mde>2</Mde></DFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFiles='@(DFiles)'>
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
						<Message Text=""I0 : @(I0), I1: @(I1)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			// build should fail because of the readonly target file
			Assert.IsFalse (project.Build ("1"));
			
			File.SetAttributes (target_file, FileAttributes.Normal);
		}

		[Test]
		public void TestCopy_Retries ()
		{
			Engine engine;
			Project project;
			string file_path = Path.Combine (source_path, "copyretries.txt");
			string target_file = Path.Combine (target_path, "copyretries.txt");			

			using (File.CreateText (file_path)) { }
			using (File.CreateText (target_file)) { }

			File.SetAttributes (target_file, FileAttributes.ReadOnly);
			Assert.AreEqual (FileAttributes.ReadOnly, File.GetAttributes (target_file), "A1");

			string documentString = @"
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup><DestFile>" + target_file + @"</DestFile></PropertyGroup>
					<ItemGroup>
						<SFiles Include='" + file_path + @"'><Md>1</Md></SFiles>
						<DFiles Include='$(DestFile)'><Mde>2</Mde></DFiles>
					</ItemGroup>
					<Target Name='1'>
						<Copy SourceFiles='@(SFiles)' DestinationFiles='@(DFiles)' Retries='3' RetryDelayMilliseconds='2000'>
							<Output TaskParameter='CopiedFiles' ItemName='I0'/>
							<Output TaskParameter='DestinationFiles' ItemName='I1'/>
						</Copy>
						<Message Text=""I0 : @(I0), I1: @(I1)""/>
					</Target>
				</Project>
			";

			engine = new Engine (Consts.BinPath);
			project = engine.CreateNewProject ();

			TestMessageLogger testLogger = new TestMessageLogger ();
			engine.RegisterLogger (testLogger);

			project.LoadXml (documentString);

			// remove the read-only flag from the file after a few secs,
			// so copying works after retries
			new Thread ( () => {
				Thread.Sleep (3000);
				File.SetAttributes (target_file, FileAttributes.Normal);
			}).Start ();

			if (!project.Build ("1")) {
				var sb = new StringBuilder ();
				testLogger.DumpMessages (sb);
				Assert.Fail ("Build failed " + sb.ToString ());
			}

			testLogger.CheckLoggedAny ("Copying failed. Retries left: 3.", MessageImportance.Normal, "A2");
		}

		void CheckCopyBuildItems (Project project, string [] source_files, string destination_folder, string prefix)
		{
			int num = source_files.Length;
			for (int i = 0; i < num; i ++)
				Assert.IsTrue (File.Exists (source_files [i]), prefix + " C1");

			BuildItemGroup big = project.GetEvaluatedItemsByName ("I0");

			Assert.AreEqual (num, big.Count, prefix + " C2");
			for (int i = 0; i < num; i++) {
				string suffix = (i + 1).ToString ();
				BuildItem bi = big [i];
				Assert.AreEqual (Path.Combine (destination_folder, Path.GetFileName (source_files [i])),
					bi.FinalItemSpec, prefix + " C3 #" + suffix);

				Assert.AreEqual (suffix, bi.GetMetadata ("Md"), prefix + " C4 #" + suffix);
			}

			big = project.GetEvaluatedItemsByName ("I1");
			Assert.AreEqual (num, big.Count, prefix + " C6");
			for (int i = 0; i < num; i++) {
				string suffix = (i + 1).ToString ();
				BuildItem bi = big [i];
				Assert.AreEqual (Path.Combine (destination_folder, Path.GetFileName (source_files [i])),
					bi.FinalItemSpec, prefix + " C7 #" + suffix);
				Assert.AreEqual (suffix, bi.GetMetadata ("Md"), prefix + " C8 #" + suffix);
			}
		 }
	}
}
