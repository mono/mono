//
// ProjectItemDefinitionTest.cs
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
	public class ProjectItemDefinitionTest
	{
		[Test]
		public void Properties ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemDefinitionGroup>
    <Foo>
    	<prop1>value1</prop1>
    	<prop2>value1</prop2>
    </Foo>
    <!-- This one is merged into existing Foo definition above. -->
    <Foo>
    	<prop1>valueX1</prop1><!-- this one overrides value1. -->
    	<prop3>value3</prop3>
    </Foo>
  </ItemDefinitionGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root);
			Assert.AreEqual (1, proj.ItemDefinitions.Count, "#1"); // Foo
			var def = proj.ItemDefinitions ["Foo"];
			Assert.AreEqual ("Foo", def.ItemType, "#1x");
			Assert.AreEqual (3, def.MetadataCount, "#2");
			var md1 = def.Metadata.First (m => m.Name == "prop1");
			Assert.AreEqual ("Foo", md1.ItemType, "#2x");
			Assert.AreEqual ("valueX1", md1.UnevaluatedValue, "#3");
			// FIXME: enable it once we implemented it.
			//Assert.AreEqual ("valueX1", md1.EvaluatedValue, "#4");
			Assert.IsNotNull (md1.Predecessor, "#5");
			Assert.AreEqual ("value1", md1.Predecessor.UnevaluatedValue, "#6");
			// FIXME: enable it once we implemented it.
			//Assert.AreEqual ("value1", md1.Predecessor.EvaluatedValue, "#7");
		}
		
		[Test]
		public void Condition ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemDefinitionGroup>
    <I Condition='{0}'>
      <DefinedMetadata>X</DefinedMetadata>
    </I>
  </ItemDefinitionGroup>
  <ItemGroup>
   <I Include='foo' />
  </ItemGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (string.Format (xml, "True")));
			var root = ProjectRootElement.Create (reader);
			var proj = new Project (root);
			var i = proj.GetItems ("I").First ();
			Assert.AreEqual ("X", i.GetMetadataValue ("DefinedMetadata"), "#1");
			
			reader = XmlReader.Create (new StringReader (string.Format (xml, "False")));
			root = ProjectRootElement.Create (reader);
			proj = new Project (root);
			i = proj.GetItems ("I").First ();
			Assert.AreEqual (string.Empty, i.GetMetadataValue ("DefinedMetadata"), "#2");
		}
	}
}

