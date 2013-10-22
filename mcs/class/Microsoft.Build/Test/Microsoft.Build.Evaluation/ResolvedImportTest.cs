using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ResolvedImportTest
	{
		[Test]
		public void SimpleImportAndSemanticValues ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='test_imported.proj' />
</Project>";
			string imported = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>x</A>
    <B>y</B>
  </PropertyGroup>
  <ItemGroup>
    <X Include=""included.txt"" />
  </ItemGroup>
</Project>";
			using (var ts = File.CreateText ("test_imported.proj"))
				ts.Write (imported);
			try {
				var reader = XmlReader.Create (new StringReader (xml));
				var root = ProjectRootElement.Create (reader);
				Assert.AreEqual ("test_imported.proj", root.Imports.First ().Project, "#1");
				var proj = new Project (root);
				var prop = proj.GetProperty ("A");
				Assert.IsNotNull (prop, "#2");
				Assert.IsTrue (prop.IsImported, "#3");
				var item = proj.GetItems ("X").FirstOrDefault ();
				Assert.IsNotNull (item, "#4");
				Assert.AreEqual ("included.txt", item.EvaluatedInclude, "#5");
			} finally {
				File.Delete ("imported.proj");
			}
		}

			string import_overrides_test_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>X</A>
  </PropertyGroup>
  <Import Condition=""{0}"" Project='imported.proj' />
  <PropertyGroup>
    <B>Y</B>
  </PropertyGroup>
</Project>";
			string import_overrides_test_imported = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <C Condition='$(A)==X'>c</C>
    <A>a</A>
    <B>b</B>
  </PropertyGroup>
  <ItemGroup>
    <X Include=""included.txt"" />
  </ItemGroup>
</Project>";

		void ImportAndPropertyOverrides (string label, string condition, string valueA, string valueB)
		{
			using (var ts = File.CreateText ("test_imported.proj"))
				ts.Write (import_overrides_test_imported);
			try {
				string xml = string.Format (import_overrides_test_xml, condition);
				var reader = XmlReader.Create (new StringReader (xml));
				var root = ProjectRootElement.Create (reader);
				var proj = new Project (root);
				var a = proj.GetProperty ("A");
				Assert.IsNotNull (a, label + "#2");
				Assert.AreEqual (valueA, a.EvaluatedValue, label + "#3");
				var b = proj.GetProperty ("B");
				Assert.IsNotNull (b, label + "#4");
				Assert.AreEqual (valueB, b.EvaluatedValue, label + "#5");
				var c = proj.GetProperty ("C");
				Assert.IsNotNull (b, label + "#6");
				Assert.AreEqual ("c", b.EvaluatedValue, label + "#7");
			} finally {
				File.Delete ("test_imported.proj");
			}
		}

		[Test]
		public void ImportAndPropertyOverrides ()
		{
			ImportAndPropertyOverrides ("[1]", "'True'", "a", "Y");
			ImportAndPropertyOverrides ("[2]", "A=='X'", "a", "Y"); // evaluated as true
			ImportAndPropertyOverrides ("[2]", "B=='Y'", "X", "Y"); // evaluated as false
			ImportAndPropertyOverrides ("[2]", "B=='b'", "X", "Y"); // of course not evaluated with imported value
		}
	}
}

