using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;
using Microsoft.Build.BuildEngine;

namespace MonoTests.Microsoft.Build.Tasks
{
	[TestFixture]
	public class CreateCSharpManifestResourceNameTest
	{

		string [,] resx_no_culture_files, resx_with_culture_files;
		string [,] non_resx_no_culture_files, non_resx_with_culture_files;

		public CreateCSharpManifestResourceNameTest ()
		{
			string sample_cs_path = Path.Combine ("Test", Path.Combine ("resources", "Sample.cs"));
			resx_no_culture_files = new string [,] {
				// With dependent file
				{ "foo.resx", null, sample_cs_path },
				{ "foo.resx", "RandomName", sample_cs_path },

				{ "Test/resources/foo.resx", null, "Sample.cs" },
				{ "Test/resources/foo.resx", "RandomName", "Sample.cs" },

				// W/o dependent file
				{ "foo.resx", null, null },
				{ "foo.resx", "RandomName", null },

				{ "Test/resources/foo.resx", null, null },
				{ "Test/resources/foo.resx", "RandomName", null },
			};

			resx_with_culture_files = new string [,] {
				// With dependent file
				{ "foo.de.resx", null, sample_cs_path },
				{ "foo.de.resx", "RandomName", sample_cs_path },

				{ "Test/resources/foo.de.resx", null, "Sample.cs" },
				{ "Test/resources/foo.de.resx", "RandomName", "Sample.cs" },

				// W/o dependent file
				{ "foo.de.resx", null, null },
				{ "foo.de.resx", "RandomName", null },

				{ "Test/resources/foo.de.resx", null, null },
				{ "Test/resources/foo.de.resx", "RandomName", null }
			};

			non_resx_no_culture_files = new string [,] {
				{ "foo.txt", null, null },
				{ "foo.txt", "RandomName", null },

				{ "Test/resources/foo.txt", null, null },
				{ "Test/resources/foo.txt", "RandomName", null }
			};

			non_resx_with_culture_files = new string [,] {
				{ "foo.de.txt", null, null },
				{ "foo.de.txt", "RandomName", null },

				{ "Test/resources/foo.de.txt", null, null },
				{ "Test/resources/foo.de.txt", "RandomName", null }
			};

		}

		[Test]
		public void TestNoRootNamespaceNoCulture ()
		{
			CheckResourceNames (resx_no_culture_files, new string [] {
				// w/ dependent file
				"Mono.Tests.Sample", "Mono.Tests.Sample",
				"Mono.Tests.Sample", "Mono.Tests.Sample",

				// W/o dependent file
				"foo", "foo" ,
				"Test.resources.foo", "Test.resources.foo"}, null);
		}

		[Test]
		public void TestWithRootNamespaceNoCulture ()
		{
			//FIXME: How does LogicalName affect things??
			CheckResourceNames (resx_no_culture_files, new string [] {
				// With dependent file
				"Mono.Tests.Sample", "Mono.Tests.Sample",
				"Mono.Tests.Sample", "Mono.Tests.Sample",
				// W/o dependent file
				"RN1.RN2.foo", "RN1.RN2.foo",
				"RN1.RN2.Test.resources.foo", "RN1.RN2.Test.resources.foo"},
				"RN1.RN2");
		}

		[Test]
		public void TestNoRootNamespaceWithCulture ()
		{
			CheckResourceNames (resx_with_culture_files, new string [] {
				// With dependent file
				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",
				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",
				// W/o dependent file
				 "foo.de", "foo.de",
				 "Test.resources.foo.de", "Test.resources.foo.de" }, null);
		}

		[Test]
		public void TestWithRootNamespaceWithCulture ()
		{
			CheckResourceNames (resx_with_culture_files, new string [] {
				// With dependent file
				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",
				 "Mono.Tests.Sample.de", "Mono.Tests.Sample.de",
				// W/o dependent file
				 "RN1.RN2.foo.de", "RN1.RN2.foo.de",
				 "RN1.RN2.Test.resources.foo.de", "RN1.RN2.Test.resources.foo.de"},
				 "RN1.RN2");
		}

		[Test]
		public void TestNonResxNoRootNamespaceWithCulture ()
		{
			CheckResourceNames (non_resx_with_culture_files, new string [] {
				Path.Combine ("de", "foo.txt"), Path.Combine ("de", "foo.txt"),
				Path.Combine ("de", "Test.resources.foo.txt"), Path.Combine ("de", "Test.resources.foo.txt")}, null);
		}

		[Test]
		public void TestNonResxWithRootNamespaceWithCulture ()
		{
			CheckResourceNames (non_resx_with_culture_files, new string [] {
				Path.Combine ("de", "RN1.RN2.foo.txt"), Path.Combine ("de", "RN1.RN2.foo.txt"),
				Path.Combine ("de", "RN1.RN2.Test.resources.foo.txt"), Path.Combine ("de", "RN1.RN2.Test.resources.foo.txt")},
				"RN1.RN2");
		}

		[Test]
		public void TestNonResxNoRootNamespaceNoCulture ()
		{
			CheckResourceNames (non_resx_no_culture_files, new string [] {
				"foo.txt", "foo.txt",
				"Test.resources.foo.txt", "Test.resources.foo.txt"}, null);
		}

		[Test]
		public void TestNonResxWithRootNamespaceNoCulture ()
		{
			CheckResourceNames (non_resx_no_culture_files, new string [] {
				// With dependent file
				"RN1.RN2.foo.txt", "RN1.RN2.foo.txt",
				"RN1.RN2.Test.resources.foo.txt", "RN1.RN2.Test.resources.foo.txt"},
				"RN1.RN2");
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
			Console.WriteLine (projectText);
			project.LoadXml (projectText);
			if (!project.Build ("1")) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

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
			}
		}

		string CreateProject (string [,] files, string rootNamespace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n");

			sb.Append ("\t<ItemGroup>\n");
			for (int i = 0; i <= files.GetUpperBound (0); i ++) {
				sb.AppendFormat ("\t\t<ResourceFiles Include = \"{0}\">\n", files [i, 0]);
				if (files [i, 1] != null)
					sb.AppendFormat ("\t\t\t<LogicalName>{0}</LogicalName>\n", files [i, 1]);
				if (files [i, 2] != null)
					sb.AppendFormat ("\t\t\t<DependentUpon>{0}</DependentUpon>\n", files [i, 2]);
				sb.AppendFormat ("\t\t</ResourceFiles>\n");
			}
			sb.Append ("\t</ItemGroup>\n");

			sb.Append ("\t<Target Name=\"1\">\n");
			sb.Append ("\t\t<CreateCSharpManifestResourceName ResourceFiles=\"@(ResourceFiles)\" ");
			if (rootNamespace != null)
				sb.AppendFormat (" RootNamespace = \"{0}\"", rootNamespace);
			sb.Append (">\n \t\t\t<Output TaskParameter=\"ManifestResourceNames\" ItemName=\"ResourceNames\" />\n");
			sb.Append ("\t\t</CreateCSharpManifestResourceName>\n\t</Target>\n");
			sb.Append ("\t<UsingTask TaskName=\"Microsoft.Build.Tasks.CreateCSharpManifestResourceName\" " +
				"AssemblyName=\"Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"/>\n");
			sb.Append ("</Project>");

			return sb.ToString ();
		}
	}
}
