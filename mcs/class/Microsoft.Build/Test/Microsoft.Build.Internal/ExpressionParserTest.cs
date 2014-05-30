//
// ExpressionParserTest.cs
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
using Microsoft.Build.Execution;
using Microsoft.Build.Exceptions;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace MonoTests.Microsoft.Build.Internal
{
	[TestFixture]
	public class ExpressionParserTest
	{
			string [] invalid_always = {
				"$(Foo..Bar)",
				"$([DateTime.Now])", // fullname required
				"$([System.DateTime.Now])", // member cannot be invoked with '.'
			};
			string [] invalid_as_boolean = {
				"$",
				"@",
				"%",
				"%-1",
				"$(",
				"%(",
				"$)",
				"%)",
				"%24",
				"()",
				"{}",
				"A", // must be evaluated as a boolean
				"1", // ditto (no default conversion to bool)
				"$ (foo) == ''",
				"@ (foo) == ''",
				"$(1)",
				"$(Foo) == And", // reserved keyword 'and'
				"$(Foo) == Or", // reserved keyword 'or'
				"$(Foo) == $(Bar) == $(Baz)", // unexpected '=='
				"$([System.DateTime]::Now)", // it is DateTime
				"$([System.String]::Format('Tr'))$([System.String]::Format('ue'))", // only one expression is accepted
				"$([System.String]::Format(null))", // causing ANE, wrapped by InvalidProjectFileException
				"yep",
				"nope",
				"ONN",
				"OFFF",
			};
			string [] valid = {
				"'%24' == 0",
				"true",
				"fAlSe",
				"(false)",
				"A==A",
				"A ==A",
				"A== A",
				"A=='A'",
				"A==\tA",
				"\tA== A",
				"$([System.String]::Format('True'))",
				"$([System.String]::Format('True', null))",
				"False And True == True And True",
				"True or True or False",
				"(True or True or False)",
				"True and False",
				"(True) and (False)",
				"yes",
				"nO",
				"oN",
				"oFf",
			};
			string [] depends = {
				// valid only if evaluated to boolean
				"$(foo)",
				"@(foo)",
			};
			
		[Test]
		public void EvaluateAsBoolean ()
		{
			foreach (var expr in invalid_always.Concat (invalid_as_boolean).Concat (valid)) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" Include='x' />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				try {
					new Project (root);
					if (invalid_as_boolean.Contains (expr) || invalid_always.Contains (expr))
						Assert.Fail ("Parsing Condition '{0}' should fail", expr);
				} catch (Exception ex) {
					if (valid.Contains (expr))
						throw new Exception (string.Format ("failed to parse '{0}'", expr), ex);
					else if (ex is InvalidProjectFileException)
						continue;
					throw new Exception (string.Format ("unexpected exception to parse '{0}'", expr), ex);
				}
			}
		}
		
		[Test]
		public void EvaluateAsString ()
		{
			foreach (var expr in invalid_always.Concat (invalid_as_boolean).Concat (valid)) {
				try {
					string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	  <ItemGroup>
	    <Foo Include=""{0}"" />
	  </ItemGroup>
	</Project>";
					var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
					var root = ProjectRootElement.Create (xml);
					// everything but 'invalid_always' should pass
					new Project (root);
				} catch (Exception ex) {
					if (!invalid_always.Contains (expr))
						throw new Exception (string.Format ("failed to parse '{0}'", expr), ex);
				}
			}
		}
		
		[Test]
		public void EvaluatePropertyReferencesWithProperties ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""$(foo)"" Include='x' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var props = new Dictionary<string,string> ();
			props ["foo"] = "true";
			new Project (root, props, null);
		}
		
		[Test]
		public void EvaluateItemReferences ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Include='false' />
    <!-- by the time Bar is evaluated, Foo is already evaluated and taken into consideration in expansion -->
    <Bar Condition=""@(foo)"" Include='x' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			new Project (root);
		}
		
		[Test]
		public void EvaluateReferencesWithoutProperties ()
		{
			foreach (var expr in depends) {
				string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Foo Condition=""{0}"" Include='x' />
  </ItemGroup>
</Project>";
				var xml = XmlReader.Create (new StringReader (string.Format (project_xml, expr)));
				var root = ProjectRootElement.Create (xml);
				try {
					new Project (root);
					Assert.Fail ("Parsing Condition '{0}' should fail", expr);
				} catch (InvalidProjectFileException) {
					continue;
				}
			}
		}
		
		[Test]
		public void SemicolonHandling ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <Foo Condition=""'A;B'=='A;B'"">'A;B'</Foo>
  </PropertyGroup>
  <ItemGroup>
    <Bar Include='$(Foo)' />
  </ItemGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root); // at this state property is parsed without error i.e. Condition evaluates fine.
			var prop = proj.GetProperty ("Foo");
			Assert.AreEqual ("'A;B'", prop.EvaluatedValue, "#1");
			var items = proj.GetItems ("Bar");
			Assert.AreEqual ("'A", items.First ().EvaluatedInclude, "#2");
			Assert.AreEqual ("$(Foo)", items.First ().UnevaluatedInclude, "#3");
			Assert.AreEqual (2, items.Count, "#4");
			Assert.AreEqual ("B'", items.Last ().EvaluatedInclude, "#5");
			Assert.AreEqual ("$(Foo)", items.Last ().UnevaluatedInclude, "#6");
			Assert.IsTrue (items.First ().Xml == items.Last ().Xml, "#7");
		}
		
		// the same as above except that ItemGroup goes first (and yet evaluated the same).
		[Test]
		public void EvaluationOrderPropertiesPrecedesItems ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Bar Include='$(Foo)' />
  </ItemGroup>
  <PropertyGroup>
    <Foo Condition=""'A;B'=='A;B'"">'A;B'</Foo>
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new Project (root); // at this state property is parsed without error i.e. Condition evaluates fine.
			var prop = proj.GetProperty ("Foo");
			Assert.AreEqual ("'A;B'", prop.EvaluatedValue, "#1");
			var items = proj.GetItems ("Bar");
			Assert.AreEqual ("'A", items.First ().EvaluatedInclude, "#2");
			Assert.AreEqual ("$(Foo)", items.First ().UnevaluatedInclude, "#3");
			Assert.AreEqual (2, items.Count, "#4");
			Assert.AreEqual ("B'", items.Last ().EvaluatedInclude, "#5");
			Assert.AreEqual ("$(Foo)", items.Last ().UnevaluatedInclude, "#6");
			Assert.IsTrue (items.First ().Xml == items.Last ().Xml, "#7");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void PropertyReferencesItem ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <ItemGroup>
    <Bar Include='True' />
  </ItemGroup>
  <PropertyGroup>
    <Foo Condition='@(Bar)'>X</Foo><!-- not allowed -->
  </PropertyGroup>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			new Project (root);
		}
		
		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void SequentialPropertyReferenceNotAllowed ()
		{
			string xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <A>x</A>
    <B>y</B>
    <C Condition=""$(A)$(B)==''"">z</C>
  </PropertyGroup>
</Project>";
			var reader = XmlReader.Create (new StringReader (xml));
			var root = ProjectRootElement.Create (reader);
			new Project (root);
		}
		
		[Test]
		public void MetadataExpansion ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <X>a/b/c.txt</X>
  </PropertyGroup>
  <Target Name='Foo'>
    <CreateItem Include='$(X)'>
      <Output TaskParameter='Include' ItemName='I' />
    </CreateItem>
    <CreateProperty Value=""@(I->'%(Filename)%(Extension)')"">
      <Output TaskParameter='Value' PropertyName='P' />
    </CreateProperty>
    <Error Text=""Expected 'c.txt' but got '$(P)'"" Condition=""'$(P)'!='c.txt'"" />
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var p = new ProjectInstance (root);
			var sw = new StringWriter ();
			var result = p.Build (new ILogger [] { new ConsoleLogger (LoggerVerbosity.Minimal, sw.WriteLine, null, null)});
			Assert.IsTrue (result, "#1: " + sw);
		}

		[Test]
		public void MultipleBinaryCondition ()
		{
			string cond = @"$(AndroidIncludeDebugSymbols) == '' And Exists ('$(_IntermediatePdbFile)') And '$(OS)' == 'Windows_NT'";
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <PropertyGroup>
    <X>a/b/c.txt</X>
  </PropertyGroup>
  <Target Name='Foo'>
    <CreateItem Include='$(X)'>
      <Output TaskParameter='Include' ItemName='I' />
    </CreateItem>
    <CreateProperty Value=""@(I->'%(Filename)%(Extension)')"">
      <Output TaskParameter='Value' PropertyName='P' />
    </CreateProperty>
    <Error Text=""Expected 'c.txt' but got '$(P)'"" Condition=""" + cond + @""" />
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var p = new ProjectInstance (root);
			var sw = new StringWriter ();
			var result = p.Build (new ILogger [] { new ConsoleLogger (LoggerVerbosity.Minimal, sw.WriteLine, null, null)});
			Assert.IsTrue (result, "#1: " + sw);
		}

		[Test]
		public void FunctionCall ()
		{
			string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Foo'>
    <Warning Text=""$([Microsoft.Build.Utilities.ToolLocationHelper]::GetPathToStandardLibraries ('$(TargetFrameworkIdentifier)', '$(TargetFrameworkVersion)', '$(TargetFrameworkProfile)'))\mscorlib.dll'"" />
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var p = new ProjectInstance (root, null, "4.0", ProjectCollection.GlobalProjectCollection);
			var sw = new StringWriter ();
			var result = p.Build (new ILogger [] { new ConsoleLogger (LoggerVerbosity.Minimal, sw.WriteLine, null, null)});
			Assert.IsTrue (result, "#1: " + sw);
		}
	}
}

