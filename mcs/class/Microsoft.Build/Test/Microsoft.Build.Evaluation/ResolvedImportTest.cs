//
// ResolvedImportTest.cs
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
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;

namespace MonoTests.Microsoft.Build.Evaluation
{
	[TestFixture]
	public class ResolvedImportTest
	{
		const string temp_file_name = "test_imported.proj";
		
		[Test]
		public void SimpleImportAndSemanticValues ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='test_imported.proj' />
</Project>";
			string imported = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>x</A>
    <B>y</B>
  </PropertyGroup>
  <ItemGroup>
    <X Include=""included.txt"" />
  </ItemGroup>
</Project>";
			using (var ts = File.CreateText (temp_file_name))
				ts.Write (imported);
			try {
				var reader = XmlReader.Create (new StringReader (xml));
				var root = ProjectRootElement.Create (reader);
				Assert.AreEqual (temp_file_name, root.Imports.First ().Project, "#1");
				var proj = new Project (root);
				var prop = proj.GetProperty ("A");
				Assert.IsNotNull (prop, "#2");
				Assert.IsTrue (prop.IsImported, "#3");
				var item = proj.GetItems ("X").FirstOrDefault ();
				Assert.IsNotNull (item, "#4");
				Assert.AreEqual ("included.txt", item.EvaluatedInclude, "#5");
			} finally {
				File.Delete (temp_file_name);
			}
		}

			string import_overrides_test_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>X</A>
  </PropertyGroup>
  <Import Condition=""{0}"" Project='test_imported.proj' />
  <PropertyGroup>
    <B>Y</B>
  </PropertyGroup>
</Project>";
			string import_overrides_test_imported = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <C Condition='$(A)==x'>c</C>
    <A>a</A>
    <B>b</B>
  </PropertyGroup>
  <ItemGroup>
    <X Include=""included.txt"" />
  </ItemGroup>
</Project>";

		void ImportAndPropertyOverrides (string label, string condition, string valueA, string valueB, string valueAPredecessor, bool existsC)
		{
			using (var ts = File.CreateText (temp_file_name))
				ts.Write (import_overrides_test_imported);
			try {
				string xml = string.Format (import_overrides_test_xml, condition);
				var reader = XmlReader.Create (new StringReader (xml));
				var root = ProjectRootElement.Create (reader);
				var proj = new Project (root);
				var a = proj.GetProperty ("A");
				Assert.IsNotNull (a, label + "#2");
				Assert.AreEqual (valueA, a.EvaluatedValue, label + "#3");
				if (valueAPredecessor == null)
					Assert.IsNull (a.Predecessor, label + "#3.1");
				else {
					Assert.IsNotNull (a.Predecessor, label + "#3.2");
					Assert.AreEqual (valueAPredecessor, a.Predecessor.EvaluatedValue, label + "#3.3");
				}
				var b = proj.GetProperty ("B");
				Assert.IsNotNull (b, label + "#4");
				Assert.AreEqual (valueB, b.EvaluatedValue, label + "#5");
				var c = proj.GetProperty ("C"); // yes it can be retrieved.
				if (existsC) {
					Assert.IsNotNull (c, label + "#6");
					Assert.AreEqual ("c", c.EvaluatedValue, label + "#7");
				}
				else
					Assert.IsNull (c, label + "#8");
			} finally {
				File.Delete (temp_file_name);
			}
		}

		[Test]
		public void ImportAndPropertyOverrides ()
		{
			ImportAndPropertyOverrides ("[1]", "'True'", "a", "Y", "X", true);
			ImportAndPropertyOverrides ("[2]", "$(A)=='X'", "a", "Y", "X", true); // evaluated as true
			ImportAndPropertyOverrides ("[3]", "$(B)=='Y'", "X", "Y", null, false); // evaluated as false
			ImportAndPropertyOverrides ("[4]", "$(B)=='b'", "X", "Y", null, false); // of course not evaluated with imported value
		}

		// FIXME:
		// Looks like $(MSBuildThisFile) is available only within property value, not via .NET MSBuild API.
		// Right now our variable is added as a Reserved property, but we will have to hide it.
		//
		[Test]
		public void EvaluateMSBuildThisFileProperty ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>$(MSBuildThisFile)</A>
  </PropertyGroup>
  <Import Project='test_imported.proj' />
</Project>";
			string imported = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <B>$(MSBuildThisFile)</B>
  </PropertyGroup>
</Project>";
			using (var ts = File.CreateText (temp_file_name))
				ts.Write (imported);
			try {
				var reader = XmlReader.Create (new StringReader (xml));
				var root = ProjectRootElement.Create (reader);
				var proj = new Project (root);
				var a = proj.GetProperty ("A");
				Assert.AreEqual (string.Empty, a.EvaluatedValue, "#1");
				var b = proj.GetProperty ("B");
				Assert.AreEqual (temp_file_name, b.EvaluatedValue, "#2");

				var pi  = new ProjectInstance (root);
				var ai = pi.GetProperty ("A");
				Assert.AreEqual (string.Empty, ai.EvaluatedValue, "#3");
				var bi = pi.GetProperty ("B");
				Assert.AreEqual (temp_file_name, bi.EvaluatedValue, "#4");
			} finally {
				File.Delete (temp_file_name);
			}
		}
	}
}

