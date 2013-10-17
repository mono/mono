using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectTest
	{
		[Test]
		[Category ("NotWorking")]
		public void EscapeDoesWTF ()
		{
			string value = "What are 'ESCAPE' &amp; \"EVALUATE\" ? $ # % ^";
			string escaped_by_collection = "What are %27ESCAPE%27 & \"EVALUATE\" %3f %24 # %25 ^";
			string xml = string.Format (@"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<PropertyGroup>
		<Foo>{0}</Foo>
		<Baz>$(FOO)</Baz>
	</PropertyGroup>
</Project>", value);
			var path = "file://localhost/foo.xml";
			var reader = XmlReader.Create (new StringReader (xml), null, path);
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			var prop = proj.Properties.First (p => p.Name == "Foo");
			Assert.AreEqual (value, prop.UnevaluatedValue, "#1");
			Assert.AreEqual (value, prop.EvaluatedValue, "#2");
			// eh?
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#3");
			prop = proj.Properties.First (p => p.Name == "Baz");
			Assert.AreEqual ("$(FOO)", prop.UnevaluatedValue, "#4");
			Assert.AreEqual (value, prop.EvaluatedValue, "#5");
			// eh?
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#6");
			
			// OK you are fine.
			Assert.AreEqual (escaped_by_collection, ProjectCollection.Escape (value), "#7");
		}
		
		[Test]
		public void FullPath ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			proj.FullPath = "ABC";
			Assert.IsTrue (proj.FullPath.EndsWith (Path.DirectorySeparatorChar + "ABC"), "#1");
			Assert.AreEqual (root.FullPath, proj.FullPath, "#2");
		}
		
		[Test]
		public void BuildEmptyProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
            
			// This seems to do nothing and still returns true
			Assert.IsTrue (new Project (root).Build (), "#1");
			// This seems to fail to find the appropriate target
			Assert.IsFalse (new Project (root).Build ("Build", null), "#2");
			// Thus, this tries to build all the targets (empty) and no one failed, so returns true(!)
			Assert.IsTrue (new Project (root).Build (new string [0], null), "#3");
			// Actially null "targets" is accepted and returns true(!!)
			Assert.IsTrue (new Project (root).Build ((string []) null, null), "#4");
			// matching seems to be blindly done, null string also results in true(!!)
			Assert.IsTrue (new Project (root).Build ((string) null, null), "#5");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		[Category ("NotWorking")]
		public void LoadInvalidProjectForBadCondition ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>What are 'ESCAPE' &amp; ""EVALUATE"" ? $ # % ^</Foo>
    <!-- Note that this contains invalid Condition expression, yet ProjectElement.Create() does NOT fail. -->
    <Baz Condition=""$(Void)=="">$(FOO)</Baz>
  </PropertyGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (xml));
			var root = ProjectRootElement.Create (reader);
			new Project (root);
		}
		
		[Test]
		public void ExpandString ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>What are 'ESCAPE' &amp; ""EVALUATE"" ? $ # % ^</Foo>
    <Bar>y</Bar>
    <Baz Condition=""$(Void)==''"">$(FOO)</Baz>
  </PropertyGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (xml));
			var root = ProjectRootElement.Create (reader);
			var proj = new Project (root);
			Assert.AreEqual ("xyz", proj.ExpandString ("x$(BAR)z"), "#1");
			Assert.AreEqual ("x$(BARz", proj.ExpandString ("x$(BARz"), "#2"); // incomplete
			Assert.AreEqual ("xz", proj.ExpandString ("x@(BAR)z"), "#3"); // not an item
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BuildCSharpTargetGetFrameworkPaths ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new Project (root);
			Assert.IsTrue (proj.Build ("GetFrameworkPaths", new ILogger [] {new ConsoleLogger ()}));
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BuildCSharpTargetBuild ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new Project (root);
			Assert.IsFalse (proj.Build ("Build", new ILogger [] {new ConsoleLogger ()})); // missing mandatory properties
		}
		
		[Test]
		public void EvaluateIncludeAsEmptyThenIgnored ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='' />
    <Bar />
  </ItemGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new Project (root);
            // note that Foo is ignored.
			Assert.AreEqual (1, proj.ItemsIgnoringCondition.Count, "#1");
			Assert.IsNotNull ("Bar", proj.ItemsIgnoringCondition.First ().ItemType, "#2");
		}
	}
}

