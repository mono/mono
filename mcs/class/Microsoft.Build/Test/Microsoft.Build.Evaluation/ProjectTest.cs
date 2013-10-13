using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectTest
	{
		[Test]
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
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#3");
			prop = proj.Properties.First (p => p.Name == "Baz");
			Assert.AreEqual ("$(FOO)", prop.UnevaluatedValue, "#4");
			Assert.AreEqual (value, prop.EvaluatedValue, "#5");
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#6");
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
	}
}

