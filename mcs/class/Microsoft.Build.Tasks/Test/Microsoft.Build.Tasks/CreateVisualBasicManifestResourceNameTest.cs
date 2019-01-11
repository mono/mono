
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;
using Microsoft.Build.BuildEngine;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class CreateVisualBasicManifestResourceNameTest
	{

		string [,] resx_no_culture_files, resx_with_culture_files;
		string [,] non_resx_no_culture_files, non_resx_with_culture_files; 
		public CreateVisualBasicManifestResourceNameTest ()
		{
			string junk_file = TestResourceHelper.GetFullPathOfResource ("Test/resources/junk.txt");
			string sample_vb_path = TestResourceHelper.GetFullPathOfResource ("Test/resources/Sample.vb");

			/* {Include, LogicalName, DependentUpon, TargetPath} */
			resx_no_culture_files = new string [,] {
				// With dependent file
				{ "foo with space.resx", null, sample_vb_path, null },
				{ "foo with space.resx", "RandomName", sample_vb_path, null },
				{ "foo with space.resx", "RandomName", sample_vb_path, "bar with space.resx" },

				// can't find a C# class in the .vb file
				{ "foo with space.resx", "RandomName", junk_file, "bar with space.resx" },

				{ TestResourceHelper.GetFullPathOfResource ("Test/resources/foo with space.resx"), null, "Sample.vb", null },
				{ TestResourceHelper.GetFullPathOfResource ("Test/resources/foo with space.resx"), "RandomName", "Sample.vb", null },
				{ TestResourceHelper.GetFullPathOfResource ("Test/resources/foo with space.resx"), "RandomName", "Sample.vb", "bar with space.resx"},

				// W/o dependent file
				{ "foo with space.resx", null, null, null },
				{ "foo with space.resx", "RandomName", null, null },

				{ "Test/resources folder/foo with space.resx", null, null, null },
				{ "Test/resources folder/foo with space.resx", "RandomName", null, null },
			};

			resx_with_culture_files = new string [,] {
				// With dependent file
				{ "foo with space.de.resx", null, sample_vb_path, null },
				{ "foo with space.de.resx", "RandomName", sample_vb_path, null },
				{ "foo with space.de.resx", "RandomName", sample_vb_path, "bar with space.fr.resx" },

				// can't find a C# class in the .vb file
				{ "foo with space.de.resx", "RandomName", junk_file, "bar with space.fr.resx" },

				{ TestResourceHelper.GetFullPathOfResource ("Test/resources/foo with space.de.resx"), null, "Sample.vb", null },
				{ TestResourceHelper.GetFullPathOfResource ("Test/resources/foo with space.de.resx"), "RandomName", "Sample.vb", null},

				// W/o dependent file
				{ "foo with space.de.resx", null, null, null },
				{ "foo with space.de.resx", "RandomName", null, null },

				{ "Test/resources folder/foo with space.de.resx", null, null, null },
				{ "Test/resources folder/foo with space.de.resx", "RandomName", null, null }
			};

			non_resx_no_culture_files = new string [,] {
				{ "foo with space.txt", null, null },
				{ "foo with space.txt", "RandomName", null },

				{ "Test/resources folder/foo with space.txt", null, null },
				{ "Test/resources folder/foo with space.txt", "RandomName", null }
			};

			non_resx_with_culture_files = new string [,] {
				{ "foo with space.de.txt", null, null },
				{ "foo with space.de.txt", "RandomName", null },

				{ "Test/resources folder/foo with space.de.txt", null, null },
				{ "Test/resources folder/foo with space.de.txt", "RandomName", null }
			};

		}

		[Test]
		public void TestNoRootNamespaceNoCulture ()
		{
			CheckResourceNames (resx_no_culture_files, new string [] {
				// w/ dependent file
				"Mono.Tests.Sample", "Mono.Tests.Sample",
				"Mono.Tests.Sample", "bar with space",
				"Mono.Tests.Sample", "Mono.Tests.Sample",

				// W/o dependent file
				"Mono.Tests.Sample", "foo with space" ,
				"foo with space", "foo with space",
				"foo with space",
			}, null);
		}

		[Test]
		public void TestWithRootNamespaceNoCulture ()
		{
			//FIXME: How does LogicalName affect things??
			CheckResourceNames (resx_no_culture_files, new string [] {
				// With dependent file
				"RN1.RN2.Mono.Tests.Sample", "RN1.RN2.Mono.Tests.Sample",
				"RN1.RN2.Mono.Tests.Sample", "RN1.RN2.bar with space",
				"RN1.RN2.Mono.Tests.Sample", "RN1.RN2.Mono.Tests.Sample",

				// W/o dependent file
				"RN1.RN2.Mono.Tests.Sample", "RN1.RN2.foo with space",
				"RN1.RN2.foo with space", "RN1.RN2.foo with space",
				"RN1.RN2.foo with space"},
				"RN1.RN2");
		}

		[Test]
		public void TestNoRootNamespaceWithCulture ()
		{
			CheckResourceNames (resx_with_culture_files, new string [] {
				// With dependent file
				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",
				 "Mono.Tests.Sample.fr", "bar with space.fr",

				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",

				// W/o dependent file
				 "foo with space.de", "foo with space.de",
				 "foo with space.de", "foo with space.de" }, null);
		}

		[Test]
		public void TestWithRootNamespaceWithCulture ()
		{
			CheckResourceNames (resx_with_culture_files, new string [] {
				// With dependent file
				 "RN1.RN2.Mono.Tests.Sample.de", "RN1.RN2.Mono.Tests.Sample.de",
				 "RN1.RN2.Mono.Tests.Sample.fr", "RN1.RN2.bar with space.fr",
				 "RN1.RN2.Mono.Tests.Sample.de", "RN1.RN2.Mono.Tests.Sample.de",
				// W/o dependent file
				 "RN1.RN2.foo with space.de", "RN1.RN2.foo with space.de",
				 "RN1.RN2.foo with space.de", "RN1.RN2.foo with space.de"},
				 "RN1.RN2");
		}

		[Test]
		public void TestNonResxNoRootNamespaceWithCulture ()
		{
			CheckResourceNames (non_resx_with_culture_files, new string [] {
				Path.Combine ("de", "foo with space.txt"), Path.Combine ("de", "foo with space.txt"),
				Path.Combine ("de", "foo with space.txt"), Path.Combine ("de", "foo with space.txt")}, null);

		}

		[Test]
		public void TestNonResxWithRootNamespaceWithCulture ()
		{
			CheckResourceNames (non_resx_with_culture_files, new string [] {
				Path.Combine ("de", "RN1.RN2.foo with space.txt"), Path.Combine ("de", "RN1.RN2.foo with space.txt"),
				Path.Combine ("de", "RN1.RN2.foo with space.txt"), Path.Combine ("de", "RN1.RN2.foo with space.txt")},
				"RN1.RN2");
		}

		[Test]
		public void TestNonResxNoRootNamespaceNoCulture ()
		{
			CheckResourceNames (non_resx_no_culture_files, new string [] {
				"foo with space.txt", "foo with space.txt",
				"foo with space.txt", "foo with space.txt"}, null);
		}

		[Test]
		public void TestNonResxWithRootNamespaceNoCulture ()
		{
			CheckResourceNames (non_resx_no_culture_files, new string [] {
				// With dependent file
				"RN1.RN2.foo with space.txt", "RN1.RN2.foo with space.txt",
				"RN1.RN2.foo with space.txt", "RN1.RN2.foo with space.txt"},
				"RN1.RN2");
		}

		[Test]
		public void TestExternalResourcesNoRootNamespaceWithTargetPath ()
		{
			CheckResourceNames (new string[,] {
					{"../folder/foo.txt", null, null, "abc.txt"},
					{"../folder/foo.de.txt", null, null, "xyz.txt"}},
					new string[] { "abc.txt", "xyz.txt" }, null);
		}

		[Test]
		public void TestExternalResourcesWithRootNamespaceWithTargetPath ()
		{
			CheckResourceNames (new string[,] {
					{"../folder/foo.txt", null, null, "abc.txt"},
					{"../folder/foo.de.txt", null, null, "xyz.txt"}},
					new string[] { "RN.abc.txt", "RN.xyz.txt" }, "RN");
		}

		[Test]
		public void TestExternalResourcesNoRootNamespaceNoTargetPath ()
		{
			CheckResourceNames (new string[,] {
					{"../folder/foo.txt", null, null},
					{"../folder/foo.de.txt", null, null}},
					new string[] { "foo.txt", Path.Combine ("de", "foo.txt") }, null);
		}

		[Test]
		public void TestExternalResourcesWithRootNamespaceNoTargetPath ()
		{
			CheckResourceNames (new string[,] {
					{"../folder/foo.txt", null, null},
					{"../folder/foo.de.txt", null, null}},
					new string[] { "RN.foo.txt", Path.Combine ("de", "RN.foo.txt") }, "RN");
		}

		[Test]
		public void TestInvalidCulture ()
		{
			string [,] files = new string [,] {
				{ "Foo.invalid.txt", null, null },
				{ "Foo.invalid.resx", null, null }
			};
			CheckResourceNames (files, new string [] {"RN1.RN2.Foo.invalid.txt", "RN1.RN2.Foo.invalid"},
				"RN1.RN2");
		}

		void CheckResourceNames (string [,] files, string [] names, string rootNamespace)
		{
			Assert.AreEqual (files.GetUpperBound (0) + 1, names.Length, "Number of files and names must match");
			string projectText = CreateProject (files, rootNamespace);

			Engine engine = new Engine (Consts.BinPath);
			Project project = engine.CreateNewProject ();
			TestMessageLogger logger = new TestMessageLogger ();
			engine.RegisterLogger (logger);
			project.LoadXml (projectText);
			if (!project.Build ("1")) {
				Console.WriteLine (projectText);
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			bool has_targetpaths = files.GetUpperBound (1) == 3;
			BuildItemGroup group = project.GetEvaluatedItemsByName ("ResourceNames");
			Assert.AreEqual (names.Length, group.Count, "A2");
			for (int i = 0; i <= files.GetUpperBound (0); i++) {
				Assert.AreEqual (names [i], group [i].FinalItemSpec, "A3 #" + (i + 1));
				Assert.AreEqual (files [i, 1] != null, group [i].HasMetadata ("LogicalName"), "A4 #" + (i + 1));
				if (files [i, 1] != null)
					Assert.AreEqual (files [i, 1], group [i].GetMetadata ("LogicalName"), "A5 #" + (i + 1));
				Assert.AreEqual (files [i, 2] != null, group [i].HasMetadata ("DependentUpon"), "A6 #" + (i + 1));
				if (files [i, 2] != null)
					Assert.AreEqual (files [i, 2], group [i].GetMetadata ("DependentUpon"), "A7 #" + (i + 1));
				if (has_targetpaths && files [i, 3] != null)
					Assert.AreEqual (files [i, 3], group [i].GetMetadata ("TargetPath"), "A8 #" + (i + 1));
			}
		}

		string CreateProject (string [,] files, string rootNamespace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");

			bool has_targetpaths = files.GetUpperBound (1) == 3;
			sb.Append ("\t<ItemGroup>\n");
			for (int i = 0; i <= files.GetUpperBound (0); i ++) {
				sb.AppendFormat ("\t\t<ResourceFiles Include = \"{0}\">\n", files [i, 0]);
				if (has_targetpaths && files [i, 3] != null)
					sb.AppendFormat ("\t\t\t<TargetPath>{0}</TargetPath>\n", files [i, 3]);
				if (files [i, 1] != null)
					sb.AppendFormat ("\t\t\t<LogicalName>{0}</LogicalName>\n", files [i, 1]);
				if (files [i, 2] != null)
					sb.AppendFormat ("\t\t\t<DependentUpon>{0}</DependentUpon>\n", files [i, 2]);
				sb.AppendFormat ("\t\t</ResourceFiles>\n");
			}
			sb.Append ("\t</ItemGroup>\n");

			sb.Append ("\t<Target Name=\"1\">\n");
			sb.Append ("\t\t<CreateVisualBasicManifestResourceName ResourceFiles=\"@(ResourceFiles)\" ");
			if (rootNamespace != null)
				sb.AppendFormat (" RootNamespace = \"{0}\"", rootNamespace);
			sb.Append (">\n \t\t\t<Output TaskParameter=\"ManifestResourceNames\" ItemName=\"ResourceNames\" />\n");
			sb.Append ("\t\t</CreateVisualBasicManifestResourceName>\n\t</Target>\n");
			sb.Append ("\t" + GetUsingTask ("CreateVisualBasicManifestResourceName"));
			sb.Append ("</Project>");

			return sb.ToString ();
		}
		
		string GetUsingTask (string taskName)
		{
			return "<UsingTask TaskName='Microsoft.Build.Tasks." + taskName + "' AssemblyFile='" + Consts.GetTasksAsmPath () + "' />";
		}

	}
}
