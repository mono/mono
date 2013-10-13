using System;
using System.IO;
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
		public void Constructor ()
		{
			string project_xml_2 = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='AfterBuild'>
    <AspNetCompiler VirtualPath='temp' PhysicalPath='$(ProjectDir)' />
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml_2), null, "file://localhost/foo.xml");
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

