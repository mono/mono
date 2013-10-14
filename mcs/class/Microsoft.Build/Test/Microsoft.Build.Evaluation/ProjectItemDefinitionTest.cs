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
	public class ProjectItemDefinitionTest
	{
		[Test]
		public void Properties ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemDefinitionGroup>
    <Foo>
    	<prop1>value1</prop1>
    	<prop2>value1</prop2>
    </Foo>
    <!-- This one is merged into existing Foo definition above. -->
    <Foo>
    	<prop1>valueX1</prop1><!-- this one overrides value1. -->
    	<prop3>value3</prop3>
    </Foo>
  </ItemDefinitionGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			Assert.AreEqual (1, proj.ItemDefinitions.Count, "#1"); // Foo
			var def = proj.ItemDefinitions ["Foo"];
			Assert.AreEqual (3, def.MetadataCount, "#2");
			var md1 = def.Metadata.First (m => m.Name == "prop1");
			Assert.AreEqual ("valueX1", md1.UnevaluatedValue, "#3");
			// FIXME: enable it once we implemented it.
			//Assert.AreEqual ("valueX1", md1.EvaluatedValue, "#4");
			Assert.IsNotNull (md1.Predecessor, "#5");
			Assert.AreEqual ("value1", md1.Predecessor.UnevaluatedValue, "#6");
			// FIXME: enable it once we implemented it.
			//Assert.AreEqual ("value1", md1.Predecessor.EvaluatedValue, "#7");
		}
	}
}

