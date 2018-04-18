//
// ProjectTargetInstanceTest.cs
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
using Microsoft.Build.Logging;
using Microsoft.Build.Framework;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectTargetInstanceTest
	{
		[Test]
		public void DefaultTargetsEmpty ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			Assert.AreEqual (new string [0], proj.DefaultTargets, "#1");
		}
		
		[Test]
		public void DefaultTargetsFromAttribute ()
		{
            string project_xml = @"<Project DefaultTargets='Foo Bar Baz;Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			string [] expected = {"Foo Bar Baz", "Foo"};
			Assert.AreEqual (expected, proj.DefaultTargets, "#1");
		}
		
		[Test]
		public void DefaultTargetsFromElements ()
		{
			string [] defaultTargetAtts = {string.Empty, "DefaultTargets=''"};
			
			for (int i = 0; i < defaultTargetAtts.Length; i++) {
				string project_xml = string.Format (@"<Project {0} xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Target Name='Foo' />
	<Target Name='Bar' />
</Project>", defaultTargetAtts [i]);
	            var xml = XmlReader.Create (new StringReader (project_xml));
	            var root = ProjectRootElement.Create (xml);
	            var proj = new ProjectInstance (root);
				string [] expected = {"Foo"}; // Bar is not included
				Assert.AreEqual (expected, proj.DefaultTargets, "#1-" + i);
			}
		}
		
		[Test]
		public void MicrosoftCommonTargets ()
		{
			string [] defaultTargetAtts = { string.Empty, "DefaultTargets=''" };
			
			for (int i = 0; i < defaultTargetAtts.Length; i++) {
				string project_xml = string.Format (@"<Project {0} xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
</Project>", defaultTargetAtts [i]);
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				var proj = new ProjectInstance (root);
				Assert.AreEqual ("Build", proj.DefaultTargets.FirstOrDefault (), "#1-" + i);
			}
		}
		
		[Test]
		public void DefaultTargetsOverride ()
		{
            string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
            var proj = new ProjectInstance (root);
			Assert.AreEqual ("Foo", proj.DefaultTargets.FirstOrDefault (), "#1");
		}
		
		[Test]
		public void MultipleDefaultTargets ()
		{
			bool[] expected = { true, false, true };
			string [] defaultTargets = {"Foo", "Foo;Bar", "Foo;Bar"};
				string [] targets = { string.Empty, string.Empty, "<Target Name='Bar' />" };
			for (int i = 0; i < expected.Length; i++) {
				string project_xml = string.Format (@"<Project DefaultTargets='{0}' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
	<Target Name='Foo' />
	{1}
</Project>", defaultTargets [i], targets [i]);
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				root.FullPath = string.Format ("ProjectInstanceTest.MultipleDefaultTargets.{0}.proj", i);
				var proj = new ProjectInstance (root);
				Assert.AreEqual ("Foo", proj.DefaultTargets.FirstOrDefault (), "#1-" + i);
				Assert.AreEqual (expected [i], proj.Build (), "#2-" + i);
			}
		}
		
		[Test]
		public void DependsOnTargets ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<Target Name='Bar' DependsOnTargets='Foo' />
	<Target Name='Foo'>
	    <Error Text='expected error' />
	</Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectInstanceTest.DependsOnTargets.proj";
			var proj = new ProjectInstance (root);
			Assert.AreEqual (2, proj.Targets.Count, "#1");
			Assert.IsFalse (proj.Build ("Bar", new ILogger [0]), "#2");
		}
		
		[Test]
		public void InputsAndOutputs ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Foo' Inputs='inputsandoutputstest.txt' Outputs='inputsandoutputstest.txt'>
    <Error Text='error' />
  </Target>
</Project>";
			try {
				if (!File.Exists ("inputsandoutputstest.txt"))
					File.CreateText ("inputsandoutputstest.txt").Close ();
				var xml = XmlReader.Create (new StringReader (project_xml));
				var root = ProjectRootElement.Create (xml);
				root.FullPath = "ProjectTargetInstanceTest.InputsAndOutputs.proj";
				var proj = new ProjectInstance (root);
				Assert.IsTrue (proj.Build (), "#1"); // if it does not skip Foo, it results in an error.
			} finally {
				if (File.Exists ("inputsandoutputstest.txt"))
					File.Delete ("inputsandoutputstest.txt");
			}
		}
		
		[Test]
		public void PropertiesInTarget ()
		{
			string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Foo' DependsOnTargets='Bar'>
    <Error Text='error' Condition='$(X)!=x' />
  </Target>
  <Target Name='Bar'>
    <PropertyGroup>
      <X>x</X>
    </PropertyGroup>
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTargetInstanceTest.PropertiesInTarget.proj";
			var proj = new ProjectInstance (root);
			Assert.IsTrue (proj.Build (), "#1"); // if it skips Bar or does not persist property X, it results in an error.
		}
		
		[Test]
		public void PropertiesInTarget2 ()
		{
			string project_xml = @"<Project DefaultTargets='Foo' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Foo'>
    <Error Text='error' Condition='$(X)!=x' />
    <!-- defined later, means it does not affect Condition above -->
    <PropertyGroup>
      <X>x</X>
    </PropertyGroup>
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			root.FullPath = "ProjectTargetInstanceTest.PropertiesInTarget.proj";
			var proj = new ProjectInstance (root);
			Assert.IsFalse (proj.Build (), "#1");
		}
	}
}
