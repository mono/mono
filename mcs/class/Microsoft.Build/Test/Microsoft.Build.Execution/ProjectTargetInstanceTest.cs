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
	public class ProjectTargetInstanceTest
	{
		[Test]
		public void DefaultTargetsEmpty ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			Assert.AreEqual (new string [0], proj.DefaultTargets, "#1");
		}
		
		[Test]
		public void DefaultTargetsFromAttribute ()
		{
            string project_xml = @"<Project DefaultTargets='Foo Bar Baz;Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			string [] expected = {"Foo Bar Baz", "Foo"};
			Assert.AreEqual (expected, proj.DefaultTargets, "#1");
		}
		
		[Test]
		public void DefaultTargetsFromElements ()
		{
			string [] defaultTargetAtts = {string.Empty, "DefaultTargets=''"};
			
			for (int i = 0; i < defaultTargetAtts.Length; i++) {
				string project_xml = string.Format (@"<Project {0} xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Target Name='Foo' />
	<Target Name='Bar' />
</Project>", defaultTargetAtts [i]);
	            var xml = XmlReader.Create (new StringReader (project_xml));
	            var root = ProjectRootElement.Create (xml);
	            var proj = new ProjectInstance (root);
				string [] expected = {"Foo"}; // Bar is not included
				Assert.AreEqual (expected, proj.DefaultTargets, "#1-" + i);
			}
		}
		
		[Test]
		public void MicrosoftCommonTargets ()
		{
			string [] defaultTargetAtts = { string.Empty, "DefaultTargets=''" };
			
			for (int i = 0; i < defaultTargetAtts.Length; i++) {
				string project_xml = string.Format (@"<Project {0} xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
</Project>", defaultTargetAtts [i]);
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				var proj = new ProjectInstance (root);
				Assert.AreEqual ("Build", proj.DefaultTargets.FirstOrDefault (), "#1-" + i);
			}
		}
		
		[Test]
		public void DefaultTargetsOverride ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			Assert.AreEqual ("Foo", proj.DefaultTargets.FirstOrDefault (), "#1");
		}
		
		[Test]
		public void MultipleDefaultTargets ()
		{
			bool[] expected = { true, false, true };
			string [] defaultTargets = {"Foo", "Foo;Bar", "Foo;Bar"};
				string [] targets = { string.Empty, string.Empty, "<Target Name='Bar' />" };
			for (int i = 0; i < expected.Length; i++) {
				string project_xml = string.Format (@"<Project DefaultTargets='{0}' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
	<Target Name='Foo' />
	{1}
</Project>", defaultTargets [i], targets [i]);
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				var proj = new ProjectInstance (root);
				Assert.AreEqual ("Foo", proj.DefaultTargets.FirstOrDefault (), "#1-" + i);
				Assert.AreEqual (expected [i], proj.Build (), "#2-" + i);
			}
		}
	}
}
