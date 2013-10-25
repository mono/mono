using System;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NUnit.Framework;
using Microsoft.Build.Execution;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectCollectionTest
	{
		[Test]
		public void GlobalProperties ()
		{
			var g = ProjectCollection.GlobalProjectCollection;
			Assert.AreEqual (0, g.GlobalProperties.Count, "#1");
			Assert.IsTrue (g.GlobalProperties.IsReadOnly, "#2");
		}
		
		[Test]
		public void DefaultToolsVersion ()
		{
			var pc = ProjectCollection.GlobalProjectCollection;
			Assert.AreEqual (pc.Toolsets.First ().ToolsVersion, pc.DefaultToolsVersion, "#1");
		}
		
		[Test]
		public void Toolsets ()
		{
			var pc = ProjectCollection.GlobalProjectCollection;
			Assert.IsNotNull (pc.Toolsets, "#1-1");
			Assert.IsTrue (pc.Toolsets.Any (), "#1-2");
			pc = new ProjectCollection ();
			Assert.IsNotNull (pc.Toolsets, "#2-1");
			Assert.IsTrue (pc.Toolsets.Any (), "#2-2");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BuildDoesNotIncreaseCollectionContent ()
		{
            string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
            var xml = XmlReader.Create (new StringReader (empty_project_xml));
            var root = ProjectRootElement.Create (xml);
            var coll = new ProjectCollection ();
            var inst = new ProjectInstance (root, null, null, coll);
            Assert.AreEqual (0, coll.Count, "#1");
            inst.Build ();
            Assert.AreEqual (0, coll.Count, "#2");
		}
		
		[Test]
		public void GetLoadedProjectsWithoutFullPath ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			string path = Path.GetFullPath ("foo.xml");
			var pc = new ProjectCollection ();
			
			pc.LoadProject (XmlReader.Create (new StringReader (project_xml), null, path));
			Assert.AreEqual (0, pc.GetLoadedProjects (path).Count, "#1"); // huh?
			
			new Project (root, null, null, pc);
			Assert.AreEqual (0, pc.GetLoadedProjects (path).Count, "#2"); // huh?
		}
			
		[Test]
		public void GetLoadedProjectsSuccess ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			string path = Path.GetFullPath ("foo.xml");
			var pc = new ProjectCollection ();
			
			var proj = new Project (root, null, null, pc);
			// this order also matters for test; It sets FullPath after Project.ctor(), and should still work.
			root.FullPath = "foo.xml";
			
			Assert.AreEqual (1, pc.GetLoadedProjects (path).Count, "#1"); // wow ok...
			Assert.AreEqual (proj, pc.GetLoadedProjects (path).First (), "#2");
		}
			
		[Test]
		public void GetLoadedProjectsForProjectInstance ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			string path = Path.GetFullPath ("foo.xml");
			var pc = new ProjectCollection ();
			root.FullPath = "foo.xml";
			
			new ProjectInstance (root, null, null, pc);			
			Assert.AreEqual (0, pc.GetLoadedProjects (path).Count, "#1"); // so, ProjectInstance does not actually load Project...
		}
	}
}

