//
// ProjectItemTest.cs
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
using Microsoft.Build.Execution;
using NUnit.Framework;
using System.Collections.Generic;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectItemTest
	{
		[Test]
		public void SetUnevaluatedInclude ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='foo/bar.txt' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.ItemGroups.Count, "#1");
			var g = root.ItemGroups.First ();
			Assert.AreEqual (1, g.Items.Count, "#2");
			var xitem = g.Items.First ();
			var proj = new Project (root);
			var item = proj.ItemsIgnoringCondition.First ();
			string inc = "foo/bar.txt";
			Assert.AreEqual (inc, xitem.Include, "#3");
			Assert.AreEqual (inc, item.UnevaluatedInclude, "#4");
			string inc2 = "foo/bar.ext.txt";
			item.UnevaluatedInclude = inc2;
			Assert.AreEqual (inc2, xitem.Include, "#5");
			Assert.AreEqual (inc2, item.UnevaluatedInclude, "#6");
		}
		
		void SetupTemporaryDirectoriesAndFiles ()
		{
			Directory.CreateDirectory ("Test/ProjectItemTestTemporary");
			Directory.CreateDirectory ("Test/ProjectItemTestTemporary/parent");
			Directory.CreateDirectory ("Test/ProjectItemTestTemporary/parent/dir1");
			Directory.CreateDirectory ("Test/ProjectItemTestTemporary/parent/dir2");
			File.CreateText ("Test/ProjectItemTestTemporary/x.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir1/a.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir1/a1.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir1/b.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir2/a2.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir2/a.cs").Close ();
			File.CreateText ("Test/ProjectItemTestTemporary/parent/dir2/b.cs").Close ();
		}
		
		void CleanupTemporaryDirectories ()
		{
			Directory.Delete ("Test/ProjectItemTestTemporary", true);
		}
		
		[Test]
		public void WildcardExpansion ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='" + "Test/ProjectItemTestTemporary/parent/dir*/a*.cs;Test/ProjectItemTestTemporary/x.cs".Replace ('/', Path.DirectorySeparatorChar) + @"' />
  </ItemGroup>
</Project>";
			try {
				SetupTemporaryDirectoriesAndFiles ();
				WildcardExpansionCommon (project_xml, false);
			} finally {
				CleanupTemporaryDirectories ();
			}
		}
		
		[Test]
		public void WildcardExpansionRecursive ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='" + "Test/ProjectItemTestTemporary/parent/**/a*.cs;Test/ProjectItemTestTemporary/x.cs".Replace ('/', Path.DirectorySeparatorChar) + @"' />
  </ItemGroup>
</Project>";
			try {
				SetupTemporaryDirectoriesAndFiles ();
				WildcardExpansionCommon (project_xml, true);
			} finally {
				CleanupTemporaryDirectories ();
			}
		}
		
		void WildcardExpansionCommon (string xmlString, bool hasRecursiveDir)
		{
			char sep = Path.DirectorySeparatorChar;
			var xml = XmlReader.Create (new StringReader (xmlString));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			var xitem = proj.Xml.Items.First ();
			// sort is needed because they are only sorted by ItemType.
			var items = proj.Items.OrderBy (p => p.EvaluatedInclude).ToArray ();
			Assert.AreEqual (5, items.Length, "#1");
			Assert.AreEqual ("Test/ProjectItemTestTemporary/parent/dir1/a.cs", items [0].EvaluatedInclude.Replace (Path.DirectorySeparatorChar, '/'), "#2");
			Assert.AreEqual ("a", items [0].GetMetadataValue ("Filename"), "#3");
			if (hasRecursiveDir)
				Assert.AreEqual ("dir1" + sep, items [0].GetMetadataValue ("RecursiveDir"), "#3.2");
			Assert.AreEqual ("Test/ProjectItemTestTemporary/parent/dir1/a1.cs", items [1].EvaluatedInclude.Replace (Path.DirectorySeparatorChar, '/'), "#4");
			Assert.AreEqual ("a1", items [1].GetMetadataValue ("Filename"), "#5");
			if (hasRecursiveDir)
				Assert.AreEqual ("dir1" + sep, items [1].GetMetadataValue ("RecursiveDir"), "#5.2");
			Assert.AreEqual ("Test/ProjectItemTestTemporary/parent/dir2/a.cs", items [2].EvaluatedInclude.Replace (Path.DirectorySeparatorChar, '/'), "#6");
			Assert.AreEqual ("a", items [2].GetMetadataValue ("Filename"), "#7");
			if (hasRecursiveDir)
				Assert.AreEqual ("dir2" + sep, items [2].GetMetadataValue ("RecursiveDir"), "#7.2");
			Assert.AreEqual ("Test/ProjectItemTestTemporary/parent/dir2/a2.cs", items [3].EvaluatedInclude.Replace (Path.DirectorySeparatorChar, '/'), "#8");
			Assert.AreEqual ("a2", items [3].GetMetadataValue ("Filename"), "#9");
			if (hasRecursiveDir)
				Assert.AreEqual ("dir2" + sep, items [3].GetMetadataValue ("RecursiveDir"), "#9.2");
			Assert.AreEqual ("Test/ProjectItemTestTemporary/x.cs", items [4].EvaluatedInclude.Replace (Path.DirectorySeparatorChar, '/'), "#10");
			for (int i = 0; i < items.Length; i++)
				Assert.AreEqual (xitem, items [i].Xml, "#11:" + i);
		}
		
		[Test]
		public void Metadata ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemDefinitionGroup>
    <Foo>
      <prop1>value1</prop1>
    </Foo>
  </ItemDefinitionGroup>
  <ItemGroup>
    <Foo Include='foo/bar.txt'>
      <prop1>valueX1</prop1>
    </Foo>
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual (1, root.ItemGroups.Count, "#1");
			var g = root.ItemGroups.First ();
			Assert.AreEqual (1, g.Items.Count, "#2");
			var proj = new Project (root);
			var item = proj.ItemsIgnoringCondition.First ();
			var meta = item.GetMetadata ("prop1");
			Assert.IsNotNull (meta, "#3");
			Assert.AreEqual ("valueX1", meta.UnevaluatedValue, "#4");
			Assert.IsNotNull (meta.Predecessor, "#5");
			Assert.AreEqual ("value1", meta.Predecessor.UnevaluatedValue, "#6");
			
			// Well-known metadata don't show up via GetMetadata(), but does show up via GetMetadataValue().
			Assert.AreEqual (null, item.GetMetadata ("Filename"), "#7");
			Assert.AreEqual ("bar", item.GetMetadataValue ("Filename"), "#8");
		}
		
		[Test]
		public void ExpandPropertyThenTrim ()
		{
			string test = "A\nB\nC\n    ";
			string project_xml = string.Format (@"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Test>{0}</Test>
    <Test2>$(TEST)</Test2>
  </PropertyGroup>
  <ItemGroup>
    <X Include='$(TEST)' />
    <X2 Include='$(TEST)z' />
  </ItemGroup>
</Project>", test);
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectItemTest.ExpandPropertyThenTrim.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual (test, proj.GetPropertyValue ("TEST"), "#1");
			Assert.AreEqual (test, proj.GetPropertyValue ("TEST2"), "#2");
			Assert.AreEqual (test.Trim (), proj.GetItems ("X").First ().EvaluatedInclude, "#3");			
			Assert.AreEqual (test + "z", proj.GetItems ("X2").First ().EvaluatedInclude, "#4");
		}
	}
}

