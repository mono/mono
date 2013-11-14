//
// ProjectMetadataInstanceTest.cs
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

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectMetadataInstanceTest
	{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <X Include='foo.txt'>
      <M>m</M>
      <N>=</N>
    </X>
  </ItemGroup>
</Project>";

		[Test]
		public void PropertiesCopiesValues ()
		{
			var xml = XmlReader.Create (new StringReader (project_xml));
			string path = Path.GetFullPath ("foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var item = proj.Items.First ();
			var md = item.Metadata.First ();
			Assert.AreEqual ("m", item.Metadata.First ().EvaluatedValue, "#1");
			Assert.AreEqual ("m", root.ItemGroups.First ().Items.First ().Metadata.First ().Value, "#2");
			root.ItemGroups.First ().Items.First ().Metadata.First ().Value = "X";
			Assert.AreEqual ("m", item.Metadata.First ().EvaluatedValue, "#3");
		}
		
		[Test]
		public void ToStringOverride ()
		{
			var xml = XmlReader.Create (new StringReader (project_xml));
			string path = Path.GetFullPath ("foo.xml");
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var item = proj.Items.First ();
			Assert.AreEqual ("M=m", item.Metadata.First ().ToString (), "#1");
			Assert.AreEqual ("N==", item.Metadata.Last ().ToString (), "#2"); // haha
		}
	}
}

