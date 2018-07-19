//
// ProjectCollectionTest.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
			root.FullPath = "ProjectCollectionTest.BuildDoesNotIncreaseCollectionContent.proj";
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
			Assert.AreEqual (0, pc.LoadedProjects.Count, "#1.1");
			
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
		public void GetLoadedProjectsSuccess2 ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			string path = Path.GetFullPath ("GetLoadedProjectsSuccess2.xml");
			var pc = new ProjectCollection ();
			
			using (var fs = File.CreateText (path))
				fs.Write (project_xml);
			try {
				var proj = pc.LoadProject (path);
				
				Assert.AreEqual (1, pc.GetLoadedProjects (path).Count, "#1"); // ok... LoadProject (with filename) adds it to the collection.
				Assert.AreEqual (proj, pc.GetLoadedProjects (path).First (), "#2");
			} finally {
				File.Delete (path);
			}
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

