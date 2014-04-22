//
// ProjectPropertyTest.cs
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
	public class ProjectPropertyTest
	{
		[Test]
		public void SetUnevaluatedValueOverwritesElementValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo>Bar</Foo>
    <Item/>
    <X>1</X>
    <X>2</X>
    <PATH>overriden</PATH>
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var pe = root.Properties.First ();
			Assert.AreEqual ("Bar", pe.Value, "#1");
			var proj = new Project (root);
			var prop = proj.Properties.First (p => p.Name == "Foo");
			Assert.AreEqual ("Bar", prop.UnevaluatedValue, "#2");
			prop.UnevaluatedValue = "x";
			Assert.AreEqual ("x", pe.Value, "#3");
			
			prop = proj.Properties.First (p => p.Name == "X");
			Assert.AreEqual ("2", prop.UnevaluatedValue, "#4");
			Assert.IsNotNull (prop.Predecessor, "#5");
			Assert.AreEqual ("1", prop.Predecessor.UnevaluatedValue, "#6");
			
			// environment property could also be Predecessor (and removed...maybe.
			// I could reproduce only NRE = .NET bug with environment property so far.)
			prop = proj.Properties.First (p => p.Name == "PATH");
			Assert.AreEqual ("overriden", prop.UnevaluatedValue, "#7");
			Assert.IsNotNull (prop.Predecessor, "#8");
		}

		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void UpdateGlobalPropertyValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var props = new Dictionary<string, string> ();
			props.Add ("GP", "GV");
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root, props, null);
			var pe = proj.Properties.First (p => p.IsGlobalProperty);
			pe.UnevaluatedValue = "UPDATED";
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UpdateEnvironmentPropertyValue ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			var pe = proj.Properties.First (p => p.IsEnvironmentProperty);
			pe.UnevaluatedValue = "UPDATED";
		}

		[Test]
		public void DeepReferences ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>1</A>
    <B>$(A)+1</B>
    <C>$(B)+2</C>
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			Assert.AreEqual ("1+1+2", new Project (root).GetProperty ("C").EvaluatedValue, "#1");
			Assert.AreEqual ("1+1+2", new ProjectInstance (root).GetProperty ("C").EvaluatedValue, "#1");
		}
		
		[Test]
		public void PlatformPropertyEmptyByDefault ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			Assert.IsNull (proj.GetProperty ("PLATFORM"), "#1");
		}
	}
}

