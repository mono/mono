using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;
using System.Collections.Generic;

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
    <X>1</X>
    <X>2</X>
    <PATH>overriden</PATH>
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var pe = root.Properties.First ();
			Assert.AreEqual ("Bar", pe.Value, "#1");
			var proj = new Project (root);
			var prop = proj.Properties.First (p => p.Name == "Foo");
			Assert.AreEqual ("Bar", prop.UnevaluatedValue, "#2");
			prop.UnevaluatedValue = "x";
			Assert.AreEqual ("x", pe.Value, "#3");
			
			prop = proj.Properties.First (p => p.Name == "X");
			Assert.AreEqual ("2", prop.UnevaluatedValue, "#4");
			Assert.IsNotNull (prop.Predecessor, "#5");
			Assert.AreEqual ("1", prop.Predecessor.UnevaluatedValue, "#6");
			
			// environment property could also be Predecessor (and removed...maybe.
			// I could reproduce only NRE = .NET bug with environment property so far.)
			prop = proj.Properties.First (p => p.Name == "PATH");
			Assert.AreEqual ("overriden", prop.UnevaluatedValue, "#7");
			Assert.IsNotNull (prop.Predecessor, "#8");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UpdateGlobalPropertyValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var props = new Dictionary<string, string> ();
			props.Add ("GP", "GV");
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root, props, null);
			var pe = proj.Properties.First (p => p.IsGlobalProperty);
			pe.UnevaluatedValue = "UPDATED";
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UpdateEnvironmentPropertyValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			var pe = proj.Properties.First (p => p.IsEnvironmentProperty);
			pe.UnevaluatedValue = "UPDATED";
		}
	}
}

