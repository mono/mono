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
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class ProjectTaskInstanceTest
	{
		[Test]
		public void OutputPropertyExists ()
		{
			string project_xml = @"
<Project DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<PropertyGroup>
		<C>False</C>
	</PropertyGroup>
	<Target Name='Build' DependsOnTargets='ResolveReferences' />
	<Target Name='Build2' DependsOnTargets='Bar' />
	<Target Name='ResolveReferences' DependsOnTargets='Foo;Bar' />
	<Target Name='Foo'>
		<CreateProperty Value='True'>
			<Output TaskParameter='Value' PropertyName='C' />
		</CreateProperty>
	</Target>
	<Target Name='Bar' Condition='!($(C))' DependsOnTargets='ResolveReferences'>
	</Target>
</Project>";
			var xml = XmlReader.Create (new StringReader(project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			Assert.AreEqual (5, proj.Targets.Count, "#1");
			var foo = proj.Targets ["Foo"];
			Assert.IsNotNull (foo, "#2");
			Assert.AreEqual (1, foo.Tasks.Count, "#3");
			var cp = foo.Tasks.First ();
			Assert.AreEqual (1, cp.Outputs.Count, "#4");
			var po = cp.Outputs.First () as ProjectTaskOutputPropertyInstance;
			Assert.IsNotNull (po, "#5");
			Assert.AreEqual ("C", po.PropertyName, "#5");
			proj.Build ("Build", null);
			Assert.AreEqual (string.Empty, foo.Outputs, "#6");
			Assert.AreEqual ("True", proj.GetPropertyValue ("C"), "#7");
		}
	}
}

