using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectInstanceTest
	{
		[Test]
		public void BuildEmptyProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			// This seems to do nothing and still returns true
			Assert.IsTrue (new ProjectInstance (root).Build (), "#1");
			// This seems to fail to find the appropriate target
			Assert.IsFalse (new ProjectInstance (root).Build ("Build", null), "#2");
			// Thus, this tries to build all the targets (empty) and no one failed, so returns true(!)
			Assert.IsTrue (new ProjectInstance (root).Build (new string [0], null), "#3");
			// Actially null "targets" is accepted and returns true(!!)
			Assert.IsTrue (new ProjectInstance (root).Build ((string []) null, null), "#4");
			// matching seems to be blindly done, null string also results in true(!!)
			Assert.IsTrue (new ProjectInstance (root).Build ((string) null, null), "#5");
		}
	}
}

