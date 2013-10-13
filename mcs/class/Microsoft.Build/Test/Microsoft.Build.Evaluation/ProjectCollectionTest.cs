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
	}
}

