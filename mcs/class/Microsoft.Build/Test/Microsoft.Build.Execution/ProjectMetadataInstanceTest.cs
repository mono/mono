using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectMetadataInstanceTest
	{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <X Include='foo.txt'>
      <M>m</M>
      <N>=</N>
    </X>
  </ItemGroup>
</Project>";

		[Test]
		public void PropertiesCopiesValues ()
		{
			var xml = XmlReader.Create (new StringReader (project_xml));
			string path = Path.GetFullPath ("foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var item = proj.Items.First ();
			var md = item.Metadata.First ();
			Assert.AreEqual ("m", item.Metadata.First ().EvaluatedValue, "#1");
			Assert.AreEqual ("m", root.ItemGroups.First ().Items.First ().Metadata.First ().Value, "#2");
			root.ItemGroups.First ().Items.First ().Metadata.First ().Value = "X";
			Assert.AreEqual ("m", item.Metadata.First ().EvaluatedValue, "#3");
		}
		
		[Test]
		public void ToStringOverride ()
		{
			var xml = XmlReader.Create (new StringReader (project_xml));
			string path = Path.GetFullPath ("foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var item = proj.Items.First ();
			Assert.AreEqual ("M=m", item.Metadata.First ().ToString (), "#1");
			Assert.AreEqual ("N==", item.Metadata.Last ().ToString (), "#2"); // haha
		}
	}
}

