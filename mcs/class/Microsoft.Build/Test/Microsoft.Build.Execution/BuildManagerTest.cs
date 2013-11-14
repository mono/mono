//
// BuildManagerTest.cs
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetProjectInstanceForBuildNullFullPath ()
		{
			string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, path);
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			var manager = new BuildManager ();
			manager.GetProjectInstanceForBuild (proj);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetProjectInstanceForBuildEmptyFullPath ()
		{
			string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, path);
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			proj.FullPath = "";
			var manager = new BuildManager ();
			manager.GetProjectInstanceForBuild (proj);
		}
		
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

