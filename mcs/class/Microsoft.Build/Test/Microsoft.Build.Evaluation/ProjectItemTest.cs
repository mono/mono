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
	public class ProjectItemTest
	{
		[Test]
		public void SetUnevaluatedInclude ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='foo/bar.txt' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.ItemGroups.Count, "#1");
			var g = root.ItemGroups.First ();
			Assert.AreEqual (1, g.Items.Count, "#2");
			var xitem = g.Items.First ();
			var proj = new Project (root);
			var item = proj.ItemsIgnoringCondition.First ();
			string inc = "foo/bar.txt";
			Assert.AreEqual (inc, xitem.Include, "#3");
			Assert.AreEqual (inc, item.UnevaluatedInclude, "#4");
			string inc2 = "foo/bar.ext.txt";
			item.UnevaluatedInclude = inc2;
			Assert.AreEqual (inc2, xitem.Include, "#5");
			Assert.AreEqual (inc2, item.UnevaluatedInclude, "#6");
		}
		
		[Test]
		public void Metadata ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemDefinitionGroup>
    <Foo>
      <prop1>value1</prop1>
    </Foo>
  </ItemDefinitionGroup>
  <ItemGroup>
    <Foo Include='foo/bar.txt'>
      <prop1>valueX1</prop1>
    </Foo>
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.ItemGroups.Count, "#1");
			var g = root.ItemGroups.First ();
			Assert.AreEqual (1, g.Items.Count, "#2");
			var proj = new Project (root);
			var item = proj.ItemsIgnoringCondition.First ();
			var meta = item.GetMetadata ("prop1");
			Assert.IsNotNull (meta, "#3");
			Assert.AreEqual ("valueX1", meta.UnevaluatedValue, "#4");
			Assert.IsNotNull (meta.Predecessor, "#5");
			Assert.AreEqual ("value1", meta.Predecessor.UnevaluatedValue, "#6");
		}
	}
}

