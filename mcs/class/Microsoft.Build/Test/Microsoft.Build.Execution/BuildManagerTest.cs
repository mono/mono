using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildManagerTest
	{
		[Test]
		public void GetProjectInstanceForBuild ()
		{
            string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
            var path = "file://localhost/foo.xml";
            var xml = XmlReader.Create (new StringReader(empty_project_xml), null, path);
            var root = ProjectRootElement.Create (xml);
            root.FullPath = path;
            var proj = new Project (root);
            var manager = new BuildManager ();
            var inst = manager.GetProjectInstanceForBuild (proj);
            Assert.AreEqual (inst, manager.GetProjectInstanceForBuild (proj), "#1");
		}
	}
}

