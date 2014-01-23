//
// ProjectInstanceTest.cs
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
using Microsoft.Build.Execution;
using NUnit.Framework;
using Microsoft.Build.Evaluation;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectInstanceTest
	{
		[Test]
		public void ItemsAndProperties ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <X Condition='false' Include='bar.txt' />
    <X Include='foo.txt'>
      <M>m</M>
      <N>=</N>
    </X>
  </ItemGroup>
  <PropertyGroup>
    <P Condition='false'>void</P>
    <P Condition='true'>valid</P>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader(project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
            var item = proj.Items.First ();
			Assert.AreEqual ("foo.txt", item.EvaluatedInclude, "#1");
			var prop = proj.Properties.First (p => p.Name=="P");
			Assert.AreEqual ("valid", prop.EvaluatedValue, "#2");
			Assert.IsNotNull (proj.GetProperty ("MSBuildProjectDirectory"), "#3");
			Assert.AreEqual ("2.0", proj.ToolsVersion, "#4");
		}
		
		[Test]
		public void ExplicitToolsVersion ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
            var xml = XmlReader.Create (new StringReader(project_xml));
            var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root, null, "4.0", new ProjectCollection ());
			Assert.AreEqual ("4.0", proj.ToolsVersion, "#1");
		}
		
		[Test]
		public void BuildEmptyProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			// This seems to do nothing and still returns true
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.1.proj";
			Assert.IsTrue (new ProjectInstance (root).Build (), "#1");
			// This seems to fail to find the appropriate target
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.2.proj";
			Assert.IsFalse (new ProjectInstance (root).Build ("Build", null), "#2");
			// Thus, this tries to build all the targets (empty) and no one failed, so returns true(!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.3.proj";
			Assert.IsTrue (new ProjectInstance (root).Build (new string [0], null), "#3");
			// Actially null "targets" is accepted and returns true(!!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.4.proj";
			Assert.IsTrue (new ProjectInstance (root).Build ((string []) null, null), "#4");
			// matching seems to be blindly done, null string also results in true(!!)
			root.FullPath = "ProjectInstanceTest.BuildEmptyProject.5.proj";
			Assert.IsTrue (new ProjectInstance (root).Build ((string) null, null), "#5");
		}
	}
}

