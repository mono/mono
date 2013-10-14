using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectPropertyTest
	{
		[Test]
		public void SetUnevaluatedValueOverwritesElementValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>Bar</Foo>
    <Item/>
  </PropertyGroup>
</Project>";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var pe = root.Properties.First ();
			Assert.AreEqual ("Bar", pe.Value, "#1");
			var proj = new Project (root);
			var prop = proj.Properties.First (p => p.Name == "Foo");
			Assert.AreEqual ("Bar", prop.UnevaluatedValue, "#2");
			prop.UnevaluatedValue = "x";
			Assert.AreEqual ("x", pe.Value, "#3");
		}
	}
}

