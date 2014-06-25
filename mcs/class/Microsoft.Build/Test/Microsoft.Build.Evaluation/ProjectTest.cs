//
// ProjectTest.cs
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
using Microsoft.Build.Exceptions;
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;
using System.Collections.Generic;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ProjectTest
	{
		[Test]
		public void EscapeDoesWTF ()
		{
			string value_xml = "What are 'ESCAPE' &amp; \"EVALUATE\" ? $ # % ^";
			string value = "What are 'ESCAPE' & \"EVALUATE\" ? $ # % ^";
			string escaped = "What are %27ESCAPE%27 & \"EVALUATE\" %3f %24 # %25 ^";
			string xml = string.Format (@"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<PropertyGroup>
		<Foo>{0}</Foo>
		<Baz>$(FOO)</Baz>
	</PropertyGroup>
</Project>", value_xml);
			var path = "file://localhost/foo.xml";
			var reader = XmlReader.Create (new StringReader (xml), null, path);
			var root = ProjectRootElement.Create (reader);
			var proj = new Project (root);
			var prop = proj.Properties.First (p => p.Name == "Foo");
			Assert.AreEqual (value, prop.UnevaluatedValue, "#1");
			Assert.AreEqual (value, prop.EvaluatedValue, "#2");
			// eh?
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#3");
			prop = proj.Properties.First (p => p.Name == "Baz");
			Assert.AreEqual ("$(FOO)", prop.UnevaluatedValue, "#4");
			Assert.AreEqual (value, prop.EvaluatedValue, "#5");
			// eh?
			Assert.AreEqual (value, Project.GetPropertyValueEscaped (prop), "#6");
			
			// OK you are fine.
			Assert.AreEqual (escaped, ProjectCollection.Escape (value), "#7");
		}
		
		[Test]
		public void FullPath ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			proj.FullPath = "ABC";
			Assert.IsTrue (proj.FullPath.EndsWith (Path.DirectorySeparatorChar + "ABC"), "#1");
			Assert.AreEqual (root.FullPath, proj.FullPath, "#2");
		}
		
		[Test]
		public void BuildEmptyProject ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml), null, "file://localhost/foo.xml");
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.BuildEmptyProject.proj";
            
			// This seems to do nothing and still returns true
			Assert.IsTrue (new Project (root) { FullPath = "ProjectTest.BuildEmptyProject.1.proj" }.Build (), "#1");
			// This seems to fail to find the appropriate target
			Assert.IsFalse (new Project (root) { FullPath = "ProjectTest.BuildEmptyProject.2.proj" }.Build ("Build", null), "#2");
			// Thus, this tries to build all the targets (empty) and no one failed, so returns true(!)
			Assert.IsTrue (new Project (root) { FullPath = "ProjectTest.BuildEmptyProject.3.proj" }.Build (new string [0], null), "#3");
			// Actially null "targets" is accepted and returns true(!!)
			Assert.IsTrue (new Project (root) { FullPath = "ProjectTest.BuildEmptyProject.4.proj" }.Build ((string []) null, null), "#4");
			// matching seems to be blindly done, null string also results in true(!!)
			Assert.IsTrue (new Project (root) { FullPath = "ProjectTest.BuildEmptyProject.5.proj" }.Build ((string) null, null), "#5");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void LoadInvalidProjectForBadCondition ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>What are 'ESCAPE' &amp; ""EVALUATE"" ? $ # % ^</Foo>
    <!-- Note that this contains invalid Condition expression, yet ProjectElement.Create() does NOT fail. -->
    <Baz Condition=""$(Void)=="">$(FOO)</Baz>
  </PropertyGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (xml));
			var root = ProjectRootElement.Create (reader);
			new Project (root);
		}
		
		[Test]
		public void ExpandString ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>What are 'ESCAPE' &amp; ""EVALUATE"" ? $ # % ^</Foo>
    <Bar>y</Bar>
    <Baz Condition=""$(Void)==''"">$(FOO)</Baz>
  </PropertyGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (xml));
			var root = ProjectRootElement.Create (reader);
			var proj = new Project (root);
			root.FullPath = "ProjectTest.ExpandString.proj";
			Assert.AreEqual ("xyz", proj.ExpandString ("x$(BAR)z"), "#1");
			Assert.AreEqual ("x$(BARz", proj.ExpandString ("x$(BARz"), "#2"); // incomplete
			Assert.AreEqual ("xz", proj.ExpandString ("x@(BAR)z"), "#3"); // not an item
		}
		
		[Test]
		public void BuildCSharpTargetGetFrameworkPaths ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new Project (root);
			root.FullPath = "ProjectTest.BuildCSharpTargetGetFrameworkPaths.proj";
			Assert.IsTrue (proj.Build ("GetFrameworkPaths", new ILogger [] {/*new ConsoleLogger ()*/}));
		}
		
		[Test]
		public void ProperiesMustBeDistinct ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <AssemblyName>Foo</AssemblyName>
    <OutputPath>Test</OutputPath>
  </PropertyGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.BuildCSharpTargetBuild.proj";
			var proj = new Project (root);
			var list = new List<ProjectProperty> ();
			foreach (var p in proj.Properties)
				if (list.Any (pp => pp.Name.Equals (p.Name, StringComparison.OrdinalIgnoreCase)))
					Assert.Fail ("Property " + p.Name + " already exists.");
		}
		
		[Test]
		public void BuildCSharpTargetBuild ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <AssemblyName>Foo</AssemblyName>
  </PropertyGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.BuildCSharpTargetBuild.proj";
			var proj = new Project (root, null, "4.0");
			Assert.IsFalse (proj.Build ("Build", new ILogger [] {/*new ConsoleLogger (LoggerVerbosity.Diagnostic)*/})); // missing mandatory properties
		}
		
		[Test]
		public void EvaluateItemConditionThenIgnored ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <P></P>
  </PropertyGroup>
  <ItemGroup>
    <Foo Condition='' Include='x' />
    <Bar Include='$(P)' />
    <Baz Include='z' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			// note that Foo is ignored BUT Bar is NOT ignored.
			Assert.AreEqual (2, proj.ItemsIgnoringCondition.Count, "#1");
			Assert.IsNotNull ("Bar", proj.ItemsIgnoringCondition.First ().ItemType, "#2");
			Assert.IsNotNull ("Baz", proj.ItemsIgnoringCondition.Last ().ItemType, "#3");
		}
		
		[Test]
		public void EvaluateSamePropertiesInOrder ()
		{
			// used in Microsoft.Common.targets
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <BaseIntermediateOutputPath Condition=""'$(BaseIntermediateOutputPath)' == ''"">obj\</BaseIntermediateOutputPath>
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			Assert.AreEqual ("obj" + Path.DirectorySeparatorChar, proj.GetPropertyValue ("BaseIntermediateOutputPath"), "#1");
		}
		
		[Test]
		public void DirtyMarking ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			Assert.IsFalse (proj.IsDirty, "#1");
			proj.MarkDirty ();
			Assert.IsTrue (proj.IsDirty, "#2");
		}
		
		[Test]
		public void DirtyMarking2 ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			proj.DisableMarkDirty = true;
			proj.MarkDirty ();
			Assert.IsFalse (proj.IsDirty, "#1"); // not rejected, just ignored.
			proj.DisableMarkDirty = false;
			Assert.IsFalse (proj.IsDirty, "#2"); // not like status pending
			proj.MarkDirty ();
			Assert.IsTrue (proj.IsDirty, "#3");
		}
		
		[Test]
		public void CreateProjectInstance ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <AssemblyName>Foo</AssemblyName>
  </PropertyGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			var proj = new Project (root, null, "4.0");
			var inst = proj.CreateProjectInstance ();
			Assert.AreEqual ("4.0", inst.ToolsVersion, "#1");
		}
		
		[Test]
		public void LoadCaseInsensitive ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <AssemblyName>Foo</AssemblyName>
  </PropertyGroup>
  <Import Project='$(MSBuildToolsPath)\Microsoft.CSharp.Targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			new Project (root, null, "4.0");
		}
		
		[Test]
		public void SameNameTargets ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Foo'><Message Text='This should not be written' /></Target>
  <Target Name='Foo'><Message Text='This will be written' /></Target>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			var proj = new Project (root, null, "4.0");
			var sw = new StringWriter ();
			proj.Build (new ConsoleLogger (LoggerVerbosity.Diagnostic, sw.WriteLine, null, null));
			Assert.IsTrue (sw.ToString ().Contains ("This will be written"), "#1");
			Assert.IsFalse (sw.ToString ().Contains ("This should not be written"), "#2");
		}

		[Test]
		public void Choose ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Choose>
    <When Condition="" '$(DebugSymbols)' != '' "">
      <PropertyGroup>
        <DebugXXX>True</DebugXXX>
      </PropertyGroup>
    </When>
    <Otherwise>
      <PropertyGroup>
        <DebugXXX>False</DebugXXX>
      </PropertyGroup>
    </Otherwise>
  </Choose>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTest.Choose.proj";
			var proj = new Project (root);
			var p = proj.GetProperty ("DebugXXX");
			Assert.IsNotNull (p, "#1");
			Assert.AreEqual ("False", p.EvaluatedValue, "#2");
		}
	}
}

